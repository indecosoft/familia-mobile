﻿using System;
using System.Collections.Generic;
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
        private Button _btnScan, _btnAnulare, _btnBenefits;
        private ConstraintLayout _formContainer;
        private string _dateTimeStart, _dateTimeEnd;
        private double _latitude, _longitude;
        private ProgressBarDialog _progressBarDialog;
        private JSONObject _location, _details, _qrJsonData;
        private JSONArray _benefitsArray;
        private List<SearchListModel> _selectedBenefits = new List<SearchListModel>();
        private Intent _distanceCalculatorService;
        LocationManager location = LocationManager.Instance;


        private void IntiUi(View v)
        {

            _tbDetails = v.FindViewById<EditText>(Resource.Id.input_details);
            _btnScan = v.FindViewById<Button>(Resource.Id.btnScan);
            _btnAnulare = v.FindViewById<Button>(Resource.Id.btnAnulare);
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
            _tbDetails.TextChanged += delegate
            {

                _btnScan.Enabled = FieldsValidation();
            };
            _btnAnulare.Click += delegate
            {
                Utils.SetDefaults("ActivityStart", "");
                Utils.SetDefaults("QrId", "");

                _formContainer.Visibility = ViewStates.Gone;
                _btnAnulare.Visibility = ViewStates.Gone;
                _btnScan.Text = "Incepe activitatea";
                Activity.StopService(_distanceCalculatorService);
                _selectedBenefits.Clear();
                _tbDetails.Text = string.Empty;
                _btnBenefits.Text = "Selecteaza beneficii";
                _btnScan.Enabled = true;
            };

            _btnBenefits.Click += OpenDiseaseList;


            if (string.IsNullOrEmpty(fromPreferences))
            {
                _formContainer.Visibility = ViewStates.Gone;
                _btnAnulare.Visibility = ViewStates.Gone;

                _btnScan.Text = "Incepe activitatea";
            }
            else
            {
                _btnAnulare.Visibility = ViewStates.Visible;
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
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Activity.StartForegroundService(new Intent(Activity, typeof(DistanceCalculator)));
                }
                else
                {
                    Activity.StartService(new Intent(Activity, typeof(DistanceCalculator)));
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

        private async void OpenDiseaseList(object sender, EventArgs e)
        {
            _progressBarDialog.Show();
            await Task.Run(async () =>
            {
                try
                {
                    //Utils.GetDefaults("QrId")
                    // string response = await WebServices.WebServices.Get($"{Constants.PublicServerAddress}/api/getUserBenefits/{Utils.GetDefaults("Id")}", Utils.GetDefaults("Token"));
                    string response = await WebServices.WebServices.Get($"{Constants.PublicServerAddress}/api/getUserBenefits/{Utils.GetDefaults("QrId")}", Utils.GetDefaults("Token"));
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
            _qrJsonData = await Utils.ScanEncryptedQrCode(Activity);
            if (_qrJsonData is null) return;
            try
            {
                using var sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
                string currentDateandTime = sdf.Format(new Date());
                Date dateTimeNow = sdf.Parse(currentDateandTime);
                try
                {
                    Log.Error("QrCode", _qrJsonData.ToString());
                    Utils.SetDefaults("QrId", _qrJsonData.GetInt("Id").ToString());
                    string expDateTime = _qrJsonData.GetString("expirationDateTime");
                    Date dateTimeExp = sdf.Parse(expDateTime);
                    if (dateTimeExp.After(dateTimeNow))
                    {

                        if (_btnScan.Text.Equals("Incepe activitatea"))
                        {
                            _formContainer.Visibility = ViewStates.Visible;

                            _btnScan.Enabled = false;
                            _btnAnulare.Visibility = ViewStates.Visible;
                            try
                            {
                                _progressBarDialog.Show();
                                _dateTimeStart = currentDateandTime;
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
                                _progressBarDialog.Show();
                                
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

        private async void LocationRequested(object source, EventArgs args)
        {

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

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Activity.StartForegroundService(_distanceCalculatorService);
                }
                else
                {
                    Activity.StartService(_distanceCalculatorService);
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
                await Task.Run(async () =>
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
                        Activity.RunOnUiThread(_progressBarDialog.Dismiss);
                    }
                    else
                        Snackbar.Make(_formContainer, "Nu se poate conecta la server!", Snackbar.LengthLong).Show();
                });
                Activity.StopService(_distanceCalculatorService);
                _tbDetails.Text = string.Empty;
                _selectedBenefits.Clear();
                _btnScan.Enabled = true;
                _btnBenefits.Text = "Selecteaza beneficii";

            }


            //if (Utils.IsServiceRunning(typeof(DistanceCalculator), Activity))
            //{
            //    await location.StopRequestionLocationUpdates();
            //    location.LocationRequested -= LocationRequested;
            //    return;
            //}

            //_location = new JSONObject().Put("latitude", _latitude).Put("longitude", _longitude);
            //_benefitsArray = new JSONArray();
            //foreach (SearchListModel t in _selectedBenefits)
            //    _benefitsArray.Put(t.Id);

            //_details = new JSONObject().Put("benefit", _benefitsArray).Put("details", _tbDetails.Text);
            //Log.Error("Details", _details.ToString());

            //await Task.Run(async () =>
            //{
            //    _latitude = double.Parse(Utils.GetDefaults("Latitude"));
            //    _longitude = double.Parse(Utils.GetDefaults("Longitude"));

            //    JSONObject dataToSend = new JSONObject().Put("dateTimeStart", _dateTimeStart)
            //        .Put("dateTimeStop", _dateTimeEnd).Put("qrCodeData", _qrJsonData)
            //        .Put("location", _location).Put("details", _details);
            //    string response = await WebServices.WebServices.Post(Constants.PublicServerAddress + "/api/consult", dataToSend, Utils.GetDefaults("Token"));
            //    if (response != null)
            //    {
            //        var responseJson = new JSONObject(response);
            //        switch (responseJson.GetInt("status"))
            //        {
            //            case 0:
            //                Snackbar.Make(_formContainer, "Nu sunteti la pacient!", Snackbar.LengthLong).Show();
            //                break;
            //            case 1:
            //                Snackbar.Make(_formContainer, "Eroare conectare la server", Snackbar.LengthLong).Show();
            //                break;
            //            case 2:
            //                break;
            //        }
            //        Activity.RunOnUiThread(_progressBarDialog.Dismiss);
            //    }
            //    else
            //        Snackbar.Make(_formContainer, "Nu se poate conecta la server!", Snackbar.LengthLong).Show();
            //});
            //Activity.StopService(_distanceCalculatorService);
            //_tbDetails.Text = string.Empty;
            //_selectedBenefits.Clear();
            //_btnScan.Enabled = true;
            //_btnBenefits.Text = "Selecteaza beneficii";
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