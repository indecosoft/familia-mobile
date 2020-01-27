using System;
using System.Linq;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using FamiliaXamarin;
using FamiliaXamarin.Devices.GlucoseDevice;
using Java.Util;

namespace Familia.Devices.Bluetooth.Callbacks.Glucose {
	public class MedisanaGattCallback : BluetoothGattCallback
	{
        private readonly Handler handler = new Handler();
        private readonly Context Context;
		public MedisanaGattCallback(Context context)
		{
			Context = context;
		}

		public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
		{
			base.OnConnectionStateChange(gatt, status, newState);
			if (newState == ProfileState.Connected)
			{
				gatt.DiscoverServices();
			}
		}

		public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
		{
			if (status == 0)
			{
				SetCharacteristicNotification(gatt,
					Constants.UuidGlucServ, Constants.UuidGlucMeasurementChar);
			}
		}

		public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
		{
			base.OnDescriptorWrite(gatt, descriptor, status);
			if (status != 0) return;
			if (descriptor.Characteristic.Uuid.Equals(Constants.UuidGlucMeasurementContextChar))
			{
				SetCharacteristicNotification(gatt,
					Constants.UuidGlucServ, Constants.UuidGlucRecordAccessControlPointChar);
				Log.Error("Aici", "1");
			}
			if (descriptor.Characteristic.Uuid.Equals(Constants.UuidGlucMeasurementChar))
			{
				SetCharacteristicNotification(gatt,
					Constants.UuidGlucServ, Constants.UuidGlucRecordAccessControlPointChar);
				//setCharacteristicNotification(gatt, Constants.UUID_GLUC_SERV, Constants.UUID_GLUC_MEASUREMENT_CONTEXT_CHAR);
				Log.Error("Aici", "2");
			}
			if (descriptor.Characteristic.Uuid.Equals(Constants.UuidGlucRecordAccessControlPointChar))
			{
				Log.Error("Aici", "3");
			}
		}

		public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
		{
			base.OnCharacteristicChanged(gatt, characteristic);
			if (!Constants.UuidGlucMeasurementChar.Equals(characteristic.Uuid)) return;
			var offset = 0;
			var flags = characteristic.GetIntValue(GattFormat.Uint8, offset).IntValue();
			offset += 1;

			var timeOffsetPresent = (flags & 0x01) > 0;
			var typeAndLocationPresent = (flags & 0x02) > 0;
			var concentrationUnit = (flags & 0x04) > 0 ? 1 : 0;
			var sensorStatusAnnunciationPresent = (flags & 0x08) > 0;

			// create and fill the new record
			var record = new GlucoseDeviceData
			{
				SequenceNumber = characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue()
			};
			offset += 2;

			var year = characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
			var month = characteristic.GetIntValue(GattFormat.Uint8, offset + 2).IntValue();
			var day = characteristic.GetIntValue(GattFormat.Uint8, offset + 3).IntValue();
			var hours = characteristic.GetIntValue(GattFormat.Uint8, offset + 4).IntValue();
			var minutes = characteristic.GetIntValue(GattFormat.Uint8, offset + 5).IntValue();
			var seconds = characteristic.GetIntValue(GattFormat.Uint8, offset + 6).IntValue();
			offset += 7;

			var calendar = Calendar.Instance;
			calendar.Set(year, month, day, hours, minutes, seconds);

			if (timeOffsetPresent)
			{
				characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
				offset += 2;
			}

			if (typeAndLocationPresent)
			{
				record.GlucoseConcentration = characteristic
					.GetFloatValue(GattFormat.Sfloat, offset).FloatValue();
				var typeAndLocation = characteristic.GetIntValue(GattFormat.Uint8, offset + 2)
					.IntValue();
				offset += 3;

                (Context as GlucoseDeviceActivity)?.RunOnUiThread(() => {
					(Context as GlucoseDeviceActivity)?.UpdateUi(
						record.GlucoseConcentration * 100000);
				});
			}

			if (sensorStatusAnnunciationPresent)
			{
				characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
			}
		}
        private void SetCharacteristicNotification(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            SetCharacteristicNotificationWithDelay(gatt, serviceUuid, characteristicUuid);
        }

        private void SetCharacteristicNotificationWithDelay(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            handler.PostDelayed(
                () => {
                    SetCharacteristicNotification_private(gatt, serviceUuid, characteristicUuid);
                }, 300);
        }

        private static void SetCharacteristicNotification_private(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            try {
                bool indication;
                var characteristic = gatt.GetService(serviceUuid).GetCharacteristic(characteristicUuid);
                gatt.SetCharacteristicNotification(characteristic, true);
                var descriptor = characteristic.GetDescriptor(Constants.ClientCharacteristicConfig);
                //indication = ((int)characteristic.Properties. & 32) != 0;
                //indication = (characteristic.Properties & GattProperty.Read) != 0;
                indication = (Convert.ToInt32(characteristic.Properties) & 32) != 0;
                Log.Error("Indication", indication.ToString());
                descriptor.SetValue(indication
                    ? BluetoothGattDescriptor.EnableIndicationValue.ToArray()
                    : BluetoothGattDescriptor.EnableNotificationValue.ToArray());

                gatt.WriteDescriptor(descriptor);
            } catch (Java.Lang.Exception e) {
                e.PrintStackTrace();
            }
        }

    }
}
