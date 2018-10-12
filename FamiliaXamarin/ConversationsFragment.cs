using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.JsonModels;
using Java.Lang;
using Org.Json;
using String = System.String;

namespace FamiliaXamarin
{
    public class ConversationsFragment : Fragment
    {
        private RecyclerView conversations;
        JSONArray SharedRooms;


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.fragment_conversations, container, false);
            // Inflate the layout for this fragment
            conversations = view.FindViewById<RecyclerView>(Resource.Id.conversations);



            try
            {
                string Data = Utils.GetDefaults("Rooms", Activity);
                //SharedPreferences mPrefs = PreferenceManager.GetDefaultSharedPreferences(Activity);
                //String Rooms = mPrefs.getString("Rooms", "[]");
               // Log.Error("**************************************", Rooms);
                //SharedRooms = new JSONArray(Rooms);
               // Log.Error("**************************************", SharedRooms.ToString());


                //            JSONObject jsonObject = new JSONObject(Rooms);
                //            Log.e("jsonObject",jsonObject+"");
                //JSONArray jsonArray = jsonObject.getJSONArray();

            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
            // Initialize contacts
            Log.Error("lung", SharedRooms.Length() + "");
            //List<ConverstionsModel> contacts = ConverstionsModel.createContactsList(SharedRooms.Length(), Activity);
            // Create adapter passing in the sample user data
            //ConvAdapter adapter = new ConvAdapter(contacts);
            // Attach the adapter to the recyclerview to populate items
            //conversations.SetAdapter(adapter);
            // Set layout manager to position the items
            //conversations.SetLayoutManager(new LinearLayoutManager(Activity));



//            adapter.ItemClick += delegate(object sender, ConvAdapterClickEventArgs args)
//            {
//                StartActivity(new Intent(Activity, typeof(ChatActivity)));
//            };

            return view;
        }
    }
}