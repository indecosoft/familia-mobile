using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndroidX.Core.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Views.InputMethods;
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
using LocationEventArgs = Familia.Location.LocationEventArgs;
using AndroidX.Fragment.App;
using AndroidX.ConstraintLayout.Widget;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Google.Android.Material.Snackbar;

namespace Familia.Asistenta_sociala {
    public class AsistentForm : Fragment, DistanceCalculator.IServiceStoppedListener {
        private EditText _tbDetails;
        private Button _btnScan, _btnAnulare, _btnBenefits, _btnBloodPressure, _btnBloodGlucose;
        private ConstraintLayout _formContainer;
        private string _dateTimeStart, _dateTimeEnd;
        private ProgressBarDialog _progressBarDialog;
        private JSONObject _location, _details, _qrJsonData;
        private JSONArray _benefitsArray;
        private List<SearchListModel> _selectedBenefits = new List<SearchListModel>();
        //private Intent _distanceCalculatorService;
        private bool isPacientWithoutApp;
        LocationManager location = LocationManager.Instance;
        string readedQR;
        string inProgressQRCode = "";

        private void IntiUi(View v) {

            _tbDetails = v.FindViewById<EditText>(Resource.Id.input_details);
            _btnScan = v.FindViewById<Button>(Resource.Id.btnScan);
            _btnAnulare = v.FindViewById<Button>(Resource.Id.btnAnulare);
            _btnBloodPressure = v.FindViewById<Button>(Resource.Id.btnBloodPressure);

            _btnBloodGlucose = v.FindViewById<Button>(Resource.Id.btnBloodGlucose);

            _btnBenefits = v.FindViewById<Button>(Resource.Id.benefits_button);
            _formContainer = v.FindViewById<ConstraintLayout>(Resource.Id.container);

            _progressBarDialog = new ProgressBarDialog("Va rugam asteptati" , "Datele sunt procesate..." , Activity , false);
            _progressBarDialog.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimary);
        }

        private bool FieldsValidation() {
            return _selectedBenefits.Count > 0 && !string.IsNullOrEmpty(_tbDetails.Text);

        }

        public override View OnCreateView(LayoutInflater inflater , ViewGroup container , Bundle savedInstanceState) {
          
            View view = inflater.Inflate(Resource.Layout.fragment_asistent_form , container , false);
            NotificationManagerCompat.From(Activity).Cancel(2);
            IntiUi(view);
           
            string fromPreferences = Utils.GetDefaults("ActivityStart");
            string qrData = Utils.GetDefaults("QrCode");
            readedQR = Utils.GetDefaults("readedQR");
            inProgressQRCode = Utils.GetDefaults("InProgressQRCode");
            _btnBloodPressure.Visibility = ViewStates.Gone;
            _btnBloodGlucose.Visibility = ViewStates.Gone;
            _tbDetails.TextChanged += delegate {

                _btnScan.Enabled = FieldsValidation();
                if (_btnScan.Enabled) {
                    StartBlinkingAnimation(Activity, _btnScan);
                }
            };
            _btnAnulare.Click += delegate {
             
                OnServiceStopped();

            };
            _btnBloodPressure.Click += _btnBloodPressure_Click;
            _btnBloodGlucose.Click += _btnBloodGlucose_Click;

            _btnBenefits.Click += OpenDiseaseList;


            if (string.IsNullOrEmpty(fromPreferences) || string.IsNullOrEmpty(qrData)) {
                _formContainer.Visibility = ViewStates.Gone;
                _btnAnulare.Visibility = ViewStates.Gone;
                _btnBloodPressure.Visibility = ViewStates.Gone;
                _btnBloodGlucose.Visibility = ViewStates.Gone;

                _btnScan.Text = "Incepe activitatea";
            } else {
                _btnAnulare.Visibility = ViewStates.Visible;
                _btnBloodPressure.Visibility = ViewStates.Visible;
                _btnBloodGlucose.Visibility = ViewStates.Visible;
                _qrJsonData = new JSONObject(qrData);
                _formContainer.Visibility = ViewStates.Visible;
                try {
                    _location = new JSONObject(new JSONObject(fromPreferences).GetString("Location"));
                    _dateTimeStart = new JSONObject(fromPreferences).GetString("Start");
                } catch (JSONException e) {
                    e.PrintStackTrace();
                }
                _btnScan.Text = "Finalizeaza activitatea";

                _btnScan.Enabled = false;
            }

            _btnScan.Click += BtnScan_Click;

            return view;
        }


