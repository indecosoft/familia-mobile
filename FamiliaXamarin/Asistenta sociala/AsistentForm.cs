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
using Android.Support.V4.Content;
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
        bool isGooglePlayServicesInstalled;
        Spinner BenefitsSpinner;
        EditText TbDetails;
        Button BtnScan;
        Calendar now;
        ConstraintLayout FormContainer;
        string DateTimeStart, DateTimeEnd;
        double Latitude, Longitude;
#pragma warning disable CS0618 // Type or member is obsolete
        ProgressDialog progressDialog;
#pragma warning restore CS0618 // Type or member is obsolete
        JSONObject Location, Details, QrJsonData;
        JSONArray BenefitsArray;
        List<BenefitSpinnerState> listVOs;
        public static int hour, minutes;
        readonly Handler handler = new Handler();
        Intent DistanceCalculatorService;
        //readonly IWebServices _webServices = new WebServices();
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }
        void IntiUI(View v)
        {
            BenefitsSpinner = v.FindViewById<Spinner>(Resource.Id.benefits_spinner);
            TbDetails = v.FindViewById<EditText>(Resource.Id.input_details);
            BtnScan = v.FindViewById<Button>(Resource.Id.btnScan);
            FormContainer = v.FindViewById<ConstraintLayout>(Resource.Id.container);

#pragma warning disable CS0618 // Type or member is obsolete
            progressDialog = new ProgressDialog(Activity);
#pragma warning restore CS0618 // Type or member is obsolete
            progressDialog.SetTitle("Va rugam asteptati ...");
            progressDialog.SetMessage("Se trimit datele");
            progressDialog.SetCancelable(false);

            now = Calendar.Instance;

        }
        bool FieldsValidation()
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
                var stateVo = new BenefitSpinnerState
                {
                    Title = t,
                    IsSelected = false
                };
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

            BtnScan.Click += BtnScan_Click;
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

        async void BtnScan_Click(object sender, EventArgs e)
        {
            //IntentIntegrator.forSupportFragment(this).InitiateScan();
#if __ANDROID__
            // Initialize the scanner first so it can track the current context
            var app = new Application();
            MobileBarcodeScanner.Initialize(app);
#endif

            var result = await StartScan();
            if (result != null)
                try
                {
                    var sdf = new SimpleDateFormat("dd/MM/yyyy HH:mm:ss");
                    var currentDateandTime = sdf.Format(new Date());

                    var dateTimeNow = sdf.Parse(currentDateandTime);


                    try
                    {
                        QrJsonData = new JSONObject(result.Text);

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

                                    Latitude = double.Parse(Utils.GetDefaults("Latitude", Activity));
                                    Longitude = double.Parse(Utils.GetDefaults("Longitude", Activity));

                                    Utils.SetDefaults("ConsultLat", Latitude.ToString(), Activity);
                                    Utils.SetDefaults("ConsultLong", Longitude.ToString(), Activity);

                                    Location = new JSONObject().Put("latitude", Latitude).Put("longitude", Longitude);
                                    Details = null;
                                    //started = true;
                                    var obj = new JSONObject().Put("QRData", QrJsonData.ToString()).Put("Start", DateTimeStart).Put("Location", Location.ToString());
                                    Utils.SetDefaults("ActivityStart", obj.ToString(), Activity);

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
                                catch (JSONException ex)
                                {
                                    ex.PrintStackTrace();
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
                                        if (t.IsSelected)
                                        {
                                            BenefitsArray = BenefitsArray.Put(t.Title);
                                        }
                                        //BenefitAdapter el = listVOs.get(i).isSelected();
                                    }

                                    Details = new JSONObject().Put("benefit", BenefitsArray).Put("details", TbDetails.Text);
                                    Log.Error("Details", Details.ToString());
                                    //started = false;
                                    //new Consult().execute(Constants.SERVER_ADDRESS + "/api/consult");
                                    //RetriveLocation.stopGetConsultLocation();

                                    Activity.StopService(new Intent(Activity, typeof(DistanceCalculator)));
                                    await Task.Run(async () =>
                                    {
                                        //GetLastLocationButtonOnClick();
                                        Latitude = double.Parse(Utils.GetDefaults("Latitude", Activity));
                                        Longitude = double.Parse(Utils.GetDefaults("Longitude", Activity));

                                        Utils.SetDefaults("ConsultLat", Latitude.ToString(), Activity);
                                        Utils.SetDefaults("ConsultLong", Longitude.ToString(), Activity);
                                        var dataToSend = new JSONObject().Put("dateTimeStart", DateTimeStart)
                                            .Put("dateTimeStop", DateTimeEnd).Put("qrCodeData", QrJsonData)
                                            .Put("location", Location).Put("details", Details);
                                        var response = await WebServices.Post(Constants.PublicServerAddress + "/api/consult", dataToSend, Utils.GetDefaults("Token", Activity));
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
                                            Snackbar.Make(FormContainer, "Unable to reach the server!", Snackbar.LengthLong).Show();
                                    });

                                    Activity.StopService(DistanceCalculatorService);
                                }
                                catch (JSONException ex)
                                {
                                    ex.PrintStackTrace();
                                }
                            }
                        }
                        else
                        {
                            Snackbar.Make(FormContainer, "QRCode Expirat! Va rugam sa generati alt cod QR!", Snackbar.LengthLong).Show();
                            progressDialog.Dismiss();
                        }

                    }
                    catch (JSONException)
                    {
                        Snackbar.Make(FormContainer, "QRCode invalid!", Snackbar.LengthLong).Show();
                        progressDialog.Dismiss();

                    }
                }
                catch (Exception)
                {
                    progressDialog.Dismiss();
                }
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

        async Task<ZXing.Result> StartScan()
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
                    scanner.AutoFocus();
                    Thread.Sleep(1000);
                }
            })).Start();
            result = await scanner.Scan(options);
            return result ?? null;


        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void GetLastLocationButtonOnClick()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                await GetLastLocationFromDevice();
            }
        }

        async Task GetLastLocationFromDevice()
        {
            var location = await fusedLocationProviderClient.GetLastLocationAsync();

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
                Latitude = location.Latitude;
                Longitude = location.Longitude;

            }
        }
    }
}