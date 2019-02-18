using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;
using Java.Lang;
using Java.Text;
using Java.Util;
using Org.Json;
using SQLite;
using Environment = System.Environment;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.Devices.PressureDevice
{
    [Activity(Label = "BloodPressureDeviceActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class BloodPressureDeviceActivity : AppCompatActivity, Animator.IAnimatorListener
    {
        private BluetoothAdapter _bluetoothAdapter;
        private BluetoothLeScanner _bluetoothScanner;
        private BluetoothManager _bluetoothManager;
        private List<BloodPressureData> _data;
        private Handler _handler;
        private bool _send;

        private TextView _systole;
        private TextView _diastole;
        private TextView _pulse;
        private Button _scanButton;
        private TextView _lbStatus;
        private ConstraintLayout _dataContainer;


        //private ProgressDialog progressDialog;
        private LottieAnimationView _animationView;
        private BloodPressureDeviceActivity _context;

        private BluetoothScanCallback _scanCallback;
        private GattCallBack _gattCallback;
        private SqlHelper<DevicesRecords> _bleDevicesDataRecords;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_blood_pressure_device);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = "Tensiune";
            
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                Finish();
            };
//            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
//            var numeDb = "devices_data.db";
//            _db = new SQLiteAsyncConnection(Path.Combine(path, numeDb));
            _bleDevicesDataRecords = await SqlHelper<DevicesRecords>.CreateAsync();
            _lbStatus = FindViewById<TextView>(Resource.Id.status);
            _dataContainer = FindViewById<ConstraintLayout>(Resource.Id.dataContainer);
            _bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);
            _context = this;
            _scanCallback = new BluetoothScanCallback(_context);
            _gattCallback = new GattCallBack(_context);
            _dataContainer.Visibility = ViewStates.Gone;
            _systole = FindViewById<TextView>(Resource.Id.SystoleTextView);
            _diastole = FindViewById<TextView>(Resource.Id.DiastoleTextView);
            _pulse = FindViewById<TextView>(Resource.Id.PulseTextView);
            _scanButton = FindViewById<Button>(Resource.Id.ScanButton);

            _animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            _animationView.AddAnimatorListener(this);
            var filter =
                new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.accent));
            _animationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter,
                new LottieValueCallback(filter));
            _scanButton.Click += delegate
            {
                if (_bluetoothManager == null || _bluetoothAdapter == null) return;
                _bluetoothScanner.StartScan(_scanCallback);
                _scanButton.Enabled = false;
                _send = false;  
                _lbStatus.Text = "Se efectueaza masuratoarea...";
                _animationView.PlayAnimation();
            };
            _animationView.PlayAnimation();
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

        public void OnAnimationCancel(Animator animation)
        {
            _dataContainer.Visibility = ViewStates.Visible;
            _lbStatus.Text = "Masuratoare efecuata cu succes";
            _animationView.Progress = 1f;
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
            _data = new List<BloodPressureData>();

            _lbStatus.Text = "Se efectueaza masuratoarea...";

            //if (_bluetoothManager == null) return;
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

            }
        }

        private class BluetoothScanCallback : ScanCallback
        {
            private readonly Context _context;
            private SqlHelper<BluetoothDeviceRecords> _bleDevicesRecords;
            public BluetoothScanCallback(Context context)
            {
                _context = context;
            }

            public override async void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);
                _bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                var list = await _bleDevicesRecords.QueryValuations( "select * from BluetoothDeviceRecords");
                
                if(!(from c in list where c.Address == result.Device.Address 
                    select new {c.Name, c.Address, c.DeviceType}).Any()) 
                    return;
                result.Device.ConnectGatt(_context, false,
                    (_context as BloodPressureDeviceActivity)?._gattCallback);
                (_context as BloodPressureDeviceActivity)?._bluetoothScanner.StopScan(
                    ((BloodPressureDeviceActivity) _context)?._scanCallback);
                // progressDialog.setMessage(GetString(R.string.conectare_info));
                ((BloodPressureDeviceActivity) _context)._lbStatus.Text =
                    _context.GetString(Resource.String.conectare_info);
            }
        }


        private class GattCallBack : BluetoothGattCallback
        {
            private readonly Context _context;
            public GattCallBack( Context context)
            {
                _context = context;
            }

            public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
            {
                base.OnConnectionStateChange(gatt, status, newState);
                switch (newState)
                {
                    case ProfileState.Connected:
                        Log.Error("Gatt", "connected");
                        gatt.DiscoverServices();
                        break;
                    case ProfileState.Disconnected:
                    {
                        Log.Error("Gatt", "Disconnected");
                        (_context as BloodPressureDeviceActivity)?.RunOnUiThread(() =>
                            {
                                ((BloodPressureDeviceActivity) _context)._lbStatus.Text =
                                    _context.GetString(Resource.String.afisare_date_info);
                            });


                        gatt.Disconnect();
                        gatt.Close();

                        for (var i = 0; i < (_context as BloodPressureDeviceActivity)?._data.Count; i++)
                        {
                            if (((BloodPressureDeviceActivity) _context)?._data[i] == null)
                            {
                                ((BloodPressureDeviceActivity) _context)?._data.RemoveAt(i);
                            }
                        }

                        var result = (_context as BloodPressureDeviceActivity)?._data
                            .Where(e => e != null).ToList();

                        result?.Sort((p, q) => q.Data.CompareTo(p.Data));


                        (_context as BloodPressureDeviceActivity)?.RunOnUiThread(() =>
                        {
                            (_context as BloodPressureDeviceActivity)?._animationView
                                .CancelAnimation();
                            //}
                            if ((_context as BloodPressureDeviceActivity)?._data.Count > 0)
                            {
                                ((BloodPressureDeviceActivity) _context)?.UpdateUi(result[0]);
                            }
                        });
                        break;
                    }
                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
            {
                //base.OnServicesDiscovered(gatt, status);
                if (status != 0) return;
                (_context as BloodPressureDeviceActivity)?.RunOnUiThread(() =>
                    {
                        ((BloodPressureDeviceActivity) _context)._lbStatus.Text =
                            _context.GetString(Resource.String.primire_date_info);
                    });

                if (HasCurrentTimeService(gatt))
                {
                    Log.Error("TimkeCaract", "ghjk");
                    var timeCharacteristic =
                        gatt.GetService(UUID.FromString("00001805-0000-1000-8000-00805f9b34fb"))
                            .GetCharacteristic(
                                UUID.FromString("00002A2B-0000-1000-8000-00805f9b34fb"));
                    timeCharacteristic.SetValue(GetCurrentTimeLocal());
                    gatt.WriteCharacteristic(timeCharacteristic);
                }
                else
                {
                    (_context as BloodPressureDeviceActivity)?.ListenToMeasurements(gatt);
                }
            }

            public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
            {
                base.OnCharacteristicWrite(gatt, characteristic, status);
                if (status == GattStatus.Success)
                {
                    (_context as BloodPressureDeviceActivity)?.ListenToMeasurements(gatt);
                }
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                base.OnCharacteristicChanged(gatt, characteristic);
                var offset = 0;
                var flags = characteristic.GetIntValue(GattFormat.Uint8, offset++);
                // See BPMManagerCallbacks.UNIT_* for unit options
                var timestampPresent = ((int)flags & 0x02) > 0;
                var pulseRatePresent = ((int)flags & 0x04) > 0;

                // following bytes - systolic, diastolic and mean arterial pressure
                var systolic = characteristic.GetFloatValue(GattFormat.Sfloat, offset).FloatValue();
                var diastolic = characteristic.GetFloatValue(GattFormat.Sfloat, offset + 2)
                    .FloatValue();
                offset += 6;

                // parse timestamp if present
                Calendar calendar = null;
                if (timestampPresent)
                {
                    calendar = Calendar.Instance;
                    calendar.Set(CalendarField.Year,
                        characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue());
                    calendar.Set(CalendarField.Month,
                        characteristic.GetIntValue(GattFormat.Uint8, offset + 2).IntValue());
                    calendar.Set(CalendarField.DayOfMonth,
                        characteristic.GetIntValue(GattFormat.Uint8, offset + 3).IntValue());
                    calendar.Set(CalendarField.HourOfDay,
                        characteristic.GetIntValue(GattFormat.Uint8, offset + 4).IntValue());
                    calendar.Set(CalendarField.Minute,
                        characteristic.GetIntValue(GattFormat.Uint8, offset + 5).IntValue());
                    calendar.Set(CalendarField.Second,
                        characteristic.GetIntValue(GattFormat.Uint8, offset + 6).IntValue());
                    offset += 7;
                }

                // parse pulse rate if present
                var pulseRate = 0.0f;
                if (pulseRatePresent)
                {
                    pulseRate = characteristic.GetFloatValue(GattFormat.Sfloat, offset).FloatValue();
                }

                if (calendar != null)
                {
                    Log.Error("Data", "Sistola: " + systolic + ", Diastola: " + diastolic +
                                      ", Puls: " + pulseRate +
                                      ", Year: " + calendar.Get(CalendarField.Year) + ", Month: " +
                                      calendar.Get(CalendarField.Month) + ", Day: " +
                                      calendar.Get(CalendarField.DayOfMonth) +
                                      ", Hour: " + calendar.Get(CalendarField.HourOfDay) +
                                      ", Minute: " + calendar.Get(CalendarField.Minute) +
                                      ", Second: " + calendar.Get(CalendarField.Second));
                }

                (_context as BloodPressureDeviceActivity)?._data.Add(
                    new BloodPressureData(systolic, diastolic, pulseRate, calendar));
            }
        }


        private async void UpdateUi(BloodPressureData data)
        {
            if (!_send)
            {
                var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);

                //await _db.CreateTableAsync<DevicesRecords>();
                if (!Utils.CheckNetworkAvailability())
                {
                    await _bleDevicesDataRecords.Insert(new DevicesRecords()
                    {
                        Imei = Utils.GetImei(this),
                        DateTime = ft.Format(new Date()),
                        BloodPresureSystolic = data.Sys,
                        BloodPresureDiastolic = data.Dia,
                        BloodPresurePulsRate = data.Pul
                    });
                }
                    //AddBloodPressureRecord(_db, data.Sys, data.Dia,data.Pul);
                else
                {
                    JSONObject jsonObject;
                    var jsonArray = new JSONArray();
                    var list = await _bleDevicesDataRecords.QueryValuations("select * from DevicesRecords");

                    foreach (var el in list)
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
                        .Put("imei", Utils.GetImei(this))
                        .Put("dateTimeISO", ft.Format(new Date()))
                        .Put("geolocation", string.Empty)
                        .Put("lastLocation", string.Empty)
                        .Put("sendPanicAlerts", string.Empty)
                        .Put("stepCounter", string.Empty)
                        .Put("bloodPressureSystolic", data.Sys)
                        .Put("bloodPressureDiastolic", data.Dia)
                        .Put("bloodPressurePulseRate", data.Pul)
                        .Put("bloodGlucose", string.Empty)
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
                        Toast.MakeText(this, "" + result, ToastLength.Long).Show();
                    }

                }

            }

            var systole = GetString(Resource.String.systole) + data.Sys;
            var diastole = GetString(Resource.String.diastole) + data.Dia;
            var pulse = GetString(Resource.String.pulse) + data.Pul;
            _systole.Text = systole;
            _diastole.Text = diastole;
            _pulse.Text = pulse;

            ActivateScanButton();
        }

        
