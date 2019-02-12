using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using FamiliaXamarin.Chat;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.JsonModels;
using Java.Lang;
using Newtonsoft.Json;

namespace FamiliaXamarin.Active_Conversations
{
    public class ConversationsFragment : Android.Support.V4.App.Fragment
    {
        private RecyclerView _conversationsRecyclerView;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_conversations, container, false);
            _conversationsRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.conversations);
            try
            {

                // Initialize contacts
                var conv = Utils.GetDefaults("Rooms", Activity);
                if (conv != null)
                {
                    var contacts = JsonConvert.DeserializeObject<List<ConverstionsModel>>(conv);
                    // Create adapter passing in the sample user data
                    var adapter = new ConvAdapter(contacts);
                    // Attach the adapter to the recyclerview to populate items
                    _conversationsRecyclerView.SetAdapter(adapter);
                    // Set layout manager to position the items
                    _conversationsRecyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
                    adapter.ItemClick += delegate(object sender, ConvAdapterClickEventArgs args)
                    {
                        var name = contacts[args.Position].Username;
                        var room = contacts[args.Position].Room;
                        var intent = new Intent(Activity, typeof(ChatActivity));
                        intent.PutExtra("Room", room);
                        intent.PutExtra("EmailFrom", name);
                        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        StartActivity(intent);
                    };
                    adapter.ItemLongClick += delegate (object sender, ConvAdapterClickEventArgs args)
                    {

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            var alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dark_Dialog).Create();
                            alertDialog.SetTitle(Html.FromHtml("<p style = 'text-align: center; color: #F47445;'>Avertisment</p>", FromHtmlOptions.ModeLegacy));
                            alertDialog.SetMessage(Html.FromHtml("<br/><p style = 'text-align: center; color: #000000;'>Doriti sa stergeti aceasta conversatie?</p>", FromHtmlOptions.ModeLegacy));
                            alertDialog.SetButton("Da", delegate
                            {
                                adapter.DeleteConversation(args.Position);
                                adapter.NotifyDataSetChanged();
                                var serialized = JsonConvert.SerializeObject(contacts);
                                Utils.SetDefaults("Rooms", serialized, Activity);
                            });
                            alertDialog.SetButton2("Nu", delegate { });
                            alertDialog.Show();
                        }
                        else
                        {
                            var alertDialog = new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dark_Dialog).Create();
                            alertDialog.SetTitle("Avertisment");
                            alertDialog.SetMessage("Doriti sa stergeti aceasta conversatie?");
                            alertDialog.SetButton("Da", delegate
                            {
                                adapter.DeleteConversation(args.Position);
                                adapter.NotifyDataSetChanged();
                                var serialized = JsonConvert.SerializeObject(contacts);
                                Utils.SetDefaults("Rooms", serialized, Activity);
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
            return view;

        }

    }
}