using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Animation;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Airbnb.Lottie.Model;
using Com.Airbnb.Lottie.Value;
using Familia;
using Familia.Devices;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using Java.IO;
using Java.Lang;
using Java.Text;
using Java.Util;
using Java.Util.Concurrent;
using Org.Json;
using SQLite;
using Environment = System.Environment;
using File = Java.IO.File;
using IOException = Java.IO.IOException;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.Devices.GlucoseDevice {
    [Activity(Label = "GlucoseDeviceActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class GlucoseDeviceActivity : AppCompatActivity, Animator.IAnimatorListener {
        private BluetoothAdapter _bluetoothAdapter;
        private BluetoothLeScanner _bluetoothScanner;
        private BluetoothManager _bluetoothManager;
        //private SQLiteAsyncConnection _db;
        private Handler _handler;
        private bool _send;

        private TextView _glucose;
        private Button _scanButton;

        // private ProgressDialog progressDialog;
        private TextView _lbStatus;
        private ConstraintLayout _dataContainer;
        Handler handler = new Handler();

        //private ProgressDialog progressDialog;
        private LottieAnimationView _animationView;
        private GlucoseDeviceActivity _context;
        private BluetoothScanCallback _scanCallback;
        private GattCallBack _gattCallback;
        private SqlHelper<DevicesRecords> _bleDevicesDataRecords;


        public static string ACTION_BLUETOOTH_GLUCOSE_METER_SERVICE_UPDATE
            = "com.eveningoutpost.dexdrip.BLUETOOTH_GLUCOSE_METER_SERVICE_UPDATE";
        public static string ACTION_BLUETOOTH_GLUCOSE_METER_NEW_SCAN_DEVICE
                = "com.eveningoutpost.dexdrip.BLUETOOTH_GLUCOSE_METER_NEW_SCAN_DEVICE";
        public static string BLUETOOTH_GLUCOSE_METER_TAG = "Bluetooth Glucose Meter";

        public static bool awaiting_ack = false;
        public static bool awaiting_data = false;
        

        private static BluetoothAdapter mBluetoothAdapter;
        public static string mBluetoothDeviceAddress;
        private static BluetoothGatt mBluetoothGatt;


        protected override async void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.blood_glucose_device);
            var toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Glicemie";

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate {
                Finish();
            };

            //            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            //            const string numeDb = "devices_data.db";
            //             _db = new SQLiteAsyncConnection(Path.Combine(path, numeDb));

            _bleDevicesDataRecords = await SqlHelper<DevicesRecords>.CreateAsync();

            _lbStatus = FindViewById<TextView>(Resource.Id.status);
            _dataContainer = FindViewById<ConstraintLayout>(Resource.Id.dataContainer);
            _bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);
            _context = this;

            _scanCallback = new BluetoothScanCallback(_context);
            _gattCallback = new GattCallBack(_context);

            _dataContainer.Visibility = ViewStates.Gone;

            _glucose = FindViewById<TextView>(Resource.Id.GlucoseTextView);
            _scanButton = FindViewById<Button>(Resource.Id.ScanButton);

            _animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            _animationView.AddAnimatorListener(this);
            var filter =
                new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.accent));
            _animationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter,
                new LottieValueCallback(filter));
            _animationView.PlayAnimation();


            _scanButton.Click += delegate {
                if (_bluetoothManager == null) return;
                _bluetoothScanner.StartScan(_scanCallback);
                _scanButton.Enabled = false;
                _send = false;

                _lbStatus.Text = "Se efectueaza masuratoarea...";
                _animationView.PlayAnimation();
                //                        progressDialog.setMessage(getstring(R.string.conectare_info));
                //                        progressDialog.show();
            };

            // Create your application here
        }

        public void OnAnimationCancel(Animator animation) {
            _dataContainer.Visibility = ViewStates.Visible;
            _lbStatus.Text = "Masuratoare efecuata cu succes";
            _animationView.Progress = 1f;
        }

        public void OnAnimationEnd(Animator animation) {
            //throw new NotImplementedException();
        }

        public void OnAnimationRepeat(Animator animation) {
            //throw new NotImplementedException();
        }

        public void OnAnimationStart(Animator animation) {
            _handler = new Handler();
            _lbStatus.Text = "Se efectueaza masuratoarea...";
            //            if (_bluetoothManager == null) return;
            _bluetoothAdapter = _bluetoothManager?.Adapter;
            if (_bluetoothAdapter == null) return;
            if (!_bluetoothAdapter.IsEnabled) {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
            } else {
                _bluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
                _bluetoothScanner.StartScan(_scanCallback);
                _scanButton.Enabled = false;
                _dataContainer.Visibility = ViewStates.Gone;
                //progressDialog.show();
            }

        }
        protected override void OnPostResume() {
            base.OnPostResume();
            _send = false;
        }

        protected override void OnPause() {
            base.OnPause();
            if (_bluetoothAdapter != null) {
                _bluetoothScanner?.StopScan(_scanCallback);
            }

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode != 11) return;
            if (resultCode == Result.Ok) {
                _bluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
                _bluetoothScanner.StartScan(_scanCallback);
                _scanButton.Enabled = false;
                _animationView.PlayAnimation();

            } else {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
            }
        }

        class BluetoothScanCallback : ScanCallback {
            private readonly Context Context;
            private SqlHelper<BluetoothDeviceRecords> _bleDevicesRecords;
            public BluetoothScanCallback(Context context) {
                Context = context;
            }

            public override async void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result) {
                base.OnScanResult(callbackType, result);
                //var data = Utils.GetDefaults(Context.Getstring(Resource.string.blood_glucose_device));
                //var jsonDevice = new JSONObject(data);

                _bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                IEnumerable<BluetoothDeviceRecords> list = await _bleDevicesRecords.QueryValuations("select * from BluetoothDeviceRecords");

                if (!(from c in list
                      where c.Address == result.Device.Address
                      select new { c.Name, c.Address, c.DeviceType }).Any())
                    return;

                //if (result.Device.Address == null ||
                //    !result.Device.Address.Equals(
                //       jsonDevice.Getstring("Address"))) return;
                result.Device.ConnectGatt(Context, true,
                    ((GlucoseDeviceActivity)Context)._gattCallback, BluetoothTransports.Le);
                ((GlucoseDeviceActivity)Context)._bluetoothScanner.StopScan(
                    ((GlucoseDeviceActivity)Context)._scanCallback);
                ((GlucoseDeviceActivity)Context)._lbStatus.Text =
                    Context.GetString(Resource.String.conectare_info);
            }
        }

        private class GattCallBack : BluetoothGattCallback {
            private readonly Context Context;
            private Handler _handler = new Handler();
            private int mTimesyncUtcTzCnt;
            public GattCallBack(Context context) {
                Context = context;
            }

            public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState) {
                base.OnConnectionStateChange(gatt, status, newState);
                switch (newState) {
                    case ProfileState.Connected:
                        Log.Error("GattState", "Connected");
                        gatt.DiscoverServices();
                        break;
                    case ProfileState.Connecting:
                        Log.Error("GattState", "Connecting");

                        break;
                    case ProfileState.Disconnected:
                        Log.Error("GattState", "Disconnected");
                        break;
                    case ProfileState.Disconnecting:
                        Log.Error("GattState", "Disconnecting");
                        break;
                }
            }
            BluetoothGattCharacteristic mGlucoseMeasurementContextCharacteristic;
            BluetoothGattCharacteristic mCustomTimeCharacteristic;
            BluetoothGattCharacteristic mGlucoseMeasurementCharacteristic;
            BluetoothGattCharacteristic mRACPCharacteristic;
            public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status) {
                Log.Error("Discover Servies", "NOW");
                if (status == GattStatus.Success) {
                    foreach (BluetoothGattService service in gatt.Services) {
                        Log.Error("Service dicovered", service.Uuid.ToString());

                        //if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_GLUCOSE)) {
                        //    //_type = "gl";
                        //    Log.Error("Dicovered", "BLE_SERVICE_GLUCOSE");
                        //    mGlucoseMeasurementCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_MEASUREMENT);
                        //    mGlucoseMeasurementContextCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_CONTEXT);
                        //    mRACPCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_GLUCOSE_RACP);
                        //    clear();
                        //} else if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_CUSTOM_TIME_MC)) {
                        //    Log.Error("Dicovered", "BLE_SERVICE_CUSTOM_TIME_MC");
                        //    mCustomTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_MC);
                        //    if (mCustomTimeCharacteristic != null) {
                        //        gatt.SetCharacteristicNotification(mCustomTimeCharacteristic, true);
                        //    }
                        //} else if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_CUSTOM_TIME_TI)) {
                        //    Log.Error("Dicovered", "BLE_SERVICE_CUSTOM_TIME_TI");
                        //    mCustomTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI);
                        //    if (mCustomTimeCharacteristic != null) {
                        //        gatt.SetCharacteristicNotification(mCustomTimeCharacteristic, true);
                        //    }
                        //} else if (service.Uuid.Equals(BLEHelpers.BLE_SERVICE_CUSTOM_TIME_TI_NEW)) {
                        //    Log.Error("Dicovered", "BLE_SERVICE_CUSTOM_TIME_TI_NEW");
                        //    mCustomTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW);
                        //    if (mCustomTimeCharacteristic != null) {
                        //        gatt.SetCharacteristicNotification(mCustomTimeCharacteristic, true);
                        //    }
                        //} 
                        switch (service.Uuid.ToString()) {
                            case "00001808-0000-1000-8000-00805f9b34fb":
                                Log.Error("Dicovered", "Glucose Service");
                                mGlucoseMeasurementContextCharacteristic = service.GetCharacteristic(UUID.FromString("00002a34-0000-1000-8000-00805f9b34fb")); // glucose context
                                mGlucoseMeasurementCharacteristic = service.GetCharacteristic(UUID.FromString("00002a18-0000-1000-8000-00805f9b34fb"));
                                mRACPCharacteristic = service.GetCharacteristic(UUID.FromString("00002a52-0000-1000-8000-00805f9b34fb"));
                                foreach (var characteristic in service.Characteristics) {
                                    switch (characteristic.Uuid.ToString()) {
                                        case "00002a18-0000-1000-8000-00805f9b34fb": //glucose content
                                            Log.Error("Glucose content characteristic", characteristic.Uuid.ToString());
                                            BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
                                            descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                                            gatt.WriteDescriptor(descriptor);
                                            break;
                                    }
                                }
                                break;
                            case "0000fff0-0000-1000-8000-00805f9b34fb": // ble service Custom Time
                                Log.Error("Dicovered", "ble service Custom Time");
                                mCustomTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI); // ble char service Custom Time

                                if (mCustomTimeCharacteristic != null) {
                                    gatt.SetCharacteristicNotification(mCustomTimeCharacteristic, true);
                                }
                                break;
                            case "11223344-5566-7788-9900-AABBCCDDEEFF": // ble service Custom Time
                                Log.Error("Dicovered", "ble service Custom Time");
                                mCustomTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI); // ble char service Custom Time

                                if (mCustomTimeCharacteristic != null) {
                                    gatt.SetCharacteristicNotification(mCustomTimeCharacteristic, true);
                                }
                                break;
                            case "c4dea010-5a9d-11e9-8647-d663bd873d93": // ble service Custom Time New
                                Log.Error("Dicovered", "ble service Custom Time TI NEW");
                                mCustomTimeCharacteristic = service.GetCharacteristic(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW);
                                if (mCustomTimeCharacteristic != null) {
                                    gatt.SetCharacteristicNotification(mCustomTimeCharacteristic, true);
                                }
                                break;
                        }
                    }
                }
            }
            public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status) {
                switch (status) {
                    case GattStatus.Success:
                        Log.Error("OnCharacteristicWrite", "Success");
                        break;
                    case GattStatus.Failure:
                        Log.Error("OnCharacteristicWrite", "Failure");
                        break;
                    case GattStatus.ReadNotPermitted:
                        Log.Error("OnCharacteristicWrite", "ReadNotPermitted");
                        break;
                    case GattStatus.InsufficientAuthentication:
                        Log.Error("OnCharacteristicWrite", "InsufficientAuthentication");
                        break;
                    case GattStatus.ConnectionCongested:
                        Log.Error("OnCharacteristicWrite", "ConnectionCongested");
                        break;
                    case GattStatus.InsufficientEncryption:
                        Log.Error("OnCharacteristicWrite", "InsufficientEncryption");
                        break;
                    case GattStatus.InvalidAttributeLength:
                        Log.Error("OnCharacteristicWrite", "InvalidAttributeLength");
                        break;
                    case GattStatus.InvalidOffset:
                        Log.Error("OnCharacteristicWrite", "InvalidOffset");
                        break;
                    case GattStatus.WriteNotPermitted:
                        Log.Error("OnCharacteristicWrite", "WriteNotPermitted");
                        break;
                    case GattStatus.RequestNotSupported:
                        Log.Error("OnCharacteristicWrite", "RequestNotSupported");
                        break;
                    default:
                        Log.Error("OnCharacteristicWrite", "Unrecognized Status");
                        break;
                }
            }
            public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status) {
                switch (status) {
                    case GattStatus.Success:
                        Log.Error("OnCharacteristicRead", "Success");
                        break;
                    case GattStatus.Failure:
                        Log.Error("OnCharacteristicRead", "Failure");
                        break;
                    case GattStatus.ReadNotPermitted:
                        Log.Error("OnCharacteristicRead", "ReadNotPermitted");
                        break;
                    case GattStatus.InsufficientAuthentication:
                        Log.Error("OnCharacteristicRead", "InsufficientAuthentication");
                        break;
                    case GattStatus.ConnectionCongested:
                        Log.Error("OnCharacteristicRead", "ConnectionCongested");
                        break;
                    case GattStatus.InsufficientEncryption:
                        Log.Error("OnCharacteristicRead", "InsufficientEncryption");
                        break;
                    case GattStatus.InvalidAttributeLength:
                        Log.Error("OnCharacteristicRead", "InvalidAttributeLength");
                        break;
                    case GattStatus.InvalidOffset:
                        Log.Error("OnCharacteristicRead", "InvalidOffset");
                        break;
                    case GattStatus.WriteNotPermitted:
                        Log.Error("OnCharacteristicRead", "WriteNotPermitted");
                        break;
                    case GattStatus.RequestNotSupported:
                        Log.Error("OnCharacteristicRead", "RequestNotSupported");
                        break;
                    default:
                        Log.Error("OnCharacteristicRead", "Unrecognized Status");
                        break;
                }
            }
            public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status) {
                if (status == GattStatus.Success) {
                    if (BLEHelpers.BLE_CHAR_GLUCOSE_MEASUREMENT.Equals(descriptor.Characteristic.Uuid)) {
                        Log.Error("DescriptorWrite", " measurement begin");
                        enableGlucoseContextNotification(gatt);
                        Log.Error("DescriptorWrite", " measurement end");
                    } else if (BLEHelpers.BLE_CHAR_GLUCOSE_CONTEXT.Equals(descriptor.Characteristic.Uuid)) {
                        //if (false) {
                        //    Log.Error("DescriptorWrite", " context begin");
                        //    requestSequence(gatt);
                        //    broadcastUpdate(BLEHelpers.INTENT_BLE_REQUEST_COUNT, "");
                        //    Log.Error("DescriptorWrite", " context end");
                        //} else {
                        Log.Error("DescriptorWrite", " context begin");
                        if (mCustomTimeCharacteristic == null || !mCustomTimeCharacteristic.Uuid.Equals(BLEHelpers.BLE_CHAR_CUSTOM_TIME_MC)) {
                            Log.Error("DescriptorWrite", " are custom time");
                            if (mCustomTimeCharacteristic == null || (!mCustomTimeCharacteristic.Uuid.Equals(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI) && !mCustomTimeCharacteristic.Uuid.Equals(BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW))) {
                                _handler.PostDelayed(() => {
                                    if (!setFlag(0 + 1, true, true, true, gatt)) {
                                        try {
                                            System.Threading.Thread.Sleep(500);
                                            setFlag(0 + 1, true, true, true, gatt);
                                        } catch (InterruptedException e) {
                                            Log.Error("error", e.Message);
                                        }
                                    }
                                }, 300);

                            } else {
                                enableTimeSyncIndication(gatt);
                            }
                        } else {
                            Log.Error("DescriptorWrite", " nu are customTime");
                            try {
                                System.Threading.Thread.Sleep(500);
                                writeTimeSync_ex(gatt);
                                System.Threading.Thread.Sleep(500);
                                requestSequence(gatt);
                            } catch (InterruptedException e) {
                                //e.printStackTrace();
                            }
                            broadcastUpdate(BLEHelpers.INTENT_BLE_RACPINDICATIONENABLED, "MC");
                        }

                    } else if (BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI.Equals(descriptor.Characteristic.Uuid) || BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW.Equals(descriptor.Characteristic.Uuid)) {
                        Log.Error("DescriptorWrite", " custom time begin");
                        _handler.PostDelayed(() => {
                            if (!setCustomFlag(0 + 1, true, true, true, gatt)) {
                                try {
                                    System.Threading.Thread.Sleep(500);
                                    setCustomFlag(0 + 1, true, true, true, gatt);
                                } catch (InterruptedException e) {
                                    Log.Error("error", e.Message);
                                }
                            }
                        }, 500);
                        Log.Error("DescriptorWrite", " custom time end");


                    } else {
                        Log.Error("desc else", descriptor.Characteristic.Uuid.ToString());
                    }
                }
            }
            private void setCustomData_MC(BluetoothGattCharacteristic bluetoothGattCharacteristic) {
                GregorianCalendar gregorianCalendar = new GregorianCalendar();
                byte b = (byte)((gregorianCalendar.Get(CalendarField.Year) % 100) & 255);
                byte[] bArr = { 1, 0, b, (byte)(((gregorianCalendar.Get(CalendarField.Year) - b) / 100) & 255), (byte)((gregorianCalendar.Get(CalendarField.Month) + 1) & 255), (byte)gregorianCalendar.Get((Java.Util.CalendarField)5), (byte)(gregorianCalendar.Get((Java.Util.CalendarField)11) & 255), (byte)(gregorianCalendar.Get((Java.Util.CalendarField)12) & 255), (byte)(gregorianCalendar.Get((Java.Util.CalendarField)13) & 255) };
                bluetoothGattCharacteristic.SetValue(new byte[bArr.Length]);
                for (int i = 0; i < bArr.Length; i++) {
                    bluetoothGattCharacteristic.SetValue(bArr);
                }
            }
            public bool writeTimeSync_ex(BluetoothGatt mBluetoothGatt) {
                bool z = false;
                try {
                    if (mBluetoothGatt != null) {
                        if (this.mCustomTimeCharacteristic != null) {
                            setCustomData_MC(this.mCustomTimeCharacteristic);
                            z = mBluetoothGatt.WriteCharacteristic(this.mCustomTimeCharacteristic);
                            return z;
                        }
                    }
                    return false;
                } catch (System.Exception e) {
                    Log.Error("Error", e.Message);
                    return false;
                }
            }

            private void requestSequence(BluetoothGatt mBluetoothGatt) {

                _handler.PostDelayed(() => {
                    if (!GetSequenceNumber(mBluetoothGatt)) {
                        try {
                            System.Threading.Thread.Sleep(500);
                            GetSequenceNumber(mBluetoothGatt);
                        } catch (InterruptedException e) {
                            Log.Error("error Sequence", e.Message);
                        }
                    }

                }, 300);

            }
            private bool GetSequenceNumber(BluetoothGatt mBluetoothGatt) {
                bool z = false;
                try {
                    if (mBluetoothGatt != null) {
                        if (this.mRACPCharacteristic != null) {
                            clear();
                            broadcastUpdate(BLEHelpers.INTENT_BLE_OPERATESTARTED, "");
                            setOpCode(this.mRACPCharacteristic, 4, 1, new Integer[0]);
                            z = mBluetoothGatt.WriteCharacteristic(this.mRACPCharacteristic);
                            return z;
                        }
                    }
                    string str = BLEHelpers.INTENT_BLE_ERROR;
                    return false;
                } catch (System.Exception e) {
                    Log.Error("Error", e.Message);
                    return false;
                }
            }
            private void setOpCode(BluetoothGattCharacteristic bluetoothGattCharacteristic, int i, int i2, Integer[] numArr) {
                if (bluetoothGattCharacteristic != null) {
                    bluetoothGattCharacteristic.SetValue(new byte[((numArr.Length > 0 ? 1 : 0) + 2 + (numArr.Length * 2))]);
                    bluetoothGattCharacteristic.SetValue(i, GattFormat.Uint8, 0);
                    bluetoothGattCharacteristic.SetValue(i2, GattFormat.Uint8, 1);
                    if (numArr.Length > 0) {
                        bluetoothGattCharacteristic.SetValue(1, GattFormat.Uint8, 2);
                        int i3 = 3;
                        foreach (var intValue in numArr) {
                            bluetoothGattCharacteristic.SetValue(intValue.IntValue(), GattFormat.Uint16, i3);
                            i3 += 2;

                        }
                    }
                }
            }
            private void clear() {
                broadcastUpdate(BLEHelpers.INTENT_BLE_DATASETCHANGED, "");
            }

            public void enableGlucoseContextNotification(BluetoothGatt bluetoothGatt) {
                BluetoothGattCharacteristic bluetoothGattCharacteristic = this.mGlucoseMeasurementContextCharacteristic;
                if (bluetoothGattCharacteristic != null) {
                    bluetoothGatt.SetCharacteristicNotification(bluetoothGattCharacteristic, true);
                    BluetoothGattDescriptor descriptor = this.mGlucoseMeasurementContextCharacteristic.GetDescriptor(BLEHelpers.BLE_DESCRIPTOR);
                    descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                    bluetoothGatt.WriteDescriptor(descriptor);
                }
            }
            //public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status) {
            //    base.OnDescriptorWrite(gatt, descriptor, status);
            //    Log.Error("OnDescriptorWrite", descriptor.Characteristic.Uuid.Tostring());
            //    if (descriptor.Characteristic.Uuid.Tostring() == "00002a18-0000-1000-8000-00805f9b34fb") {
            //        switch (status) {
            //            case GattStatus.Success:
            //                Log.Error("OnDescriptorWrite", "Success");
            //                //gatt.SetCharacteristicNotification(mGlucoseMeasurementContextCharacteristic, true);
            //                //BluetoothGattDescriptor desc = mGlucoseMeasurementContextCharacteristic.GetDescriptor(UUID.Fromstring("00002902-0000-1000-8000-00805f9b34fb"));// BLE descriptor
            //                //desc.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
            //                //gatt.WriteDescriptor(desc);

            //                BluetoothGattCharacteristic bluetoothGattCharacteristic = mGlucoseMeasurementCharacteristic;
            //                if (bluetoothGattCharacteristic != null) {
            //                    gatt.SetCharacteristicNotification(bluetoothGattCharacteristic, true);
            //                    BluetoothGattDescriptor desc = mGlucoseMeasurementCharacteristic.GetDescriptor(UUID.Fromstring("00002902-0000-1000-8000-00805f9b34fb"));
            //                    descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
            //                    gatt.WriteDescriptor(descriptor);
            //                }
            //                break;
            //            case GattStatus.Failure:
            //                Log.Error("OnDescriptorWrite", "Failure");
            //                break;
            //            case GattStatus.ReadNotPermitted:
            //                Log.Error("OnDescriptorWrite", "ReadNotPermitted");
            //                break;
            //            case GattStatus.InsufficientAuthentication:
            //                Log.Error("OnDescriptorWrite", "InsufficientAuthentication");
            //                break;
            //            case GattStatus.ConnectionCongested:
            //                Log.Error("OnDescriptorWrite", "ConnectionCongested");
            //                break;
            //            case GattStatus.InsufficientEncryption:
            //                Log.Error("OnDescriptorWrite", "InsufficientEncryption");
            //                break;
            //            case GattStatus.InvalidAttributeLength:
            //                Log.Error("OnDescriptorWrite", "InvalidAttributeLength");
            //                break;
            //            case GattStatus.InvalidOffset:
            //                Log.Error("OnDescriptorWrite", "InvalidOffset");
            //                break;
            //            case GattStatus.WriteNotPermitted:
            //                Log.Error("OnDescriptorWrite", "WriteNotPermitted");
            //                break;
            //            case GattStatus.RequestNotSupported:
            //                Log.Error("OnDescriptorWrite", "RequestNotSupported");
            //                break;
            //            default:
            //                Log.Error("OnDescriptorWrite", "Unrecognized Status: " + status);

            //                break;
            //        }
            //    } else if (descriptor.Characteristic.Uuid.Tostring() == "00002a34-0000-1000-8000-00805f9b34fb") {
            //        if (mCustomTimeCharacteristic == null || !mCustomTimeCharacteristic.Uuid.Equals(UUID.Fromstring("01020304-0506-0708-0900-0a0b0c0d0e0f"))) {
            //            if (mCustomTimeCharacteristic == null || (!mCustomTimeCharacteristic.Uuid.Equals(UUID.Fromstring("0000fff1-0000-1000-8000-00805f9b34fb")) && !mCustomTimeCharacteristic.Uuid.Equals(UUID.Fromstring("c4dea3bc-5a9d-11e9-8647-d663bd873d93")))) {

            //                (Context as GlucoseDeviceActivity).handler.PostDelayed(() => {
            //                    if (!setFlag(0 + 1, true, true, true, gatt)) {
            //                        try {
            //                            System.Threading.Thread.Sleep(500);
            //                            setFlag(0 + 1, true, true, true, gatt);
            //                        } catch (InterruptedException e) {
            //                            e.PrintStackTrace();
            //                        }
            //                    }
            //                }, 300);
            //            } else {
            //                enableTimeSyncIndication(gatt);
            //            }
            //        }
            //        //else if (Serial != null) {
            //        //    try {
            //        //        System.Threading.Thread.Sleep(500);
            //        //        //writeTimeSync_ex();
            //        //        System.Threading.Thread.Sleep(500);
            //        //        //requestSequence();
            //        //    } catch (InterruptedException e) {
            //        //        e.PrintStackTrace();
            //        //    }
            //        //    //BroadcastUpdate(Const.INTENT_BLE_RACPINDICATIONENABLED, "MC");
            //        //} else {
            //        //    return;
            //        //}
            //    } else if (descriptor.Characteristic.Uuid.Tostring() == "0000fff1-0000-1000-8000-00805f9b34fb" || descriptor.Characteristic.Uuid.Tostring() == "c4dea3bc-5a9d-11e9-8647-d663bd873d93") {
            //        try {
            //            System.Threading.Thread.Sleep(500);
            //        } catch (InterruptedException e3) {
            //            e3.PrintStackTrace();
            //        }

            //        (Context as GlucoseDeviceActivity).handler.PostDelayed(() => {
            //            if (!setCustomFlag(0 + 1, true, true, true, gatt)) {
            //                try {
            //                    System.Threading.Thread.Sleep(500);
            //                    setCustomFlag(0 + 1, true, true, true, gatt);
            //                } catch (InterruptedException e) {
            //                    e.PrintStackTrace();
            //                }
            //            }
            //        }, 300);
            //    }
            //}
            public bool setFlag(int i, bool z, bool z2, bool z3, BluetoothGatt gatt) {

                if (gatt == null || this.mRACPCharacteristic == null) {
                    Log.Error("setFlag", "null");
                    return false;
                }
                // clear();
                // broadcastUpdate(Const.INTENT_BLE_OPERATESTARTED, "");
                sbyte[] bArr = { -64, 2, -31, 1, 5, (sbyte)i, 1, (sbyte)(z ? 1 : 0), (sbyte)(z2 ? 1 : 0), (sbyte)(z3 ? 1 : 0), 0 };
                mRACPCharacteristic.SetValue(new byte[bArr.Length]);
                for (int i2 = 0; i2 < bArr.Length; i2++) {
                    mRACPCharacteristic.SetValue((byte[])(Array)bArr);
                }
                return gatt.WriteCharacteristic(this.mRACPCharacteristic);
            }
            public bool setCustomFlag(int i, bool z, bool z2, bool z3, BluetoothGatt gatt) {
                Log.Error("GlucoseManager.setCustomFlag()", "NOW");
                bool z4 = false;
                if (gatt == null || this.mCustomTimeCharacteristic == null) {
                    return false;
                }
                sbyte[] bArr = { -64, 2, -31, 1, 5, (sbyte)i, 1, (sbyte)(z ? 1 : 0), (sbyte)(z2 ? 1 : 0), (sbyte)(z3 ? 1 : 0), 0 };
                try {
                    this.mCustomTimeCharacteristic.SetValue(new byte[bArr.Length]);
                    for (int i2 = 0; i2 < bArr.Length; i2++) {
                        this.mCustomTimeCharacteristic.SetValue((byte[])(Array)bArr);
                    }
                    z4 = gatt.WriteCharacteristic(this.mCustomTimeCharacteristic);
                } catch (System.Exception e) {
                    Log.Error("setCustomFlagError", e.Message);
                }
                return z4;
            }
            public void broadcastUpdate(string str, string str2) {
                Intent intent = new Intent(str);
                if (str2 != "") {
                    intent.PutExtra("air.SmartLog.android.ble.BLE_EXTRA_DATA", str2);
                }
                Context.SendBroadcast(intent);
            }

            public void enableTimeSyncIndication(BluetoothGatt bluetoothGatt) {
                BluetoothGattCharacteristic bluetoothGattCharacteristic = this.mCustomTimeCharacteristic;
                if (bluetoothGattCharacteristic != null) {
                    bluetoothGatt.SetCharacteristicNotification(bluetoothGattCharacteristic, true);
                    BluetoothGattDescriptor descriptor = this.mCustomTimeCharacteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb")); // BLE descriptor
                    descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                    bluetoothGatt.WriteDescriptor(descriptor);
                }
            }
            public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status) {
                base.OnDescriptorRead(gatt, descriptor, status);
            }
            bool isUtc = false;
            public bool getCustomTimeSync(BluetoothGatt mBluetoothGatt) {
                try {
                    if (mBluetoothGatt != null) {
                        if (this.mCustomTimeCharacteristic != null) {
                            clear();
                            broadcastUpdate(BLEHelpers.INTENT_BLE_OPERATESTARTED, "");
                            //setCustomTimeSync(this.mCustomTimeCharacteristic, new GregorianCalendar());
                            if (!isUtc) {
                                switch (this.mTimesyncUtcTzCnt) {
                                    case 0:
                                       
                                        Calendar instance = Calendar.Instance;
                                        instance.Set(2030, 0, 1, 0, 0, 0);
                                        setCustomTimeSync(this.mCustomTimeCharacteristic, instance);
                                        break;
                                    case 1:
                                        Calendar i = Calendar.Instance;
                                        i.TimeZone = Java.Util.TimeZone.GetTimeZone("UTC");
                                        setCustomTimeSync(this.mCustomTimeCharacteristic, i);
                                        isUtc = true;
                                        break;
                                    case 2:
                                        setCustomTimeSync(this.mCustomTimeCharacteristic, new GregorianCalendar());
                                        break;
                                }
                            } else {
                                setCustomTimeSync(this.mCustomTimeCharacteristic, new GregorianCalendar());
                            }
                            return mBluetoothGatt.WriteCharacteristic(this.mCustomTimeCharacteristic);
                        }
                    }
                    return false;
                } catch (System.Exception e) {
                    Log.Error("Error", e.Message);
                    return false;
                }
            }
            private void setCustomTimeSync(BluetoothGattCharacteristic bluetoothGattCharacteristic, Calendar calendar) {
                if (bluetoothGattCharacteristic != null) {
                    mTimesyncUtcTzCnt++;
                    sbyte[] bArr = { -64, 3, 1, 0, (sbyte)(calendar.Get((CalendarField)1) & 255),
                        (sbyte)((calendar.Get((CalendarField)1) >> 8) & 255),
                        (sbyte)((calendar.Get((CalendarField)2) + 1) & 255),
                        (sbyte)(calendar.Get((CalendarField)5) & 255),
                        (sbyte)(calendar.Get((CalendarField)11) & 255),
                        (sbyte)(calendar.Get((CalendarField)12) & 255),
                        (sbyte)(calendar.Get((CalendarField)13) & 255) };
                    bluetoothGattCharacteristic.SetValue(new byte[bArr.Length]);
                    for (int i = 0; i < bArr.Length; i++) {
                        bluetoothGattCharacteristic.SetValue((byte[])(Array)bArr);
                    }
                }
            }
            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {
                Log.Error("OnCharacteristicChanged", characteristic.Uuid.ToString());
                Log.Error("OnCharacteristicChanged Data", characteristic.GetValue().ToString());

                
                bool z;
                int i;
                bool z2;
                bool z3;
                int i2;
                BluetoothGattCharacteristic bluetoothGattCharacteristic2 = characteristic;
                UUID uuid = characteristic.Uuid;
                if (!BLEHelpers.BLE_CHAR_CUSTOM_TIME_MC.Equals(uuid)) {
                   
                    int i3 = 3;
                    if (BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI.Equals(uuid) || BLEHelpers.BLE_CHAR_CUSTOM_TIME_TI_NEW.Equals(uuid)) {
                        Log.Error("intra", " aici");
                        int intValue = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 0).IntValue();
                        if (intValue == 5) {
                            getCustomTimeSync(gatt);
                        } else if (intValue == 192 && bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 1).IntValue() == 2) {
                            if (gatt == null || characteristic == null) {
                                return;
                            }
                            clear();
                            getCustomTimeSync(gatt);
                        }
                    } else {
                        int i4 = 1000;
                        if (BLEHelpers.BLE_CHAR_GLUCOSE_MEASUREMENT.Equals(uuid)) {
                            if (characteristic.GetValue()[0] == 6 || characteristic.GetValue()[0] == 7) {
                                broadcastUpdate(BLEHelpers.INTENT_BLE_READCOMPLETED, "");
                                //var m_data_list = convertData(mRecords);
                                //completeDownloading();
                                return;
                            }
                            int intValue2 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 0).IntValue();
                            if (intValue2 != 5) {
                                bool z4 = (intValue2 & 1) > 0;
                                bool z5 = (intValue2 & 2) > 0;
                                bool z6 = (intValue2 & 8) > 0;
                                bool z7 = (intValue2 & 16) > 0;
                                int intValue3 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint16, 1).IntValue();
                                //GlucoseRecord glucoseRecord = (GlucoseRecord)GlucoseBleService.mRecords.get(intValue3);
                                //if (glucoseRecord == null) {
                                //    glucoseRecord = new GlucoseRecord();
                                //    z3 = false;
                                //} else {
                                //    z3 = true;
                                //}
                                //glucoseRecord.sequenceNumber = intValue3;
                                //glucoseRecord.flag_context = 0;
                                int intValue4 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint16, 3).IntValue();
                                int intValue5 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 5).IntValue();
                                int intValue6 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 6).IntValue();
                                int intValue7 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 7).IntValue();
                                int intValue8 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 8).IntValue();
                                int intValue9 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 9).IntValue();
                                int i5 = 10;
                                Calendar instance = Calendar.Instance;
                                instance.Set(intValue4, intValue5 - 1, intValue6, intValue7, intValue8, intValue9);
                                //glucoseRecord.time = instance.TimeInMillis / 1000;
                                //if (z4) {
                                //    glucoseRecord.timeoffset = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Sint16, 10).IntValue();
                                //    glucoseRecord.time += (long)(glucoseRecord.timeoffset * 60);
                                //    i5 = 12;
                                //}
                                //if (z5) {
                                //    byte[] value = characteristic.GetValue();
                                //    glucoseRecord.glucoseData = bytesToFloat(value[i5], value[i5 + 1]);
                                //    glucoseRecord.flag_cs = ((bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, i5 + 2).IntValue() & 240) >> 4) == 10 ? 1 : 0;
                                //    i5 += 3;
                                //}

                                //if (z6) {
                                //    int intValue10 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint16, i5).IntValue();
                                //    if (intValue10 == 64) {
                                //        glucoseRecord.flag_hilow = -1;
                                //    }
                                //    if (intValue10 == 32) {
                                //        i2 = 1;
                                //        glucoseRecord.flag_hilow = 1;
                                //    } else {
                                //        i2 = 1;
                                //    }
                                //} else {
                                //    i2 = 1;
                                //}
                                //if (!z7) {
                                //    glucoseRecord.flag_context = i2;
                                //}
                                //if (!z3) {
                                //    try {
                                //        GlucoseBleService.mRecords.put(glucoseRecord.sequenceNumber, glucoseRecord);
                                //    } catch (System.Exception unused) {
                                //    }
                                //}
                                if (!z7) {
                                    broadcastUpdate(BLEHelpers.INTENT_BLE_DATASETCHANGED, "");
                                }
                            }
                        } else if (BLEHelpers.BLE_CHAR_GLUCOSE_CONTEXT.Equals(uuid)) {
                            int intValue12 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 0).IntValue();
                            bool z8 = (intValue12 & 1) > 0;
                            int i6 = (intValue12 & 2) > 0 ? 1 : 0;
                            if ((intValue12 & 128) > 0) {
                                i = 1;
                                z = true;
                            } else {
                                i = 1;
                                z = false;
                            }
                            int intValue13 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint16, i).IntValue();
                            if (z) {
                                i3 = 4;
                            }
                            if (z8) {
                                i3 += 3;
                            }
                            int intValue14 = i6 == i ? bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, i3).IntValue() : -1;
                            //GlucoseRecord glucoseRecord2 = GlucoseBleService.mRecords.get(intValue13);
                            //if (glucoseRecord2 == null) {
                            //    glucoseRecord2 = new GlucoseRecord();
                            //    z2 = false;
                            //} else {
                            //    z2 = true;
                            //}
                            //if (glucoseRecord2 != null && i6 != 0) {
                            //    glucoseRecord2.flag_context = 1;
                            //    if (intValue14 != 6) {
                            //        switch (intValue14) {
                            //            case 0:
                            //                if (glucoseRecord2.flag_cs == 0) {
                            //                    glucoseRecord2.flag_nomark = 1;
                            //                    break;
                            //                }
                            //                break;
                            //            case 1:
                            //                glucoseRecord2.flag_meal = -1;
                            //                break;
                            //            case 2:
                            //                glucoseRecord2.flag_meal = 1;
                            //                break;
                            //            case 3:
                            //                glucoseRecord2.flag_fasting = 1;
                            //                break;
                            //        }
                            //    } else {
                            //        glucoseRecord2.flag_ketone = 1;
                            //    }

                            //    if (!z2) {
                            //        try {
                            //            GlucoseBleService.mRecords.put(glucoseRecord2.sequenceNumber, glucoseRecord2);
                            //        } catch (System.Exception ex) {
                            //        }
                            //    }
                            //    broadcastUpdate(BLEHelpers.INTENT_BLE_DATASETCHANGED, "");
                            //}
                        } else if (BLEHelpers.BLE_CHAR_GLUCOSE_RACP.Equals(uuid)) {
                            int intValue15 = bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 0).IntValue();
                            if (intValue15 == 192) {
                                switch (bluetoothGattCharacteristic2.GetIntValue(GattFormat.Uint8, 1).IntValue()) {
                                    case 1:
                                        mBluetoothGatt.WriteCharacteristic(bluetoothGattCharacteristic2);
                                        broadcastUpdate(BLEHelpers.INTENT_BLE_READCOMPLETED, "");
                                        //glucoseBleService6.m_data_list = glucoseBleService6.convertData(GlucoseBleService.mRecords);
                                        //completeDownloading();
                                        break;
                                    case 2:
                                        //getTimeSyncAndSequence();
                                        break;
                                }
                            }
                        }
                    }
                }

            }
        }

        //public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {

        //        Log.Error("OnCharacteristicChanged", characteristic.Uuid.Tostring());
        //        //        if (!Constants.UuidGlucMeasurementChar.Equals(characteristic.Uuid)) return;
        //        //        var offset = 0;
        //        //        var flags = characteristic.GetIntValue(GattFormat.Uint8, offset).IntValue();
        //        //        offset += 1;

        //        //        var timeOffsetPresent = (flags & 0x01) > 0;
        //        //        var typeAndLocationPresent = (flags & 0x02) > 0;
        //        //        var concentrationUnit = (flags & 0x04) > 0 ? 1 : 0;
        //        //        var sensorStatusAnnunciationPresent = (flags & 0x08) > 0;

        //        //        // create and fill the new record
        //        //        var record = new GlucoseDeviceData {
        //        //            SequenceNumber = characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue()
        //        //        };
        //        //        offset += 2;

        //        //        var year = characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
        //        //        var month = characteristic.GetIntValue(GattFormat.Uint8, offset + 2).IntValue();
        //        //        var day = characteristic.GetIntValue(GattFormat.Uint8, offset + 3).IntValue();
        //        //        var hours = characteristic.GetIntValue(GattFormat.Uint8, offset + 4).IntValue();
        //        //        var minutes = characteristic.GetIntValue(GattFormat.Uint8, offset + 5).IntValue();
        //        //        var seconds = characteristic.GetIntValue(GattFormat.Uint8, offset + 6).IntValue();
        //        //        offset += 7;

        //        //        var calendar = Calendar.Instance;
        //        //        calendar.Set(year, month, day, hours, minutes, seconds);

        //        //        if (timeOffsetPresent) {
        //        //            characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
        //        //            offset += 2;
        //        //        }

        //        //        if (typeAndLocationPresent) {
        //        //            record.GlucoseConcentration = characteristic
        //        //                .GetFloatValue(GattFormat.Sfloat, offset).FloatValue();
        //        //            var typeAndLocation = characteristic.GetIntValue(GattFormat.Uint8, offset + 2)
        //        //                .IntValue();
        //        //            offset += 3;

        //        //            (Context as GlucoseDeviceActivity)?.RunOnUiThread(() => {
        //        //                (Context as GlucoseDeviceActivity)?._animationView.CancelAnimation();
        //        //                (Context as GlucoseDeviceActivity)?.UpdateUi(
        //        //                    record.GlucoseConcentration * 100000);
        //        //            });
        //        //        }

        //        //        if (sensorStatusAnnunciationPresent) {
        //        //            characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
        //        //        }
        //        //    }

        //        //}
        //        //private async void UpdateUi(float g) {
        //        //    Log.Error("UpdateUI", "Aici");
        //        //    if (!_send) {
        //        //        var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
        //        //        //await _db.CreateTableAsync<DevicesRecords>();
        //        //        if (!Utils.CheckNetworkAvailability()) {
        //        //            await _bleDevicesDataRecords.Insert(new DevicesRecords() {
        //        //                Imei = Utils.GetDeviceIdentificator(this),
        //        //                DateTime = ft.Format(new Date()),
        //        //                BloodGlucose = (int)g
        //        //            });
        //        //        }
        //        //        //AddGlucoseRecord(_db, (int) g);
        //        //        else {
        //        //            JSONObject jsonObject;
        //        //            var jsonArray = new JSONArray();
        //        //            var list = await _bleDevicesDataRecords.QueryValuations("select * from DevicesRecords");

        //        //            foreach (var el in list) {
        //        //                try {
        //        //                    jsonObject = new JSONObject();
        //        //                    jsonObject
        //        //                        .Put("imei", el.Imei)
        //        //                        .Put("dateTimeISO", el.DateTime)
        //        //                        .Put("geolocation", new JSONObject()
        //        //                        .Put("latitude", $"{el.Latitude}")
        //        //                        .Put("longitude", $"{el.Longitude}"))
        //        //                        .Put("lastLocation", el.LastLocation)
        //        //                        .Put("sendPanicAlerts", el.SendPanicAlerts)
        //        //                        .Put("stepCounter", el.StepCounter)
        //        //                        .Put("bloodPressureSystolic", el.BloodPresureSystolic)
        //        //                        .Put("bloodPressureDiastolic", el.BloodPresureDiastolic)
        //        //                        .Put("bloodPressurePulseRate", el.BloodPresurePulsRate)
        //        //                        .Put("bloodGlucose", "" + el.BloodGlucose)
        //        //                        .Put("oxygenSaturation", el.OxygenSaturation)
        //        //                        .Put("extension", el.Extension);
        //        //                    jsonArray.Put(jsonObject);
        //        //                } catch (JSONException e) {
        //        //                    e.PrintStackTrace();
        //        //                }
        //        //            }
        //        //            jsonObject = new JSONObject();
        //        //            jsonObject
        //        //                .Put("imei", Utils.GetDeviceIdentificator(this))
        //        //                .Put("dateTimeISO", ft.Format(new Date()))
        //        //                .Put("geolocation", string.Empty)
        //        //                .Put("lastLocation", string.Empty)
        //        //                .Put("sendPanicAlerts", string.Empty)
        //        //                .Put("stepCounter", string.Empty)
        //        //                .Put("bloodPressureSystolic", string.Empty)
        //        //                .Put("bloodPressureDiastolic", string.Empty)
        //        //                .Put("bloodPressurePulseRate", string.Empty)
        //        //                .Put("bloodGlucose", (int)g)
        //        //                .Put("oxygenSaturation", string.Empty)
        //        //                .Put("extension", string.Empty);
        //        //            jsonArray.Put(jsonObject);
        //        //            var result = await WebServices.Post(Constants.SaveDeviceDataUrl, jsonArray);
        //        //            if (result == "Succes!") {
        //        //                Toast.MakeText(this, "Succes", ToastLength.Long).Show();
        //        //                await _bleDevicesDataRecords.DropTable();
        //        //            } else {
        //        //                Toast.MakeText(this, "" + result, ToastLength.Long).Show();
        //        //            }

        //        //        }

        //        //        //WriteBloodGlucoseData(jsonObject);

        //        //        //new BGM().execute(Constants.DATA_URL);
        //        //    }

        //        //var gl = Getstring(Resource.string.glucose) + (int)g;
        //        //_glucose.Text = gl;
        //        //ActivateScanButton();
        //    }
        //}

        void ActivateScanButton() {
            _scanButton.Enabled = true;
        }

        private void SetCharacteristicNotification(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            SetCharacteristicNotificationWithDelay(gatt, serviceUuid, characteristicUuid);
        }

        private void SetCharacteristicNotificationWithDelay(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid) {
            _handler.PostDelayed(
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