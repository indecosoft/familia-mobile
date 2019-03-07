using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.JsonModels;
using FamiliaXamarin.Sharing;
using Newtonsoft.Json;
using Org.Json;

namespace Familia.Sharing
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
                        var imei = contacts[args.Position].Imei;
                        var intent = new Intent(Activity, typeof(SharingMenuActivity));
                        intent.PutExtra("Name", name);
                        intent.PutExtra("Email", email);
                        intent.PutExtra("Imei", imei);
                        StartActivity(intent);

                    };
                   
                }

            }
            catch (Java.Lang.Exception e)
            {
                e.PrintStackTrace();
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