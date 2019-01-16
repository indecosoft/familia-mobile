using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.JsonModels;
using Java.Lang;
using Newtonsoft.Json;
using Org.Json;
using String = System.String;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin
{
    [Activity(Label = "ChatActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ChatActivity : AppCompatActivity
    {
        Button send;

        public static RecyclerView _recyclerView;
        public static EditText mInputMessageView;
        static List<ChatModel> mMessages;
        static ChatAdapter mAdapter;
        public static string Email;
        public static string RoomName = "";
        private static LinearLayoutManager layoutManager;
        public static bool Active;

        readonly Handler mTypingHandler = new Handler();
        static string mUsername;
        public static string EmailDest;
        static ChatActivity Ctx;
        public static string Avatar;
        public static string NewMessage = "";
        readonly IWebSocketClient _socketClient = new WebSocketClient();

        protected override void OnStart()
        {
            base.OnStart();
            Active = true;
        }

        protected override void OnPause()
        {
            //Active = false;
            base.OnPause();
        }
        protected override void OnResume()
        {
            base.OnResume();
            Active = true;
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            Active = false;
            mAdapter.Clear();
        }

        protected override void OnPostResume()
        {
            Active = true;
            base.OnPostResume();
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Utils.HideKeyboard(this);
            Finish();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_chat);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            mMessages = new List<ChatModel>();
            Active = true;
            mAdapter = new ChatAdapter(this, mMessages);
            //mAdapter.ItemClick += delegate (object sender, int i)
            //{
            //    Toast.MakeText(this, mMessages[i].Username, ToastLength.Short).Show();
            //};
            layoutManager = new LinearLayoutManager(this) {Orientation = LinearLayoutManager.Vertical};

            //layoutManager.ReverseLayout = true;
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.messages);
            _recyclerView.SetLayoutManager(layoutManager);
            _recyclerView.SetAdapter(mAdapter);
           
            Ctx = this;
            mInputMessageView = (EditText)FindViewById(Resource.Id.tbMessage);
            send = FindViewById<Button>(Resource.Id.Send);
            send.Click += delegate { AttemptSend(); };
            


            if (savedInstanceState == null)
            {
                Bundle extras = Intent.Extras;
                RoomName = Intent.GetStringExtra("Room");
                mUsername = Intent.GetStringExtra("EmailFrom");
                Active = Intent.GetBooleanExtra("Active", false);
                if (extras == null)
                {
                    Finish();
                }
                else
                if (Intent.GetBooleanExtra("RejectClick",false))
                {
                    NotificationManagerCompat.From(this).Cancel(2);
                    var emailFrom = Utils.GetDefaults("Email", this);
                    RoomName = extras.GetString("Room");
                    try
                    {
                        var mailObject = new JSONObject().Put("dest", mUsername).Put("from", emailFrom).Put("accepted", false);
                        Log.Error("aici", mailObject.ToString());
                        WebSocketClient.Client.Emit("chat accepted", mailObject);
                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                    }
                    Finish();
                }
                else if (Intent.GetBooleanExtra("AcceptClick", false))
                {
                    NotificationManagerCompat.From(this).Cancel(2);
                    try
                    {
                        //adaugare la lista de prieteni
                        var SharedRooms = Utils.GetDefaults("Rooms", this);
                        if (SharedRooms != null)
                        {

                            var model = JsonConvert.DeserializeObject<List<ConverstionsModel>>(SharedRooms);

                            var currentModel = new ConverstionsModel { Username = mUsername, Room = RoomName };
                            bool existingElement = false;
                            foreach (var conversation in model)
                            {
                                if (!conversation.Username.Equals(currentModel.Username)) continue;
                                existingElement = true;
                                break;
                            }
                            if (!existingElement)
                            {
                                model.Add(currentModel);
                            }

                            var serialized = JsonConvert.SerializeObject(model);
                            Utils.SetDefaults("Rooms", serialized, this);
                        }
                        else
                        {
                            var model = new List<ConverstionsModel>();
                            var currentModel = new ConverstionsModel { Username = mUsername, Room = RoomName };

                            model.Add(currentModel);

                            var serialized = JsonConvert.SerializeObject(model);
                            Utils.SetDefaults("Rooms", serialized, this);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("Error", e.Message);
                    }
                    var emailFrom = Utils.GetDefaults("Email", this);
                    try
                    {
                        var dest = extras.GetString("EmailFrom");
                        var mailObject = new JSONObject().Put("dest", dest).Put("from", emailFrom).Put("accepted", true).Put("room", RoomName);
                        Log.Error("aici", mailObject.ToString());
                        WebSocketClient.Client.Emit("chat accepted", mailObject);
                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                    }
                }
                NotificationManagerCompat.From(this).Cancel(4);


                if (extras != null && extras.ContainsKey("NewMessage"))
                {
                    AddMessage(extras.GetString("NewMessage"), ChatModel.TypeMessage);
                }
            }
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate { Finish(); };
            Title = mUsername;
        }
        void AttemptSend()
        {
            if (!mInputMessageView.Text.Equals(""))
            {
                var message = mInputMessageView.Text;
                mInputMessageView.Text = string.Empty;
                AddMessage(message, ChatModel.TypeMyMessage);
                JSONObject messageToSend = null;
                try
                {
                    messageToSend = new JSONObject().Put("message", message).Put("username", Utils.GetDefaults("Email", this)).Put("room", RoomName);
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
                // perform the sending message attempt.
                WebSocketClient.Client.Emit("send message", messageToSend);
            }
        }
        public static void AddMessage(string message, int type)
        {
            Ctx.RunOnUiThread(() =>
            {
                mAdapter.AddMessage(new ChatModel { Message = message, Type = type });
                mAdapter.NotifyDataSetChanged();
                ScrollToBottom();
            });
        }
        static void ScrollToBottom()
        {
            //_recyclerView.ScrollToPosition(mAdapter.ItemCount-1);
            _recyclerView.SmoothScrollToPosition(mAdapter.ItemCount - 1);
            //layoutManager.ReverseLayout = true;
            Log.Error("positiooooooooooooon: ", mAdapter.ItemCount - 1 + "");
        }

    }
}