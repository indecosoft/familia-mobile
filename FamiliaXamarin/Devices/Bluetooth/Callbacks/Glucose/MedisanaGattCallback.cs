using System;
using System.Linq;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using Familia.Devices.GlucoseDevice;
using Familia.Devices.Helpers;
using Java.Util;
using Exception = Java.Lang.Exception;

namespace Familia.Devices.Bluetooth.Callbacks.Glucose {
	public class MedisanaGattCallback : BluetoothGattCallback {
		private readonly Handler _handler = new Handler();
		private readonly Context _context;
		private GlucoseDeviceData _record;

		public MedisanaGattCallback(Context context) {
			_context = context;
		}

		private void DisplayMessageToUi(string message) {
			((GlucoseDeviceActivity) _context).RunOnUiThread(() =>
				((GlucoseDeviceActivity) _context).LbStatus.Text = message);
		}

		public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState) {
			switch (status) {
				case GattStatus.ConnectionCongested:
					break;
				case GattStatus.Failure:
					break;
				case GattStatus.InsufficientAuthentication:
					break;
				case GattStatus.InsufficientEncryption:
					break;
				case GattStatus.InvalidAttributeLength:
					break;
				case GattStatus.InvalidOffset:
					break;
				case GattStatus.ReadNotPermitted:
					break;
				case GattStatus.RequestNotSupported:
					break;
				case GattStatus.Success:
					switch (newState) {
						case ProfileState.Connected:
							break;
						case ProfileState.Connecting:
							break;
						case ProfileState.Disconnected:
							if (_record != null) {
								(_context as GlucoseDeviceActivity)?.RunOnUiThread(() => {
									DisplayMessageToUi("Citirea s-a efectuat cu success");
									(_context as GlucoseDeviceActivity)?.UpdateUi(
										_record?.GlucoseConcentration * 100000);
								});
							} else {
								(_context as GlucoseDeviceActivity)?.RunOnUiThread(() => {
									DisplayMessageToUi("Nu s-au gasit date");
									(_context as GlucoseDeviceActivity)?.UpdateUi();
								});
							}

							break;
						case ProfileState.Disconnecting:
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
					}

					if (newState == ProfileState.Connected) {
						gatt.DiscoverServices();
					}

					break;
				case GattStatus.WriteNotPermitted:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(status), status, null);
			}
		}

		public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status) {
			if (status == GattStatus.Success) {
				SetCharacteristicNotification(gatt, BLEHelpers.UuidGlucServ, BLEHelpers.UuidGlucMeasurementChar);
			}
		}

		public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor,
			GattStatus status) {
			if (status != GattStatus.Success) return;
			if (descriptor.Characteristic.Uuid.Equals(BLEHelpers.UuidGlucMeasurementContextChar)) {
				SetCharacteristicNotification(gatt, BLEHelpers.UuidGlucServ,
					BLEHelpers.UuidGlucRecordAccessControlPointChar);
			}

			if (descriptor.Characteristic.Uuid.Equals(BLEHelpers.UuidGlucMeasurementChar)) {
				SetCharacteristicNotification(gatt, BLEHelpers.UuidGlucServ,
					BLEHelpers.UuidGlucRecordAccessControlPointChar);
			}

			if (descriptor.Characteristic.Uuid.Equals(BLEHelpers.UuidGlucRecordAccessControlPointChar)) { }
		}

		public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {
			if (!BLEHelpers.UuidGlucMeasurementChar.Equals(characteristic.Uuid)) return;
			var offset = 0;
			int flags = characteristic.GetIntValue(GattFormat.Uint8, offset).IntValue();
			offset += 1;

			bool timeOffsetPresent = (flags & 0x01) > 0;
			bool typeAndLocationPresent = (flags & 0x02) > 0;
			bool sensorStatusAnnunciationPresent = (flags & 0x08) > 0;

			// create and fill the new record
			_record = new GlucoseDeviceData {
				SequenceNumber = characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue()
			};
			offset += 2;

			int year = characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
			int month = characteristic.GetIntValue(GattFormat.Uint8, offset + 2).IntValue();
			int day = characteristic.GetIntValue(GattFormat.Uint8, offset + 3).IntValue();
			int hours = characteristic.GetIntValue(GattFormat.Uint8, offset + 4).IntValue();
			int minutes = characteristic.GetIntValue(GattFormat.Uint8, offset + 5).IntValue();
			int seconds = characteristic.GetIntValue(GattFormat.Uint8, offset + 6).IntValue();
			offset += 7;

			var calendar = Calendar.Instance;
			calendar.Set(year, month, day, hours, minutes, seconds);

			if (timeOffsetPresent) {
				characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
				offset += 2;
			}

			if (typeAndLocationPresent) {
				_record.GlucoseConcentration = characteristic.GetFloatValue(GattFormat.Sfloat, offset).FloatValue();
				offset += 3;
			}

			if (sensorStatusAnnunciationPresent) {
				characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
			}

			gatt.Disconnect();
		}

		private void SetCharacteristicNotification(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
			SetCharacteristicNotificationWithDelay(gatt, serviceUuid, characteristicUuid);
		}

		private void SetCharacteristicNotificationWithDelay(BluetoothGatt gatt, UUID serviceUuid,
			UUID characteristicUuid) {
			_handler.PostDelayed(
				() => { SetCharacteristicNotification_private(gatt, serviceUuid, characteristicUuid); }, 300);
		}

		private static void SetCharacteristicNotification_private(BluetoothGatt gatt, UUID serviceUuid,
			UUID characteristicUuid) {
			try {
				bool indication;
				BluetoothGattCharacteristic characteristic =
					gatt.GetService(serviceUuid).GetCharacteristic(characteristicUuid);
				gatt.SetCharacteristicNotification(characteristic, true);
				BluetoothGattDescriptor descriptor =
					characteristic.GetDescriptor(BLEHelpers.ClientCharacteristicConfig);
				indication = (Convert.ToInt32(characteristic.Properties) & 32) != 0;
				Log.Error("Indication", indication.ToString());
				descriptor.SetValue(indication
					? BluetoothGattDescriptor.EnableIndicationValue.ToArray()
					: BluetoothGattDescriptor.EnableNotificationValue.ToArray());

				gatt.WriteDescriptor(descriptor);
			} catch (Exception e) {
				e.PrintStackTrace();
			}
		}
	}
}