﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Familia;
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
using Com.Bumptech.Glide;
using FamiliaXamarin.Helpers;
using Org.Json;
using Resource = Familia.Resource;
using Familia.Helpers;

namespace FamiliaXamarin.Sharing
{
    public class Tab1Fragment : Android.Support.V4.App.Fragment
    {

        private Button btnScan;
//        private TextView personFound;
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
            return view;
        }
        private async void BtnScan_Click(object sender, EventArgs e)
        {
            var qrJsonData = await Utils.ScanEncryptedQRCode(Activity);
            if (qrJsonData == null) return;
            try
            {
                var dialog = OpenMiniProfileDialog();
                dialog.Name.Text = qrJsonData.GetString("Name");
                Glide.With(this).Load(qrJsonData.GetString("Avatar")).Into(dialog.Image);
                dialog.ButtonConfirm.Click += (o, args) =>
                {
                    Task.Run(async () =>
                    {
                        Log.Error("ListaConexiuni", "send data");
                        Log.Error("qrjsondata", qrJsonData + "  " );
                        var response = await WebServices.Post($"{Constants.PublicServerAddress}/api/newSharingPeople",
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
            CustomDialogProfileSharingData cdd = new CustomDialogProfileSharingData(Activity);

            WindowManagerLayoutParams lp = new WindowManagerLayoutParams();
            lp.CopyFrom(cdd.Window.Attributes);
            lp.Width = ViewGroup.LayoutParams.MatchParent;
            lp.Height = ViewGroup.LayoutParams.MatchParent;

            cdd.Show();
            cdd.Window.Attributes = lp;
            return cdd;
        }

    }
}