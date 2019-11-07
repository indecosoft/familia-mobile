using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Android;
using Familia;
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
using Resource = Familia.Resource;
using Familia.Helpers;
using Familia.Asistentasociala;
using Newtonsoft.Json;

namespace FamiliaXamarin.Asistenta_sociala
{
    public class AsistentForm : Android.Support.V4.App.Fragment
    {
        private FusedLocationProviderClient _fusedLocationProviderClient;
        private bool _isGooglePlayServicesInstalled;
        private EditText _tbDetails;
        private Button _btnScan, _btnAnulare, _btnBenefits;
        private ConstraintLayout _formContainer;
        private string _dateTimeStart, _dateTimeEnd;
        private double _latitude, _longitude;
        private ProgressBarDialog _progressBarDialog;
        private JSONObject _location, _details, _qrJsonData;
        private JSONArray _benefitsArray;
        private List<SearchListModel> SelectedBenefits = new List<SearchListModel>();
        private Intent _distanceCalculatorService, _medicalAsistanceService;

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
            return SelectedBenefits.Count > 0 && !string.IsNullOrEmpty(_tbDetails.Text);

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

            var fromPreferences = Utils.GetDefaults("ActivityStart");
            _tbDetails.TextChanged += delegate
            {

                _btnScan.Enabled = FieldsValidation();
            };
            _btnAnulare.Click += delegate(object sender, EventArgs args)
            {
                Utils.SetDefaults("ActivityStart", "");
                Utils.SetDefaults("QrId", "");

                _formContainer.Visibility = ViewStates.Gone;
                _btnAnulare.Visibility = ViewStates.Gone;
                _btnScan.Text = "Incepe activitatea";
                Activity.StopService(_medicalAsistanceService);
                Activity.StopService(_distanceCalculatorService);
                SelectedBenefits.Clear();
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
            _isGooglePlayServicesInstalled = Utils.IsGooglePlayServicesInstalled(Activity);


            if (!_isGooglePlayServicesInstalled) return view;
            new LocationRequest()
                .SetPriority(LocationRequest.PriorityHighAccuracy)
                .SetInterval(1000)
                .SetFastestInterval(1000);
            _ = new FusedLocationProviderCallback(Activity);

            _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(Activity);


            return view;
        }

