using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Familia.Chat;
using Familia.Helpers;
using Familia.JsonModels;
using Java.Lang;
using Newtonsoft.Json;
using AndroidX.Fragment.App;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Familia.Active_Conversations
{
    public class ConversationsFragment : Fragment
    {
        private RecyclerView _conversationsRecyclerView;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_conversations, container, false);
            _conversationsRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.conversations);
            try
            {
                // Initialize contacts
                string conv = Utils.GetDefaults("Rooms");
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
                        string name = contacts[args.Position].Username;
                        string room = contacts[args.Position].Room;
                        var intent = new Intent(Activity, typeof(ChatActivity));
                        intent.PutExtra("Room", room);
                        intent.PutExtra("EmailFrom", name);
                        // intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        StartActivity(intent);
                    };
                    adapter.ItemLongClick += delegate(object sender, ConvAdapterClickEventArgs args)
                    {
                        AlertDialog alertDialog =
                            new AlertDialog.Builder(Activity, Resource.Style.AppTheme_Dialog)
                                .Create();
                        alertDialog.SetTitle("Avertisment");
                        alertDialog.SetMessage("Doriti sa stergeti aceasta conversatie?");
                        
                        alertDialog.SetButton((int)DialogButtonType.Positive, "Da", delegate
                        {
                            adapter.DeleteConversation(args.Position);
                            adapter.NotifyDataSetChanged();
                            string serialized = JsonConvert.SerializeObject(contacts);
                            Utils.SetDefaults("Rooms", serialized);
                        });
                        alertDialog.SetButton((int)DialogButtonType.Negative, "Nu" , delegate { });
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