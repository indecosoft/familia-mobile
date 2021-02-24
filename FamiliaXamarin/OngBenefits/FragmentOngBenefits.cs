using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using Familia.Asistenta_sociala;
using Familia.Helpers;
using Familia.Location;
using Java.Text;
using Java.Util;
using Newtonsoft.Json;
using Android.Views.Animations;
using Org.Json;
using Fragment = AndroidX.Fragment.App.Fragment;
using ZXingResult = ZXing.Result;
using Android.Views.InputMethods;
using Google.Android.Material.Snackbar;

namespace Familia.OngBenefits {
    public class FragmentOngBenefits : Fragment {
        private EditText _tbDetails;
        private Button _btnScan, _btnCancel, _btnBenefits;
        private ConstraintLayout _formContainer;
        private TextView tvNume;
        private TextView tvPrenume;
        private string _dateTimeStart;
        private ProgressBarDialog _progressBarDialog;
        private List<SearchListModel> _selectedBenefits = new List<SearchListModel>();
        private readonly SimpleDateFormat _dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
        private Dictionary<int, OngBenefitModel> benefitsDictionary;
        private LocationManager location = LocationManager.Instance;
        private double latitude;
        private double longitude;
        private string serverResponse;


        private string _scannedQrCode;

        private void IntiUi(View v) {

            _tbDetails = v.FindViewById<EditText>(Resource.Id.input_details);
            _btnScan = v.FindViewById<Button>(Resource.Id.btnScan);
            _btnCancel = v.FindViewById<Button>(Resource.Id.btnAnulare);

            _btnBenefits = v.FindViewById<Button>(Resource.Id.benefits_button);
            _formContainer = v.FindViewById<ConstraintLayout>(Resource.Id.container);

            _progressBarDialog = new ProgressBarDialog("Va rugam asteptati" , "Datele sunt procesate..." , Activity , false);
            _progressBarDialog.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimary);

            tvNume = v.FindViewById<TextView>(Resource.Id.tvNume);
            tvPrenume = v.FindViewById<TextView>(Resource.Id.tvPrenume);
            
            if (IsActivityInProgress()) {
                _dateTimeStart = Utils.GetDefaults("StartingDateTime");
                _btnScan.Enabled = IsFormValid();
                ShowUi();
            } else {
                HideUi();
            }
        }

        private void InitEvents() {
            _btnCancel.Click += _btnAnulare_Click;
            _btnScan.Click += _btnScan_Click;
            _btnBenefits.Click += _btnBenefits_Click;
            _tbDetails.TextChanged += _tbDetails_TextChanged;
        }

        private void _tbDetails_TextChanged(object sender , Android.Text.TextChangedEventArgs e) {
            _btnScan.Enabled = IsFormValid();
            if (_btnScan.Enabled)
            {
                StartBlinkingAnimation(Activity, _btnScan);
            }
        }

        private async void _btnBenefits_Click(object sender , EventArgs e) {
            Log.Error("AAAAAAAAAAA get benefits", "clicked");
            _progressBarDialog.Show();
            await Task.Run(async () => {
                try {
                   
                    if (serverResponse == null) {
                        return;
                    }

                    Log.Error("AAAAAAAAAAA get benefits", serverResponse);
                    var jsonObjectResponse = new JSONObject(serverResponse);
                    var jsonArrayResponse = jsonObjectResponse.GetJSONArray("beneficii");
                        var items = new List<SearchListModel>();

                        benefitsDictionary = new Dictionary<int, OngBenefitModel>();
                        for (var i = 0; i < jsonArrayResponse.Length(); i++) {

                        var jsonObj = jsonArrayResponse.GetJSONObject(i);
                        var ongModel = new OngBenefitModel(
                            jsonObj.GetInt("id_beneficiu"),
                            jsonObj.GetInt("id_beneficiu_asisoc"),
                            jsonObj.GetInt("idClient"),
                            jsonObj.GetString("nume"),
                            jsonObj.GetString("detalii")
                            );

                        benefitsDictionary.Add(ongModel.idBeneficiu, ongModel);

                            items.Add(new SearchListModel {
                                Id = ongModel.idBeneficiu, 
                                Title = ongModel.nume
                            });
                        }

                        var intent = new Intent(Activity , typeof(SearchListActivity));
                        intent.PutExtra("Items" , JsonConvert.SerializeObject(items));
                        intent.PutExtra("SelectedItems" , JsonConvert.SerializeObject(_selectedBenefits));
                        StartActivityForResult(intent , 1);

                 
                } catch (Exception ex) {
                    Log.Error("Error getting the list of benefits." , ex.Message);
                }
            });
            _progressBarDialog.Dismiss();
        }

