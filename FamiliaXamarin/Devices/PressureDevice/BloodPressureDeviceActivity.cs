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
using Familia.Devices.Bluetooth.Callbacks.BloodPressure;
using Familia.Devices.DevicesManagement.Dialogs.Events;
using Familia.Devices.DevicesManagement.Dialogs.Helpers;
using Familia.Devices.PressureDevice.Dialogs;
using Familia.Helpers;
using Java.Text;
using Java.Util;
using Org.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Devices.PressureDevice
{
    [Activity(Label = "BloodPressureDeviceActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class BloodPressureDeviceActivity : AppCompatActivity, Animator.IAnimatorListener
    {
        private BluetoothAdapter _bluetoothAdapter;
        internal BluetoothLeScanner BluetoothScanner;
        private BluetoothManager _bluetoothManager;
        private TextView _systole;
        private TextView _diastole;
        private TextView _pulse;
        //private Button _scanButton;
        private Button _manualRegisterButton;
        internal TextView LbStatus;
        private ConstraintLayout _dataContainer;
        internal LottieAnimationView AnimationView;
        internal BloodPressureScanCallback ScanCallback;
        internal BloodPressureGattCallBack GattCallback;
        private SqlHelper<DevicesRecords> _bleDevicesDataRecords;
        private string _imei;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_blood_pressure_device);
            InitUi();
            InitEvents();
            IEnumerable<BluetoothDeviceRecords> list;
            string data = Intent.GetStringExtra("Data");
            if(data != null)
            {
                if (Utils.isJson(data))
                {
                    _imei = new JSONObject(data).GetString("deviceId");
                } else {
                    var decripted = Encryption.Decrypt(data);
                    if (Utils.isJson(decripted))
                    {
                        _imei = new JSONObject(decripted).GetString("imei");
                    } else
                    {
                        _imei = data;
                    }
                }
            }
            if (string.IsNullOrEmpty(_imei))
            {
                _imei = Utils.GetDeviceIdentificator(this);
            }
            Task.Run(async () =>
            {
                _bleDevicesDataRecords = await SqlHelper<DevicesRecords>.CreateAsync();
                var bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                list = await bleDevicesRecords.QueryValuations("select * from BluetoothDeviceRecords");
                RunOnUiThread(() =>
                {
                    _bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);
                    var listOfSavedDevices = list.ToList();
                    ScanCallback = new BloodPressureScanCallback(this, listOfSavedDevices);
                    GattCallback = new BloodPressureGattCallBack(this, listOfSavedDevices);
                    AnimationView.PlayAnimation();
                });

            }).Wait();
        }

        
        private void InitEvents()
        {
            AnimationView.AddAnimatorListener(this);
            _manualRegisterButton.Click += delegate {
                BloodPressureData model = new BloodPressureData();
                var cdd = new BloodPressureManualRegisterDialog(this, model);
                cdd.DialogState += async delegate (object o, DialogStateEventArgs eventArgs) {
                    if (eventArgs.Status != DialogStatuses.Dismissed) return;

                    if(model != null)
                    {
                        UpdateUi(model);
                    }
                };
                cdd.Show();
                cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimaryDark);

            };
            //_scanButton.Click += delegate {
            //    if (_bluetoothManager == null || _bluetoothAdapter == null) return;
            //    BluetoothScanner.StartScan(ScanCallback);
            //    _scanButton.Enabled = false;
            //    _send = false;
            //    LbStatus.Text = "Se efectueaza masuratoarea...";
            //    AnimationView.PlayAnimation();
            //};

        }

        private void InitUi()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Tensiune";

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += (sender, args) => Finish();
            LbStatus = FindViewById<TextView>(Resource.Id.status);
            _manualRegisterButton = FindViewById<Button>(Resource.Id.manual_register);
            _dataContainer = FindViewById<ConstraintLayout>(Resource.Id.dataContainer);
            _systole = FindViewById<TextView>(Resource.Id.SystoleTextView);
            _diastole = FindViewById<TextView>(Resource.Id.DiastoleTextView);
            _pulse = FindViewById<TextView>(Resource.Id.PulseTextView);
            //_scanButton = FindViewById<Button>(Resource.Id.ScanButton);
            AnimationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            var filter = new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.accent));
            AnimationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter, new LottieValueCallback(filter));
        }

        protected override void OnPostResume()
        {
            base.OnPostResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (_bluetoothAdapter != null)
            {
                BluetoothScanner?.StopScan(ScanCallback);
            }
        }

        public void OnAnimationCancel(Animator animation)
        {
            _dataContainer.Visibility = ViewStates.Visible;
            AnimationView.Progress = 1f;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode != 11) return;
            if (resultCode == Result.Ok)
            {
                BluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
                BluetoothScanner.StartScan(ScanCallback);
                //_scanButton.Enabled = false;
                AnimationView.PlayAnimation();
            }
            else
            {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
            }
        }

        public void OnAnimationEnd(Animator animation) { }

        public void OnAnimationRepeat(Animator animation) { }

        public void OnAnimationStart(Animator animation)
        {
            LbStatus.Text = "Se efectueaza masuratoarea...";

            if (_bluetoothManager == null) return;
            _bluetoothAdapter = _bluetoothManager.Adapter;
            if (!_bluetoothAdapter.IsEnabled)
            {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
            }
            else
            {
                BluetoothScanner = _bluetoothAdapter.BluetoothLeScanner;
                BluetoothScanner.StartScan(ScanCallback);
                //_scanButton.Enabled = false;
                _dataContainer.Visibility = ViewStates.Gone;
            }
        }
        internal async void UpdateUi(BloodPressureData data)
        {
                var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
                if (data != null)
                {
                    if (!Utils.CheckNetworkAvailability())
                    {
                        await _bleDevicesDataRecords.Insert(new DevicesRecords
                        {
                            Imei = _imei,
                            DateTime = ft.Format(new Date()),
                            BloodPresureSystolic = data.Systolic,
                            BloodPresureDiastolic = data.Diastolic,
                            BloodPresurePulsRate = data.PulseRate
                        });
                    }
                    else
                    {
                        JSONObject jsonObject;
                        var jsonArray = new JSONArray();
                        var list = await _bleDevicesDataRecords.QueryValuations("select * from DevicesRecords");
                        Log.Error("list", list.Count().ToString());

                        foreach (DevicesRecords el in list)
                        {
                            try
                            {
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
                            }
                            catch (JSONException e)
                            {
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
                            .Put("bloodPressureSystolic", data.Systolic)
                            .Put("bloodPressureDiastolic", data.Diastolic)
                            .Put("bloodPressurePulseRate", data.PulseRate)
                            .Put("bloodGlucose", string.Empty)
                            .Put("oxygenSaturation", string.Empty)
                            .Put("extension", string.Empty);
                        jsonArray.Put(jsonObject);
                        string result = await WebServices.WebServices.Post(Constants.SaveDeviceDataUrl, jsonArray);
                        if (result == "Succes!")
                        {
                            Toast.MakeText(this, "Succes", ToastLength.Long).Show();
                            await _bleDevicesDataRecords.DeleteAll();
                        }
                        else
                        {
                            Toast.MakeText(this, "" + result, ToastLength.Long).Show();
                        }
                    }
                }

            if (data != null)
            {
                _systole.Text = GetString(Resource.String.systole) + " " + data.Systolic + " mmHg";
                _diastole.Text = GetString(Resource.String.diastole) + " " + data.Diastolic + " mmHg";
                _pulse.Text = GetString(Resource.String.pulse) + " " + data.PulseRate + " b/min";
            }
            else
            {
                _systole.Text = string.Empty;
                _diastole.Text = string.Empty;
                _pulse.Text = string.Empty;
            }
            //ActivateScanButton();
        }

        //private void ActivateScanButton() {
        //    _scanButton.Enabled = true;
        //}
    }
}