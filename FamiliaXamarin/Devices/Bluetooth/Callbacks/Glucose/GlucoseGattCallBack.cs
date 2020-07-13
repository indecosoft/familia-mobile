using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Familia.DataModels;
using Familia.Devices.GlucoseDevice;
using Familia.Devices.Helpers;
using Familia.Devices.Models;
using Java.Util;

namespace Familia.Devices.Bluetooth.Callbacks.Glucose {
	public class GlucoseGattCallBack : BluetoothGattCallback {
		private readonly Context _context;
		private readonly Handler _handler = new Handler();
		private int _timesyncUtcTzCnt;
		private readonly IEnumerable<BluetoothDeviceRecords> _listOfSavedDevices;
		//private BluetoothGatt _bluetoothGatt;
		private BluetoothGattCharacteristic _glucoseMeasurementContextCharacteristic;
		private BluetoothGattCharacteristic _customTimeCharacteristic;
		private BluetoothGattCharacteristic _glucoseMeasurementCharacteristic;
		private BluetoothGattCharacteristic _deviceManufacturerCharacteristic;
		private BluetoothGattCharacteristic _deviceSoftwareRevisionCharacteristic;
		private BluetoothGattCharacteristic _deviceSerialCharacteristic;
		private BluetoothGattCharacteristic _racpCharacteristic;
		private readonly Dictionary<int, GlucoseRecord> _records = new Dictionary<int, GlucoseRecord>();
		private bool _isDownloadComplete;
		private bool _isIsensMeter;
		private string[] _bleSwRevision;
		private string _serial;

		public GlucoseGattCallBack(Context context, IEnumerable<BluetoothDeviceRecords> listOfSavedDevices) {
			_context = context;
			_listOfSavedDevices = listOfSavedDevices;
		}

		private void DisplayMessageToUi(string message) {
			((GlucoseDeviceActivity) _context).RunOnUiThread(() =>
				((GlucoseDeviceActivity) _context).LbStatus.Text = message);
		}

		public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState) {
			base.OnConnectionStateChange(gatt, status, newState);
			var devicesDataNormalized = from c in _listOfSavedDevices
				where c.Address == gatt.Device.Address
				select new {c.Name, c.Address, c.DeviceType};
			switch (newState) {
				case ProfileState.Connected:
					DisplayMessageToUi($"S-a conectat la {devicesDataNormalized.FirstOrDefault()?.Name}...");
					gatt.DiscoverServices();
					break;
				case ProfileState.Connecting:
					DisplayMessageToUi($"Se conecteaza la {devicesDataNormalized.FirstOrDefault()?.Name}...");
					break;
				case ProfileState.Disconnected:
					gatt.Close();
                    gatt.Disconnect();
					DisplayMessageToUi("Citirea s-a efectuat cu succes");
					if (_records.Count > 0) {
						var oderedRecords = _records.OrderByDescending(r => r.Value.DateTimeRecord).ToList();
						Task.Run(() => {
							((GlucoseDeviceActivity) _context).RunOnUiThread(async () =>
								await ((GlucoseDeviceActivity) _context).UpdateUi(oderedRecords.First().Value
									.GlucoseData));
						});
					} else {
						DisplayMessageToUi("Nu s-au gasit date");
						Task.Run(() => {
							((GlucoseDeviceActivity) _context).RunOnUiThread(async () =>
								await ((GlucoseDeviceActivity) _context).UpdateUi());
						});
					}

					break;
				case ProfileState.Disconnecting:
					DisplayMessageToUi($"Se deconecteaza de la {devicesDataNormalized.FirstOrDefault()?.Name}...");
					break;
			}
		}