        private async void _btnScan_Click(object sender , EventArgs e) {
            ZXingResult result = await Utils.ScanQrCode(Activity);
            if(result is null) {
                return;
            }

            Log.Error("AAAAAAAAAAAA Result qr code" , result.Text);
            if (IsActivityInProgress()) {
                if (IsFormValid()) {
                    if (result.Text == _scannedQrCode) {
                        _progressBarDialog.Show();
                        await SendData(result.Text);
                        _progressBarDialog.Dismiss();

                    } else {
                        Snackbar.Make(_formContainer , "Codul scanat nu corespunde cu cel scanat la inceputul activitatii" , Snackbar.LengthLong).Show();
                    }

                } else {
                    Snackbar.Make(_formContainer , "A fost intampinata o eroare" , Snackbar.LengthLong).Show();
                }

            } else {
                _scannedQrCode = result.Text;
                Utils.SetDefaults("ScannedQrCode" , _scannedQrCode);
                Utils.SetDefaults("StartingDateTime" , _dateFormat.Format(new Date()));
                _btnScan.Enabled = IsFormValid();

                await Task.Run(async () => {
                    JSONObject resultJSON = null;
                    try
                    {
                        resultJSON = new JSONObject(_scannedQrCode);
                        var obj = new JSONObject().Put("idPers", resultJSON.GetString("id_pers"));
                        serverResponse = await WebServices.WebServices.Post($"{Constants.PublicServerAddress}/api/get-asisoc-benefits/", obj,
                                                     Utils.GetDefaults("Token"));

                        if (serverResponse == null) {
                            return;
                        }


                        var jsonReponse = new JSONObject(serverResponse);
                        Activity.RunOnUiThread(() =>
                        {
                            tvNume.Text = jsonReponse.GetString("nume");
                            tvPrenume.Text = jsonReponse.GetString("prenume");
                        });
          

                    }
                    catch (Exception e)
                    {
                        Log.Error("AAAAAAAAA error parsing result json from qr code", e.Message);
                    }
                   
                });

                ShowUi();
            }
        }

        private Tuple<bool , int> IsValueInteger(string scannedValue) {
            bool isInteger = int.TryParse(scannedValue , out int idAsisocPerson);
            return new Tuple<bool , int>(isInteger , idAsisocPerson);
        }

        private async Task SendData(string scannedValue) {

            
            await location.StartRequestingLocation();

            JSONObject resultJSON = null;
            try {
                resultJSON = new JSONObject(scannedValue);
            }
            catch (Exception e) {
                Log.Error("AAAAAAAAA error parsing result json from qr code", e.Message);
            }



            JSONObject payload = new JSONObject()
           //.Put("data" , dateFormat.Format(new Date()))
           .Put("observatii", _tbDetails.Text)
           .Put("idClient", benefitsDictionary.First().Value.idClient)
           .Put("beneficii", GetFormInformation())
           .Put("latitudine", latitude)
           .Put("longitudine", longitude);

            if (resultJSON == null){
                return;
            }

            payload.Put("id_persoana", resultJSON.GetString("id_pers"));

            string response = await WebServices.WebServices.Post($"{Constants.PublicServerAddress}/api/save-asisoc-benefits/" , payload , Utils.GetDefaults("Token"));
            if (response != null) {
                Log.Error("Response" , response);
                var responseJson = new JSONObject(response);
                if (responseJson.Has("message"))
                {
                    Snackbar.Make(_formContainer, responseJson.GetString("message"), Snackbar.LengthLong).Show();
                    Utils.SetDefaults("ScannedQrCode", null);
                    HideUi();
                    ResetForm();
                }
                else {
                    Snackbar.Make(_formContainer, "Eroare de comunicare cu server-ul", Snackbar.LengthLong).Show();
                }
    
            } else {
                Snackbar.Make(_formContainer , "Eroare de comunicare cu server-ul" , Snackbar.LengthLong).Show();
            }
        }

