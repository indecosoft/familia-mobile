using System;
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
using Familia;
using Familia.Sharing;
using FamiliaXamarin.Active_Conversations;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.JsonModels;
using Newtonsoft.Json;
using Org.Json;
using Exception = Java.Lang.Exception;
using Fragment = Android.Support.V4.App.Fragment;

namespace FamiliaXamarin.Sharing
{
    public class Tab2Fragment : Fragment
    {
        private RecyclerView _sharingRecyclerView;
        private RecyclerView _sharingRecyclerViewReceived;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        private async void LoadData()
        {
            try
            {
                ProgressBarDialog progressBarDialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", Activity, false);
                progressBarDialog.Show();
                // Initialize contacts
                List<SharingModel> contacts = null;
                await Task.Run(async () =>
                {
                    var response = await WebServices.Post($"{Constants.PublicServerAddress}/api/getSharedPeople",
                        new JSONObject().Put("id", Utils.GetDefaults("IdClient")),
                            Utils.GetDefaults("Token"));
                    if (!string.IsNullOrEmpty(response))
                    {

                        contacts = JsonConvert.DeserializeObject<List<SharingModel>>(response);
                        Log.Error("ListaConexiuni", "No. of contacts: " + contacts.Count);
                    }
                });


                Activity.RunOnUiThread(() =>
                {
                    progressBarDialog.Dismiss();
                });



                if (contacts != null)
                {

                    // Create adapter passing in the sample user data
                    var adapter = new SharingAdapter(contacts);
                    // Attach the adapter to the recyclerview to populate items
                    _sharingRecyclerView.SetAdapter(adapter);
                    // Set layout manager to position the items
                    _sharingRecyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
                    adapter.ItemClick += delegate(object sender, SharingAdapterClickEventArgs args)
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
                        dialog.ButtonCancel.Text = "Sterge";
                        dialog.ButtonCancel.SetPadding(5,5,5,5);
                        dialog.ButtonCancel.Click += delegate(object o, EventArgs eventArgs)
                        {

                            var alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dialog).Create();
                            alertDialog.SetTitle("Avertisment");
                            alertDialog.SetMessage("Doriti sa stergeti aceasta conexiune?");
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
                            var alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dialog).Create();
                            alertDialog.SetTitle("Avertisment");
                            alertDialog.SetMessage("Doriti sa stergeti aceasta conexiune?");
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
                            var alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dialog).Create();
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
                e.PrintStackTrace();
            }
        }



        private async void LoadDataReceived()
        {
            try
            {
                ProgressBarDialog dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", Activity, false);
                dialog.Show();
                // Initialize contacts
                List<SharingModel> contacts = null;
                await Task.Run(async () =>
                {
                    var response = await WebServices.Post($"{Constants.PublicServerAddress}/api/getSharingPeople",
                        new JSONObject().Put("email", Utils.GetDefaults("Email")),
                            Utils.GetDefaults("Token"));
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
                    _sharingRecyclerViewReceived.SetAdapter(adapter);
                    // Set layout manager to position the items
                    _sharingRecyclerViewReceived.SetLayoutManager(new LinearLayoutManager(Activity));
                    adapter.ItemClick += delegate (object sender, SharingAdapterClickEventArgs args)
                    {
                        var name = contacts[args.Position].Name;
                        var email = contacts[args.Position].Email;
                        var imei = contacts[args.Position].Imei;
                        var intent = new Intent(Activity, typeof(SharingMenuActivity));
                        intent.PutExtra("Name", name);
                        intent.PutExtra("Email", email);
                        intent.PutExtra("Imei", imei);
                        StartActivity(intent);
                    };

                }

                Activity.RunOnUiThread(() =>
                {
                    dialog.Dismiss();
                });


            }
            catch (Java.Lang.Exception e)
            {
                e.PrintStackTrace();
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            View view = inflater.Inflate(Resource.Layout.layout_tab2, container, false);

            _sharingRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.rv_persons);
            _sharingRecyclerViewReceived = view.FindViewById<RecyclerView>(Resource.Id.rv_persons_received);
            LoadData();
           LoadDataReceived();

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