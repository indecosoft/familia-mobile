using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Location;
using FamiliaXamarin.Services;
using Java.Text;
using Java.Util;
using Org.Json;
using ZXing.Mobile;

namespace FamiliaXamarin.Asistenta_sociala
{
    public class AsistentForm : Android.Support.V4.App.Fragment
    {
        private FusedLocationProviderClient _fusedLocationProviderClient;
        private bool _isGooglePlayServicesInstalled;
        private Spinner _benefitsSpinner;
        private EditText _tbDetails;
        private Button _btnScan;
        private ConstraintLayout _formContainer;
        private string _dateTimeStart, _dateTimeEnd;
        private double _latitude, _longitude;
        private ProgressBarDialog _progressBarDialog;
        private JSONObject _location, _details, _qrJsonData;
        private JSONArray _benefitsArray;
        private List<BenefitSpinnerState> _listVOs;
        private Intent _distanceCalculatorService;
        private Intent _medicalAsistanceService;

        private void IntiUi(View v)
        {
            _benefitsSpinner = v.FindViewById<Spinner>(Resource.Id.benefits_spinner);
            _tbDetails = v.FindViewById<EditText>(Resource.Id.input_details);
            _btnScan = v.FindViewById<Button>(Resource.Id.btnScan);
            _formContainer = v.FindViewById<ConstraintLayout>(Resource.Id.container);


            _progressBarDialog = new ProgressBarDialog("Va rugam asteptati", "Se trimit datele...", Activity, false);
        }

