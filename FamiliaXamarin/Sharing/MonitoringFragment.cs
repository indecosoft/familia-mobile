using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Familia.Helpers;
using Familia.JsonModels;
using Java.Lang;
using Newtonsoft.Json;
using Org.Json;

namespace Familia.Sharing
{
    public class MonitoringFragment : Fragment
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
                var dialog = new ProgressBarDialog("Asteptati", "Se incarca datele...", Activity, false);
                dialog.Show();
                // Initialize contacts
                List<SharingModel> contacts = null;
                await Task.Run(async () =>
                {
                    string response = await WebServices.WebServices.Post("/api/getSharingPeople",
                        new JSONObject().Put("email", Utils.GetDefaults("Email")),
                            Utils.GetDefaults("Token"));
                    if (!string.IsNullOrEmpty(response))
                    {

                        contacts = JsonConvert.DeserializeObject<List<SharingModel>>(response);
                    }
                });
                if (contacts != null)
                {
                    var adapter = new SharingAdapter(contacts);
                    _sharingRecyclerView.SetAdapter(adapter);
                    _sharingRecyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
                    adapter.ItemClick += delegate (object sender, SharingAdapterClickEventArgs args)
                    {
                        string name = contacts[args.Position].Name;
                        string email = contacts[args.Position].Email;
                        string imei = contacts[args.Position].Imei;
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
            catch (Exception e)
            {
                e.PrintStackTrace();
            }
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_monitoring, container, false);
            _sharingRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.rv_persons);
            LoadData();
            return view;
        }
        private CustomDialogProfileSharingData OpenMiniProfileDialog()
        {
            var cdd = new CustomDialogProfileSharingData(Activity);

            var windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

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