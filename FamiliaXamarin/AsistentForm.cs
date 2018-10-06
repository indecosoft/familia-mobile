using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Text;
using Java.Util;
using Org.Json;
using ZXing.Mobile;

namespace FamiliaXamarin
{
    public class AsistentForm : Android.Support.V4.App.Fragment
    {
        FusedLocationProviderClient fusedLocationProviderClient;
        LocationCallback locationCallback;
        LocationRequest locationRequest;
        static readonly int RC_LAST_LOCATION_PERMISSION_CHECK = 1000;
        static readonly int RC_LOCATION_UPDATES_PERMISSION_CHECK = 1100;

        static readonly string KEY_REQUESTING_LOCATION_UPDATES = "requesting_location_updates";
        bool isGooglePlayServicesInstalled;
        bool isRequestingLocationUpdates;

        Spinner BenefitsSpinner;
        EditText TbDetails;
        Button BtnScan;
        Context Ctx;
        Calendar now;
        ConstraintLayout FormContainer;
        String DateTimeStart, DateTimeEnd, Token;
        double Latitude, Longitude;
        ProgressDialog progressDialog;
        //GetLocation location = new GetLocation(getActivity());
        JSONObject Location, Details, QrJsonData;
        JSONArray BenefitsArray;
        List<BenefitSpinnerState> listVOs;
        public static int hour, minutes;
        private Handler handler = new Handler();
        bool started;
        Intent DistanceCalculatorService;
        private readonly IWebServices _webServices = new WebServices();
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }
        private void IntiUI(View v)
        {
            BenefitsSpinner = v.FindViewById<Spinner>(Resource.Id.benefits_spinner);
            TbDetails = v.FindViewById<EditText>(Resource.Id.input_details);
            BtnScan = v.FindViewById<Button>(Resource.Id.btnScan);
            FormContainer = v.FindViewById<ConstraintLayout>(Resource.Id.container);

            progressDialog = new ProgressDialog(Activity);
            progressDialog.SetTitle("Va rugam asteptati ...");
            progressDialog.SetMessage("Se trimit datele");
            progressDialog.SetCancelable(false);

            now = Calendar.Instance;

        }
        private bool FieldsValidation()
        {
            return BenefitsSpinner.SelectedItem != null && !BenefitsSpinner.SelectedItem.Equals("") && !TbDetails.Text.Equals("");

        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            var view = inflater.Inflate(Resource.Layout.fragment_asistent_form, container, false);

            IntiUI(view);
            DistanceCalculatorService = new Intent(Activity, typeof(DistanceCalculator));
            string[] selectQualification = {
                "Beneficiu acordat", "Masaj", "Baie", "Perfuzie", "Pansament"};
            listVOs = new List<BenefitSpinnerState>();

            foreach (var t in selectQualification)
            {
                var stateVo = new BenefitSpinnerState();
                stateVo.SetTitle(t);
                stateVo.SetSelected(false);
                listVOs.Add(stateVo);
            }
            var myAdapter = new BenefitAdapter(Activity, 0, listVOs);
            BenefitsSpinner.Adapter = myAdapter;
            TbDetails.TextChanged += delegate
            {
                BtnScan.Enabled = FieldsValidation();
            };
            var fromPreferences = Utils.GetDefaults("ActivityStart", Activity);
            if (string.IsNullOrEmpty(fromPreferences))
            {
                FormContainer.Visibility = ViewStates.Gone;
                BtnScan.Text = "Incepe activitatea";
            }
            else
            {
                FormContainer.Visibility = ViewStates.Visible;
                try
                {
                    Location = new JSONObject((new JSONObject(fromPreferences).GetString("Location")));
                    DateTimeStart = new JSONObject(fromPreferences).GetString("Start");
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
                BtnScan.Text = "Finalizeaza activitatea";
                BtnScan.Enabled = false;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Activity.StartForegroundService(new Intent(Activity, typeof(DistanceCalculator)));
                }
                else
                {
                    Activity.StartService(new Intent(Activity, typeof(DistanceCalculator)));
                }
            }

            BtnScan.Click += delegate
            {
                //IntentIntegrator.forSupportFragment(this).InitiateScan();
                #if __ANDROID__
                // Initialize the scanner first so it can track the current context
                var app = new Android.App.Application();
                MobileBarcodeScanner.Initialize(app);
                #endif

                StartScan();
            };
            isGooglePlayServicesInstalled = Utils.IsGooglePlayServicesInstalled(Activity);


            if (!isGooglePlayServicesInstalled) return view;
            locationRequest = new LocationRequest()
                .SetPriority(LocationRequest.PriorityHighAccuracy)
                .SetInterval(1000)
                .SetFastestInterval(1000);
            locationCallback = new FusedLocationProviderCallback(Activity);

            fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(Activity);


            return view;
        }
        CameraResolution HandleCameraResolutionSelectorDelegate(List<CameraResolution> availableResolutions)
        {
            //Don't know if this will ever be null or empty
            if (availableResolutions == null || availableResolutions.Count < 1)
                return new CameraResolution() { Width = 1200, Height = 1000 };

            //Debugging revealed that the last element in the list
            //expresses the highest resolution. This could probably be more thorough.
            return availableResolutions[availableResolutions.Count - 1];
        }
        private async void StartScan()
        {
            var options = new MobileBarcodeScanningOptions
            {
                PossibleFormats = new List<ZXing.BarcodeFormat>()
                {
                    ZXing.BarcodeFormat.QR_CODE
                },
                UseNativeScanning = true,
                AutoRotate = true,
                //                CameraResolutionSelector = HandleCameraResolutionSelectorDelegate
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
                    scanner.AutoFocus();
                    Thread.Sleep(1000);
                }
            })).Start();
            result = await scanner.Scan(options);
            if (result == null) return;
            try
            {
                var sdf = new SimpleDateFormat("dd/MM/yyyy HH:mm:ss");
                var currentDateandTime = sdf.Format(new Date());

                var dateTimeNow = sdf.Parse(currentDateandTime);


                try
                {
                    QrJsonData = new JSONObject(result.Text);
                    //QrJsonData = QrJsonData.put("scannedDateTime", currentDateandTime);

                    Log.Error("QrCode", QrJsonData.ToString());


                    var expDateTime = QrJsonData.GetString("expirationDateTime");
                    var dateTimeExp = sdf.Parse(expDateTime);
                    if (dateTimeExp.After(dateTimeNow))
                    {

                        if (BtnScan.Text.Equals("Incepe activitatea"))
                        {
                            FormContainer.Visibility = ViewStates.Visible;
                            BtnScan.Text = "Finalizeaza activitatea";
                            BtnScan.Enabled = false;
                            try
                            {
                                progressDialog.Show();
                                DateTimeStart = currentDateandTime;
                                DateTimeEnd = null;

                                //GetLastLocationButtonOnClick();
                                
                                Latitude = double.Parse(Utils.GetDefaults("Latitude", Ctx));
                                Longitude = double.Parse(Utils.GetDefaults("Longitude", Ctx));

                                Utils.SetDefaults("ConsultLat", Latitude.ToString(), Activity);
                                Utils.SetDefaults("ConsultLong", Longitude.ToString(), Activity);

                                Location = new JSONObject().Put("latitude", Latitude).Put("longitude", Longitude);
                                Details = null;
                                started = true;
                                var obj = new JSONObject().Put("QRData", QrJsonData.ToString()).Put("Start", DateTimeStart).Put("Location", Location.ToString());
                                Utils.SetDefaults("ActivityStart", obj.ToString(), Activity);
                                //handler.PostDelayed(runnable, 1000);
                                //new Consult().execute(Constants.SERVER_ADDRESS + "/api/consult");

                                //Activity.StartForegroundService(DistanceCalculatorService);
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                                {
                                    Activity.StartForegroundService(DistanceCalculatorService);
                                }
                                else
                                {
                                    Activity.StartService(DistanceCalculatorService);
                                }
                                progressDialog.Dismiss();
                            }
                            catch (JSONException e)
                            {
                                e.PrintStackTrace();
                            }
                        }
                        else
                        {
                            FormContainer.Visibility = ViewStates.Gone;
                            BtnScan.Text = "Incepe activitatea";
                            try
                            {
                                progressDialog.Show();
                                //JSONObject obj = new JSONObject().put("dateTimeStop", currentDateandTime).put("QRData", QrJsonData.toString());
                                Utils.SetDefaults("ActivityStart", "", Activity);
                                //DateTimeStart = null;
                                DateTimeEnd = currentDateandTime;
                                GetLastLocationButtonOnClick();
                                Location = new JSONObject().Put("latitude", Latitude).Put("longitude", Longitude);
                                BenefitsArray = new JSONArray();
                                foreach (var t in listVOs)
                                {
                                    if (t.IsSelected())
                                    {
                                        BenefitsArray = BenefitsArray.Put(t.GetTitle());
                                    }
                                    //BenefitAdapter el = listVOs.get(i).isSelected();
                                }

                                Details = new JSONObject().Put("benefit", BenefitsArray).Put("details", TbDetails.Text);
                                Log.Error("Details", Details.ToString());
                                started = false;
                                //new Consult().execute(Constants.SERVER_ADDRESS + "/api/consult");
                                //RetriveLocation.stopGetConsultLocation();

                                Activity.StopService(new Intent(Activity, typeof(DistanceCalculator)));
                                await Task.Run(async () =>
                                {
                                    //GetLastLocationButtonOnClick();
                                    Latitude = double.Parse(Utils.GetDefaults("Latitude", Ctx));
                                    Longitude = double.Parse(Utils.GetDefaults("Longitude", Ctx));

                                    Utils.SetDefaults("ConsultLat", Latitude.ToString(), Activity);
                                    Utils.SetDefaults("ConsultLong", Longitude.ToString(), Activity);
                                    var dataToSend = new JSONObject().Put("dateTimeStart", DateTimeStart)
                                        .Put("dateTimeStop", DateTimeEnd).Put("qrCodeData", QrJsonData)
                                        .Put("location", Location).Put("details", Details);
                                    Log.Error("DataToSend", dataToSend.ToString());
                                    var response = await _webServices.Post(Constants.PublicServerAddress + "api/consult", dataToSend, Utils.GetDefaults("Token", Activity));
                                    if (response != null)
                                    {
                                        Snackbar snack;
                                        var responseJson = new JSONObject(response);
                                        switch (responseJson.GetInt("status"))
                                        {
                                            case 0:
                                                snack = Snackbar.Make(FormContainer, "Nu esti la pacient!", Snackbar.LengthLong);
                                                snack.Show();
                                                break;
                                            case 1:
                                                snack = Snackbar.Make(FormContainer, "Internal Server Error", Snackbar.LengthLong);
                                                snack.Show();
                                                break;
                                            case 2:
                                                break;
                                        }
                                        progressDialog.Dismiss();
                                    }
                                    else
                                    {
                                        Snackbar.Make(FormContainer, "Unable to reach the server!", Snackbar.LengthLong).Show();
                                    }
                                });

                                Activity.StopService(DistanceCalculatorService);
                            }
                            catch (JSONException e)
                            {
                                e.PrintStackTrace();
                            }
                        }
                    }
                    else
                    {
                        Snackbar.Make(FormContainer, "QRCode Expirat! Va rugam sa generati alt cod QR!", Snackbar.LengthLong).Show();
                        //Utils.DisplayNotification(getActivity(), "Eroare", "QRCode Expirat! Va rugam sa generati alt cod QR!");
                        progressDialog.Dismiss();
                    }

                }
                catch (JSONException e)
                {
                    Snackbar.Make(FormContainer, "QRCode invalid!", Snackbar.LengthLong).Show();

                    //Utils.DisplayNotification(getActivity(), "Eroare", "QRCode invalid!");
                    //Log.Error("caca", e.Message);

                    progressDialog.Dismiss();

                }
            }
            catch (Exception e)
            {
                //                    e.PrintStackTrace();
                progressDialog.Dismiss();
            }


        }

        private async void GetLastLocationButtonOnClick()
        {
            if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                await GetLastLocationFromDevice();
            }
            else
            {
                //RequestLocationPermission(RC_LAST_LOCATION_PERMISSION_CHECK);
            }
        }

        private async Task GetLastLocationFromDevice()
        {
            // This method assumes that the necessary run-time permission checks have succeeded.
            //getLastLocationButton.SetText(Resource.String.getting_last_location);
            var location = await fusedLocationProviderClient.GetLastLocationAsync();

            if (location == null)
            {
                // Seldom happens, but should code that handles this scenario
                Log.Error("Location is null", "Bag pula");
            }
            else
            {
                // Do something with the location 
                Log.Debug("Sample", "The Latitude is " + location.Latitude);
                Log.Debug("Sample", "The Longitude is " + location.Longitude);
                Utils.SetDefaults("ConsultLat", location.Latitude.ToString(), Activity);
                Utils.SetDefaults("ConsultLong", location.Longitude.ToString(), Activity);
                Utils.SetDefaults("ActivityStart", "nu e null si nici gol", Activity);
                Latitude = location.Latitude;
                Longitude = location.Longitude;

                //Activity.StartService(new Intent(Activity, typeof(DistanceCalculator)));
            }
        }
    }
}