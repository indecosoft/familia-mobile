﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.DataModels;
using Familia.Devices.DevicesManagement;
using Familia.Devices.GlucoseDevice;
using Familia.Devices.Helpers;
using Familia.Devices.PressureDevice;
using Familia.Helpers;
using Familia.Location;
using Familia.Services;
using Java.Text;
using Java.Util;
using Newtonsoft.Json;
using Org.Json;
using Fragment = Android.Support.V4.App.Fragment;
using LocationEventArgs = Familia.Location.LocationEventArgs;

namespace Familia.Asistenta_sociala
{
    public class AsistentForm : Fragment
    {
        private EditText _tbDetails;
        private Button _btnScan, _btnAnulare, _btnBenefits, _btnBloodPressure, _btnBloodGlucose;
        private ConstraintLayout _formContainer;
        private string _dateTimeStart, _dateTimeEnd;
        private double _latitude, _longitude;
        private ProgressBarDialog _progressBarDialog;
        private JSONObject _location, _details, _qrJsonData;
        private JSONArray _benefitsArray;
        private List<SearchListModel> _selectedBenefits = new List<SearchListModel>();
        private Intent _distanceCalculatorService;
        private bool isPacientWithoutApp;
        LocationManager location = LocationManager.Instance;
        string readedQR;


        private void IntiUi(View v)
        {

            _tbDetails = v.FindViewById<EditText>(Resource.Id.input_details);
            _btnScan = v.FindViewById<Button>(Resource.Id.btnScan);
            _btnAnulare = v.FindViewById<Button>(Resource.Id.btnAnulare);
            _btnBloodPressure = v.FindViewById<Button>(Resource.Id.btnBloodPressure);

            _btnBloodGlucose = v.FindViewById<Button>(Resource.Id.btnBloodGlucose);

            _btnBenefits = v.FindViewById<Button>(Resource.Id.benefits_button);
            _formContainer = v.FindViewById<ConstraintLayout>(Resource.Id.container);

            _progressBarDialog = new ProgressBarDialog("Va rugam asteptati", "Datele sunt procesate...", Activity, false);
            _progressBarDialog.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimary);
        }

        private bool FieldsValidation()
        {
            return _selectedBenefits.Count > 0 && !string.IsNullOrEmpty(_tbDetails.Text);

        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.fragment_asistent_form, container, false);
            NotificationManagerCompat.From(Activity).Cancel(Constants.NotifMedicationId--);
            IntiUi(view);
            _distanceCalculatorService = new Intent(Activity, typeof(DistanceCalculator));
            //_medicalAsistanceService = new Intent(Activity, typeof(MedicalAsistanceService));

            string fromPreferences = Utils.GetDefaults("ActivityStart");
            string qrData = Utils.GetDefaults("QrCode");
            readedQR = Utils.GetDefaults("readedQR");
            _btnBloodPressure.Visibility = ViewStates.Gone;
            _btnBloodGlucose.Visibility = ViewStates.Gone;
            _tbDetails.TextChanged += delegate
            {

                _btnScan.Enabled = FieldsValidation();
            };
            _btnAnulare.Click += delegate
            {
                Utils.SetDefaults("ActivityStart", string.Empty);
                Utils.SetDefaults("QrId", string.Empty);
                Utils.SetDefaults("QrCode", string.Empty);
                Utils.SetDefaults("readedQR", string.Empty);


                _formContainer.Visibility = ViewStates.Gone;
                _btnAnulare.Visibility = ViewStates.Gone;
                _btnBloodPressure.Visibility = ViewStates.Gone;
                _btnBloodGlucose.Visibility = ViewStates.Gone;

                _btnScan.Text = "Incepe activitatea";
                Activity.StopService(_distanceCalculatorService);
                _selectedBenefits.Clear();
                _tbDetails.Text = string.Empty;
                _btnBenefits.Text = "Selecteaza beneficii";
                _btnScan.Enabled = true;
            };
            _btnBloodPressure.Click += _btnBloodPressure_Click;
            _btnBloodGlucose.Click += _btnBloodGlucose_Click;

            _btnBenefits.Click += OpenDiseaseList;


