using System.Collections.Generic;
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
using Familia;
using Familia.Devices.Bluetooth.Callbacks.Glucose;
using Familia.Devices.BluetoothCallbacks.Glucose;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using Java.Text;
using Java.Util;
using Org.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.Devices.GlucoseDevice {
    [Activity(Label = "GlucoseDeviceActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class GlucoseDeviceActivity : AppCompatActivity, Animator.IAnimatorListener {
        private BluetoothAdapter _bluetoothAdapter;
        internal BluetoothLeScanner _bluetoothScanner;
        private BluetoothManager _bluetoothManager;
        private bool _send;
        private TextView _glucose;
        private Button _scanButton;
        internal TextView _lbStatus;
        private ConstraintLayout _dataContainer;
        private LottieAnimationView _animationView;
        internal GlucoseScanCallback _scanCallback;
        internal GlucoseGattCallBack _gattCallback;
        internal MedisanaGattCallback _medisanaGattCallback;

        private SqlHelper<DevicesRecords> _bleDevicesDataRecords;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.blood_glucose_device);
            InitUI();
            InitEvents();
            Task.Run(async () => {
                _bleDevicesDataRecords = await SqlHelper<DevicesRecords>.CreateAsync();
                var bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                IEnumerable<BluetoothDeviceRecords> list = await bleDevicesRecords.QueryValuations("select * from BluetoothDeviceRecords");
                this.RunOnUiThread(() => {
                    _bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);
                    _scanCallback = new GlucoseScanCallback(this, list);
                    _gattCallback = new GlucoseGattCallBack(this, list);
                    _medisanaGattCallback = new MedisanaGattCallback(this);
                    _animationView.PlayAnimation();
                });
            }).Wait();
        }

        private void InitUI() {
            var toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Glicemie";

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate {
                Finish();
            };

            _lbStatus = FindViewById<TextView>(Resource.Id.status);
            _dataContainer = FindViewById<ConstraintLayout>(Resource.Id.dataContainer);
            _dataContainer.Visibility = ViewStates.Gone;
            _glucose = FindViewById<TextView>(Resource.Id.GlucoseTextView);
            _scanButton = FindViewById<Button>(Resource.Id.ScanButton);
            _animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            var filter =
                new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.accent));
            _animationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter,
                new LottieValueCallback(filter));
        }

        private void InitEvents() {
            _animationView.AddAnimatorListener(this);
            _scanButton.Click += delegate {
                if (_bluetoothManager == null) return;
                _bluetoothScanner.StartScan(_scanCallback);
                _scanButton.Enabled = false;
                _send = false;
                _lbStatus.Text = "Se efectueaza masuratoarea...";
                _animationView.PlayAnimation();
            };
        }

        public void OnAnimationCancel(Animator animation) {
            _dataContainer.Visibility = ViewStates.Visible;
            _lbStatus.Text = "Masuratoare efecuata cu succes";
            _animationView.Progress = 1f;
        }

        public void OnAnimationEnd(Animator animation) { }

        public void OnAnimationRepeat(Animator animation) { }

        public void OnAnimationStart(Animator animation) {
            _lbStatus.Text = "Se cauta dispozitive...";
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

        internal async Task UpdateUi(float g) {
            Log.Error("glucose", g.ToString());
            if (!_send) {
                using var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
                if (!Utils.CheckNetworkAvailability()) {
                    await _bleDevicesDataRecords.Insert(new DevicesRecords() {
                        Imei = Utils.GetDeviceIdentificator(this),
                        DateTime = ft.Format(new Date()),
                        BloodGlucose = (int)g
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
                        .Put("bloodPressureSystolic", string.Empty)
                        .Put("bloodPressureDiastolic", string.Empty)
                        .Put("bloodPressurePulseRate", string.Empty)
                        .Put("bloodGlucose", (int)g)
                        .Put("oxygenSaturation", string.Empty)
                        .Put("extension", string.Empty);
                    jsonArray.Put(jsonObject);
                    var result = await WebServices.Post(Constants.SaveDeviceDataUrl, jsonArray);
                    if (result == "Succes!") {
                        Toast.MakeText(this, "Succes", ToastLength.Long).Show();
                        await _bleDevicesDataRecords.DeleteAll();
                    } else {
                        Toast.MakeText(this, "" + result, ToastLength.Long).Show();
                    }
                }
            }
            var gl = GetString(Resource.String.glucose) + " " + (int)g + " md/dL";
            _glucose.Text = gl;
            _animationView.CancelAnimation();
            ActivateScanButton();
        }

        void ActivateScanButton() {
            _scanButton.Enabled = true;
        }
    }
}