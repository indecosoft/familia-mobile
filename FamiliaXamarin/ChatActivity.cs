using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
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
using FamiliaXamarin.JsonModels;
using Java.Lang;
using Newtonsoft.Json;
using Org.Json;
using String = System.String;
using Toolbar = Android.Widget.Toolbar;

namespace FamiliaXamarin
{
    [Activity(Label = "ChatActivity", Theme = "@style/AppTheme.Dark")]
    public class ChatActivity : AppCompatActivity
    {

        private Button start;
        private Button send;
        private static int REQUEST_LOGIN = 0;

        private static int TYPING_TIMER_LENGTH = 600;

        public static RecyclerView _recyclerView;
        public static EditText mInputMessageView;
        private static List<ChatModel> mMessages;
        private static ChatAdapter mAdapter;
        public static string Email;
        public static string RoomName = "";

        public static bool Active = false;
        public static bool FromNotify = false;
        private bool mTyping = false;
        private Handler mTypingHandler = new Handler();
        private static string mUsername;
        private static string Token;
        public static string EmailDest;
        private static ChatActivity Ctx;
        public static string Avatar;
        public static string NewMessage = "";
        IWebSocketClient _socketClient = new WebSocketClient();


        protected override void OnPause()
        {
            Active = false;
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
            //OnBackPressed();
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
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            mMessages = new List<ChatModel>();
            Active = true;
            mAdapter = new ChatAdapter(this, mMessages);
            //            mAdapter.ItemClick += delegate (object sender, int i)
            //            {
            //                Toast.MakeText(this, mMessages[i].Username, ToastLength.Short).Show();
            //            };
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.messages);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerView.SetAdapter(mAdapter);
            Ctx = this;
            mInputMessageView = (EditText)FindViewById(Resource.Id.tbMessage);
            send = FindViewById<Button>(Resource.Id.Send);
            send.Click += delegate { attemptSend(); };
            NotificationManagerCompat.From(this).Cancel(100);

            if (savedInstanceState == null)
            {Bundle extras = Intent.Extras;
                if (extras == null)
                {

                }
                else if (extras.GetBoolean("AcceptClick"))
                {
                    try
                    {
                        //adaugare la lista de pretini
                        string SharedRooms = Utils.GetDefaults("Rooms", this);
                        //if (!string.IsNullOrEmpty(SharedRooms))
                        if (SharedRooms != null)
                            {
                                
                            var model = JsonConvert.DeserializeObject<List<ConverstionsModel>>(SharedRooms);

                            var currentModel = new ConverstionsModel { Username = extras.GetString("EmailFrom"), Room = extras.GetString("Room") };
                            bool existingElement = false;
                            foreach (var conversation in model)
                            {
                                if (conversation.Username.Equals(currentModel.Username))
                                {
                                    existingElement = true;
                                    break;
                                }
                            }
                            if (!existingElement)
                            {
                                model.Add(currentModel);
                            }

                            string serialized = JsonConvert.SerializeObject(model);
                            Utils.SetDefaults("Rooms", serialized, this);
                        }
                        else
                        {
                            List<ConverstionsModel> model = new List<ConverstionsModel>();
                            var currentModel = new ConverstionsModel { Username = extras.GetString("EmailFrom"), Room = extras.GetString("Room") };
                      
                            model.Add(currentModel);
                          

                            string serialized = JsonConvert.SerializeObject(model);
                            Utils.SetDefaults("Rooms", serialized, this);
                        }
                    }
                    catch
                    {
                        //ignored
                    }
                    RoomName = extras.GetString("Room");
                    mUsername = extras.GetString("EmailFrom");
                    var emailFrom = Utils.GetDefaults("Email", this);
                    try
                    {
                        var dest = extras.GetString("EmailFrom");
                        var mailObject = new JSONObject().Put("dest", dest).Put("from", emailFrom).Put("accepted", true);
                        Log.Error("aici", mailObject.ToString());
                        WebSocketClient.Client.Emit("chat accepted", mailObject);
                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                    }
                }
                else if (extras.GetBoolean("RejectClick"))
                {
                    var emailFrom = Utils.GetDefaults("Email", this);
                    RoomName = extras.GetString("Room");
                    try
                    {
                        var mailObject = new JSONObject().Put("from", extras.GetString("EmailFrom")).Put("dest", emailFrom).Put("accepted", false);
                        Log.Error("aici", mailObject.ToString());
                        WebSocketClient.Client.Emit("chat accepted", mailObject);
                        //finish();
                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                    }
                    Finish();
                }
      
                RoomName = extras.GetString("Room");
                mUsername = extras.GetString("EmailFrom");
                if(extras.ContainsKey("NewMessage"))
                    addMessage(extras.GetString("NewMessage"), ChatModel.TypeMessage);
            }
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate { Finish(); };
            Title = mUsername;


            // Create your application here
        }
        private void attemptSend()
        {
            if (!mInputMessageView.Text.Equals(""))
            {
                string message = mInputMessageView.Text;
                mInputMessageView.Text = string.Empty;
                addMessage(message, ChatModel.TypeMyMessage);
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
        public static void addMessage(string message, int type)
        {
                    Ctx.RunOnUiThread(() =>
                    {
                        mAdapter.AddMessage(new ChatModel { Message = message, Type = type });
                        mAdapter.NotifyDataSetChanged();
                        scrollToBottom();
                    });
        }
        private static void scrollToBottom()
        {
            _recyclerView.ScrollToPosition(mAdapter.ItemCount);
        }
        void ChangedData()
        {
            Task.Delay(100).ContinueWith(t =>
            {
                mAdapter.NotifyDataSetChanged();
                ChangedData();//This is for repeate every 5s.
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}