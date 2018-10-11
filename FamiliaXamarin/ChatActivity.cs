﻿using System.Collections.Generic;
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
        private static string RoomName = "";
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_chat);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            mMessages = new List<ChatModel>();

            mAdapter = new ChatAdapter(this, mMessages);
            //            mAdapter.ItemClick += delegate (object sender, int i)
            //            {
            //                Toast.MakeText(this, mMessages[i].Username, ToastLength.Short).Show();
            //            };

            mAdapter.Clear();
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.messages);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerView.SetAdapter(mAdapter);
            //ChangedData();

            mInputMessageView = (EditText)FindViewById(Resource.Id.tbMessage);
            send = FindViewById<Button>(Resource.Id.Send);
            send.Click += delegate { attemptSend(); };
            if (savedInstanceState == null)
            {
                Bundle extras = Intent.Extras;
                if (extras == null)
                {

                }
                else if (extras.GetBoolean("AcceptClick"))
                {
                    try
                    {
                        string SharedRooms = Utils.GetDefaults("Rooms", this);
                        if (!string.IsNullOrEmpty(SharedRooms))
                        {
                            var model = JsonConvert.DeserializeObject<ConverstionsModel>(SharedRooms);

                            if (!model.Conversations.Contains(extras.GetString("EmailFrom")))
                            {
                                model.Conversations.Add(extras.GetString("EmailFrom"));
                                model.Rooms.Add(extras.GetString("Room"));

                            }

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
                        //finish();

                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                    }


                    

                    //finish();
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
                else if (extras.GetBoolean("Conv"))
                {
                    RoomName = extras.GetString("Room");
                    mUsername = extras.GetString("EmailFrom");
                }
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
                    messageToSend = new JSONObject().Put("message", message).Put("username", mUsername).Put("room", RoomName);
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
                //                if (type == 0)
                //                {
                mMessages.Add(new ChatModel { Message = message, Type = type });
                //mMessages.Add(new ChatModel { Username = username, Message = message, Type = ChatModel.TypeMessage });
                //mMessages.Add(new ChatModel.Builder(ChatModel.TypeMessage)
                //                .Username(username).Message(message).Build());
                //mMessages.Add(new ChatModel.Builder(ChatModel.TypeMyMessage)
                //                .Username(username).Message(message).Build());
                //                }
                //                else if (type == 1)
                //                {
                //                    mMessages.Add(new ChatModel.Builder(ChatModel.TypeMyMessage)
                //                        .Username(username).Message(message).Avatar(avatar).Build());
                //                }
                mAdapter.NotifyItemInserted(mMessages.Count - 1);
                mAdapter.NotifyDataSetChanged();
                 scrollToBottom();
            });

        }
        private static void scrollToBottom()
        {
            _recyclerView.ScrollToPosition(mAdapter.ItemCount - 1);
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