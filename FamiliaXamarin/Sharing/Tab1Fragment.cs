using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using ZXing.Mobile;
using System.Threading;
using Android.App;
using Org.Json;

namespace FamiliaXamarin.Sharing
{
    public class Tab1Fragment : Android.Support.V4.App.Fragment, View.IOnClickListener
    {

        private Button btnScan;
        private TextView personFound;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.layout_tab1, container, false);
            btnScan = view.FindViewById<Button>(Resource.Id.btn_scanQR);
            btnScan.Click += BtnScan_Click;
            personFound = view.FindViewById<TextView>(Resource.Id.tv_person_found);
            personFound.SetOnClickListener(this);
           // btnScan.SetOnClickListener(Activity);
            return view;
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
                    scanner.AutoFocus();
                    Thread.Sleep(1000);
                }
            })).Start();
            result = await scanner.Scan(options);
            return result;


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
                var _qrJsonData = new JSONObject(result.Text);

                Log.Error("QR_CODE", _qrJsonData.ToString());
            }
            catch (JSONException ex)
            {
                Toast.MakeText(Activity, "Cod Invalid", ToastLength.Long).Show();
                ex.PrintStackTrace();
            }
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.tv_person_found:
                    openMiniProfileDialog();
                    break;
            }
        }

        private CustomDialogProfileSharingData openMiniProfileDialog()
        {
            CustomDialogProfileSharingData cdd = new CustomDialogProfileSharingData(this.Activity);
           
            cdd.Show();

            return cdd;
        }

    }
}