//        private async void AddBloodPressureRecord(SQLiteAsyncConnection db, float systolic, float diastolic, float pulsRate)
//        {
//            var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
//            await db.InsertAsync(new DevicesRecords()
//            {
//                Imei = Utils.GetImei(this),
//                DateTime = ft.Format(new Date()),
//                BloodPresureSystolic = systolic,
//                BloodPresureDiastolic = diastolic,
//                BloodPresurePulsRate = pulsRate
//            });
//        }

        private void ActivateScanButton()
        {
            _scanButton.Enabled = true;
        }

        private static bool HasCurrentTimeService(BluetoothGatt gatt)
        {
            return gatt.Services.Any(service =>
                service.Uuid.Equals(UUID.FromString("00001805-0000-1000-8000-00805f9b34fb")));
        }

        private static byte[] GetCurrentTimeLocal()
        {
            return GetCurrentTimeWithOffset();
        }

        private static byte[] GetCurrentTimeWithOffset()
        {
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
            if (dayOfWeek == 1)
            {
                dayOfWeek = 7;
            }
            else
            {
                dayOfWeek--;
            }
            time[7] = (byte)dayOfWeek;
            time[8] = 0;
            time[9] = 1;

            return time;
        }
        private void ListenToMeasurements(BluetoothGatt gatt)
        {
            SetCharacteristicNotification(gatt, Constants.UuidBloodPressureService,
                Constants.UuidBloodPressureMeasurementChar);
        }

        private void SetCharacteristicNotification(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid)
        {
            SetCharacteristicNotificationWithDelay(gatt, serviceUuid, characteristicUuid);

        }
        private void SetCharacteristicNotificationWithDelay(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid)
        {
            _handler.PostDelayed(() =>
            {
                SetCharacteristicNotification_private(gatt, serviceUuid, characteristicUuid);
            }, 300);
        }
        private static void SetCharacteristicNotification_private(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid)
        {
            try
            {
                var characteristic = gatt.GetService(serviceUuid).GetCharacteristic(characteristicUuid);
                gatt.SetCharacteristicNotification(characteristic, true);
                var descriptor = characteristic.GetDescriptor(Constants.ClientCharacteristicConfig);
                var indication = (Convert.ToInt32(characteristic.Properties) & 32) != 0;
                descriptor.SetValue(indication
                    ? BluetoothGattDescriptor.EnableIndicationValue.ToArray()
                    : BluetoothGattDescriptor.EnableNotificationValue.ToArray());

                gatt.WriteDescriptor(descriptor);
            }
            catch (System.Exception)
            {
                //e.PrintStackTrace();
            }
        }

    }


}