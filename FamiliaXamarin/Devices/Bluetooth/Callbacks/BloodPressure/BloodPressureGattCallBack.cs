using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using Familia.DataModels;
using Familia.Devices.Helpers;
using Familia.Devices.PressureDevice;
using Java.Lang;
using Java.Util;
using Exception = System.Exception;

namespace Familia.Devices.Bluetooth.Callbacks.BloodPressure {
    public class BloodPressureGattCallBack : BluetoothGattCallback {
        private readonly Context _context;
        private readonly Handler _handler = new Handler();
        internal List<BloodPressureData> Records = new List<BloodPressureData>();
        private readonly IEnumerable<BluetoothDeviceRecords> _listOfSavedDevices;
        public BloodPressureGattCallBack(Context context , IEnumerable<BluetoothDeviceRecords> listOfSavedDevices) {
            _context = context;
            _listOfSavedDevices = listOfSavedDevices;
        }
        private void DisplayMessageToUi(string message) {
            ((BloodPressureDeviceActivity)_context).RunOnUiThread(() => ((BloodPressureDeviceActivity)_context).LbStatus.Text = message);
        }
        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState) {
            var devicesDataNormalized = from c in _listOfSavedDevices
                                        where c.Address == gatt.Device.Address
                                        select new { c.Name, c.Address, c.DeviceType };
            switch (newState) {
                case ProfileState.Connected:
                    DisplayMessageToUi($"S-a conectat la {devicesDataNormalized.FirstOrDefault()?.Name}...");
                    gatt.DiscoverServices();
                    break;
                case ProfileState.Connecting:
                    DisplayMessageToUi($"Se conecteaza la {devicesDataNormalized.FirstOrDefault()?.Name}...");
                    break;
                case ProfileState.Disconnecting:
                    DisplayMessageToUi($"Se deconecteaza de la {devicesDataNormalized.FirstOrDefault()?.Name}...");
                    break;
                case ProfileState.Disconnected: {
                        gatt.Disconnect();
                        gatt.Close();
                        Log.Error("GattPressure", "Disconnect");
                        if (Records.Count > 0) {
                            DisplayMessageToUi("Citirea s-a efectuat cu success");
                            var result = Records.Where(e => e != null).ToList().OrderByDescending(v => v.RecordDateTime).ToList();
                            (_context as BloodPressureDeviceActivity)?.RunOnUiThread(() => {
                                if (Records.Count > 0) {
                                    ((BloodPressureDeviceActivity)_context).UpdateUi(result[0]);
                                }
                                (_context as BloodPressureDeviceActivity)?.AnimationView
                                    .CancelAnimation();
                            });
                        } else {
                            DisplayMessageToUi("Nu s-au gasit date");
                            (_context as BloodPressureDeviceActivity)?.RunOnUiThread(() => {
                                (_context as BloodPressureDeviceActivity)?.AnimationView
                                    .CancelAnimation();
                                    ((BloodPressureDeviceActivity)_context).UpdateUi(null);
                            });
                        }
                        break;
                    }
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status) {
            if (status != 0) return;

            if (HasCurrentTimeService(gatt)) {
                BluetoothGattCharacteristic timeCharacteristic =
                    gatt.GetService(UUID.FromString("00001805-0000-1000-8000-00805f9b34fb"))
                        .GetCharacteristic(
                            UUID.FromString("00002A2B-0000-1000-8000-00805f9b34fb"));
                timeCharacteristic.SetValue(GetCurrentTimeLocal());
                gatt.WriteCharacteristic(timeCharacteristic);
            } else {
                ListenToMeasurements(gatt);
            }
        }

        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status) {
            base.OnCharacteristicWrite(gatt, characteristic, status);
            if (status == GattStatus.Success) {
                ListenToMeasurements(gatt);
            }
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {
            var record = new BloodPressureData();
            var offset = 0;
            Integer flags = characteristic.GetIntValue(GattFormat.Uint8, offset++);
            // See BPMManagerCallbacks.UNIT_* for unit options
            bool timestampPresent = ((int)flags & 0x02) > 0;
            bool pulseRatePresent = ((int)flags & 0x04) > 0;

            // following bytes - systolic, diastolic and mean arterial pressure
            record.Systolic = characteristic.GetFloatValue(GattFormat.Sfloat, offset).FloatValue();
            record.Diastolic = characteristic.GetFloatValue(GattFormat.Sfloat, offset + 2)
                .FloatValue();
            offset += 6;
            if (timestampPresent) {
                record.RecordDateTime = new DateTime(
                    characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue(),
                    characteristic.GetIntValue(GattFormat.Uint8, offset + 2).IntValue(),
                    characteristic.GetIntValue(GattFormat.Uint8, offset + 3).IntValue(),
                    characteristic.GetIntValue(GattFormat.Uint8, offset + 4).IntValue(),
                    characteristic.GetIntValue(GattFormat.Uint8, offset + 5).IntValue(),
                    characteristic.GetIntValue(GattFormat.Uint8, offset + 6).IntValue());
                offset += 7;
            }
            if (pulseRatePresent) {
                record.PulseRate = characteristic.GetFloatValue(GattFormat.Sfloat, offset).FloatValue();
            }
            Records.Add(record);
        }
        private bool HasCurrentTimeService(BluetoothGatt gatt) {
            return gatt.Services.Any(service =>
                service.Uuid.Equals(UUID.FromString("00001805-0000-1000-8000-00805f9b34fb")));
        }

        private byte[] GetCurrentTimeLocal() {
            return GetCurrentTimeWithOffset();
        }

        private byte[] GetCurrentTimeWithOffset() {
            var now = Calendar.Instance;
            now.Time = new Date();
            now.Add(CalendarField.HourOfDay, 0);
            var time = new byte[10];
            time[0] = (byte)((now.Get(CalendarField.Year) >> 8) & 255);
            time[1] = (byte)(now.Get(CalendarField.Year) & 255);
            time[2] = (byte)(now.Get(CalendarField.Month) + 1);
            time[3] = (byte)now.Get(CalendarField.DayOfMonth);
            time[4] = (byte)now.Get(CalendarField.HourOfDay);
            time[5] = (byte)now.Get(CalendarField.Minute);
            time[6] = (byte)now.Get(CalendarField.Second);
            int dayOfWeek = now.Get(CalendarField.DayOfWeek);
            if (dayOfWeek == 1) {
                dayOfWeek = 7;
            } else {
                dayOfWeek--;
            }
            time[7] = (byte)dayOfWeek;
            time[8] = 0;
            time[9] = 1;

            return time;
        }
        private void ListenToMeasurements(BluetoothGatt gatt) {
            DisplayMessageToUi("Se preiau date...");
            SetCharacteristicNotification(gatt, BLEHelpers.UuidBloodPressureService,
                BLEHelpers.UuidBloodPressureMeasurementChar);
        }

        private void SetCharacteristicNotification(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            SetCharacteristicNotificationWithDelay(gatt, serviceUuid, characteristicUuid);

        }
        private void SetCharacteristicNotificationWithDelay(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            try {
                _handler.PostDelayed(() => {
                    SetCharacteristicNotification_private(gatt, serviceUuid, characteristicUuid);
                }, 300);
            } catch (Exception ex) {
                Log.Error("Error", ex.Message);
            }
            
        }
        private static void SetCharacteristicNotification_private(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            try {
                BluetoothGattCharacteristic characteristic = gatt.GetService(serviceUuid).GetCharacteristic(characteristicUuid);
                gatt.SetCharacteristicNotification(characteristic, true);
                BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(BLEHelpers.ClientCharacteristicConfig);
                bool indication = (Convert.ToInt32(characteristic.Properties) & 32) != 0;
                descriptor.SetValue(indication
                    ? BluetoothGattDescriptor.EnableIndicationValue.ToArray()
                    : BluetoothGattDescriptor.EnableNotificationValue.ToArray());

                gatt.WriteDescriptor(descriptor);
            } catch (Exception e) {
                Log.Error("BloodPressureGattCallbackerror", e.Message);
            }
        }

    }
}