        private async void StartNewActivity(Type activity , DeviceType deviceType) {
            if (readedQR is null) return;
            var bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
            var list = await bleDevicesRecords.QueryValuations("SELECT * FROM BluetoothDeviceRecords");
            if (list.Where(d => d.DeviceType == deviceType).Count() > 0) {
                var intent = new Intent(Activity , activity);
                intent.PutExtra("Data" , readedQR);
                Activity.StartActivity(intent);
            } else {
                var alertDialog = new AlertDialog.Builder(Activity);
                alertDialog.SetTitle("Avertisment");

                alertDialog.SetMessage("Nu aveti niciun dispozitiv inregistrat!");
                alertDialog.SetPositiveButton( "OK" , (senderAlert, args) => { Activity.StartActivity(typeof(DevicesManagementActivity)); });
                alertDialog.Show();
            }
        }

        private void _btnBloodGlucose_Click(object sender , EventArgs e) {
            StartNewActivity(typeof(GlucoseDeviceActivity) , DeviceType.Glucose);
        }

        private void _btnBloodPressure_Click(object sender , EventArgs e) {
            StartNewActivity(typeof(BloodPressureDeviceActivity) , DeviceType.BloodPressure);
        }

        private async void OpenDiseaseList(object sender , EventArgs e) {
            _progressBarDialog.Show();
            await Task.Run(async () => {
                try {
                    
                    string response = await WebServices.WebServices.Get($"{Constants.PublicServerAddress}/api/getAllBenefits/" , Utils.GetDefaults("Token"));
                    Log.Error("Debug Log in " + nameof(AsistentForm) , "Response: " + response);
                    var jsonResponse = new JSONObject(response);
                    Log.Error("ASISTEN FORM BENEFITS" , jsonResponse.ToString());
                    if (jsonResponse.GetInt("status") == 2) {
                        JSONArray dataArray = jsonResponse.GetJSONArray("data");
                        var items = new List<SearchListModel>();
                        for (var i = 0; i < dataArray.Length(); i++) {
                            items.Add(new SearchListModel {
                                Id = dataArray.GetJSONObject(i).GetInt("id") ,
                                Title = dataArray.GetJSONObject(i).GetString("benefit")
                            });
                        }

                        var intent = new Intent(Activity , typeof(SearchListActivity));
                        intent.PutExtra("Items" , JsonConvert.SerializeObject(items));
                        intent.PutExtra("SelectedItems" , JsonConvert.SerializeObject(_selectedBenefits));
                        StartActivityForResult(intent , 1);

                    }
                } catch (Exception ex) {
                    Log.Error("error la beneficii" , ex.Message);
                }
            });
            _progressBarDialog.Dismiss();


        }

