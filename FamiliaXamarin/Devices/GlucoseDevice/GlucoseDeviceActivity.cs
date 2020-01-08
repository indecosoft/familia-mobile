using System;
using System.Collections;
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
using Familia.Devices.BluetoothCallbacks.Glucose;
using Familia.Devices.Models;
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
        internal BluetoothLeScanner _bluetoothScanner;
        private BluetoothManager _bluetoothManager;
        //private SQLiteAsyncConnection _db;
        private Handler _handler;
        private bool _send;

        private TextView _glucose;
        private Button _scanButton;

        // private ProgressDialog progressDialog;
        internal TextView _lbStatus;
        private ConstraintLayout _dataContainer;

        //private ProgressDialog progressDialog;
        private LottieAnimationView _animationView;
        private GlucoseDeviceActivity _context;
        internal GlucoseScanCallback _scanCallback;
        internal GlucoseGattCallBack _gattCallback;
        private SqlHelper<DevicesRecords> _bleDevicesDataRecords;



        private static BluetoothAdapter mBluetoothAdapter;
        public static string mBluetoothDeviceAddress;



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

            var bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
            IEnumerable<BluetoothDeviceRecords> list = await bleDevicesRecords.QueryValuations("select * from BluetoothDeviceRecords");
            _scanCallback = new GlucoseScanCallback(_context, list);

            _gattCallback = new GlucoseGattCallBack(_context, list);

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

        internal async Task UpdateUi(float g) {
            if (!_send) {
                using var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
                if (!Utils.CheckNetworkAvailability()) {
                    await _bleDevicesDataRecords.Insert(new DevicesRecords() {
                        Imei = Utils.GetDeviceIdentificator(this),
                        DateTime = ft.Format(new Date()),
                        BloodGlucose = (int)g
                    });
                }
                else {
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
                        await _bleDevicesDataRecords.DropTable();
                    } else {
                        Toast.MakeText(this, "" + result, ToastLength.Long).Show();
                    }
                }
            }

            var gl = GetString(Resource.String.glucose) + (int)g + " md/dL";
            _glucose.Text = gl;
            _animationView.CancelAnimation();
            ActivateScanButton();
        }

        void ActivateScanButton() {
            _scanButton.Enabled = true;
        }
    }
}