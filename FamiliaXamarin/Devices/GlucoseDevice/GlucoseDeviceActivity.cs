using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Animation;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Airbnb.Lottie.Model;
using Com.Airbnb.Lottie.Value;
using Familia.DataModels;
using Familia.Devices.Bluetooth.Callbacks.Glucose;
using Familia.Devices.DevicesManagement.Dialogs.Events;
using Familia.Devices.DevicesManagement.Dialogs.Helpers;
using Familia.Devices.GlucoseDevice.Dialogs;
using Familia.Helpers;
using Java.Text;
using Java.Util;
using Org.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Devices.GlucoseDevice {
    [Activity(Label = "GlucoseDeviceActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class GlucoseDeviceActivity : AppCompatActivity, Animator.IAnimatorListener {
        private BluetoothAdapter _bluetoothAdapter;
        internal BluetoothLeScanner BluetoothScanner;
        private BluetoothManager _bluetoothManager;
        private TextView _glucose;
        //private Button _scanButton;
        private Button _manualRegisterButton;
        internal TextView LbStatus;
        private ConstraintLayout _dataContainer;
        private LottieAnimationView _animationView;
        internal GlucoseScanCallback ScanCallback;
        internal GlucoseGattCallBack GattCallback;
        internal MedisanaGattCallback MedisanaGattCallback;

        private SqlHelper<DevicesRecords> _bleDevicesDataRecords;
        private string _imei;

        protected override async void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.blood_glucose_device);
            InitUi();
            InitEvents();
            string data = Intent.GetStringExtra("Data");
            if (data != null)
            {
                if (Utils.isJson(data))
                {
                    _imei = new JSONObject(data).GetString("deviceId");
                }
                else
                {
                    var decripted = Encryption.Decrypt(data);
                    if (Utils.isJson(decripted))
                    {
                        _imei = new JSONObject(decripted).GetString("imei");
                    }
                    else
                    {
                        _imei = data;
                    }
                }
            }
            if (string.IsNullOrEmpty(_imei))
            {
                _imei = Utils.GetDeviceIdentificator(this);
            }
            List<BluetoothDeviceRecords> list = new List<BluetoothDeviceRecords>();
           await Task.Run(async () =>
            {
                _bleDevicesDataRecords = await SqlHelper<DevicesRecords>.CreateAsync();
                 list = (await (await SqlHelper<BluetoothDeviceRecords>.CreateAsync()).QueryValuations("select * from BluetoothDeviceRecords")).ToList();
               
            });
            _bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);
            ScanCallback = new GlucoseScanCallback(this, list);
            GattCallback = new GlucoseGattCallBack(this, list);
            MedisanaGattCallback = new MedisanaGattCallback(this);
            _animationView.PlayAnimation();
        }

        private void InitUi() {
            var toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Glicemie";

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate {
                Finish();
            };

            LbStatus = FindViewById<TextView>(Resource.Id.status);
            _dataContainer = FindViewById<ConstraintLayout>(Resource.Id.dataContainer);
            _manualRegisterButton = FindViewById<Button>(Resource.Id.manual_register);

            _dataContainer.Visibility = ViewStates.Gone;
            _glucose = FindViewById<TextView>(Resource.Id.GlucoseTextView);
            //_scanButton = FindViewById<Button>(Resource.Id.ScanButton);
            _animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            var filter =
                new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.accent));
            _animationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter,
                new LottieValueCallback(filter));
        }

        private void InitEvents() {
            _animationView.AddAnimatorListener(this);
            _manualRegisterButton.Click += delegate {
                var cdd = new GlucoseManualRegisterDialog(this);
                cdd.DialogState += async delegate (object o, DialogStateEventArgs eventArgs) {
                    if (eventArgs.Status != DialogStatuses.Dismissed) return;

                    if (cdd.Glucose != null)
                    {
                        await UpdateUi(cdd.Glucose);
                    }
                };
                cdd.Show();
                cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);

            };
            //_scanButton.Click += delegate {
            //    if (_bluetoothManager == null) return;
            //    BluetoothScanner.StartScan(ScanCallback);
            //    _scanButton.Enabled = false;
            //    _send = false;
            //    LbStatus.Text = "Se efectueaza masuratoarea...";
            //    _animationView.PlayAnimation();
            //};
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GattCallback.Dispose();
            MedisanaGattCallback.Dispose();
            ScanCallback.Dispose();

        }
        public void OnAnimationCancel(Animator animation) {
            _dataContainer.Visibility = ViewStates.Visible;
            _animationView.Progress = 1f;
        }

        public  void OnAnimationEnd(Animator animation) { }

        public void OnAnimationRepeat(Animator animation) { }

        public void OnAnimationStart(Animator animation) {
            LbStatus.Text = "Se cauta dispozitive...";
            if (_bluetoothManager == null) return;
            _bluetoothAdapter = _bluetoothManager.Adapter;
            if (!_bluetoothAdapter.IsEnabled) {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
            } else {
                BluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
                BluetoothScanner.StartScan(ScanCallback);
                //_scanButton.Enabled = false;
                _dataContainer.Visibility = ViewStates.Gone;
            }

        }
        protected override void OnPause() {
            base.OnPause();
            if (_bluetoothAdapter != null) {
                BluetoothScanner?.StopScan(ScanCallback);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode != 11) return;
            if (resultCode == Result.Ok) {
                BluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
                BluetoothScanner.StartScan(ScanCallback);
                //_scanButton.Enabled = false;
                _animationView.PlayAnimation();

            } else {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
            }
        }

        internal async Task UpdateUi(float? g = null) {
            Log.Error("glucose", g.ToString());
                if (g != null) {
                    Log.Error("Data", "send Data");
                    using var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
                    if (!Utils.CheckNetworkAvailability()) {
                        await _bleDevicesDataRecords.Insert(new DevicesRecords {
                            Imei = _imei,
                            DateTime = ft.Format(new Date()),
                            BloodGlucose = (int)g
                        });
                    } else {
                        JSONObject jsonObject;
                        var jsonArray = new JSONArray();
                        var list = await _bleDevicesDataRecords.QueryValuations("select * from DevicesRecords");
                        Log.Error("list", list.Count().ToString());
                        foreach (DevicesRecords el in list) {
                            try {
                                jsonObject = new JSONObject();
                                jsonObject
                                    .Put("imei", el.Imei)
                                    .Put("dateTimeISO", el.DateTime)
                                    .Put("geolocation", new JSONObject()
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
                            } catch (JSONException e) {
                                e.PrintStackTrace();
                            }
                        }
                        jsonObject = new JSONObject();
                        jsonObject
                            .Put("imei", _imei)
                            .Put("dateTimeISO", ft.Format(new Date()))
                            .Put("geolocation", string.Empty)
                            .Put("lastLocation", string.Empty)
                            .Put("sendPanicAlerts", string.Empty)
                            .Put("stepCounter", string.Empty)
                            .Put("bloodPressureSystolic", string.Empty)
                            .Put("bloodPressureDiastolic", string.Empty)
                            .Put("bloodPressurePulseRate", string.Empty)
                            .Put("bloodGlucose", g)
                            .Put("oxygenSaturation", string.Empty)
                            .Put("extension", string.Empty);
                        jsonArray.Put(jsonObject);
                        string result = await WebServices.WebServices.Post(Constants.SaveDeviceDataUrl, jsonArray);
                        if (result == "Succes!") {
                            Toast.MakeText(this, "Succes", ToastLength.Long).Show();
                            await _bleDevicesDataRecords.DeleteAll();
                        } else {
                            Toast.MakeText(this, "" + result, ToastLength.Long).Show();
                        }
                    }
                }

            if (g != 1) {
                _glucose.Text = GetString(Resource.String.glucose) + " " + g + " md/dL";
            } else {
                _glucose.Text = string.Empty;
            }
            _animationView.CancelAnimation();
        }

    }
}