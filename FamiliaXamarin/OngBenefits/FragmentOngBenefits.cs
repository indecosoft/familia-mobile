
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.Asistenta_sociala;
using Familia.Helpers;
using Java.Text;
using Java.Util;
using Newtonsoft.Json;
using Org.Json;
using Fragment = Android.Support.V4.App.Fragment;
using ZXingResult = ZXing.Result;

namespace Familia.OngBenefits {
    public class FragmentOngBenefits : Fragment {


        private EditText _tbDetails;
        private Button _btnScan, _btnCancel, _btnBenefits;
        private ConstraintLayout _formContainer;
        private string _dateTimeStart;
        private ProgressBarDialog _progressBarDialog;
        private List<SearchListModel> _selectedBenefits = new List<SearchListModel>();
        private readonly SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");

        private string scannedQrCode;

        private void IntiUi(View v) {

            _tbDetails = v.FindViewById<EditText>(Resource.Id.input_details);
            _btnScan = v.FindViewById<Button>(Resource.Id.btnScan);
            _btnCancel = v.FindViewById<Button>(Resource.Id.btnAnulare);


            _btnBenefits = v.FindViewById<Button>(Resource.Id.benefits_button);
            _formContainer = v.FindViewById<ConstraintLayout>(Resource.Id.container);

            _progressBarDialog = new ProgressBarDialog("Va rugam asteptati" , "Datele sunt procesate..." , Activity , false);
            _progressBarDialog.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimary);
            if (IsActivityInProgress()) {
                _dateTimeStart = Utils.GetDefaults("StartingDateTime");
                _btnScan.Enabled = IsFormValid();
                ShowUI();
            } else {
                HideUI();
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
        }

        private async void _btnBenefits_Click(object sender , EventArgs e) {
            _progressBarDialog.Show();
            await Task.Run(async () => {
                try {
                    string response = await WebServices.WebServices.Get($"{Constants.PublicServerAddress}/api/selfRegisteredBenefits/" ,
                                                            Utils.GetDefaults("Token"));

                    var jsonResponse = new JSONObject(response);
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

        private async void _btnScan_Click(object sender , EventArgs e) {
            ZXingResult result = await Utils.ScanQrCode(Activity);
            if(result is null) {
                return;
            }
            if (!IsValueInteger(result.Text).Item1) {
                Snackbar.Make(_formContainer , "Codul scanat nu este valid" , Snackbar.LengthLong).Show();
                return;
            }
            Log.Debug("Result" , result.Text);
            if (IsActivityInProgress()) {
                if (IsFormValid()) {
                    if (result.Text == scannedQrCode) {
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
                scannedQrCode = result.Text;
                Utils.SetDefaults("ScannedQrCode" , scannedQrCode);
                Utils.SetDefaults("StartingDateTime" , dateFormat.Format(new Date()));
                _btnScan.Enabled = IsFormValid();
                ShowUI();
            }
        }

        private Tuple<bool , int> IsValueInteger(string scannedValue) {
            bool isInteger = int.TryParse(scannedValue , out int idAsisocPerson);
            return new Tuple<bool , int>(isInteger , idAsisocPerson);
        }

        private async Task SendData(string scannedValue) {
            JSONObject payload = new JSONObject()
           .Put("data" , dateFormat.Format(new Date()))
           .Put("obs" , _tbDetails.Text)
           .Put("idPersAsisoc" , IsValueInteger(scannedValue).Item2)
           .Put("benefits" , GetFormInformation());
            Log.Error("Token" , Utils.GetDefaults("Token") + "");

            string response = await WebServices.WebServices.Post($"{Constants.PublicServerAddress}/api/selfRegisteredBenefits/" , payload , Utils.GetDefaults("Token"));
            if (response != null) {
                Log.Error("Response" , response);
                var responseJson = new JSONObject(response);
                switch (responseJson.GetInt("status")) {
                    case 1:
                        Snackbar.Make(_formContainer , "Eroare de comunicare cu server-ul" , Snackbar.LengthLong).Show();
                        break;
                    case 2:
                        Log.Debug("Activity in progress" , "Should send data and reset form.");
                        Log.Debug("Payload" , payload.ToString());
                        Utils.SetDefaults("ScannedQrCode" , null);
                        HideUI();
                        ResetForm();
                        break;
                }
            } else {
                Snackbar.Make(_formContainer , "Eroare de comunicare cu server-ul" , Snackbar.LengthLong).Show();
            }
        }

        private void _btnAnulare_Click(object sender , EventArgs e) {
            HideUI();
            ResetForm();
        }

        private bool IsActivityInProgress() {
            scannedQrCode = Utils.GetDefaults("ScannedQrCode");
            _dateTimeStart = Utils.GetDefaults("StartingDateTime");
            return scannedQrCode != null && _dateTimeStart != null;
        }

        private void HideUI() {
            _btnCancel.Visibility = ViewStates.Gone;
            _formContainer.Visibility = ViewStates.Gone;
        }

        private void ShowUI() {
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
        }

        private JSONArray GetFormInformation() {
            JSONArray benefitsArray = new JSONArray();
            foreach (SearchListModel t in _selectedBenefits) {
                benefitsArray.Put(t.Id);
            }

            return benefitsArray;
        }

        private bool IsFormValid() {
            return _selectedBenefits.Count > 0 && !string.IsNullOrEmpty(_tbDetails.Text);
        }

        public override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

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
                _btnScan.Enabled = IsFormValid();
                _btnBenefits.Text = $"Ati Selectat {_selectedBenefits.Count} beneficii";
            } else {
                Log.Error("Nu avem result" , "User-ul a zis CANCEL");
                _btnBenefits.Text = "Selectati beneficii";
                _selectedBenefits.Clear();
            }
        }

        public override void OnDestroy() {
            dateFormat.Dispose();
            base.OnDestroy();
        }
    }
}