        private bool FieldsValidation()
        {
            return _benefitsSpinner.SelectedItem != null && !_benefitsSpinner.SelectedItem.Equals("") && !_tbDetails.Text.Equals("");

        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            var view = inflater.Inflate(Resource.Layout.fragment_asistent_form, container, false);
            NotificationManagerCompat.From(Activity).Cancel(Constants.NotifMedicationId--);
            IntiUi(view);
            _distanceCalculatorService = new Intent(Activity, typeof(DistanceCalculator));
            _medicalAsistanceService = new Intent(Activity, typeof(MedicalAsistanceService));
            string[] selectQualification = {
                "Beneficiu acordat", "Masaj", "Baie", "Perfuzie", "Pansament"};
            _listVOs = new List<BenefitSpinnerState>();

            foreach (var t in selectQualification)
            {
                var stateVo = new BenefitSpinnerState
                {
                    Title = t,
                    IsSelected = false
                };
                _listVOs.Add(stateVo);
            }
            var myAdapter = new BenefitAdapter(Activity, 0, _listVOs);
            _benefitsSpinner.Adapter = myAdapter;
            _tbDetails.TextChanged += delegate
            {
                _btnScan.Enabled = FieldsValidation();
            };
            var fromPreferences = Utils.GetDefaults("ActivityStart", Activity);
            if (string.IsNullOrEmpty(fromPreferences))
            {
                _formContainer.Visibility = ViewStates.Gone;
                _btnScan.Text = "Incepe activitatea";
            }
            else
            {
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
            _isGooglePlayServicesInstalled = Utils.IsGooglePlayServicesInstalled(Activity);


            if (!_isGooglePlayServicesInstalled) return view;
            new LocationRequest()
                .SetPriority(LocationRequest.PriorityHighAccuracy)
                .SetInterval(1000)
                .SetFastestInterval(1000);
            new FusedLocationProviderCallback(Activity);

            _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(Activity);


            return view;
        }

        private async void BtnScan_Click(object sender, EventArgs e)
        {
            //IntentIntegrator.forSupportFragment(this).InitiateScan();
#if __ANDROID__
            // Initialize the scanner first so it can track the current context
            var app = new Application();
            MobileBarcodeScanner.Initialize(app);
#endif

            var result = await StartScan();
            if (result == null) return;
            try
            {
                var sdf = new SimpleDateFormat("dd/MM/yyyy HH:mm:ss");
                var currentDateandTime = sdf.Format(new Date());

                var dateTimeNow = sdf.Parse(currentDateandTime);


                try
                {
                    _qrJsonData = new JSONObject(result.Text);

                    Log.Error("QrCode", _qrJsonData.ToString());


                    var expDateTime = _qrJsonData.GetString("expirationDateTime");
                    var dateTimeExp = sdf.Parse(expDateTime);
                    if (dateTimeExp.After(dateTimeNow))
                    {

                        if (_btnScan.Text.Equals("Incepe activitatea"))
                        {
                            _formContainer.Visibility = ViewStates.Visible;
                            _btnScan.Text = "Finalizeaza activitatea";
                            _btnScan.Enabled = false;
                            try
                            {
                                _progressBarDialog.Show();
                                _dateTimeStart = currentDateandTime;
                                _dateTimeEnd = null;
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                                {
                                    Activity.StartForegroundService(_medicalAsistanceService);
                                    
                                }
                                else
                                {
                                    Activity.StartService(_medicalAsistanceService);
                                }

                        
                                _latitude = double.Parse(Utils.GetDefaults("Latitude", Activity));
                                _longitude = double.Parse(Utils.GetDefaults("Longitude", Activity));
                                Log.Error("Latitude12", _latitude.ToString());
                                Log.Error("Longitude12", _longitude.ToString());

                                Utils.SetDefaults("ConsultLat", _latitude.ToString(), Activity);
                                Utils.SetDefaults("ConsultLong", _longitude.ToString(), Activity);

                                _location = new JSONObject().Put("latitude", _latitude).Put("longitude", _longitude);
                                _details = null;
                                //started = true;
                                var obj = new JSONObject().Put("QRData", _qrJsonData.ToString()).Put("Start", _dateTimeStart).Put("Location", _location.ToString());
                                Utils.SetDefaults("ActivityStart", obj.ToString(), Activity);

                                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                                {
                                    Activity.StartForegroundService(_distanceCalculatorService);
                                }
                                else
                                {
                                    Activity.StartService(_distanceCalculatorService);
                                }
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
                            _btnScan.Text = "Incepe activitatea";
                            try
                            {
                                _progressBarDialog.Show();
                                //JSONObject obj = new JSONObject().put("dateTimeStop", currentDateandTime).put("QRData", QrJsonData.toString());
                                Utils.SetDefaults("ActivityStart", "", Activity);
                                //DateTimeStart = null;
                                _dateTimeEnd = currentDateandTime;
                                GetLastLocationButtonOnClick();
                                _location = new JSONObject().Put("latitude", _latitude).Put("longitude", _longitude);
                                _benefitsArray = new JSONArray();
                                foreach (var t in _listVOs)
                                {
                                    if (t.IsSelected)
                                    {
                                        _benefitsArray = _benefitsArray.Put(t.Title);
                                    }
                                    //BenefitAdapter el = listVOs.get(i).isSelected();
                                }

                                _details = new JSONObject().Put("benefit", _benefitsArray).Put("details", _tbDetails.Text);
                                Log.Error("Details", _details.ToString());

                                //Activity.StopService(_distanceCalculatorService);
                                
                                await Task.Run(async () =>
                                {
                                    //GetLastLocationButtonOnClick();
                                    _latitude = double.Parse(Utils.GetDefaults("Latitude", Activity));
                                    _longitude = double.Parse(Utils.GetDefaults("Longitude", Activity));

                                    Utils.SetDefaults("ConsultLat", _latitude.ToString(), Activity);
                                    Utils.SetDefaults("ConsultLong", _longitude.ToString(), Activity);
                                    var dataToSend = new JSONObject().Put("dateTimeStart", _dateTimeStart)
                                        .Put("dateTimeStop", _dateTimeEnd).Put("qrCodeData", _qrJsonData)
                                        .Put("location", _location).Put("details", _details);
                                    var response = await WebServices.Post(Constants.PublicServerAddress + "/api/consult", dataToSend, Utils.GetDefaults("Token", Activity));
                                    if (response != null)
                                    {
                                        Snackbar snack;
                                        var responseJson = new JSONObject(response);
                                        switch (responseJson.GetInt("status"))
                                        {
                                            case 0:
                                                snack = Snackbar.Make(_formContainer, "Nu esti la pacient!", Snackbar.LengthLong);
                                                snack.Show();
                                                break;
                                            case 1:
                                                snack = Snackbar.Make(_formContainer, "Internal Server Error", Snackbar.LengthLong);
                                                snack.Show();
                                                break;
                                            case 2:
                                                break;
                                        }
                                        _progressBarDialog.Dismiss();
                                    }
                                    else
                                        Snackbar.Make(_formContainer, "Unable to reach the server!", Snackbar.LengthLong).Show();
                                });
                                Activity.StopService(_medicalAsistanceService);
                                Activity.StopService(_distanceCalculatorService);
                            }
                            catch (JSONException ex)
                            {
                                ex.PrintStackTrace();
                            }
                        }
                    }
                    else
                    {
                        Snackbar.Make(_formContainer, "QRCode Expirat! Va rugam sa generati alt cod QR!", Snackbar.LengthLong).Show();
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

        static async Task<ZXing.Result> StartScan()
        {
            var options = new MobileBarcodeScanningOptions
            {
                PossibleFormats = new List<ZXing.BarcodeFormat>()
                {
                    ZXing.BarcodeFormat.QR_CODE
                },
                UseNativeScanning = true,
                AutoRotate = true,
                TryHarder = true

            };

            ZXing.Result result = null;
            var scanner = new MobileBarcodeScanner();
            //var result = await scanner.scan(options);
            // Start thread to adjust focus at 1-sec intervals
            new Thread(new ThreadStart(delegate
            {
                while (result == null)
                {
                    try
                    {
                        scanner.AutoFocus();
                        Thread.Sleep(1000);
                    }
                    catch
                    {
                        //Ignored
                    }
                    
                }
            })).Start();
            result = await scanner.Scan(options);
            return result;


        }


        private async void GetLastLocationButtonOnClick()
        {
            if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                await GetLastLocationFromDevice();
            }
        }

        private async Task GetLastLocationFromDevice()
        {
            var location = await _fusedLocationProviderClient.GetLastLocationAsync();

            if (location == null)
            {
                // Seldom happens, but should code that handles this scenario
                Log.Error("Location is null", "******************");
            }
            else
            {
                Log.Debug("Sample", "The Latitude is " + location.Latitude);
                Log.Debug("Sample", "The Longitude is " + location.Longitude);
                Utils.SetDefaults("ConsultLat", location.Latitude.ToString(), Activity);
                Utils.SetDefaults("ConsultLong", location.Longitude.ToString(), Activity);
                _latitude = location.Latitude;
                _longitude = location.Longitude;

            }
        }
    }
}