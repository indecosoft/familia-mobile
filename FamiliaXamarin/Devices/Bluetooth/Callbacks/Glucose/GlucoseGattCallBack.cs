using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Familia.Devices.Models;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Devices.GlucoseDevice;
using Java.Util;

namespace Familia.Devices.BluetoothCallbacks.Glucose {
    public class GlucoseGattCallBack : BluetoothGattCallback {
        private readonly Context Context;
        private readonly Handler handler = new Handler();
        private int timesyncUtcTzCnt;
        private readonly IEnumerable<BluetoothDeviceRecords> ListOfSavedDevices;
        private BluetoothGatt bluetoothGatt;
        private BluetoothGattCharacteristic glucoseMeasurementContextCharacteristic;
        private BluetoothGattCharacteristic customTimeCharacteristic;
        private BluetoothGattCharacteristic glucoseMeasurementCharacteristic;
        private BluetoothGattCharacteristic deviceManufacturerCharacteristic;
        private BluetoothGattCharacteristic deviceSoftwareRevisionCharacteristic;
        private BluetoothGattCharacteristic deviceSerialCharacteristic;
        private BluetoothGattCharacteristic racpCharacteristic;
        private readonly Dictionary<int, GlucoseRecord> records = new Dictionary<int, GlucoseRecord>();
        private bool isDownloadComplete;
        private bool isIsensMeter;
        private string[] ble_sw_revision;
        private string serial;