            if (string.IsNullOrEmpty(fromPreferences) || string.IsNullOrEmpty(qrData))
            {
                _formContainer.Visibility = ViewStates.Gone;
                _btnAnulare.Visibility = ViewStates.Gone;
                _btnBloodPressure.Visibility = ViewStates.Gone;
                _btnBloodGlucose.Visibility = ViewStates.Gone;

                _btnScan.Text = "Incepe activitatea";
            }
            else
            {
                _btnAnulare.Visibility = ViewStates.Visible;
                _btnBloodPressure.Visibility = ViewStates.Visible;
                _btnBloodGlucose.Visibility = ViewStates.Visible;
                _qrJsonData = new JSONObject(qrData);
                _formContainer.Visibility = ViewStates.Visible;
                try
                {
                    _location = new JSONObject(new JSONObject(fromPreferences).GetString("Location"));
                    _dateTimeStart = new JSONObject(fromPreferences).GetString("Start");
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
                _btnScan.Text = "Finalizeaza activitatea";
                _btnScan.Enabled = false;
                Intent distanceIntent = new Intent(Activity, typeof(DistanceCalculator));
                if (_qrJsonData.Has("latitude") && _qrJsonData.Has("longitudine"))
                {
                    distanceIntent.PutExtra("Latitude", _qrJsonData.GetString("latitude"));
                    distanceIntent.PutExtra("Longitude", _qrJsonData.GetString("longitudine"));
                }
                else
                {
                    distanceIntent.PutExtra("Latitude", _location.GetString("latitude"));
                    distanceIntent.PutExtra("Longitude", _location.GetString("longitude"));
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Activity.StartForegroundService(distanceIntent);
                }
                else
                {
                    Activity.StartService(distanceIntent);
                }
            }

            _btnScan.Click += BtnScan_Click;
            //_isGooglePlayServicesInstalled = Utils.IsGooglePlayServicesInstalled(Activity);


            //if (!_isGooglePlayServicesInstalled) return view;
            //var locationRequest = new LocationRequest();
            //locationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            //locationRequest.SetInterval(1000);
            //locationRequest.SetFastestInterval(1000);
            //_ = new FusedLocationProviderCallback(this);

            //_fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(Activity);


            return view;
        }
        private async void StartNewActivity(Type activity, DeviceType deviceType)
        {
            if (readedQR is null) return;
            var bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
            var list = await bleDevicesRecords.QueryValuations("SELECT * FROM BluetoothDeviceRecords");
            if (list.Where(d => d.DeviceType == deviceType).Count() > 0)
            {
                var intent = new Intent(Activity, activity);
                intent.PutExtra("Data", readedQR);
                Activity.StartActivity(intent);
            }
            else
            {
                using AlertDialog alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dialog).Create();
                alertDialog.SetTitle("Avertisment");

                alertDialog.SetMessage("Nu aveti niciun dispozitiv inregistrat!");
                alertDialog.SetButton("OK", delegate { Activity.StartActivity(typeof(DevicesManagementActivity)); });
                alertDialog.Show();
            }
        }

        private void _btnBloodGlucose_Click(object sender, EventArgs e)
        {
		    StartNewActivity(typeof(GlucoseDeviceActivity), DeviceType.Glucose);
        }

        private void _btnBloodPressure_Click(object sender, EventArgs e)
        {
            StartNewActivity(typeof(BloodPressureDeviceActivity), DeviceType.BloodPressure);
        }

        private async void OpenDiseaseList(object sender, EventArgs e)
        {
            _progressBarDialog.Show();
            await Task.Run(async () =>
            {
                try
                {
                    //Utils.GetDefaults("QrId")
                    // string response = await WebServices.WebServices.Get($"{Constants.PublicServerAddress}/api/getUserBenefits/{Utils.GetDefaults("Id")}", Utils.GetDefaults("Token"));
                    string response = await WebServices.WebServices.Get($"{Constants.PublicServerAddress}/api/getAllBenefits/", Utils.GetDefaults("Token"));
                    Log.Error("Debug Log in " + nameof(AsistentForm), "Response: " + response);
                    var jsonResponse = new JSONObject(response);
                    Log.Error("ASISTEN FORM BENEFITS", jsonResponse.ToString());
                    if (jsonResponse.GetInt("status") == 2)
                    {
                        JSONArray dataArray = jsonResponse.GetJSONArray("data");
                        var items = new List<SearchListModel>();
                        for (var i = 0; i < dataArray.Length(); i++)
                        {
                            items.Add(new SearchListModel
                            {
                                Id = dataArray.GetJSONObject(i).GetInt("id"),
                                Title = dataArray.GetJSONObject(i).GetString("benefit")
                            });
                        }

                        var intent = new Intent(Activity, typeof(SearchListActivity));
                        intent.PutExtra("Items", JsonConvert.SerializeObject(items));
                        intent.PutExtra("SelectedItems", JsonConvert.SerializeObject(_selectedBenefits));
                        StartActivityForResult(intent, 1);

                    }
                }
                catch (Exception ex)
                {
                    Log.Error("error la beneficii", ex.Message);
                }
            });
            _progressBarDialog.Dismiss();


        }

        private async void BtnScan_Click(object sender, EventArgs e)
        {
            using var sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
            string currentDateandTime = sdf.Format(new Date());
            Date dateTimeNow = sdf.Parse(currentDateandTime);
            var result = await Utils.ScanQrCode(Activity);
            if (result is null) return;
            readedQR = result.Text;
            Utils.SetDefaults("readedQR", readedQR);
            if (Utils.isJson(readedQR))
            {
                Log.Error("QrCode", "is json");
                //Utils.SetDefaults("QrId", _qrJsonData.GetInt("Id").ToString());
                isPacientWithoutApp = true;
                _qrJsonData = new JSONObject(readedQR);
                Utils.SetDefaults("QrCode", _qrJsonData.ToString());

                await CheckActivity(currentDateandTime);

            } else
            {
                isPacientWithoutApp = false;

                _qrJsonData = new JSONObject(Encryption.Decrypt(readedQR));
                if (_qrJsonData is null) return;
                try
                {

                    try
                    {
                        Log.Error("QrCode", _qrJsonData.ToString());
                        Utils.SetDefaults("QrCode", _qrJsonData.ToString());
                        Utils.SetDefaults("QrId", _qrJsonData.GetInt("Id").ToString());
                        string expDateTime = _qrJsonData.GetString("expirationDateTime");
                        Date dateTimeExp = sdf.Parse(expDateTime);
                        if (dateTimeExp.After(dateTimeNow))
                        {

                            await CheckActivity(currentDateandTime);
                        }
                        else
                        {
                            Snackbar.Make(_formContainer, "QRCode expirat! Va rugam sa generati alt cod QR!", Snackbar.LengthLong).Show();
                            _progressBarDialog.Dismiss();
                        }

                    }
                    catch (JSONException)
                    {
                        Snackbar.Make(_formContainer, "QRCode invalid!", Snackbar.LengthLong).Show();
                        _progressBarDialog.Dismiss();

                    }


                }
                catch (Exception)
                {
                    _progressBarDialog.Dismiss();
                }

            }
        }

        private async Task CheckActivity(string start)
        {
            if (_btnScan.Text.Equals("Incepe activitatea"))
            {
                _formContainer.Visibility = ViewStates.Visible;

                _btnScan.Enabled = false;
                _btnAnulare.Visibility = ViewStates.Visible;
                _btnBloodPressure.Visibility = ViewStates.Visible;
                _btnBloodGlucose.Visibility = ViewStates.Visible;
                try
                {
                    _progressBarDialog.Show();
                    _dateTimeStart = start;
                    _dateTimeEnd = null;
                    _details = null;

                    location.LocationRequested += LocationRequested;
                    await location.StartRequestingLocation();


                    _progressBarDialog.Dismiss();
                }
                catch (JSONException ex)
                {
                    ex.PrintStackTrace();
                }
            }
            else
            {
                _formContainer.Visibility = ViewStates.Gone;
                _btnAnulare.Visibility = ViewStates.Gone;
                Utils.SetDefaults("QrId", string.Empty);

                try
                {

                    if (ContextCompat.CheckSelfPermission(Activity,
                            Manifest.Permission.AccessFineLocation) != Permission.Granted) return;
                    location.LocationRequested += LocationRequested;
                    await location.StartRequestingLocation();

                }
                catch (JSONException ex)
                {
                    ex.PrintStackTrace();
                }
            }
        }

        private async void LocationRequested(object source, EventArgs args)
        {
            _progressBarDialog.Show();
            Log.Error("Latitude tst", ((LocationEventArgs)args).Location.Latitude.ToString());
            Log.Error("Longitude tst", ((LocationEventArgs)args).Location.Longitude.ToString());
            using var locationObj = new JSONObject();
            locationObj.Put("latitude", ((LocationEventArgs)args).Location.Latitude);
            locationObj.Put("longitude", ((LocationEventArgs)args).Location.Longitude);

            if (_btnScan.Text.Equals("Incepe activitatea"))
            {
                Log.Error("Asist", "Start");
                await location.StopRequestionLocationUpdates();
                location.LocationRequested -= LocationRequested;

                _btnScan.Text = "Finalizeaza activitatea";
                JSONObject obj = new JSONObject().Put("QRData", _qrJsonData.ToString()).Put("Start", _dateTimeStart).Put("Location", locationObj.ToString());
                Utils.SetDefaults("ActivityStart", obj.ToString());

                Intent distanceIntent = new Intent(Activity, typeof(DistanceCalculator));
                if(_qrJsonData.Has("latitude") && _qrJsonData.Has("longitudine"))
                {
                    distanceIntent.PutExtra("Latitude", _qrJsonData.GetString("latitude"));
                    distanceIntent.PutExtra("Longitude", _qrJsonData.GetString("longitudine"));
                } else
                {
                    distanceIntent.PutExtra("Latitude", locationObj.GetString("latitude"));
                    distanceIntent.PutExtra("Longitude", locationObj.GetString("longitude"));
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Activity.StartForegroundService(distanceIntent);
                }
                else
                {
                    Activity.StartService(distanceIntent);
                }

            }
            else
            {
                Log.Error("Asist", "Stop");

                await location.StopRequestionLocationUpdates();
                location.LocationRequested -= LocationRequested;
                Utils.SetDefaults("ActivityStart", "");
                using var sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
                string currentDateandTime = sdf.Format(new Date());
                _dateTimeEnd = currentDateandTime;
                _btnScan.Text = "Incepe activitatea";
                _benefitsArray = new JSONArray();
                foreach (SearchListModel t in _selectedBenefits)
                    _benefitsArray.Put(t.Id);

                _details = new JSONObject().Put("benefit", _benefitsArray).Put("details", _tbDetails.Text);
                Log.Error("Details", _details.ToString());
                Activity.StopService(_distanceCalculatorService);
                _tbDetails.Text = string.Empty;
                _selectedBenefits.Clear();
                _btnScan.Enabled = true;
                _btnBenefits.Text = "Selecteaza beneficii";
                //await Task.Run(async () =>
                //{
                Log.Error("before send", _qrJsonData.ToString());

                if (isPacientWithoutApp)
                    {
                        JSONObject dataToSend = new JSONObject().Put("dateTimeStart", _dateTimeStart)
                        .Put("dateTimeStop", _dateTimeEnd)
                        .Put("imei", _qrJsonData.GetString("deviceId"))
                        .Put("location", locationObj).Put("details", _details);
                    string response = await WebServices.WebServices.Post(Constants.PublicServerAddress + "/api/consultByImei", dataToSend, Utils.GetDefaults("Token"));
                    if (response != null)
                    {
                        var responseJson = new JSONObject(response);
                        switch (responseJson.GetInt("status"))
                        {
                            case 0:
                                Snackbar.Make(_formContainer, "Nu sunteti la pacient!", Snackbar.LengthLong).Show();
                                break;
                            case 1:
                                Snackbar.Make(_formContainer, "Eroare conectare la server", Snackbar.LengthLong).Show();
                                break;
                            case 2:
                                break;
                        }
                        //Activity.RunOnUiThread(_progressBarDialog.Dismiss);
                    }
                    else
                        Snackbar.Make(_formContainer, "Nu se poate conecta la server!", Snackbar.LengthLong).Show();
                    } else
                    {
                        JSONObject dataToSend = new JSONObject().Put("dateTimeStart", _dateTimeStart)
                        .Put("dateTimeStop", _dateTimeEnd).Put("qrCodeData", _qrJsonData)
                        .Put("location", locationObj).Put("details", _details);
                        string response = await WebServices.WebServices.Post(Constants.PublicServerAddress + "/api/consult", dataToSend, Utils.GetDefaults("Token"));
                        if (response != null)
                        {
                            var responseJson = new JSONObject(response);
                            switch (responseJson.GetInt("status"))
                            {
                                case 0:
                                    Snackbar.Make(_formContainer, "Nu sunteti la pacient!", Snackbar.LengthLong).Show();
                                    break;
                                case 1:
                                    Snackbar.Make(_formContainer, "Eroare conectare la server", Snackbar.LengthLong).Show();
                                    break;
                                case 2:
                                    break;
                            }
                            //Activity.RunOnUiThread(_progressBarDialog.Dismiss);
                        }
                        else
                            Snackbar.Make(_formContainer, "Nu se poate conecta la server!", Snackbar.LengthLong).Show();
                    }

                   
                    
                //});
             

            }
            _progressBarDialog.Dismiss();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == (int)Result.Ok)
            {
                _selectedBenefits = JsonConvert.DeserializeObject<List<SearchListModel>>(data.GetStringExtra("result"));
                _btnScan.Enabled = FieldsValidation();
                _btnBenefits.Text = $"Ati Selectat {_selectedBenefits.Count} beneficii";
            }
            else
            {
                Log.Error("Nu avem result", "User-ul a zis CANCEL");
                _btnBenefits.Text = "Selectati beneficii";
                _selectedBenefits.Clear();
            }
        }

    }

}