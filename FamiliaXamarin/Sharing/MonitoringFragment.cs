﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.JsonModels;
using Newtonsoft.Json;
using Org.Json;

namespace FamiliaXamarin.Sharing
{
    public class MonitoringFragment : Android.Support.V4.App.Fragment
    {
        private RecyclerView _sharingRecyclerView;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        private async void LoadData()
        {
            try
            {

                // Initialize contacts
                List<SharingModel> contacts = null;
                await Task.Run(async () =>
                {
                    var response = await WebServices.Post($"{Constants.PublicServerAddress}/api/getSharingPeople",
                        new JSONObject().Put("email", Utils.GetDefaults("Email", Activity)),
                            Utils.GetDefaults("Token", Activity));
                    if (!string.IsNullOrEmpty(response))
                    {

                        contacts = JsonConvert.DeserializeObject<List<SharingModel>>(response);
                    }
                });
                if (contacts != null)
                {

                    // Create adapter passing in the sample user data
                    var adapter = new SharingAdapter(contacts);
                    // Attach the adapter to the recyclerview to populate items
                    _sharingRecyclerView.SetAdapter(adapter);
                    // Set layout manager to position the items
                    _sharingRecyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
                    adapter.ItemClick += delegate (object sender, SharingAdapterClickEventArgs args)
                    {
                        var name = contacts[args.Position].Name;
                        var email = contacts[args.Position].Email;
                        var dialog = OpenMiniProfileDialog();
                        dialog.Name.Text = name;
                        Glide.With(this).Load($"{Constants.PublicServerAddress}/{contacts[args.Position].Avatar}").Into(dialog.Image);
                        //                        Picasso.With(Activity)
                        //                            .Load($"{Constants.PublicServerAddress}/{contacts[args.Position].Avatar}")
                        //                            //.Load("https://i.imgur.com/EepDV83.jpg")
                        //                            //.Resize(100, 100)
                        //                            .CenterCrop()
                        //                            .Into(dialog.Image);
                        dialog.ButtonConfirm.Visibility = ViewStates.Gone;
                        dialog.ButtonCancel.Text = "Anuleaza partajarea";
                        dialog.ButtonCancel.Click += delegate (object o, EventArgs eventArgs)
                        {

                            var alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dark_Dialog).Create();
                            alertDialog.SetTitle(Html.FromHtml("<p style = 'text-align: center; color: #F47445;'>Avertisment</p>", FromHtmlOptions.ModeLegacy));
                            alertDialog.SetMessage(Html.FromHtml("<br/><p style = 'text-align: center; color: #000000;'>Doriti sa stergeti aceasta conexiune?</p>", FromHtmlOptions.ModeLegacy));
                            alertDialog.SetButton("Da", delegate
                            {
                                adapter.DeleteItemAt(args.Position);
                                adapter.NotifyDataSetChanged();
                                dialog.Dismiss();
                                //                                var serialized = JsonConvert.SerializeObject(contacts);
                                //                                Utils.SetDefaults("Rooms", serialized, Activity);
                            });
                            alertDialog.SetButton2("Nu", delegate { });
                            alertDialog.Show();

                        };
                    };
                    adapter.ItemLongClick += delegate (object sender, SharingAdapterClickEventArgs args)
                    {

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            var alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dark_Dialog).Create();
                            alertDialog.SetTitle(Html.FromHtml("<p style = 'text-align: center; color: #F47445;'>Avertisment</p>", FromHtmlOptions.ModeLegacy));
                            alertDialog.SetMessage(Html.FromHtml("<br/><p style = 'text-align: center; color: #000000;'>Doriti sa stergeti aceasta conexiune?</p>", FromHtmlOptions.ModeLegacy));
                            alertDialog.SetButton("Da", delegate
                            {
                                adapter.DeleteItemAt(args.Position);
                                adapter.NotifyDataSetChanged();
                                //                                var serialized = JsonConvert.SerializeObject(contacts);
                                //                                Utils.SetDefaults("Rooms", serialized, Activity);
                            });
                            alertDialog.SetButton2("Nu", delegate { });
                            alertDialog.Show();
                        }
                        else
                        {
                            var alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dark_Dialog).Create();
                            alertDialog.SetTitle("Avertisment");
                            alertDialog.SetMessage("Doriti sa stergeti aceasta conexiune?");
                            alertDialog.SetButton("Da", delegate
                            {
                                adapter.DeleteItemAt(args.Position);
                                adapter.NotifyDataSetChanged();
                                //                                var serialized = JsonConvert.SerializeObject(contacts);
                                //                                Utils.SetDefaults("Rooms", serialized, Activity);
                            });
                            alertDialog.SetButton2("Nu", delegate
                            {
                                //just close dialog

                            });
                            alertDialog.Show();
                        }

                    };
                }

            }
            catch (Exception e)
            {
                //e.PrintStackTrace();
            }
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.fragment_monitoring, container, false);
            _sharingRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.rv_persons);
            LoadData();
            return view;
        }
        private CustomDialogProfileSharingData OpenMiniProfileDialog()
        {
            CustomDialogProfileSharingData cdd = new CustomDialogProfileSharingData(Activity);

            IWindowManager windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

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