        public GlucoseGattCallBack(Context context, IEnumerable<BluetoothDeviceRecords> listOfSavedDevices) {
            Context = context;
            ListOfSavedDevices = listOfSavedDevices;
        }
        private void DisplayMessageToUI(string message) {
            ((GlucoseDeviceActivity)Context).RunOnUiThread(() => ((GlucoseDeviceActivity)Context)._lbStatus.Text = message);
        }
        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState) {
            base.OnConnectionStateChange(gatt, status, newState);
            bluetoothGatt = gatt;
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
                case ProfileState.Disconnected:
                    DisplayMessageToUI("Citirea s-a efectuat cu success");
                    if (records.Count > 0) {
                        var oderedRecords = records.OrderByDescending(r => r.Value.DateTimeRecord).ToList();
                        foreach (var record in oderedRecords) {
                            Log.Error("Glucose Data MEASUREMENT: ", $"" +
                                            $"Glucose: {record.Value.glucoseData} " +
                                            $"High/Low: {record.Value.flag_hilow.ToString()} " +
                                            $"Time: {record.Value.DateTimeRecord.ToString()}");
                        }
                        Task.Run(() => {
                            ((GlucoseDeviceActivity)Context).RunOnUiThread(async () => await ((GlucoseDeviceActivity)Context).UpdateUi(oderedRecords.First().Value.glucoseData));
                        });
                    } else {
                        DisplayMessageToUI("Nu s-au gasit date");
                    }
                    break;
                case ProfileState.Disconnecting:
                    DisplayMessageToUI($"Se deconecteaza de la {devicesDataNormalized.FirstOrDefault().Name}...");
                    break;
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status) {
            if (status == GattStatus.Success) {
                foreach (BluetoothGattService service in gatt.Services) {
                    if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_GLUCOSE)) {
                        glucoseMeasurementCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_MEASUREMENT);
                        glucoseMeasurementContextCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_CONTEXT);
                        racpCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_RACP);
                        records.Clear();
                    } else if (BLEHelpers.BLE_SERVICE_DEVICE_INFO.Equals(service.Uuid)) {
                        deviceSoftwareRevisionCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_SOFTWARE_REVISION);
                        deviceManufacturerCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_MANUFACTURE);
                        deviceSerialCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_SERIALNUM);
                    } else if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_CUSTOM_TIME_MC)) {
                        customTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_MC);
                        if (customTimeCharacteristic != null) {
                            gatt.SetCharacteristicNotification(customTimeCharacteristic, true);
                        }
                    } else if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_CUSTOM_TIME_TI)) {
                        customTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI);
                        if (customTimeCharacteristic != null) {
                            gatt.SetCharacteristicNotification(customTimeCharacteristic, true);
                        }
                    } else if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_CUSTOM_TIME_TI_NEW)) {
                        customTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW);
                        if (customTimeCharacteristic != null) {
                            gatt.SetCharacteristicNotification(customTimeCharacteristic, true);
                        }
                    }
                    if (deviceManufacturerCharacteristic != null) {
                        gatt.ReadCharacteristic(deviceManufacturerCharacteristic);
                    }
                }
            }
        }

        private void EnableRecordAccessControlPointIndication() {
            if (racpCharacteristic != null) {
                bluetoothGatt.SetCharacteristicNotification(racpCharacteristic, true);
                BluetoothGattDescriptor descriptor = racpCharacteristic.GetDescriptor(BLEHelpers.BLE_DESCRIPTOR);
                descriptor.SetValue(BluetoothGattDescriptor.EnableIndicationValue.ToArray());
                bluetoothGatt.WriteDescriptor(descriptor);
            }
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status) {
            switch (status) {
                case GattStatus.Success:
                    DisplayMessageToUI("Se preiau date...");
                    if (BLEHelpers.BLE_CHAR_GLUCOSE_MANUFACTURE.Equals(characteristic.Uuid)) {
                        string manufacturer = characteristic.GetStringValue(0);
                        isIsensMeter = manufacturer.Equals("i-SENS");
                        if (!manufacturer.Equals("i-SENS")) {
                            Log.Error("Manfacture Error", "Device not supported");
                            gatt.Disconnect();
                        } else if (deviceSoftwareRevisionCharacteristic != null) {
                            gatt.ReadCharacteristic(deviceSoftwareRevisionCharacteristic);
                        } else if (deviceSerialCharacteristic != null) {
                            gatt.ReadCharacteristic(deviceSerialCharacteristic);
                        }
                    } else if (BLEHelpers.BLE_CHAR_SOFTWARE_REVISION.Equals(characteristic.Uuid)) {
                        if (isIsensMeter) {
                            Log.Error("Revision", characteristic.GetStringValue(0));
                            ble_sw_revision = characteristic.GetStringValue(0).Split(".");
                            int parseInt = int.Parse(ble_sw_revision[0]);
                            if (parseInt > 1 || customTimeCharacteristic == null) {
                                Log.Error("Error Revision", "Revision greater or equal with 1 and mCustomTimeCharacteristic is null. Disconecting...");
                                gatt.Disconnect();
                                return;
                            }
                        }
                        if (deviceSerialCharacteristic != null) {
                            gatt.ReadCharacteristic(deviceSerialCharacteristic);
                        }
                    } else if (BLEHelpers.BLE_CHAR_GLUCOSE_SERIALNUM.Equals(characteristic.Uuid)) {
                        serial = characteristic.GetStringValue(0);
                        Log.Error("Serial", serial);
                        EnableRecordAccessControlPointIndication();
                    }
                    break;
                default:
                    Log.Error("OnCharacteristicRead", "Unrecognized Status " + status);
                    break;
            }
        }
        public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status) {
            if (status == GattStatus.Success) {
                if (BLEHelpers.BLE_CHAR_GLUCOSE_MEASUREMENT.Equals(descriptor.Characteristic.Uuid)) {
                    EnableGlucoseContextNotification();
                } else if (BLEHelpers.BLE_CHAR_GLUCOSE_CONTEXT.Equals(descriptor.Characteristic.Uuid)) {
                    if (!isIsensMeter) {
                        RequestSequence();
                    } else {
                        if (customTimeCharacteristic == null || !customTimeCharacteristic.Uuid.Equals(BLEHelpers.BLE_CHAR_CUSTOM_TIME_MC)) {
                            if (customTimeCharacteristic == null || (!customTimeCharacteristic.Uuid.Equals(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI) && !customTimeCharacteristic.Uuid.Equals(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW))) {
                                handler.Post(() => {
                                    if (!SetFlag()) {
                                        try {
                                            System.Threading.Thread.Sleep(500);
                                            SetFlag();
                                        } catch (Exception e) {
                                            Log.Error("error", e.Message);
                                        }
                                    }
                                });

                            } else {
                                EnableTimeSyncIndication();
                            }
                        } else {
                            try {
                                System.Threading.Thread.Sleep(500);
                                WriteTimeSync_ex();
                                System.Threading.Thread.Sleep(500);
                                RequestSequence();
                            } catch (Exception e) {
                                Log.Error("DescriptorWrite nu are customTime characteristic", e.Message);
                            }
                        }
                    }
                } else if (BLEHelpers.BLE_CHAR_GLUCOSE_RACP.Equals(descriptor.Characteristic.Uuid)) {
                    EnableGlucoseMeasurementNotification();
                } else if (BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI.Equals(descriptor.Characteristic.Uuid) || BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW.Equals(descriptor.Characteristic.Uuid)) {
                    handler.Post(() => {
                        if (!SetCustomFlag()) {
                            try {
                                System.Threading.Thread.Sleep(500);
                                SetCustomFlag();
                            } catch (Exception e) {
                                Log.Error("error", e.Message);
                            }
                        }
                    });
                } else {
                    Log.Error("desc else", descriptor.Characteristic.Uuid.ToString());
                }
            }
        }
        private void EnableGlucoseMeasurementNotification() {
            if (glucoseMeasurementCharacteristic != null) {
                bluetoothGatt.SetCharacteristicNotification(glucoseMeasurementCharacteristic, true);
                BluetoothGattDescriptor descriptor = glucoseMeasurementCharacteristic.GetDescriptor(BLEHelpers.BLE_DESCRIPTOR);
                descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                bluetoothGatt.WriteDescriptor(descriptor);
            }
        }
        private void SetCustomData_MC(BluetoothGattCharacteristic bluetoothGattCharacteristic) {
            using var gregorianCalendar = new GregorianCalendar();
            byte b = (byte)((gregorianCalendar.Get(CalendarField.Year) % 100) & 255);
            byte[] bArr = { 1, 0,
                    b,
                    (byte)(((gregorianCalendar.Get(CalendarField.Year) - b) / 100) & 255),
                    (byte)((gregorianCalendar.Get(CalendarField.Month) + 1) & 255),
                    (byte)gregorianCalendar.Get(CalendarField.DayOfMonth),
                    (byte)(gregorianCalendar.Get(CalendarField.HourOfDay) & 255),
                    (byte)(gregorianCalendar.Get(CalendarField.Minute) & 255),
                    (byte)(gregorianCalendar.Get(CalendarField.Second) & 255) };
            bluetoothGattCharacteristic.SetValue(new byte[bArr.Length]);
            for (int i = 0; i < bArr.Length; i++) {
                bluetoothGattCharacteristic.SetValue(bArr);
            }
        }
        private bool WriteTimeSync_ex() {
            try {
                if (bluetoothGatt != null && customTimeCharacteristic != null) {
                    SetCustomData_MC(customTimeCharacteristic);
                    return bluetoothGatt.WriteCharacteristic(customTimeCharacteristic);
                }
            } catch (Exception e) {
                Log.Error("Error", e.Message);
            }
            return false;
        }

        private void RequestSequence() {
            handler.Post(() => {
                if (!GetSequenceNumber()) {
                    try {
                        System.Threading.Thread.Sleep(500);
                        GetSequenceNumber();
                    } catch (Exception e) {
                        Log.Error("error Sequence", e.Message);
                    }
                }
            });
        }
        private bool GetSequenceNumber() {
            try {
                if (bluetoothGatt != null && racpCharacteristic != null) {
                    SetOpCode(racpCharacteristic, 4, 1, new int[0]);
                    return bluetoothGatt.WriteCharacteristic(racpCharacteristic);
                }
            } catch (Exception e) {
                Log.Error("Error", e.Message);
            }
            return false;

        }
        private void SetOpCode(BluetoothGattCharacteristic bluetoothGattCharacteristic, int i, int i2, int[] numArr) {
            if (bluetoothGattCharacteristic != null) {
                bluetoothGattCharacteristic.SetValue(new byte[((numArr.Length > 0 ? 1 : 0) + 2 + (numArr.Length * 2))]);
                bluetoothGattCharacteristic.SetValue(i, GattFormat.Uint8, 0);
                bluetoothGattCharacteristic.SetValue(i2, GattFormat.Uint8, 1);
                if (numArr.Length > 0) {
                    bluetoothGattCharacteristic.SetValue(1, GattFormat.Uint8, 2);
                    int i3 = 3;
                    foreach (var intValue in numArr) {
                        bluetoothGattCharacteristic.SetValue(intValue, GattFormat.Uint16, i3);
                        i3 += 2;
                    }
                }
            }
        }

        private void EnableGlucoseContextNotification() {
            if (glucoseMeasurementContextCharacteristic != null) {
                bluetoothGatt.SetCharacteristicNotification(glucoseMeasurementContextCharacteristic, true);
                BluetoothGattDescriptor descriptor = glucoseMeasurementContextCharacteristic.GetDescriptor(BLEHelpers.BLE_DESCRIPTOR);
                descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                bluetoothGatt.WriteDescriptor(descriptor);
            }
        }
        private bool SetFlag() {

            if (bluetoothGatt == null || racpCharacteristic == null) {
                return false;
            }
            sbyte[] bArr = { -64, 2, -31, 1, 5, 1, 1, 1, 1, 1, 0 };
            racpCharacteristic.SetValue(new byte[bArr.Length]);
            for (int i2 = 0; i2 < bArr.Length; i2++) {
                racpCharacteristic.SetValue((byte[])(Array)bArr);
            }
            return bluetoothGatt.WriteCharacteristic(racpCharacteristic);
        }
        private bool SetCustomFlag() {
            if (bluetoothGatt == null || customTimeCharacteristic == null) {
                return false;
            }
            sbyte[] bArr = { -64, 2, -31, 1, 5, 1, 1, 1, 1, 1, 0 };
            try {
                customTimeCharacteristic.SetValue(new byte[bArr.Length]);
                for (int i2 = 0; i2 < bArr.Length; i2++) {
                    customTimeCharacteristic.SetValue((byte[])(Array)bArr);
                }
                return bluetoothGatt.WriteCharacteristic(customTimeCharacteristic);
            } catch (Exception e) {
                Log.Error("setCustomFlagError", e.Message);
            }
            return false;
        }

        private void EnableTimeSyncIndication() {
            if (customTimeCharacteristic != null) {
                bluetoothGatt.SetCharacteristicNotification(customTimeCharacteristic, true);
                BluetoothGattDescriptor descriptor = customTimeCharacteristic.GetDescriptor(BLEHelpers.BLE_DESCRIPTOR);
                descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                bluetoothGatt.WriteDescriptor(descriptor);
            }
        }

        private bool GetCustomTimeSync() {
            try {
                if (bluetoothGatt != null && customTimeCharacteristic != null) {
                    SetCustomTimeSync(customTimeCharacteristic, new GregorianCalendar());
                    return bluetoothGatt.WriteCharacteristic(customTimeCharacteristic);
                }
                return false;
            } catch (Exception e) {
                Log.Error("Error", e.Message);
                return false;
            }
        }


        private void SetCustomTimeSync(BluetoothGattCharacteristic bluetoothGattCharacteristic, Calendar calendar) {
            if (bluetoothGattCharacteristic != null) {
                timesyncUtcTzCnt++;
                sbyte[] bArr = { -64, 3, 1, 0,
                        (sbyte)(calendar.Get(CalendarField.Year) & 255),
                        (sbyte)((calendar.Get(CalendarField.Year) >> 8) & 255),
                        (sbyte)((calendar.Get(CalendarField.Month) + 1) & 255),
                        (sbyte)(calendar.Get(CalendarField.DayOfMonth) & 255),
                        (sbyte)(calendar.Get(CalendarField.HourOfDay) & 255),
                        (sbyte)(calendar.Get(CalendarField.Minute) & 255),
                        (sbyte)(calendar.Get(CalendarField.Second) & 255)};
                bluetoothGattCharacteristic.SetValue(new byte[bArr.Length]);
                for (int i = 0; i < bArr.Length; i++) {
                    bluetoothGattCharacteristic.SetValue((byte[])(Array)bArr);
                }
            }
        }
        private void RequestBleAll() {
            handler.Post(() => {
                if (!GetAllRecords()) {
                    try {
                        System.Threading.Thread.Sleep(500);
                        GetAllRecords();
                    } catch (Exception e) {
                        Log.Error("requestBleAll", e.Message);
                    }
                }
            });
        }
        private bool GetAllRecords() {
            try {
                if (bluetoothGatt != null && racpCharacteristic != null) {
                    SetOpCode(racpCharacteristic, 1, 1, new int[0]);
                    return bluetoothGatt.WriteCharacteristic(racpCharacteristic);
                }
            } catch (Exception e) {
                Log.Error("getAllRecords", e.Message);
            }
            return false;
        }
        private float BytesToFloat(byte b, byte b2) {
            return ((float)UnsignedByteToInt(b)) + ((UnsignedByteToInt(b2) & 15) << 8);
        }
        private int UnsignedByteToInt(byte b) {
            return b & 255;
        }
        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {
            if (!BLEHelpers.BLE_CHAR_CUSTOM_TIME_MC.Equals(characteristic.Uuid)) {
                if (BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI.Equals(characteristic.Uuid) || BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW.Equals(characteristic.Uuid)) {
                    int intValue = characteristic.GetIntValue(GattFormat.Uint8, 0).IntValue();
                    if (intValue == 5) {
                        if (!isIsensMeter || timesyncUtcTzCnt >= 3) {
                            RequestSequence();
                        } else {
                            GetCustomTimeSync();
                        }
                    } else if (intValue == 192 && characteristic.GetIntValue(GattFormat.Uint8, 1).IntValue() == 2) {
                        if (bluetoothGatt == null || racpCharacteristic == null) {
                            return;
                        }
                        GetCustomTimeSync();
                    }
                } else {
                    if (BLEHelpers.BLE_CHAR_GLUCOSE_MEASUREMENT.Equals(characteristic.Uuid)) {
                        if (characteristic.GetValue()[0] == 6 || characteristic.GetValue()[0] == 7) {
                            return;
                        }
                        int intValue2 = characteristic.GetIntValue(GattFormat.Uint8, 0).IntValue();
                        if (intValue2 != 5) {
                            bool checkIfTimeIsAvailable = (intValue2 & 1) > 0;
                            bool checkIfGlucoseDataIsAvailable = (intValue2 & 2) > 0;
                            bool checkIfValueIsHighOrLow = (intValue2 & 8) > 0;
                            GlucoseRecord glucoseRecord = new GlucoseRecord();
                            int year = characteristic.GetIntValue(GattFormat.Uint16, 3).IntValue();
                            int month = characteristic.GetIntValue(GattFormat.Uint8, 5).IntValue();
                            int date = characteristic.GetIntValue(GattFormat.Uint8, 6).IntValue();
                            int hourOfDay = characteristic.GetIntValue(GattFormat.Uint8, 7).IntValue();
                            int minutes = characteristic.GetIntValue(GattFormat.Uint8, 8).IntValue();
                            int seconds = characteristic.GetIntValue(GattFormat.Uint8, 9).IntValue();
                            DateTime dateTimeRecord = new DateTime(year, month, date, hourOfDay, minutes, seconds);
                            int offset = 10;
                            if (checkIfTimeIsAvailable) {
                                glucoseRecord.DateTimeRecord = dateTimeRecord;
                                offset = 12;
                            }
                            if (checkIfGlucoseDataIsAvailable) {
                                byte[] value = characteristic.GetValue();
                                glucoseRecord.glucoseData = BytesToFloat(value[offset], value[offset + 1]);
                                offset += 3;
                            }
                            if (checkIfValueIsHighOrLow) {
                                glucoseRecord.flag_hilow = characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue() == 64 ? -1 : 1;
                            }
                            try {
                                if (glucoseRecord.glucoseData >= 20) {
                                    records.Add(records.Count, glucoseRecord);
                                }
                            } catch (Exception e) {
                                Log.Error("Data insertion failed", e.Message);
                            }
                        }
                    } else if (BLEHelpers.BLE_CHAR_GLUCOSE_RACP.Equals(characteristic.Uuid)) {
                        int status = characteristic.GetIntValue(GattFormat.Uint8, 0).IntValue();
                        if (status == 5) {
                            if (bluetoothGatt == null || racpCharacteristic == null) {
                                return;
                            }
                            if (serial == null) {
                                Log.Error("Error", "serial is null");
                                return;
                            }
                            RequestBleAll();
                        } else if (status == 6) {
                            if (!isDownloadComplete) {
                                isDownloadComplete = true;
                                bluetoothGatt.WriteCharacteristic(characteristic);
                            }
                        }
                    }
                }
            }

        }
    }
}
