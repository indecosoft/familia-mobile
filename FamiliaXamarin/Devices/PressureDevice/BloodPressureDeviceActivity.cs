using System.Collections.Generic;
using System.Threading;
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
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Airbnb.Lottie.Model;
using Com.Airbnb.Lottie.Value;
using Familia;
using Familia.Devices.BluetoothCallbacks.BloodPressure;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using Java.Text;
using Java.Util;
using Org.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.Devices.PressureDevice {
    [Activity(Label = "BloodPressureDeviceActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class BloodPressureDeviceActivity : AppCompatActivity, Animator.IAnimatorListener {
        private BluetoothAdapter _bluetoothAdapter;
        internal BluetoothLeScanner _bluetoothScanner;
        private BluetoothManager _bluetoothManager;
        private bool _send;
        private TextView _systole;
        private TextView _diastole;
        private TextView _pulse;
        private Button _scanButton;
        internal TextView _lbStatus;
        private ConstraintLayout _dataContainer;
        internal LottieAnimationView _animationView;
        internal BloodPressureScanCallback _scanCallback;
        internal BloodPressureGattCallBack _gattCallback;
        private SqlHelper<DevicesRecords> _bleDevicesDataRecords;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_blood_pressure_device);
            InitUI();
            InitEvents();
            //test();
            IEnumerable<BluetoothDeviceRecords> list = null;
            Task.Run(async () => {
                _bleDevicesDataRecords = await SqlHelper<DevicesRecords>.CreateAsync();
                var bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                list = await bleDevicesRecords.QueryValuations("select * from BluetoothDeviceRecords");
                this.RunOnUiThread(() => {
                    _bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);
                    _scanCallback = new BloodPressureScanCallback(this, list);
                    _gattCallback = new BloodPressureGattCallBack(this, list);
                    _animationView.PlayAnimation();
                });
                
            }).Wait();
            
            
        }

        private void InitEvents() {
            _animationView.AddAnimatorListener(this);
            _scanButton.Click += delegate {
                if (_bluetoothManager == null || _bluetoothAdapter == null) return;
                _bluetoothScanner.StartScan(_scanCallback);
                _scanButton.Enabled = false;
                _send = false;
                _lbStatus.Text = "Se efectueaza masuratoarea...";
                _animationView.PlayAnimation();
            };
            
        }

        private void InitUI() {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Tensiune";

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate {
                Finish();
            };
            _lbStatus = FindViewById<TextView>(Resource.Id.status);
            _dataContainer = FindViewById<ConstraintLayout>(Resource.Id.dataContainer);
            _systole = FindViewById<TextView>(Resource.Id.SystoleTextView);
            _diastole = FindViewById<TextView>(Resource.Id.DiastoleTextView);
            _pulse = FindViewById<TextView>(Resource.Id.PulseTextView);
            _scanButton = FindViewById<Button>(Resource.Id.ScanButton);
            _animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            //_dataContainer.Visibility = ViewStates.Gone;
            var filter = new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.accent));
            _animationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter, new LottieValueCallback(filter));
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

        public void OnAnimationCancel(Animator animation) {
            _dataContainer.Visibility = ViewStates.Visible;
            _lbStatus.Text = "Masuratoare efecuata cu succes";
            _animationView.Progress = 1f;
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

        public void OnAnimationEnd(Animator animation) { }

        public void OnAnimationRepeat(Animator animation) { }

        public void OnAnimationStart(Animator animation) {

            _lbStatus.Text = "Se efectueaza masuratoarea...";

            if (_bluetoothManager == null) return;
            _bluetoothAdapter = _bluetoothManager.Adapter;
            if (!_bluetoothAdapter.IsEnabled) {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
            } else {
                _bluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
                _bluetoothScanner.StartScan(_scanCallback);
                _scanButton.Enabled = false;
                _dataContainer.Visibility = ViewStates.Gone;
            }
        }
        internal async void UpdateUi(BloodPressureData data) {
            if (!_send) {
                var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);

                if (!Utils.CheckNetworkAvailability()) {
                    await _bleDevicesDataRecords.Insert(new DevicesRecords() {
                        Imei = Utils.GetDeviceIdentificator(this),
                        DateTime = ft.Format(new Date()),
                        BloodPresureSystolic = data.Systolic,
                        BloodPresureDiastolic = data.Diastolic,
                        BloodPresurePulsRate = data.PulseRate
                    });
                } else {
                    JSONObject jsonObject;
                    var jsonArray = new JSONArray();
                    var list = await _bleDevicesDataRecords.QueryValuations("select * from DevicesRecords");

                    foreach (var el in list) {
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
                        .Put("imei", Utils.GetDeviceIdentificator(this))
                        .Put("dateTimeISO", ft.Format(new Date()))
                        .Put("geolocation", string.Empty)
                        .Put("lastLocation", string.Empty)
                        .Put("sendPanicAlerts", string.Empty)
                        .Put("stepCounter", string.Empty)
                        .Put("bloodPressureSystolic", data.Systolic)
                        .Put("bloodPressureDiastolic", data.Diastolic)
                        .Put("bloodPressurePulseRate", data.PulseRate)
                        .Put("bloodGlucose", string.Empty)
                        .Put("oxygenSaturation", string.Empty)
                        .Put("extension", string.Empty);
                    jsonArray.Put(jsonObject);
                    var result = await WebServices.Post(Constants.SaveDeviceDataUrl, jsonArray);
                    if (result == "Succes!") {
                        Toast.MakeText(this, "Succes", ToastLength.Long).Show();
                        await _bleDevicesDataRecords.DropTable();
                    } else {
                        Toast.MakeText(this, "" + result, ToastLength.Long).Show();
                    }
                }
            }
            _systole.Text = GetString(Resource.String.systole) + " " + data.Systolic + " mmHg";
            _diastole.Text = GetString(Resource.String.diastole) + " " + data.Diastolic + " mmHg";
            _pulse.Text = GetString(Resource.String.pulse) + " " + data.PulseRate + " b/min";
            ActivateScanButton();
        }

        private void ActivateScanButton() {
            _scanButton.Enabled = true;
        }
    }
}