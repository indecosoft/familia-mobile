using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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
using Java.Lang;
using Org.Json;
using String = System.String;
using Toolbar = Android.Widget.Toolbar;

namespace FamiliaXamarin
{
    [Activity(Label = "ChatActivity")]
    public class ChatActivity : AppCompatActivity
    {

        private Button start;
        private Button send;
        private static  int REQUEST_LOGIN = 0;

        private static  int TYPING_TIMER_LENGTH = 600;

        public static RecyclerView _recyclerView;
        public static EditText mInputMessageView;
        private List<ChatModel> mMessages;
        private  ChatAdapter mAdapter;
        public static string Email;
        public static string RoomName = "";
        public static bool FromNotify = false;
        private bool mTyping = false;
        private Handler mTypingHandler = new Handler();
        private string mUsername;
        private string Token;
        public static string EmailDest;
        public static ChatActivity Ctx;
        public static string Avatar;
        public static string NewMessage = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_chat);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            mMessages = new List<ChatModel>();

            mAdapter = new ChatAdapter(this,mMessages);
            mAdapter.ItemClick += delegate (object sender, int i)
            {
                Toast.MakeText(this, mMessages[i].Username, ToastLength.Short).Show();
            };


            _recyclerView = FindViewById<RecyclerView>(Resource.Id.messages);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerView.SetAdapter(mAdapter);


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


                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    toolbar.NavigationClick += delegate { Finish(); };
          
                //finish();
            } else if (extras.GetBoolean("RejectClick")) {
                var emailFrom = Utils.GetDefaults("Email",this);
                try
                {
                    var mailObject = new JSONObject().Put("from", extras.GetString("EmailFrom")).Put("dest", emailFrom).Put("accepted", false);
                    Log.Error("aici", mailObject.ToString());
                    WebSocketClient.Client.Emit("chat accepted", mailObject);
                    //finish();
                } catch (JSONException e) {
                    e.PrintStackTrace();
                }
                Finish();
            } else if (extras.GetBoolean("Conv")) {

                
            }
        }


            // Create your application here
        }
        private void attemptSend()
        {
            if (!mInputMessageView.Text.Equals(""))
            {
                string message = mInputMessageView.Text;
                mInputMessageView.Text = string.Empty;
                addMessage("Eu", message, 1);
//                JSONObject messageToSend = null;
//                try
//                {
//                    messageToSend = new JSONObject().Put("message", message).Put("username", mUsername).Put("room", RoomName);
//                }
//                catch (JSONException e)
//                {
//                    e.PrintStackTrace();
//                }
                // perform the sending message attempt.
                //WebSocketClient.Client.Emit("send message", messageToSend);
            }
        }
        public void addMessage(string username, string message, int type)
        {

//                if (type == 0)
//                {
                    mMessages.Add(new ChatModel {Username = username, Message = message, Type = ChatModel.TypeMyMessage});
                    mMessages.Add(new ChatModel {Username = username, Message = message, Type = ChatModel.TypeMessage});
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
            


        }
        private void scrollToBottom()
        {
            _recyclerView.ScrollToPosition(mAdapter.ItemCount - 1);
        }
    }
}