		public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status) {
			if (status != GattStatus.Success) return;
			foreach (BluetoothGattService service in gatt.Services) {
				if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_GLUCOSE)) {
					_glucoseMeasurementCharacteristic =
						service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_MEASUREMENT);
					_glucoseMeasurementContextCharacteristic =
						service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_CONTEXT);
					_racpCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_RACP);
					_records.Clear();
				} else if (BLEHelpers.BLE_SERVICE_DEVICE_INFO.Equals(service.Uuid)) {
					_deviceSoftwareRevisionCharacteristic =
						service.GetCharacteristic(BLEHelpers.BLE_CHAR_SOFTWARE_REVISION);
					_deviceManufacturerCharacteristic =
						service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_MANUFACTURE);
					_deviceSerialCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_SERIALNUM);
				} else if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_CUSTOM_TIME_MC)) {
					_customTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_MC);
					if (_customTimeCharacteristic != null) {
						gatt.SetCharacteristicNotification(_customTimeCharacteristic, true);
					}
				} else if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_CUSTOM_TIME_TI)) {
					_customTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI);
					if (_customTimeCharacteristic != null) {
						gatt.SetCharacteristicNotification(_customTimeCharacteristic, true);
					}
				} else if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_CUSTOM_TIME_TI_NEW)) {
					_customTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW);
					if (_customTimeCharacteristic != null) {
						gatt.SetCharacteristicNotification(_customTimeCharacteristic, true);
					}
				}

				if (_deviceManufacturerCharacteristic != null) {
					gatt.ReadCharacteristic(_deviceManufacturerCharacteristic);
				}
			}
		}

		private void EnableRecordAccessControlPointIndication(BluetoothGatt gatt) {
			if (_racpCharacteristic == null) return;
			gatt.SetCharacteristicNotification(_racpCharacteristic, true);
			BluetoothGattDescriptor descriptor = _racpCharacteristic.GetDescriptor(BLEHelpers.BLE_DESCRIPTOR);
			descriptor.SetValue(BluetoothGattDescriptor.EnableIndicationValue.ToArray());
			gatt.WriteDescriptor(descriptor);
		}

		public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic,
			[GeneratedEnum] GattStatus status) {
			switch (status) {
				case GattStatus.Success:
					DisplayMessageToUi("Se preiau date...");
					if (BLEHelpers.BLE_CHAR_GLUCOSE_MANUFACTURE.Equals(characteristic.Uuid)) {
						string manufacturer = characteristic.GetStringValue(0);
						_isIsensMeter = manufacturer.Equals("i-SENS");
						if (!manufacturer.Equals("i-SENS")) {
							Log.Error("Manfacture Error", "Device not supported");
							gatt.Disconnect();
						} else if (_deviceSoftwareRevisionCharacteristic != null) {
							gatt.ReadCharacteristic(_deviceSoftwareRevisionCharacteristic);
						} else if (_deviceSerialCharacteristic != null) {
							gatt.ReadCharacteristic(_deviceSerialCharacteristic);
						}
					} else if (BLEHelpers.BLE_CHAR_SOFTWARE_REVISION.Equals(characteristic.Uuid)) {
						if (_isIsensMeter) {
							Log.Error("Revision", characteristic.GetStringValue(0));
							_bleSwRevision = characteristic.GetStringValue(0).Split(".");
							int parseInt = int.Parse(_bleSwRevision[0]);
							if (parseInt > 1 || _customTimeCharacteristic == null) {
								Log.Error("Error Revision",
									"Revision greater or equal with 1 and mCustomTimeCharacteristic is null. Disconecting...");
								gatt.Disconnect();
								return;
							}
						}

						if (_deviceSerialCharacteristic != null) {
							gatt.ReadCharacteristic(_deviceSerialCharacteristic);
						}
					} else if (BLEHelpers.BLE_CHAR_GLUCOSE_SERIALNUM.Equals(characteristic.Uuid)) {
						_serial = characteristic.GetStringValue(0);
						Log.Error("Serial", _serial);
						EnableRecordAccessControlPointIndication(gatt);
					}

					break;
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
				case GattStatus.WriteNotPermitted:
					break;
				default:
					Log.Error("OnCharacteristicRead", "Unrecognized Status " + status);
					break;
			}
		}

		public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor,
			[GeneratedEnum] GattStatus status) {
			if (status != GattStatus.Success) return;
			if (BLEHelpers.BLE_CHAR_GLUCOSE_MEASUREMENT.Equals(descriptor.Characteristic.Uuid)) {
				EnableGlucoseContextNotification(gatt);
			} else if (BLEHelpers.BLE_CHAR_GLUCOSE_CONTEXT.Equals(descriptor.Characteristic.Uuid)) {
				if (!_isIsensMeter) {
					RequestSequence(gatt);
				} else {
					if (_customTimeCharacteristic == null ||
					    !_customTimeCharacteristic.Uuid.Equals(BLEHelpers.BLE_CHAR_CUSTOM_TIME_MC)) {
						if (_customTimeCharacteristic == null ||
						    !_customTimeCharacteristic.Uuid.Equals(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI) &&
						    !_customTimeCharacteristic.Uuid.Equals(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW)) {
							_handler.Post(() => {
								if (SetFlag(gatt)) return;
								try {
									Thread.Sleep(500);
									SetFlag(gatt);
								} catch (Exception e) {
									Log.Error("error", e.Message);
								}
							});
						} else {
							EnableTimeSyncIndication(gatt);
						}
					} else {
						try {
							Thread.Sleep(500);
							WriteTimeSync_ex(gatt);
							Thread.Sleep(500);
							RequestSequence(gatt);
						} catch (Exception e) {
							Log.Error("DescriptorWrite nu are customTime characteristic", e.Message);
						}
					}
				}
			} else if (BLEHelpers.BLE_CHAR_GLUCOSE_RACP.Equals(descriptor.Characteristic.Uuid)) {
				EnableGlucoseMeasurementNotification(gatt);
			} else if (BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI.Equals(descriptor.Characteristic.Uuid) ||
			           BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW.Equals(descriptor.Characteristic.Uuid)) {
				_handler.Post(() => {
					if (SetCustomFlag(gatt)) return;
					try {
						Thread.Sleep(500);
						SetCustomFlag(gatt);
					} catch (Exception e) {
						Log.Error("error", e.Message);
					}
				});
			} else {
				Log.Error("desc else", descriptor.Characteristic.Uuid.ToString());
			}
		}

		private void EnableGlucoseMeasurementNotification(BluetoothGatt gatt) {
			if (_glucoseMeasurementCharacteristic == null) return;
			gatt.SetCharacteristicNotification(_glucoseMeasurementCharacteristic, true);
			BluetoothGattDescriptor descriptor =
				_glucoseMeasurementCharacteristic.GetDescriptor(BLEHelpers.BLE_DESCRIPTOR);
			descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
			gatt.WriteDescriptor(descriptor);
		}

		private void SetCustomData_MC(BluetoothGattCharacteristic bluetoothGattCharacteristic) {
			using var gregorianCalendar = new GregorianCalendar();
			var b = (byte) ((gregorianCalendar.Get(CalendarField.Year) % 100) & 255);
			byte[] bArr = {
				1, 0, b, (byte) (((gregorianCalendar.Get(CalendarField.Year) - b) / 100) & 255),
				(byte) ((gregorianCalendar.Get(CalendarField.Month) + 1) & 255),
				(byte) gregorianCalendar.Get(CalendarField.DayOfMonth),
				(byte) (gregorianCalendar.Get(CalendarField.HourOfDay) & 255),
				(byte) (gregorianCalendar.Get(CalendarField.Minute) & 255),
				(byte) (gregorianCalendar.Get(CalendarField.Second) & 255)
			};
			bluetoothGattCharacteristic.SetValue(new byte[bArr.Length]);
			for (var i = 0; i < bArr.Length; i++) {
				bluetoothGattCharacteristic.SetValue(bArr);
			}
		}

		private void WriteTimeSync_ex(BluetoothGatt gatt) {
			try {
				if (gatt == null || _customTimeCharacteristic == null) return;
				SetCustomData_MC(_customTimeCharacteristic);
				gatt.WriteCharacteristic(_customTimeCharacteristic);
			} catch (Exception e) {
				Log.Error("Error", e.Message);
			}
		}

		private void RequestSequence(BluetoothGatt gatt) {
			_handler.Post(() => {
				if (GetSequenceNumber(gatt)) return;
				try {
					Thread.Sleep(500);
					GetSequenceNumber(gatt);
				} catch (Exception e) {
					Log.Error("error Sequence", e.Message);
				}
			});
		}

		private bool GetSequenceNumber(BluetoothGatt gatt) {
			try {
				if (gatt != null && _racpCharacteristic != null) {
					SetOpCode(_racpCharacteristic, 4, 1, new int[0]);
					return gatt.WriteCharacteristic(_racpCharacteristic);
				}
			} catch (Exception e) {
				Log.Error("Error", e.Message);
			}

			return false;
		}

		private void SetOpCode(BluetoothGattCharacteristic bluetoothGattCharacteristic, int i, int i2, int[] numArr) {
			if (bluetoothGattCharacteristic == null) return;
			bluetoothGattCharacteristic.SetValue(new byte[(numArr.Length > 0 ? 1 : 0) + 2 + numArr.Length * 2]);
			bluetoothGattCharacteristic.SetValue(i, GattFormat.Uint8, 0);
			bluetoothGattCharacteristic.SetValue(i2, GattFormat.Uint8, 1);
			if (numArr.Length <= 0) return;
			bluetoothGattCharacteristic.SetValue(1, GattFormat.Uint8, 2);
			var i3 = 3;
			foreach (int intValue in numArr) {
				bluetoothGattCharacteristic.SetValue(intValue, GattFormat.Uint16, i3);
				i3 += 2;
			}
		}

		private void EnableGlucoseContextNotification(BluetoothGatt gatt) {
			if (_glucoseMeasurementContextCharacteristic != null) {
				gatt.SetCharacteristicNotification(_glucoseMeasurementContextCharacteristic, true);
				BluetoothGattDescriptor descriptor =
					_glucoseMeasurementContextCharacteristic.GetDescriptor(BLEHelpers.BLE_DESCRIPTOR);
				descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
				gatt.WriteDescriptor(descriptor);
			}
		}

		private bool SetFlag(BluetoothGatt gatt) {
			if (gatt == null || _racpCharacteristic == null) {
				return false;
			}

			sbyte[] bArr = {-64, 2, -31, 1, 5, 1, 1, 1, 1, 1, 0};
			_racpCharacteristic.SetValue(new byte[bArr.Length]);
			for (var i = 0; i < bArr.Length; i++) {
				_racpCharacteristic.SetValue((byte[]) (Array) bArr);
			}

			return gatt.WriteCharacteristic(_racpCharacteristic);
		}

		private bool SetCustomFlag(BluetoothGatt gatt) {
			if (gatt == null || _customTimeCharacteristic == null) {
				return false;
			}

			sbyte[] bArr = {-64, 2, -31, 1, 5, 1, 1, 1, 1, 1, 0};
			try {
				_customTimeCharacteristic.SetValue(new byte[bArr.Length]);
				for (var i = 0; i < bArr.Length; i++) {
					_customTimeCharacteristic.SetValue((byte[]) (Array) bArr);
				}

				return gatt.WriteCharacteristic(_customTimeCharacteristic);
			} catch (Exception e) {
				Log.Error("setCustomFlagError", e.Message);
			}

			return false;
		}

		private void EnableTimeSyncIndication(BluetoothGatt gatt) {
			if (_customTimeCharacteristic == null) return;
			gatt.SetCharacteristicNotification(_customTimeCharacteristic, true);
			BluetoothGattDescriptor descriptor = _customTimeCharacteristic.GetDescriptor(BLEHelpers.BLE_DESCRIPTOR);
			descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
			gatt.WriteDescriptor(descriptor);
		}

		private void GetCustomTimeSync(BluetoothGatt gatt) {
			try {
				if (gatt == null || _customTimeCharacteristic == null) return;
				SetCustomTimeSync(_customTimeCharacteristic, new GregorianCalendar());
				gatt.WriteCharacteristic(_customTimeCharacteristic);
			} catch (Exception e) {
				Log.Error("Error", e.Message);
			}
		}


		private void SetCustomTimeSync(BluetoothGattCharacteristic bluetoothGattCharacteristic, Calendar calendar) {
			if (bluetoothGattCharacteristic == null) return;
			_timesyncUtcTzCnt++;
			sbyte[] bArr = {
				-64, 3, 1, 0, (sbyte) (calendar.Get(CalendarField.Year) & 255),
				(sbyte) ((calendar.Get(CalendarField.Year) >> 8) & 255),
				(sbyte) ((calendar.Get(CalendarField.Month) + 1) & 255),
				(sbyte) (calendar.Get(CalendarField.DayOfMonth) & 255),
				(sbyte) (calendar.Get(CalendarField.HourOfDay) & 255),
				(sbyte) (calendar.Get(CalendarField.Minute) & 255), (sbyte) (calendar.Get(CalendarField.Second) & 255)
			};
			bluetoothGattCharacteristic.SetValue(new byte[bArr.Length]);
			for (var i = 0; i < bArr.Length; i++) {
				bluetoothGattCharacteristic.SetValue((byte[]) (Array) bArr);
			}
		}

		private void RequestBleAll(BluetoothGatt gatt) {
			_handler.Post(() => {
				if (GetAllRecords(gatt)) return;
				try {
					Thread.Sleep(500);
					GetAllRecords(gatt);
				} catch (Exception e) {
					Log.Error("requestBleAll", e.Message);
				}
			});
		}

		private bool GetAllRecords(BluetoothGatt gatt) {
			try {
				if (gatt != null && _racpCharacteristic != null) {
					SetOpCode(_racpCharacteristic, 1, 1, new int[0]);
					return gatt.WriteCharacteristic(_racpCharacteristic);
				}
			} catch (Exception e) {
				Log.Error("getAllRecords", e.Message);
			}

			return false;
		}

		private int BytesToInt(byte b, byte b2) => UnsignedByteToInt(b) + ((UnsignedByteToInt(b2) & 15) << 8);

		private int UnsignedByteToInt(byte b) => b & 255;

		public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {
			if (BLEHelpers.BLE_CHAR_CUSTOM_TIME_MC.Equals(characteristic.Uuid)) return;
			if (BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI.Equals(characteristic.Uuid) ||
			    BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW.Equals(characteristic.Uuid)) {
				int intValue = characteristic.GetIntValue(GattFormat.Uint8, 0).IntValue();
				switch (intValue) {
					case 5 when !_isIsensMeter || _timesyncUtcTzCnt >= 3:
						RequestSequence(gatt);
						break;
					case 5:
						GetCustomTimeSync(gatt);
						break;
					case 192 when characteristic.GetIntValue(GattFormat.Uint8, 1).IntValue() == 2: {
						if (gatt == null || _racpCharacteristic == null) {
							return;
						}

						GetCustomTimeSync(gatt);
						break;
					}
				}
			} else {
				if (BLEHelpers.BLE_CHAR_GLUCOSE_MEASUREMENT.Equals(characteristic.Uuid)) {
					if (characteristic.GetValue()[0] == 6 || characteristic.GetValue()[0] == 7) {
						return;
					}

					int intValue2 = characteristic.GetIntValue(GattFormat.Uint8, 0).IntValue();
					if (intValue2 == 5) return;
					bool checkIfTimeIsAvailable = (intValue2 & 1) > 0;
					bool checkIfGlucoseDataIsAvailable = (intValue2 & 2) > 0;
					bool checkIfValueIsHighOrLow = (intValue2 & 8) > 0;
					var glucoseRecord = new GlucoseRecord();
					int year = characteristic.GetIntValue(GattFormat.Uint16, 3).IntValue();
					int month = characteristic.GetIntValue(GattFormat.Uint8, 5).IntValue();
					int date = characteristic.GetIntValue(GattFormat.Uint8, 6).IntValue();
					int hourOfDay = characteristic.GetIntValue(GattFormat.Uint8, 7).IntValue();
					int minutes = characteristic.GetIntValue(GattFormat.Uint8, 8).IntValue();
					int seconds = characteristic.GetIntValue(GattFormat.Uint8, 9).IntValue();
					var dateTimeRecord = new DateTime(year, month, date, hourOfDay, minutes, seconds);
					var offset = 10;
					if (checkIfTimeIsAvailable) {
						glucoseRecord.DateTimeRecord = dateTimeRecord;
						offset = 12;
					}

					if (checkIfGlucoseDataIsAvailable) {
						var value = characteristic.GetValue();
						glucoseRecord.GlucoseData = BytesToInt(value[offset], value[offset + 1]);
						offset += 3;
					}

					if (checkIfValueIsHighOrLow) {
						glucoseRecord.FlagHilow = characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue() == 64
							? -1
							: 1;
					}

					try {
						if (glucoseRecord.GlucoseData >= 20) {
							_records.Add(_records.Count, glucoseRecord);
						}
					} catch (Exception e) {
						Log.Error("Data insertion failed", e.Message);
					}
				} else if (BLEHelpers.BLE_CHAR_GLUCOSE_RACP.Equals(characteristic.Uuid)) {
					int status = characteristic.GetIntValue(GattFormat.Uint8, 0).IntValue();
					switch (status) {
						case 5 when gatt == null || _racpCharacteristic == null:
							return;
						case 5 when _serial == null:
							Log.Error("Error", "serial is null");
							return;
						case 5:
							RequestBleAll(gatt);
							break;
						case 6: {
							if (!_isDownloadComplete) {
								_isDownloadComplete = true;
									gatt.WriteCharacteristic(characteristic);
							}

							break;
						}
					}
				}
			}
		}
	}
}