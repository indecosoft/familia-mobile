using System;
using System.IO;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using FamiliaXamarin.Helpers;
using Java.IO;
using Java.Text;
using Java.Util;
using Org.Json;
using File = Java.IO.File;
using IOException = Java.IO.IOException;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.GlucoseDevice
{
    [Activity(Label = "GlucoseDeviceActivity", Theme = "@style/AppTheme.Dark")]
    public class GlucoseDeviceActivity : AppCompatActivity , Animator.IAnimatorListener
    {

        BluetoothAdapter bluetoothAdapter;
        BluetoothLeScanner bluetoothScanner;
        BluetoothManager bluetoothManager;


        Handler handler;
        bool send = false;

        TextView glucose;
        Button scanButton;

        // private ProgressDialog progressDialog;
        TextView lbStatus;
        ConstraintLayout DataContainer;

        //private ProgressDialog progressDialog;
        LottieAnimationView animationView;
        GlucoseDeviceActivity Context;
        BluetoothScanCallback scanCallback;
        GattCallBack gattCallback;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.blood_glucose_device);
            Toolbar toolbar = (Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title ="Glicemie";

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

            glucose = FindViewById<TextView>(Resource.Id.GlucoseTextView);
            scanButton = FindViewById<Button>(Resource.Id.ScanButton);

            animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            animationView.AddAnimatorListener(this);
            animationView.PlayAnimation();


            scanButton.Click += delegate
            {
                if (bluetoothManager != null)
                {
                    if (bluetoothAdapter != null)
                    {
                        bluetoothScanner.StartScan(scanCallback);
                        scanButton.Enabled =false;
                        send = false;

                        lbStatus.Text = "Se efectueaza masuratoarea...";
                        animationView.PlayAnimation();
                        //                        progressDialog.setMessage(getString(R.string.conectare_info));
                        //                        progressDialog.show();
                    }
                }
            }; 

        // Create your application here
    }

        public void OnAnimationCancel(Animator animation)
        {
            DataContainer.Visibility = ViewStates.Visible;
            lbStatus.Text = "Masuratoare efecuata cu succes";
            animationView.Progress = 1f;
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
                        DataContainer.Visibility = ViewStates.Gone;
                        //progressDialog.show();
                    }
                }
            }

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
                bluetoothScanner?.StopScan(scanCallback);
            }

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode != 11) return;
            if (resultCode == Result.Ok)
            {
                bluetoothScanner = bluetoothAdapter.BluetoothLeScanner;
                bluetoothScanner.StartScan(scanCallback);
                scanButton.Enabled = false;
                animationView.PlayAnimation();

            }
            else
            {
                StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), 11);
            }
        }

        class BluetoothScanCallback : ScanCallback
        {
            Context Context;
            public BluetoothScanCallback(Context context)
            {
                Context = context;
            }

            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);
                if (result.Device.Address == null ||
                    !result.Device.Address.Equals(
                        Utils.GetDefaults(Context.GetString(Resource.String.blood_glucose_device), Context))) return;
                result.Device.ConnectGatt(Context, false, ((GlucoseDeviceActivity)Context).gattCallback);
                ((GlucoseDeviceActivity)Context).bluetoothScanner.StopScan(((GlucoseDeviceActivity)Context).scanCallback);
                ((GlucoseDeviceActivity)Context).lbStatus.Text = Context.GetString(Resource.String.conectare_info);
            }
        }
        class GattCallBack : BluetoothGattCallback
        {
            Context Context;
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
                    (Context as GlucoseDeviceActivity)?.SetCharacteristicNotification(gatt, Constants.UuidGlucServ, Constants.UuidGlucMeasurementChar);
                }
            }

            public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status)
            {
                base.OnDescriptorWrite(gatt, descriptor, status);
                if (status != 0) return;
                if (descriptor.Characteristic.Uuid.Equals(Constants.UuidGlucMeasurementContextChar))
                {
                    (Context as GlucoseDeviceActivity)?.SetCharacteristicNotification(gatt, Constants.UuidGlucServ, Constants.UuidGlucRecordAccessControlPointChar);
                    Log.Error("Aici", "1");
                }
                if (descriptor.Characteristic.Uuid.Equals(Constants.UuidGlucMeasurementChar))
                {
                    (Context as GlucoseDeviceActivity)?.SetCharacteristicNotification(gatt, Constants.UuidGlucServ, Constants.UuidGlucRecordAccessControlPointChar);
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
                record.Time = calendar;

                if (timeOffsetPresent)
                {
                    record.TimeOffset = characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
                    offset += 2;
                }

                if (typeAndLocationPresent)
                {
                    record.GlucoseConcentration = characteristic.GetFloatValue(GattFormat.Sfloat, offset).FloatValue();
                    record.Unit = concentrationUnit;
                    var typeAndLocation = characteristic.GetIntValue(GattFormat.Uint8, offset + 2).IntValue();
                    record.Type = (typeAndLocation & 0xF0) >> 4;
                    record.SampleLocation = (typeAndLocation & 0x0F);
                    offset += 3;

                    (Context as GlucoseDeviceActivity)?.RunOnUiThread(() =>
                    {
                        (Context as GlucoseDeviceActivity)?.animationView.CancelAnimation();
                        (Context as GlucoseDeviceActivity)?.UpdateUi(record.GlucoseConcentration * 100000);
                    });
                }

                if (sensorStatusAnnunciationPresent)
                {
                    record.Status = characteristic.GetIntValue(GattFormat.Uint16, offset).IntValue();
                }
            }

        }
        void UpdateUi(float g)
        {
            Log.Error("UpdateUI", "Aici");
            if (!send)
            {
                var ft = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.Uk);
                var jsonObject = new JSONObject();
                try
                {
                    jsonObject
                        .Put("imei", Utils.GetDefaults("imei", this))
                        .Put("dateTimeISO", ft.Format(new Date()))
                        .Put("geolocation", "")
                        .Put("lastLocation", "")
                        .Put("sendPanicAlerts", "")
                        .Put("stepCounter", "")
                        .Put("bloodPressureSystolic", "")
                        .Put("bloodPressureDiastolic", "")
                        .Put("bloodPressurePulseRate", "")
                        .Put("bloodGlucose", "" + (int)g)
                        .Put("oxygenSaturation", "")
                        .Put("extension", "");
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }

                WriteBloodGlucoseData(jsonObject);

                //new BGM().execute(Constants.DATA_URL);
            }

            var gl = GetString(Resource.String.glucose) + (int)g;
            glucose.Text = gl;
            ActivateScanButton();
        }
        public void WriteBloodGlucoseData(JSONObject jsonObject)
        {
            try
            {
                File file = new File(Context.FilesDir, Constants.BloodGlucoseFile);
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

        public string ReadBloodGlucoseData()
        {
            Stream fis;
            try
            {
                fis = Context.OpenFileInput(Constants.BloodGlucoseFile);
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

        public void ClearBloodGlucoseData()
        {
//            Stream fileOutputStream;
//
//            try
//            {
//                fileOutputStream = Application.Context.OpenFileOutput(Constants.BloodGlucoseFile, FileCreationMode.Private);
//                fileOutputStream.Write("".getBytes());
//                fileOutputStream.Close();
//            }
//            catch (Exception e)
//            {
//                e.PrintStackTrace();
//            }
        }
        void ActivateScanButton()
        {
            scanButton.Enabled = true;
        }

        protected void SetCharacteristicNotification(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid)
        {
            SetCharacteristicNotificationWithDelay(gatt, serviceUuid, characteristicUuid);
        }

        protected void SetCharacteristicNotificationWithDelay(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid)
        {
            handler.PostDelayed(
                () => { SetCharacteristicNotification_private(gatt, serviceUuid, characteristicUuid); }, 300);
        }

        void SetCharacteristicNotification_private(BluetoothGatt gatt, UUID serviceUuid, UUID characteristicUuid)
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