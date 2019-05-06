using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using FamiliaXamarin.Active_Conversations;
using FamiliaXamarin.Chat;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.JsonModels;
using Java.Lang;
using Newtonsoft.Json;

namespace Familia.Active_Conversations
{
    public class ConversationsFragment : Android.Support.V4.App.Fragment
    {
        private RecyclerView _conversationsRecyclerView;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_conversations, container, false);
            _conversationsRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.conversations);
            try
            {
                // Initialize contacts
                var conv = Utils.GetDefaults("Rooms");
                if (conv != null)
                {
                    var contacts = JsonConvert.DeserializeObject<List<ConverstionsModel>>(conv);
                    // Create adapter passing in the sample user data
                    var adapter = new ConvAdapter(contacts);
                    // Attach the adapter to the recyclerview to populate items
                    _conversationsRecyclerView.SetAdapter(adapter);
                    // Set layout manager to position the items
                    _conversationsRecyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
                    adapter.ItemClick += (sender, args) =>
                    {
                        var name = contacts[args.Position].Username;
                        var room = contacts[args.Position].Room;
                        var intent = new Intent(Activity, typeof(ChatActivity));
                        intent.PutExtra("Room", room);
                        intent.PutExtra("EmailFrom", name);
                        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        StartActivity(intent);
                    };
                    adapter.ItemLongClick += delegate(object sender, ConvAdapterClickEventArgs args)
                    {
                        var alertDialog =
                            new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dialog)
                                .Create();
                        alertDialog.SetTitle("Avertisment");
                        alertDialog.SetMessage("Doriti sa stergeti aceasta conversatie?");
                        alertDialog.SetButton("Da", delegate
                        {
                            adapter.DeleteConversation(args.Position);
                            adapter.NotifyDataSetChanged();
                            var serialized = JsonConvert.SerializeObject(contacts);
                            Utils.SetDefaults("Rooms", serialized);
                        });
                        alertDialog.SetButton2("Nu", delegate { });
                        alertDialog.Show();
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