        private async void BtnScan_Click(object sender , EventArgs e) {
            using var sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
            string currentDateandTime = sdf.Format(new Date());
            Date dateTimeNow = sdf.Parse(currentDateandTime);
            var result = await Utils.ScanQrCode(Activity);
            if (result is null) return;
           
            readedQR = result.Text;
      
            Utils.SetDefaults("readedQR" , readedQR);
            if (Utils.isJson(readedQR)) {
                Log.Error("QrCode" , "is json");
                isPacientWithoutApp = true;
                _qrJsonData = new JSONObject(readedQR);
                Utils.SetDefaults("QrCode" , _qrJsonData.ToString());

                await CheckActivity(currentDateandTime);

            } else {
                isPacientWithoutApp = false;
                try {
                    _qrJsonData = new JSONObject(Encryption.Decrypt(readedQR));
                    if (_qrJsonData is null) return;
                    try {
                        Log.Error("QrCode" , _qrJsonData.ToString());
                        Utils.SetDefaults("QrCode" , _qrJsonData.ToString());
                        Utils.SetDefaults("QrId" , _qrJsonData.GetInt("Id").ToString());
                        string expDateTime = _qrJsonData.GetString("expirationDateTime");
                        Date dateTimeExp = sdf.Parse(expDateTime);
                        if (dateTimeExp.After(dateTimeNow)) {

                            await CheckActivity(currentDateandTime);
                        } else {
                            Snackbar.Make(_formContainer , "QRCode expirat! Va rugam sa generati alt cod QR!" , Snackbar.LengthLong).Show();
                        }

                    } catch (JSONException) {
                        Snackbar.Make(_formContainer , "QRCode invalid!" , Snackbar.LengthLong).Show();

                    }

                } catch (Exception) {
                    Snackbar.Make(_formContainer , "QRCode invalid!" , Snackbar.LengthLong).Show();
                }
            }
        }

        private async Task CheckActivity(string start) {
            if (_btnScan.Text.Equals("Incepe activitatea")) {
                _formContainer.Visibility = ViewStates.Visible;

                _btnScan.Enabled = false;
                _btnAnulare.Visibility = ViewStates.Visible;
                _btnBloodPressure.Visibility = ViewStates.Visible;
                _btnBloodGlucose.Visibility = ViewStates.Visible;
                try {
                    _progressBarDialog.Show();
                    _dateTimeStart = start;
                    _dateTimeEnd = null;
                    _details = null;

                    location.LocationRequested += LocationRequested;
                    await location.StartRequestingLocation();


                    _progressBarDialog.Dismiss();
                } catch (JSONException ex) {
                    ex.PrintStackTrace();
                }
            } else {
                try {

                    location.LocationRequested += LocationRequested;
                    await location.StartRequestingLocation(1000);

                } catch (JSONException ex) {
                    ex.PrintStackTrace();
                }
            }
        }

        private async void LocationRequested(object source , LocationEventArgs args) {
            _progressBarDialog.Show();
            using var locationObj = new JSONObject();
            locationObj.Put("latitude" , args.Location.Latitude);
            locationObj.Put("longitude" , args.Location.Longitude);

            if (_btnScan.Text.Equals("Incepe activitatea")) {
                location.LocationRequested -= LocationRequested;
                await location.StopRequestionLocationUpdates();
                Log.Error("Asist" , "Start");

                _btnScan.Text = "Finalizeaza activitatea";
                JSONObject obj = new JSONObject().Put("QRData" , _qrJsonData.ToString()).Put("Start" , _dateTimeStart).Put("Location" , locationObj.ToString());
                Utils.SetDefaults("ActivityStart" , obj.ToString());

            } else {
                location.LocationRequested -= LocationRequested;
                await location.StopRequestionLocationUpdates();
                Log.Error("Asist" , "Stop");
                using var sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
                string currentDateandTime = sdf.Format(new Date());
                _dateTimeEnd = currentDateandTime;
                _benefitsArray = new JSONArray();
                foreach (SearchListModel t in _selectedBenefits)
                    _benefitsArray.Put(t.Id);

                _details = new JSONObject().Put("benefit" , _benefitsArray).Put("details" , _tbDetails.Text);
                Log.Error("Details" , _details.ToString());


                    Log.Error("before send" , _qrJsonData.ToString());

                    if (isPacientWithoutApp) {
                        JSONObject dataToSend = new JSONObject().Put("dateTimeStart" , _dateTimeStart)
                        .Put("dateTimeStop" , _dateTimeEnd)
                        .Put("imei" , _qrJsonData.GetString("deviceId"))
                        .Put("location" , locationObj).Put("details" , _details);
                        string response = await WebServices.WebServices.Post(Constants.PublicServerAddress + "/api/consultByImei" , dataToSend , Utils.GetDefaults("Token"));
                        Log.Error("Data Payload" , dataToSend.ToString());
                        if (response != null) {
                            var responseJson = new JSONObject(response);
                            switch (responseJson.GetInt("status")) {
                                case 0:
                                    Snackbar.Make(_formContainer , "Nu sunteti la pacient!" , Snackbar.LengthLong).Show();
                                    break;
                                case 1:
                                    Snackbar.Make(_formContainer , "Eroare conectare la server" , Snackbar.LengthLong).Show();
                                    break;
                                case 2:
                                    break;
                            }
                        } else
                            Snackbar.Make(_formContainer , "Nu se poate conecta la server!" , Snackbar.LengthLong).Show();
                    } else {
                        JSONObject dataToSend = new JSONObject().Put("dateTimeStart" , _dateTimeStart)
                        .Put("dateTimeStop" , _dateTimeEnd).Put("qrCodeData" , _qrJsonData)
                        .Put("location" , locationObj).Put("details" , _details);
                        string response = await WebServices.WebServices.Post(Constants.PublicServerAddress + "/api/consult" , dataToSend , Utils.GetDefaults("Token"));
                        if (response != null) {
                            var responseJson = new JSONObject(response);
                            switch (responseJson.GetInt("status")) {
                                case 0:
                                    Snackbar.Make(_formContainer , "Nu sunteti la pacient!" , Snackbar.LengthLong).Show();
                                    break;
                                case 1:
                                    Snackbar.Make(_formContainer , "Eroare conectare la server" , Snackbar.LengthLong).Show();
                                    break;
                                case 2:
                                    break;
                            }
                        } else
                            Snackbar.Make(_formContainer , "Nu se poate conecta la server!" , Snackbar.LengthLong).Show();
                    }
                OnServiceStopped();

            }
            _progressBarDialog.Dismiss();
        }

