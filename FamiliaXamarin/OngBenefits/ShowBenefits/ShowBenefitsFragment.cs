using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.Helpers;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fragment = Android.Support.V4.App.Fragment;
using ZXingResult = ZXing.Result;

namespace Familia.OngBenefits.ShowBenefits
{
    public class ShowBenefitsFragment : Fragment
    {
        public static string KEY_SHOW_BENEFITS = "key show benefits";
        private View containerView;
        private Button btnScanQRCode;
        private RelativeLayout relativeLayout;
        private ProgressBar progressBar;


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            containerView = inflater.Inflate(Resource.Layout.fragment_show_benefits, container, false);
            InitView();
            return containerView;
        }

        public void InitView() {
            btnScanQRCode = containerView.FindViewById<Button>(Resource.Id.btnScanQR);
            btnScanQRCode.Click += OnButtonScanQRCodeClicked;
            relativeLayout = containerView.FindViewById<RelativeLayout>(Resource.Id.rlData);
            progressBar = containerView.FindViewById<ProgressBar>(Resource.Id.progressBar);
            HideUI();
        }

        public async void OnButtonScanQRCodeClicked(object sender, EventArgs e) {
            try
            {
                ZXingResult result = await Utils.ScanQrCode(Activity);
                if (result is null)
                {
                    return;
                }

                var jsonQrCodeScanned = new JSONObject(result.Text);
                var idPers = jsonQrCodeScanned.GetString("id_pers");
                progressBar.Visibility = ViewStates.Visible;
                var jsonServerResponse = await GetBenefits(idPers);
                progressBar.Visibility = ViewStates.Gone;
                ShowUI(jsonServerResponse);
            }
            catch (Exception ex) {
                Log.Error("AAAAAAAA", ex.Message);
            }
        }

        private void ShowUI(JSONObject jsonServerResponse)
        {
            relativeLayout.Visibility = ViewStates.Visible;
            SetFirstAndLastNameInUI(jsonServerResponse);
         
            List<Benefit> list = ParseResponse(jsonServerResponse.GetJSONArray("istoric"));

            var recyclerView = containerView.FindViewById<RecyclerView>(Resource.Id.rvData);
            var layoutManager = new LinearLayoutManager(Activity);
            recyclerView.SetLayoutManager(layoutManager);
            var adapter = new BenefitsAdapter(list);
            recyclerView.SetAdapter(adapter);
        }

        private List<Benefit> ParseResponse(JSONArray jsonArray)
        {
            var list = new List<Benefit>();

            for (int i = 0; i < jsonArray.Length(); i++)
            {
                var jsonItem = jsonArray.GetJSONObject(i);

                var item = new Benefit(
                    jsonItem.GetString("beneficiu"),
                    jsonItem.GetString("detalii"),
                    jsonItem.GetString("data_primirii"),
                    jsonItem.GetString("observatii")
                    );

                list.Add(item);
            }

            return list;
        }

        private void HideUI() {
            relativeLayout.Visibility = ViewStates.Gone;
            // TODO clear adapter
            
        }

        private void SetFirstAndLastNameInUI(JSONObject jsonServerResponse)
        {
            Activity.RunOnUiThread(() =>
            {
                var tvFirstName = containerView.FindViewById<TextView>(Resource.Id.tvFirstName);
                var tvLastName = containerView.FindViewById<TextView>(Resource.Id.tvLastName);
                tvFirstName.Text = jsonServerResponse.GetString("prenume");
                tvLastName.Text = jsonServerResponse.GetString("nume");
            });
        }

        public async Task<JSONObject> GetBenefits(string idPers)
        {
                try
                {
                    var payload = new JSONObject().Put("idPers", idPers);
                    var serverResponse = await WebServices.WebServices.Post($"{Constants.PublicServerAddress}/api/show-asisoc-benefits/", payload,
                                                 Utils.GetDefaults("Token"));

                    if (serverResponse == null) {
                        return null;
                    }
                    Log.Error("AAAAAAAAAA", serverResponse);

                    return new JSONObject(serverResponse); ;
                    
                }
                catch (Exception e)
                {
                    Log.Error("AAAAAAA", e.Message);
                    return null;
                }

        }
    }
}