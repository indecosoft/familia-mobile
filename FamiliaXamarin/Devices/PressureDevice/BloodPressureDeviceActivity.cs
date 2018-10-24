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
using Android.Opengl;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Java.IO;
using Java.Lang;
using Java.Text;
using Java.Util;
using Org.Json;
using Debug = System.Diagnostics.Debug;
using File = Java.IO.File;
using IOException = Java.IO.IOException;
using Object = Java.Lang.Object;
using StringBuilder = Java.Lang.StringBuilder;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.PressureDevice
{
    [Activity(Label = "BloodPressureDeviceActivity", Theme = "@style/AppTheme.Dark")]
    public class BloodPressureDeviceActivity : AppCompatActivity, Animator.IAnimatorListener, IComparator
    {
        private BluetoothAdapter bluetoothAdapter;
        private BluetoothLeScanner bluetoothScanner;
        private BluetoothManager bluetoothManager;
        private List<BloodPressureData> data;
        private Handler handler;
        private bool send;

        private TextView Systole;
        private TextView Diastole;
        private TextView Pulse;
        private Button scanButton;
        private TextView lbStatus;
        private ConstraintLayout DataContainer;


        //private ProgressDialog progressDialog;
        private LottieAnimationView animationView;
        private BloodPressureDeviceActivity Context;

        private BluetoothScanCallback scanCallback;
        private GattCallBack gattCallback;


        protected override void OnCreate(Bundle savedInstanceState)
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

            lbStatus = FindViewById<TextView>(Resource.Id.status);
            DataContainer = FindViewById<ConstraintLayout>(Resource.Id.dataContainer);
            bluetoothManager = (BluetoothManager)GetSystemService(BluetoothService);
            Context = this;
            scanCallback = new BluetoothScanCallback(Context);
            gattCallback = new GattCallBack(Context);
            DataContainer.Visibility = ViewStates.Gone;
            Systole = FindViewById<TextView>(Resource.Id.SystoleTextView);
            Diastole = FindViewById<TextView>(Resource.Id.DiastoleTextView);
            Pulse = FindViewById<TextView>(Resource.Id.PulseTextView);
            scanButton = FindViewById<Button>(Resource.Id.ScanButton);

            animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            animationView.AddAnimatorListener(this);
            scanButton.Click += delegate
            {
                if (bluetoothManager != null)
                {
                    //bluetoothAdapter = bluetoothManager.Adapter;
                    if (bluetoothAdapter != null)
                    {
                        bluetoothScanner.StartScan(scanCallback);
                        scanButton.Enabled = false;
                        send = false;  
                        lbStatus.Text = "Se efectueaza masuratoarea...";
                        animationView.PlayAnimation();
                        //                        progressDialog.setMessage();
                        //
                        //                        progressDialog.show();
                    }
                }
            };
            animationView.PlayAnimation();
            // Create your application here
        }


        protected override void OnPostResume()
        {
            base.OnPostResume();
            send = false;
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (bluetoothAdapter != null)
            {
                if (bluetoothScanner != null)
                {
                    bluetoothScanner.StopScan(scanCallback);
                }
            }
        }

        public void OnAnimationCancel(Animator animation)
        {
            DataContainer.Visibility = ViewStates.Visible;
            lbStatus.Text = "Masuratoare efecuata cu succes";
            animationView.Progress = 1f;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 11)
            {
                if (resultCode == Result.Ok)
                {
                    bluetoothScanner = bluetoothAdapter.BluetoothLeScanner;
                    bluetoothScanner.StartScan(scanCallback);
                    scanButton.Enabled = false;
                    //progressDialog.show();
                    animationView.PlayAnimation();
                }
                else
                {
                    StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
                }
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
            handler = new Handler();
            data = new List<BloodPressureData>();

            lbStatus.Text = "Se efectueaza masuratoarea...";

            if (bluetoothManager != null)
            {
                bluetoothAdapter = bluetoothManager.Adapter;
                if (bluetoothAdapter != null)
                {
                    if (!bluetoothAdapter.IsEnabled)
                    {
                        StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
                    }
                    else
                    {
                        bluetoothScanner = bluetoothAdapter.BluetoothLeScanner;
                        bluetoothScanner.StartScan(scanCallback);
                        scanButton.Enabled = false;
                        //progressDialog.show();
                        DataContainer.Visibility = ViewStates.Gone;

                    }
                }
            }
        }

        private class BluetoothScanCallback : ScanCallback
        {
            private Context Context;
            public BluetoothScanCallback(Context context)
            {
                Context = context;
            }

            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);
                Log.Error("$$$$$$$$$$$$$$$$$", result.Device.Address);
                if (result.Device.Address != null && result.Device.Address.Equals(Utils.GetDefaults(Context.GetString(Resource.String.blood_pressure_device), Context)))
                {
                    result.Device.ConnectGatt(Context, false, (Context as BloodPressureDeviceActivity)?.gattCallback);
                    (Context as BloodPressureDeviceActivity)?.bluetoothScanner.StopScan((Context as BloodPressureDeviceActivity)?.scanCallback);
                    // progressDialog.setMessage(GetString(R.string.conectare_info));
                    ((BloodPressureDeviceActivity) Context).lbStatus.Text = Context.GetString(Resource.String.conectare_info);
                    
                }
            }
        }


        private class GattCallBack : BluetoothGattCallback
        {
            private Context Context;
            public GattCallBack( Context context)
            {
                Context = context;
            }

            public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
            {
                base.OnConnectionStateChange(gatt, status, newState);
                if (newState == ProfileState.Connected)
                {
                    Log.Error("Gatt", "connected");
                    gatt.DiscoverServices();
                }

                if (newState == ProfileState.Disconnected)
                {
                    Log.Error("Gatt", "Disconnected");
                    (Context as BloodPressureDeviceActivity)?.RunOnUiThread(() =>
                    {
                        ((BloodPressureDeviceActivity) Context).lbStatus.Text = Context.GetString(Resource.String.afisare_date_info);
                    });


                    gatt.Disconnect();
                    gatt.Close();

                    for (var i = 0; i < (Context as BloodPressureDeviceActivity)?.data.Count; i++)
                    {
                        if ((Context as BloodPressureDeviceActivity)?.data[i] == null)
                        {
                            (Context as BloodPressureDeviceActivity)?.data.RemoveAt(i);
                        }
                    }
                    //
                    //                    try
                    //                    {
                    //                        Collections.Sort(Context.data, Context);
                    //                    }
                    //                    catch (System.Exception e)
                    //                    {
                    //                        //e.PrintStackTrace();
                    //                    }
                    var result = (Context as BloodPressureDeviceActivity)?.data.Where(e => e != null).ToList();

                    result.Sort((p, q) => q.Data.CompareTo(p.Data));
                    //var q = result.OrderByDescending(e => e.Data).FirstOrDefault();

                    //var t = ;

                    (Context as BloodPressureDeviceActivity)?.RunOnUiThread(() =>
                    {
                        (Context as BloodPressureDeviceActivity)?.animationView.CancelAnimation();
                        //}
                        if ((Context as BloodPressureDeviceActivity)?.data.Count > 0)
                        {
                            (Context as BloodPressureDeviceActivity)?.UpdateUI(result[0]);
                        }
                    });
                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
            {
                //base.OnServicesDiscovered(gatt, status);
                if (status == 0)
                {
                    (Context as BloodPressureDeviceActivity)?.RunOnUiThread(() =>
                    {
                        ((BloodPressureDeviceActivity) Context).lbStatus.Text = Context.GetString(Resource.String.primire_date_info);
                    });

                    if (((BloodPressureDeviceActivity) Context).HasCurrentTimeService(gatt))
                    {
                        Log.Error("TimkeCaract", "ghjk");
                        BluetoothGattCharacteristic timeCharacteristic = gatt.GetService(UUID.FromString("00001805-0000-1000-8000-00805f9b34fb")).GetCharacteristic(UUID.FromString("00002A2B-0000-1000-8000-00805f9b34fb"));
                        timeCharacteristic.SetValue(GetCurrentTimeLocal());
                        gatt.WriteCharacteristic(timeCharacteristic);
                    }
                    else
                    {
                        (Context as BloodPressureDeviceActivity)?.ListenToMeasurements(gatt);
                    }
                }
            }

            public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
            {
                base.OnCharacteristicWrite(gatt, characteristic, status);
                if (status == GattStatus.Success)
                {
                    (Context as BloodPressureDeviceActivity)?.ListenToMeasurements(gatt);
                }
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                base.OnCharacteristicChanged(gatt, characteristic);
                int offset = 0;
                Integer flags = characteristic.GetIntValue(GattFormat.Uint8, offset++);
                // See BPMManagerCallbacks.UNIT_* for unit options
                bool timestampPresent = ((int)flags & 0x02) > 0;
                bool pulseRatePresent = ((int)flags & 0x04) > 0;

                // following bytes - systolic, diastolic and mean arterial pressure
                var systolic = characteristic.GetFloatValue(GattFormat.Sfloat, offset).FloatValue();
                var diastolic = characteristic.GetFloatValue(GattFormat.Sfloat, offset + 2).FloatValue();
                offset += 6;

                // parse timestamp if present
                Calendar calendar = null;
                if (timestampPresent)
                {
                    calendar = Calendar.Instance;
                    calendar.Set(CalendarField.Year, characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue());
                    calendar.Set(CalendarField.Month, characteristic.GetIntValue(GattFormat.Uint8, offset + 2).IntValue());
                    calendar.Set(CalendarField.DayOfMonth, characteristic.GetIntValue(GattFormat.Uint8, offset + 3).IntValue());
                    calendar.Set(CalendarField.HourOfDay, characteristic.GetIntValue(GattFormat.Uint8, offset + 4).IntValue());
                    calendar.Set(CalendarField.Minute, characteristic.GetIntValue(GattFormat.Uint8, offset + 5).IntValue());
                    calendar.Set(CalendarField.Second, characteristic.GetIntValue(GattFormat.Uint8, offset + 6).IntValue());
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
                    Log.Error("Data", "Sistola: " + systolic + ", Diastola: " + diastolic + ", Puls: " + pulseRate +
                            ", Year: " + calendar.Get(CalendarField.Year) + ", Month: " + calendar.Get(CalendarField.Month) + ", Day: " + calendar.Get(CalendarField.DayOfMonth) +
                            ", Hour: " + calendar.Get(CalendarField.HourOfDay) + ", Minute: " + calendar.Get(CalendarField.Minute) + ", Second: " + calendar.Get(CalendarField.Second));
                }
                (Context as BloodPressureDeviceActivity)?.data.Add(new BloodPressureData(systolic, diastolic, pulseRate, calendar));
            }
        }


        private void UpdateUI(BloodPressureData data)
        {
            if (!send)
            {
                SimpleDateFormat ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
                JSONObject jsonObject = new JSONObject();
                try
                {
                    jsonObject
                        .Put("imei", Utils.GetDefaults("imei", Context))
                        .Put("dateTimeISO", ft.Format(new Date()))
                        .Put("geolocation", "")
                        .Put("lastLocation", "")
                        .Put("sendPanicAlerts", "")
                        .Put("stepCounter", "")
                        .Put("bloodPressureSystolic", "" + data.Sys)
                        .Put("bloodPressureDiastolic", "" + data.Dia)
                        .Put("bloodPressurePulseRate", "" + data.Pul)
                        .Put("bloodGlucose", "")
                        .Put("oxygenSaturation", "")
                        .Put("extension", "");
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }

                WriteBloodPressureData(jsonObject);

                //TODO: Exporta la server datele
                //new BPM().execute("http://192.168.101.161:10000/data");
            }

            string systole = GetString(Resource.String.systole) + data.Sys;
            string diastole = GetString(Resource.String.diastole) + data.Dia;
            string pulse = GetString(Resource.String.pulse) + data.Pul;
            Systole.Text = systole;
            Diastole.Text = diastole;
            Pulse.Text = pulse;

            ActivateScanButton();
        }
        private void ActivateScanButton()
        {
            scanButton.Enabled = true;
        }

        protected bool HasCurrentTimeService(BluetoothGatt gatt)
        {
            foreach (BluetoothGattService service in gatt.Services)
            {
                if (service.Uuid.Equals(UUID.FromString("00001805-0000-1000-8000-00805f9b34fb")))
                {
                    return true;
                }
            }

            return false;
        }
        public static byte[] GetCurrentTimeLocal()
        {
            return GetCurrentTimeWithOffset(0);
        }

        public static byte[] GetCurrentTimeWithOffset(int offset)
        {
            Calendar now = Calendar.Instance;
            now.Time = new Date();
            now.Add(CalendarField.HourOfDay, 0);
            byte[] time = new byte[10];
            time[0] = (byte)((now.Get(CalendarField.Year) >> 8) & 255);
            time[1] = (byte)(now.Get(CalendarField.Year) & 255);
            time[2] = (byte)(now.Get(CalendarField.Month) + 1);
            time[3] = (byte)now.Get(CalendarField.DayOfMonth);
            time[4] = (byte)now.Get(CalendarField.HourOfDay);
            time[5] = (byte)now.Get(CalendarField.Minute);
            time[6] = (byte)now.Get(CalendarField.Second);
            int dayOfWeek = now.Get(CalendarField.DayOfWeek);
            if (dayOfWeek == 1)
            {
                dayOfWeek = 7;
            }
            else
            {
                dayOfWeek--;
            }
            time[7] = (byte)dayOfWeek;
            time[8] = (byte)0;
            time[9] = (byte)1;

            return time;
        }

        private void ListenToMeasurements(BluetoothGatt gatt)
        {
            SetCharacteristicNotification(gatt, Constants.UuidBloodPressureService, Constants.UuidBloodPressureMeasurementChar);
        }

        protected void SetCharacteristicNotification(BluetoothGatt gatt, UUID serviceUUID, UUID characteristicUUID)
        {
            SetCharacteristicNotificationWithDelay(gatt, serviceUUID, characteristicUUID);
//            if (bluetoothAdapter == null || gatt == null)
//            {
//                Log.Error("Null", "BluetoothAdapter not initialized");
//                return;
//            }
//            BluetoothGattCharacteristic characteristic = gatt.GetService(serviceUUID).GetCharacteristic(characteristicUUID);
//            gatt.SetCharacteristicNotification(characteristic, true);
//            if (A.equals(characteristic.getUuid()))
//            {
//                BluetoothGattDescriptor descriptor = characteristic.getDescriptor(UUID.fromString("00002902-0000-1000-8000-00805f9b34fb"));
//                descriptor.setValue(BluetoothGattDescriptor.ENABLE_NOTIFICATION_VALUE);
//                mBluetoothGatt.writeDescriptor(descriptor);
//            }

        }

        protected void SetCharacteristicNotificationWithDelay(BluetoothGatt gatt, UUID serviceUUID, UUID characteristicUUID)
        {

            handler.PostDelayed(() =>
            {
                setCharacteristicNotification_private(gatt, serviceUUID, characteristicUUID);
            }, 300);

        }

        private static void setCharacteristicNotification_private(BluetoothGatt gatt, UUID serviceUUID, UUID characteristicUUID)
        {
            try
            {
                bool indication;
                BluetoothGattCharacteristic characteristic = gatt.GetService(serviceUUID).GetCharacteristic(characteristicUUID);
                gatt.SetCharacteristicNotification(characteristic, true);
                BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(Constants.ClientCharacteristicConfig);
                //indication = ((int)characteristic.Properties. & 32) != 0;
                //indication = (characteristic.Properties & GattProperty.Read) != 0;
                indication = (Convert.ToInt32(characteristic.Properties) & 32) != 0;
                Log.Error("Indication", indication.ToString());
                if (indication)
                {
                    descriptor.SetValue(BluetoothGattDescriptor.EnableIndicationValue.ToArray());
                }
                else
                {
                    descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                }

                //                BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
                //                descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue as byte[]);
                //                //mBluetoothGatt.writeDescriptor(descriptor);

                gatt.WriteDescriptor(descriptor);
            }
            catch (System.Exception e)
            {
                //e.PrintStackTrace();
            }
        }

        public void WriteBloodPressureData(JSONObject jsonObject)
        {
            try
            {
                File file = new File(Context.FilesDir, Constants.BloodPressureFile);
                FileWriter fileWriter = new FileWriter(file, true);
                BufferedWriter bufferedWriter = new BufferedWriter(fileWriter);
                bufferedWriter.Write(jsonObject + ";");
                bufferedWriter.Close();
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }
        }

        public string readBloodPressureData()
        {
            Stream fis;
            try
            {
                fis = Context.OpenFileInput(Constants.BloodPressureFile);
                var isr = new InputStreamReader(fis);
                var bufferedReader = new BufferedReader(isr);
                var sb = new StringBuilder();
                string line;
                while ((line = bufferedReader.ReadLine()) != null)
                {
                    sb.Append(line);
                }
                fis.Close();
                return sb.ToString();
            }
            catch (Java.Lang.Exception e)
            {
                e.PrintStackTrace();
            }
            return null;
        }

        public void clearBloodPressureData()
        {
//            Stream fileOutputStream;
//
//            try
//            {
//                fileOutputStream = Context.OpenFileOutput(Constants.BloodPressureFile, FileCreationMode.Private);
//                byte[] arrayOfByte1 = Encoding.UTF8.GetBytes("");
//                fileOutputStream.Write( arrayOfByte1, 0, );
//                fileOutputStream.Close();
//            }
//            catch (Java.Lang.Exception e)
//            {
//                e.PrintStackTrace();
//            }
        }


        public int Compare(Object o1, Object o2)
        {
            var d1 = o1 as BloodPressureData;
            var d2 = o2 as BloodPressureData;

            Debug.Assert(d1 != null, nameof(d1) + " != null");
            return d1.Data.CompareTo(d2?.Data);
        }
    }


}