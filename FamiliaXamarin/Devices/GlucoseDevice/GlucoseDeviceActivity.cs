using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using Java.IO;
using Java.Text;
using Java.Util;
using Org.Json;
using SQLite;
using Environment = System.Environment;
using File = Java.IO.File;
using IOException = Java.IO.IOException;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.Devices.GlucoseDevice
{
    [Activity(Label = "GlucoseDeviceActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class GlucoseDeviceActivity : AppCompatActivity , Animator.IAnimatorListener
    {
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

        //private ProgressDialog progressDialog;
        private LottieAnimationView _animationView;
        private GlucoseDeviceActivity _context;
        private BluetoothScanCallback _scanCallback;
        private GattCallBack _gattCallback;
        private SqlHelper<DevicesRecords> _bleDevicesDataRecords;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.blood_glucose_device);
            var toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title ="Glicemie";

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
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


            _scanButton.Click += delegate
            {
                if (_bluetoothManager == null) return;
                _bluetoothScanner.StartScan(_scanCallback);
                _scanButton.Enabled =false;
                _send = false;

                _lbStatus.Text = "Se efectueaza masuratoarea...";
                _animationView.PlayAnimation();
                //                        progressDialog.setMessage(getString(R.string.conectare_info));
                //                        progressDialog.show();
            }; 

        // Create your application here
    }

        public void OnAnimationCancel(Animator animation)
        {
            _dataContainer.Visibility = ViewStates.Visible;
            _lbStatus.Text = "Masuratoare efecuata cu succes";
            _animationView.Progress = 1f;
        }

        public void OnAnimationEnd(Animator animation)
        {
            //throw new NotImplementedException();
        }

        public void OnAnimationRepeat(Animator animation)
        {
            //throw new NotImplementedException();
        }

        public void OnAnimationStart(Animator animation)
        {
            _handler = new Handler();
            _lbStatus.Text = "Se efectueaza masuratoarea...";
//            if (_bluetoothManager == null) return;
            _bluetoothAdapter = _bluetoothManager?.Adapter;
            if (_bluetoothAdapter == null) return;
            if (!_bluetoothAdapter.IsEnabled)
            {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
            }
            else
            {
                _bluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
                _bluetoothScanner.StartScan(_scanCallback);
                _scanButton.Enabled = false;
                _dataContainer.Visibility = ViewStates.Gone;
                //progressDialog.show();
            }

        }
        protected override void OnPostResume()
        {
            base.OnPostResume();
            _send = false;
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (_bluetoothAdapter != null)
            {
                _bluetoothScanner?.StopScan(_scanCallback);
            }

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode != 11) return;
            if (resultCode == Result.Ok)
            {
                _bluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
                _bluetoothScanner.StartScan(_scanCallback);
                _scanButton.Enabled = false;
                _animationView.PlayAnimation();

            }
            else
            {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
            }
        }

        class BluetoothScanCallback : ScanCallback
        {
            private readonly Context Context;
            private SqlHelper<BluetoothDeviceRecords> _bleDevicesRecords;
            public BluetoothScanCallback(Context context)
            {
                Context = context;
            }

            public override async void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);
//                var jsonDevice = new JSONObject(Utils.GetDefaults(Context.GetString(Resource.String.blood_glucose_device),
//                    Context));

                _bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                var list = await _bleDevicesRecords.QueryValuations("select * from BluetoothDeviceRecords");
                
                if(!(from c in list where c.Address == result.Device.Address 
                    select new {c.Name, c.Address, c.DeviceType}).Any()) 
                    return;
                
//                if (result.Device.Address == null ||
//                    !result.Device.Address.Equals(
//                       jsonDevice.GetString("Address"))) return;
                result.Device.ConnectGatt(Context, false,
                    ((GlucoseDeviceActivity) Context)._gattCallback);
                ((GlucoseDeviceActivity) Context)._bluetoothScanner.StopScan(
                    ((GlucoseDeviceActivity) Context)._scanCallback);
                ((GlucoseDeviceActivity) Context)._lbStatus.Text =
                    Context.GetString(Resource.String.conectare_info);
            }
        }

        private class GattCallBack : BluetoothGattCallback
        {
            private readonly Context Context;
            public GattCallBack(Context context)
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
                    (Context as GlucoseDeviceActivity)?.SetCharacteristicNotification(gatt,
                        Constants.UuidGlucServ, Constants.UuidGlucMeasurementChar);
                }
            }

            public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
            {
                base.OnDescriptorWrite(gatt, descriptor, status);
                if (status != 0) return;
                if (descriptor.Characteristic.Uuid.Equals(Constants.UuidGlucMeasurementContextChar))
                {
                    (Context as GlucoseDeviceActivity)?.SetCharacteristicNotification(gatt,
                        Constants.UuidGlucServ, Constants.UuidGlucRecordAccessControlPointChar);
                    Log.Error("Aici", "1");
                }
                if (descriptor.Characteristic.Uuid.Equals(Constants.UuidGlucMeasurementChar))
                {
                    (Context as GlucoseDeviceActivity)?.SetCharacteristicNotification(gatt,
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

                    (Context as GlucoseDeviceActivity)?.RunOnUiThread(() =>
                    {
                        (Context as GlucoseDeviceActivity)?._animationView.CancelAnimation();
                        (Context as GlucoseDeviceActivity)?.UpdateUi(
                            record.GlucoseConcentration * 100000);
                    });
                }

                if (sensorStatusAnnunciationPresent)
                {
                    characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
                }
            }

        }
        private async void UpdateUi(float g)
        {
            Log.Error("UpdateUI", "Aici");
            if (!_send)
            {
                var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
                //await _db.CreateTableAsync<DevicesRecords>();
                if (!Utils.CheckNetworkAvailability())
                {
                    await _bleDevicesDataRecords.Insert(new DevicesRecords()
                    {
                        Imei = Utils.GetDeviceIdentificator(this),
                        DateTime = ft.Format(new Date()),
                        BloodGlucose = (int) g
                    });
                }
                    //AddGlucoseRecord(_db, (int) g);
                else
                {
                    JSONObject jsonObject;
                    var jsonArray = new JSONArray();
                    var list = await _bleDevicesDataRecords.QueryValuations( "select * from DevicesRecords");

                    foreach (var el in list)
                    {
                        try
                        {
                            jsonObject = new JSONObject();
                            jsonObject
                                .Put("imei", el.Imei)
                                .Put("dateTimeISO", el.DateTime)
                                .Put("geolocation",new JSONObject()
                                .Put("latitude", $"{el.Latitude}")
                                .Put("longitude", $"{el.Longitude}"))
                                .Put("lastLocation", el.LastLocation)
                                .Put("sendPanicAlerts", el.SendPanicAlerts)
                                .Put("stepCounter", el.StepCounter)
                                .Put("bloodPressureSystolic", el.BloodPresureSystolic)
                                .Put("bloodPressureDiastolic", el.BloodPresureDiastolic)
                                .Put("bloodPressurePulseRate", el.BloodPresurePulsRate)
                                .Put("bloodGlucose", "" + el.BloodGlucose)
                                .Put("oxygenSaturation", el.OxygenSaturation)
                                .Put("extension", el.Extension);
                            jsonArray.Put(jsonObject);
                        }
                        catch (JSONException e)
                        {
                            e.PrintStackTrace();
                        }
                    }
                    jsonObject = new JSONObject();
                    jsonObject
                        .Put("imei", Utils.GetDeviceIdentificator(this))
                        .Put("dateTimeISO", ft.Format(new Date()))
                        .Put("geolocation", string.Empty)
                        .Put("lastLocation", string.Empty)
                        .Put("sendPanicAlerts", string.Empty)
                        .Put("stepCounter", string.Empty)
                        .Put("bloodPressureSystolic", string.Empty)
                        .Put("bloodPressureDiastolic", string.Empty)
                        .Put("bloodPressurePulseRate", string.Empty)
                        .Put("bloodGlucose", (int)g)
                        .Put("oxygenSaturation", string.Empty)
                        .Put("extension", string.Empty);
                    jsonArray.Put(jsonObject);
                    var result = await WebServices.Post(Constants.SaveDeviceDataUrl, jsonArray);
                    if (result == "Succes!")
                    {
                         Toast.MakeText(this, "Succes", ToastLength.Long).Show();
                        await _bleDevicesDataRecords.DropTable();
                    }
                    else
                    {
                        Toast.MakeText(this, ""+result, ToastLength.Long).Show();
                    }

                }
                
                //WriteBloodGlucoseData(jsonObject);

                //new BGM().execute(Constants.DATA_URL);
            }

            var gl = GetString(Resource.String.glucose) + (int)g;
            _glucose.Text = gl;
            ActivateScanButton();
        }

        void ActivateScanButton()
        {
            _scanButton.Enabled = true;
        }

        private void SetCharacteristicNotification(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid)
        {
            SetCharacteristicNotificationWithDelay(gatt, serviceUuid, characteristicUuid);
        }

        private void SetCharacteristicNotificationWithDelay(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid)
        {
            _handler.PostDelayed(
                () =>
                {
                    SetCharacteristicNotification_private(gatt, serviceUuid, characteristicUuid);
                }, 300);
        }

        private static void SetCharacteristicNotification_private(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid)
        {
            try
            {
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
            }
            catch (Java.Lang.Exception e)
            {
                e.PrintStackTrace();
            }
        }

    }
}