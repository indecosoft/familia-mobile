using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Com.Bumptech.Glide;
using Familia.Helpers;
using Org.Json;

namespace Familia.Sharing
{
    public class Tab1Fragment : Fragment
    {

        private Button _btnScan;
//        private TextView personFound;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.layout_tab1, container, false);
            _btnScan = view.FindViewById<Button>(Resource.Id.btn_scanQR);
            _btnScan.Click += BtnScan_Click;
            return view;
        }
        private async void BtnScan_Click(object sender, EventArgs e)
        {
            JSONObject qrJsonData = await Utils.ScanEncryptedQrCode(Activity);
            if (qrJsonData == null) return;
            try
            {
                CustomDialogProfileSharingData dialog = OpenMiniProfileDialog();
                dialog.Name.Text = qrJsonData.GetString("Name");
                Glide.With(Activity).Load(qrJsonData.GetString("Avatar")).Into(dialog.Image);
                dialog.ButtonConfirm.Click += (o, args) =>
                {
                    Task.Run(async () =>
                    {
                        Log.Error("ListaConexiuni", "send data");
                        Log.Error("qrjsondata", qrJsonData + "  " );
                        var response = await WebServices.WebServices.Post("/api/newSharingPeople",
                            new JSONObject().Put("from", qrJsonData.GetString("Id")).Put("dest", Utils.GetDefaults("Id")), Utils.GetDefaults("Token"));
                        Log.Error("SharingData scan", response + " " );
                        if (!string.IsNullOrEmpty(response))
                        {
                            Log.Error("SharingData scan", response);
                           // Activity.RunOnUiThread(() => Toast.MakeText(Activity, response, ToastLength.Long).Show());
                        }

                         //Activity.RunOnUiThread(() => );

                    }).Wait();
                    dialog.Dismiss();


                };dialog.ButtonCancel.Click += (o, args) =>
                {
                    
                    dialog.Dismiss();
                };
            }
            catch (JSONException ex)
            {
                Toast.MakeText(Activity, "Cod Invalid", ToastLength.Long).Show();
                ex.PrintStackTrace();
            }
        }

        private CustomDialogProfileSharingData OpenMiniProfileDialog()
        {
            var cdd = new CustomDialogProfileSharingData(Activity);

            var lp = new WindowManagerLayoutParams();
            lp.CopyFrom(cdd.Window.Attributes);
            lp.Width = ViewGroup.LayoutParams.MatchParent;
            lp.Height = ViewGroup.LayoutParams.MatchParent;

            cdd.Show();
            cdd.Window.Attributes = lp;
            return cdd;
        }

    }
}