        public override void OnActivityResult(int requestCode , int resultCode , Intent data) {
            base.OnActivityResult(requestCode , resultCode , data);
            if (resultCode == (int)Android.App.Result.Ok) {
                _selectedBenefits = JsonConvert.DeserializeObject<List<SearchListModel>>(data.GetStringExtra("result"));
                _btnScan.Enabled = FieldsValidation();

                _tbDetails.RequestFocus();
                ShowKeyboard();

                if (_btnScan.Enabled) {
                    StartBlinkingAnimation(Activity, _btnScan);
                }

                _btnBenefits.Text = $"Ati Selectat {_selectedBenefits.Count} beneficii";
            } else {
                Log.Error("Nu avem result" , "User-ul a zis CANCEL");
                _btnBenefits.Text = "Selectati beneficii";
                _selectedBenefits.Clear();
            }
        }

        public async void OnServiceStopped() {
            await location.StopRequestionLocationUpdates();

            Utils.SetDefaults("ActivityStart" , string.Empty);
            Utils.SetDefaults("QrId" , string.Empty);
            Utils.SetDefaults("QrCode" , string.Empty);
            Utils.SetDefaults("readedQR" , string.Empty);


            _formContainer.Visibility = ViewStates.Gone;
            _btnAnulare.Visibility = ViewStates.Gone;
            _btnBloodPressure.Visibility = ViewStates.Gone;
            _btnBloodGlucose.Visibility = ViewStates.Gone;

            _btnScan.Text = "Incepe activitatea";
            _btnBenefits.Text = "Selecteaza beneficii";

            _selectedBenefits.Clear();
            _tbDetails.Text = string.Empty;
            _btnScan.Enabled = true;
            inProgressQRCode = string.Empty;
        }

        public override void OnPause()
        {
            HideKeyboard();
            base.OnPause();
        }

        private void ShowKeyboard()
        {
            var inputMethodManager = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
        }

        private void HideKeyboard()
        {
            var inputMethodManager = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(_tbDetails.WindowToken, 0);
        }

        private void StartBlinkingAnimation(Context context, View view)
        {
            Animation startAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.blink_effect);
            view.StartAnimation(startAnimation);
        }

    }

   

}