        private void LocationRequested(object source, LocationEventArgs args)
        {
            latitude = args.Location.Latitude;
            longitude = args.Location.Longitude;
            _ = location.StopRequestionLocationUpdates();
        }

        private void _btnAnulare_Click(object sender , EventArgs e) {
            HideUi();
            ResetForm();
        }

        private bool IsActivityInProgress() {
            _scannedQrCode = Utils.GetDefaults("ScannedQrCode");
            _dateTimeStart = Utils.GetDefaults("StartingDateTime");
            return _scannedQrCode != null && _dateTimeStart != null;
        }

        private void HideUi() {
            _btnCancel.Visibility = ViewStates.Gone;
            _formContainer.Visibility = ViewStates.Gone;
        }

        private void ShowUi() {
            _btnCancel.Visibility = ViewStates.Visible;
            _formContainer.Visibility = ViewStates.Visible;
        }

        private void ResetForm() {
            Utils.SetDefaults("ScannedQrCode" , null);
            Utils.SetDefaults("StartingDateTime" , null);
            _tbDetails.Text = string.Empty;
            _selectedBenefits.Clear();
            _btnBenefits.Text = "Selecteaza beneficii";
            _btnScan.Enabled = true;
            tvNume.Text = null;
            tvPrenume.Text = null;
        }

        private JSONArray GetFormInformation() {
            JSONArray benefitsArray = new JSONArray();
            foreach (SearchListModel t in _selectedBenefits) {
                var jsonObj = new JSONObject().Put("id_beneficiu", t.Id);
                if (benefitsDictionary != null) {

                    var obj = benefitsDictionary.GetValueOrDefault(t.Id);
                    jsonObj.Put("id_beneficiu_asisoc", obj.idBeneficiuAsisoc);
                    
                    benefitsArray.Put(jsonObj);
                }
               
            }

            return benefitsArray;
        }

        private bool IsFormValid() {
            return _selectedBenefits.Count > 0 && !string.IsNullOrEmpty(_tbDetails.Text);
        }

        public override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            location.LocationRequested += LocationRequested;
        }

        public override View OnCreateView(LayoutInflater inflater , ViewGroup container , Bundle savedInstanceState) {
            View view = inflater.Inflate(Resource.Layout.fragment_ong_benefits , container , false);
            IntiUi(view);
            InitEvents();
            return view;
        }

        public override void OnActivityResult(int requestCode , int resultCode , Intent data) {
            base.OnActivityResult(requestCode , resultCode , data);
            if (resultCode == (int)Result.Ok) {
                _selectedBenefits = JsonConvert.DeserializeObject<List<SearchListModel>>(data.GetStringExtra("result"));

                if (_selectedBenefits.Count != 0)
                {
                    _tbDetails.RequestFocus();
                    ShowKeyboard();
                }

                _btnScan.Enabled = IsFormValid();

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


        public override void OnPause()
        {
            HideKeyboard();
            base.OnPause();
        }
        public override void OnDestroy() {
            _dateFormat.Dispose();
            base.OnDestroy();
        }

        private void StartBlinkingAnimation(Context context, View view)
        {
            Animation startAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.blink_effect);
            view.StartAnimation(startAnimation);
        }
    }
}
