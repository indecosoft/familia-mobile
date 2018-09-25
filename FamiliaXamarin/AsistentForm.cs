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
            List<string> categories = new List<string> {"Masaj", "Baie", "Perfuzie", "Pansament"};
            string[] select_qualification = {
                "Beneficiu acordat", "Masaj", "Baie", "Perfuzie", "Pansament"};
            listVOs = new List<BenefitSpinnerState>();

            for (int i = 0; i < select_qualification.Length; i++)
            {
                BenefitSpinnerState stateVO = new BenefitSpinnerState();
                stateVO.SetTitle(select_qualification[i]);
                stateVO.SetSelected(false);
                listVOs.Add(stateVO);
            }
            BenefitAdapter myAdapter = new BenefitAdapter(Activity, 0,
                listVOs);
            BenefitsSpinner.Adapter =myAdapter;
                        TbDetails.TextChanged += delegate
                        {
                            BtnScan.Enabled = FieldsValidation();
                        };
            string fromPreferences = Utils.GetDefaults("ActivityStart", Activity);
            if (fromPreferences.Equals("") || fromPreferences.Equals("null"))
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
                if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Activity.StartForegroundService(new Intent(Activity, typeof(DistanceCalculator)));
                }
                else
                {
                    Activity.StartService(new Intent(Activity, typeof(DistanceCalculator)));
                }
            }

            BtnScan.Click += async delegate
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



            if (isGooglePlayServicesInstalled)
            {
                locationRequest = new LocationRequest()
                    .SetPriority(LocationRequest.PriorityHighAccuracy)
                    .SetInterval(1000)
                    .SetFastestInterval(1000);
                locationCallback = new FusedLocationProviderCallback(Activity);

                fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(Activity);
                
            }

            
            return view;
        }

        private async void StartScan()
        {


            var options = new MobileBarcodeScanningOptions
            {
                PossibleFormats = new List<ZXing.BarcodeFormat>()
                {
                    ZXing.BarcodeFormat.CODE_128,
                    ZXing.BarcodeFormat.QR_CODE
                }
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
                if (result != null)
                {

                try
                {
                    SimpleDateFormat sdf = new SimpleDateFormat("dd/MM/yyyy HH:mm:ss");
                    string currentDateandTime = sdf.Format(new Date());

                    Date dateTimeNow = sdf.Parse(currentDateandTime);


                    try
                    {
                        QrJsonData = new JSONObject(result.Text);
                        //QrJsonData = QrJsonData.put("scannedDateTime", currentDateandTime);

                        Log.Error("QrCode", QrJsonData.ToString());


                        string ExpDateTime = QrJsonData.GetString("expirationDateTime");
                        Date dateTimeExp = sdf.Parse(ExpDateTime);
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
                                    //Token = result.getContents();
                                    //RetriveLocation.getLocation();

                                    //handler.postDelayed(runnableGetLocation, 10000);
                                    //                                    Latitude = Double.Parse(Objects.requireNonNull(Utils.GetValuesFromSharedPreferences(getActivity(), "Latitude", "String")).toString());
                                    //                                    Longitude = Double.parseDouble(Objects.requireNonNull(Utils.GetValuesFromSharedPreferences(getActivity(), "Longitude", "String")).toString());
                                    //
                                    //                                    Utils.SetValuesInSharedPreferences(getActivity(), "ConsultLong", "" + Longitude);
                                    //                                    Utils.SetValuesInSharedPreferences(getActivity(), "ConsultLat", "" + Latitude);
                                    GetLastLocationButtonOnClick();
                                    Location = new JSONObject().Put("latitude", Latitude).Put("longitude", Longitude);
                                    Details = null;
                                    started = true;
                                    JSONObject obj = new JSONObject().Put("QRData", QrJsonData.ToString()).Put("Start", DateTimeStart).Put("Location", Location.ToString());
                                    Utils.SetDefaults("ActivityStart", obj.ToString(),Activity);
                                    //handler.PostDelayed(runnable, 1000);
                                    //new Consult().execute(Constants.SERVER_ADDRESS + "/api/consult");

                                    //Activity.StartService(DistanceCalculatorService);
                                    if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
                                    {
                                        Activity.StartForegroundService(new Intent(Activity, typeof(DistanceCalculator)));
                                    }
                                    else
                                    {
                                        Activity.StartService(new Intent(Activity, typeof(DistanceCalculator)));
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
                                    //Token = result.getContents();

                                    //RetriveLocation.getLocation();
                                    //                                    Latitude = Double.parseDouble(Objects.requireNonNull(Utils.GetValuesFromSharedPreferences(getActivity(), "Latitude", "String")).toString());
                                    //                                    Longitude = Double.parseDouble(Objects.requireNonNull(Utils.GetValuesFromSharedPreferences(getActivity(), "Longitude", "String")).toString());
                                    GetLastLocationButtonOnClick();
                                    Location = new JSONObject().Put("latitude", Latitude).Put("longitude", Longitude);
                                    BenefitsArray = new JSONArray();
                                    for (int i = 0; i < listVOs.Count; i++)
                                    {
                                        if (listVOs[i].IsSelected())
                                        {
                                            BenefitsArray = BenefitsArray.Put(listVOs[i].GetTitle());
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
                                        var dataToSend = new JSONObject().Put("dateTimeStart", DateTimeStart)
                                            .Put("dateTimeStop", DateTimeEnd).Put("qrCodeData", QrJsonData)
                                            .Put("location", Location).Put("details", Details);

                                        string response = await _webServices.Post(Constants.PUBLIC_SERVER_ADDRESS + "api/consult", dataToSend, Utils.GetDefaults("Token", Activity));
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
                                            var snack = Snackbar.Make(FormContainer, "Unable to reach the server!", Snackbar.LengthLong);
                                            snack.Show();
                                        }
                                    });

                                    //Objects.requireNonNull(getActivity()).stopService(DistanceCalculatorService);
                                }
                                catch (JSONException e)
                                {
                                    e.PrintStackTrace();
                                }
                            }
                        }
                        else
                        {
                            //Utils.DisplayNotification(getActivity(), "Eroare", "QRCode Expirat! Va rugam sa generati alt cod QR!");
                            progressDialog.Dismiss();
                        }

                    }
                    catch (JSONException e)
                    {
                        //Utils.DisplayNotification(getActivity(), "Eroare", "QRCode invalid!");
                        Log.Error("caca", e.Message);

                        progressDialog.Dismiss();

                    }
                }
                catch (Exception e)
                {
//                    e.PrintStackTrace();
                    progressDialog.Dismiss();
                }


            }
            

        }

        async void GetLastLocationButtonOnClick()
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
        async Task GetLastLocationFromDevice()
        {
            // This method assumes that the necessary run-time permission checks have succeeded.
            //getLastLocationButton.SetText(Resource.String.getting_last_location);
            Android.Locations.Location location = await fusedLocationProviderClient.GetLastLocationAsync();

            if (location == null)
            {
                // Seldom happens, but should code that handles this scenario
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