        private async void OpenDiseaseList(object sender, EventArgs e)
        {
            _progressBarDialog.Show();
            await Task.Run(async () => {
                try
                {
                    //Utils.GetDefaults("QrId")
                    var response = await WebServices.Get($"{Constants.PublicServerAddress}/api/getBenefits", Utils.GetDefaults("Token"));
                    Log.Error("Debug Log in " + nameof(AsistentForm), "Response: " + response);
                    var jsonResponse = new JSONObject(response);
                    Log.Error("ASISTEN FORM BENEFITS", jsonResponse.ToString());
                    if (jsonResponse.GetInt("status") == 2)
                    {
                        var dataArray = jsonResponse.GetJSONArray("data");
                        List<SearchListModel> items = new List<SearchListModel>();
                        for (int i = 0; i < dataArray.Length(); i++)
                        {
                            items.Add(new SearchListModel{
                            Id= dataArray.GetJSONObject(i).GetInt("id"),
                            Title = dataArray.GetJSONObject(i).GetString("benefit")
                            } );
                        }
                        
                        Intent intent = new Intent(Activity, typeof(SearchListActivity));
                        intent.PutExtra("Items", JsonConvert.SerializeObject(items));
                        intent.PutExtra("SelectedItems", JsonConvert.SerializeObject(SelectedBenefits)); 
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
            //IntentIntegrator.forSupportFragment(this).InitiateScan();
            // Initialize the scanner first so it can track the current context
            var app = new Application();
            MobileBarcodeScanner.Initialize(app);

            var result = await StartScan();
            if (result == null) return;
            try
            {
                var sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
                var currentDateandTime = sdf.Format(new Date());

                var dateTimeNow = sdf.Parse(currentDateandTime);


                try
                {
                    _qrJsonData = new JSONObject(Encryption.Decrypt(result.Text));

                    Log.Error("QrCode", _qrJsonData.ToString());
                    Utils.SetDefaults("QrId", _qrJsonData.GetInt("Id").ToString());
                    var expDateTime = _qrJsonData.GetString("expirationDateTime");
                    var dateTimeExp = sdf.Parse(expDateTime);
                    if (dateTimeExp.After(dateTimeNow))
                    {

                        if (_btnScan.Text.Equals("Incepe activitatea"))
                        {
                            _formContainer.Visibility = ViewStates.Visible;
                            _btnScan.Text = "Finalizeaza activitatea";
                            _btnScan.Enabled = false;
                            _btnAnulare.Visibility = ViewStates.Visible;
                            try
                            {
                                _progressBarDialog.Show();
                                _dateTimeStart = currentDateandTime;
                                _dateTimeEnd = null;
                                Activity.StartForegroundService(_medicalAsistanceService);
          

                        
                                _latitude = double.Parse(Utils.GetDefaults("Latitude"));
                                _longitude = double.Parse(Utils.GetDefaults("Longitude"));
                                Log.Error("Latitude12", _latitude.ToString());
                                Log.Error("Longitude12", _longitude.ToString());

                                Utils.SetDefaults("ConsultLat", _latitude.ToString());
                                Utils.SetDefaults("ConsultLong", _longitude.ToString());

                                _location = new JSONObject().Put("latitude", _latitude).Put("longitude", _longitude);
                                _details = null;
                                //started = true;
                                var obj = new JSONObject().Put("QRData", _qrJsonData.ToString()).Put("Start", _dateTimeStart).Put("Location", _location.ToString());
                                Utils.SetDefaults("ActivityStart", obj.ToString());

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
                            _btnAnulare.Visibility = ViewStates.Gone;
                            Utils.SetDefaults("QrId","");

                            try
                            {
                                _progressBarDialog.Show();
                                //JSONObject obj = new JSONObject().put("dateTimeStop", currentDateandTime).put("QRData", QrJsonData.toString());
                                Utils.SetDefaults("ActivityStart", "");
                                //DateTimeStart = null;
                                _dateTimeEnd = currentDateandTime;
                                if (ContextCompat.CheckSelfPermission(Activity, Manifest.Permission.AccessFineLocation) == Permission.Granted)
                                {
                                    await GetLastLocationFromDevice();
                                }
                                _location = new JSONObject().Put("latitude", _latitude).Put("longitude", _longitude);
                                _benefitsArray = new JSONArray();
                                foreach (var t in SelectedBenefits)
                                    _benefitsArray.Put(t.Id);

                                _details = new JSONObject().Put("benefit", _benefitsArray).Put("details", _tbDetails.Text);
                                Log.Error("Details", _details.ToString());

                                //Activity.StopService(_distanceCalculatorService);
                                
                                await Task.Run(async () =>
                                {
                                    //GetLastLocationButtonOnClick();
                                    _latitude = double.Parse(Utils.GetDefaults("Latitude"));
                                    _longitude = double.Parse(Utils.GetDefaults("Longitude"));

                                    Utils.SetDefaults("ConsultLat", _latitude.ToString());
                                    Utils.SetDefaults("ConsultLong", _longitude.ToString());
                                    var dataToSend = new JSONObject().Put("dateTimeStart", _dateTimeStart)
                                        .Put("dateTimeStop", _dateTimeEnd).Put("qrCodeData", _qrJsonData)
                                        .Put("location", _location).Put("details", _details);
                                    var response = await WebServices.Post(Constants.PublicServerAddress + "/api/consult", dataToSend, Utils.GetDefaults("Token"));
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
                                Activity.StopService(_medicalAsistanceService);
                                Activity.StopService(_distanceCalculatorService);
                                _tbDetails.Text = string.Empty;
                                SelectedBenefits.Clear();
                                _btnScan.Enabled = true;
                                _btnBenefits.Text = "Selecteaza beneficii";
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

        private async Task<ZXing.Result> StartScan()
        {
            var options = new MobileBarcodeScanningOptions
            {
                PossibleFormats = new List<ZXing.BarcodeFormat>()
                {
                    ZXing.BarcodeFormat.QR_CODE
                },
                CameraResolutionSelector = availableResolutions => availableResolutions[0],
                UseNativeScanning = true,
                TryHarder = true

            };
            var scanner = new MobileBarcodeScanner();
            WindowManagerFlags flags = WindowManagerFlags.Fullscreen;
            Activity.Window.SetFlags(flags,
                flags);
            //var result = await scanner.scan(options);
            // Start thread to adjust focus at 1-sec intervals
            var result = await scanner.Scan(options);
//            new Thread(new ThreadStart(delegate
//            {
//                while (result == null)
//                {
//                    try
//                    {
//                        scanner.AutoFocus();
//                        Thread.Sleep(3000);
//                    }
//                    catch
//                    {
//                        //Ignored
//                    }
//                    
//                }
//            })).Start();
            
            Activity.Window.ClearFlags(flags);
            return result;


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
                Utils.SetDefaults("ConsultLat", location.Latitude.ToString());
                Utils.SetDefaults("ConsultLong", location.Longitude.ToString());
                _latitude = location.Latitude;
                _longitude = location.Longitude;

            }
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == (int)Result.Ok)
            {
                SelectedBenefits = JsonConvert.DeserializeObject<List<SearchListModel>>(data.GetStringExtra("result"));
                Log.Error("Avem result", data.GetStringExtra("result"));
                _btnScan.Enabled = FieldsValidation();
                _btnBenefits.Text = $"Ati Selectat {SelectedBenefits.Count} beneficii";
            } else
            {
                Log.Error("Nu avem result", "User-ul a zis CANCEL");
                _btnBenefits.Text = "Selectati beneficii";
                SelectedBenefits.Clear();
            }
        }
    }

}