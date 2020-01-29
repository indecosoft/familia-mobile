using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using FamiliaXamarin;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Devices.PressureDevice;
using Java.Util;

namespace Familia.Devices.BluetoothCallbacks.BloodPressure {
    public class BloodPressureGattCallBack : BluetoothGattCallback {
        private readonly Context context;
        private readonly Handler handler = new Handler();
        internal List<BloodPressureData> records = new List<BloodPressureData>();
        private readonly IEnumerable<BluetoothDeviceRecords> ListOfSavedDevices;
        public BloodPressureGattCallBack(Context context , IEnumerable<BluetoothDeviceRecords> listOfSavedDevices) {
            this.context = context;
            ListOfSavedDevices = listOfSavedDevices;
        }
        private void DisplayMessageToUI(string message) {
            ((BloodPressureDeviceActivity)context).RunOnUiThread(() => ((BloodPressureDeviceActivity)context)._lbStatus.Text = message);
        }
        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState) {
            var devicesDataNormalized = from c in ListOfSavedDevices
                                        where c.Address == gatt.Device.Address
                                        select new { c.Name, c.Address, c.DeviceType };
            switch (newState) {
                case ProfileState.Connected:
                    DisplayMessageToUI($"S-a conectat la {devicesDataNormalized.FirstOrDefault().Name}...");
                    gatt.DiscoverServices();
                    break;
                case ProfileState.Connecting:
                    DisplayMessageToUI($"Se conecteaza la {devicesDataNormalized.FirstOrDefault().Name}...");
                    break;
                case ProfileState.Disconnecting:
                    DisplayMessageToUI($"Se deconecteaza de la {devicesDataNormalized.FirstOrDefault().Name}...");
                    break;
                case ProfileState.Disconnected: {
                        gatt.Disconnect();
                        gatt.Close();
                        if (records.Count > 0) {
                            DisplayMessageToUI("Citirea s-a efectuat cu success");
                            var result = records.Where(e => e != null).ToList().OrderByDescending(v => v.RecordDateTime).ToList();
                            foreach (var record in result) {
                                Log.Error("Data sorted", "Sistola: " + record.PulseRate + ", Diastola: " + record.Systolic +
                                     ", Puls: " + record.PulseRate +
                                     ", DateTime: " + record.RecordDateTime);
                            }
                            (context as BloodPressureDeviceActivity).RunOnUiThread(() => {
                                (context as BloodPressureDeviceActivity)._animationView
                                    .CancelAnimation();
                                if (records.Count > 0) {
                                    ((BloodPressureDeviceActivity)context).UpdateUi(result[0]);
                                }
                            });
                        } else {
                            DisplayMessageToUI("Nu s-au gasit date");
                        }
                        break;
                    }
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status) {
            if (status != 0) return;

            if (HasCurrentTimeService(gatt)) {
                var timeCharacteristic =
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
            var flags = characteristic.GetIntValue(GattFormat.Uint8, offset++);
            // See BPMManagerCallbacks.UNIT_* for unit options
            var timestampPresent = ((int)flags & 0x02) > 0;
            var pulseRatePresent = ((int)flags & 0x04) > 0;

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
            records.Add(record);
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
            var dayOfWeek = now.Get(CalendarField.DayOfWeek);
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
            DisplayMessageToUI("Se preiau date...");
            SetCharacteristicNotification(gatt, Constants.UuidBloodPressureService,
                Constants.UuidBloodPressureMeasurementChar);
        }

        private void SetCharacteristicNotification(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            SetCharacteristicNotificationWithDelay(gatt, serviceUuid, characteristicUuid);

        }
        private void SetCharacteristicNotificationWithDelay(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            try {
                handler.PostDelayed(() => {
                    SetCharacteristicNotification_private(gatt, serviceUuid, characteristicUuid);
                }, 300);
            } catch (Exception ex) {
                Log.Error("Error", ex.Message);
            }
            
        }
        private static void SetCharacteristicNotification_private(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            try {
                var characteristic = gatt.GetService(serviceUuid).GetCharacteristic(characteristicUuid);
                gatt.SetCharacteristicNotification(characteristic, true);
                var descriptor = characteristic.GetDescriptor(Constants.ClientCharacteristicConfig);
                var indication = (Convert.ToInt32(characteristic.Properties) & 32) != 0;
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
