using System.Collections.Generic;
using Familia;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Widget;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.JsonModels;
using Newtonsoft.Json;
using Org.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.Chat
{
    public class MessagesModel
    {
        public string Room { get; set; }
        public string Message { get; set; }
    }
    
    [Activity(Label = "ChatActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ChatActivity : AppCompatActivity
    {
        private Button send;

        public static RecyclerView RecyclerView;
        public static EditText mInputMessageView;
        private static List<ChatModel> mMessages;
        private static ChatAdapter mAdapter;
        public static List<MessagesModel> Messages = new List<MessagesModel>();
        public static string RoomName = "";
        private static LinearLayoutManager layoutManager;
        private static string mUsername;
        private static ChatActivity Ctx;

        protected override void OnUserLeaveHint()
        {
            Utils.CloseRunningActivity(typeof(ChatActivity));
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Utils.HideKeyboard(this);
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.PutExtra("FromChat", true);
            StartActivity(intent);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_chat);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;

            mMessages = new List<ChatModel>();
            mAdapter = new ChatAdapter(this, mMessages);
            //mAdapter.ItemClick += delegate (object sender, int i)
            //{
            //    Toast.MakeText(this, mMessages[i].Username, ToastLength.Short).Show();
            //};
            layoutManager = new LinearLayoutManager(this)
                {Orientation = LinearLayoutManager.Vertical};

            //layoutManager.ReverseLayout = true;
            RecyclerView = FindViewById<RecyclerView>(Resource.Id.messages);
            RecyclerView.SetLayoutManager(layoutManager);
            RecyclerView.SetAdapter(mAdapter);

            Ctx = this;
            mInputMessageView = (EditText) FindViewById(Resource.Id.tbMessage);
            send = FindViewById<Button>(Resource.Id.Send);
            send.Click += delegate { AttemptSend(); };

            mAdapter.Clear();
            
            if (savedInstanceState == null)
            {
                var extras = Intent.Extras;
                RoomName = Intent.GetStringExtra("Room");
                mUsername = Intent.GetStringExtra("EmailFrom");
                var ids = RoomName.Split(':');
                NotificationManagerCompat.From(this).Cancel(
                    ids[0] == Utils.GetDefaults("IdClient", this)
                        ? int.Parse(ids[1])
                        : int.Parse(ids[0]));
                //Active = Intent.GetBooleanExtra("Active", false);
                if (extras == null)
                {
                    Finish();
                }
                else if (Intent.GetBooleanExtra("AcceptClick", false))
                {
                    
                    try
                    {
                        //adaugare la lista de prieteni
                        var sharedRooms = Utils.GetDefaults("Rooms", this);
                        if (sharedRooms != null)
                        {
                            var model =
                                JsonConvert.DeserializeObject<List<ConverstionsModel>>(sharedRooms);

                            var currentModel = new ConverstionsModel
                                {Username = mUsername, Room = RoomName};
                            var existingElement = false;
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
                            var currentModel = new ConverstionsModel
                                {Username = mUsername, Room = RoomName};

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
                        var mailObject = new JSONObject().Put("dest", dest)
                            .Put("from", emailFrom)
                            .Put("accepted", true).Put("room", RoomName);
                        Log.Error("aici", mailObject.ToString());
                        WebSocketClient.Client.Emit("chat accepted", mailObject);
                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                    }
                }

                //NotificationManagerCompat.From(this).Cancel(400);

                for (var i = 0; i < Messages.Count; i++)
                {
                    if (Messages[i].Room != RoomName) continue;
                    AddMessage(Messages[i].Message, ChatModel.TypeMessage);
                    Messages.RemoveAt(i--);
                }
            }

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate { OnBackPressed(); };
            Title = mUsername;
        }

        private void AttemptSend()
        {
            if (mInputMessageView.Text.Equals("")) return;
            var message = mInputMessageView.Text;
            mInputMessageView.Text = string.Empty;
            AddMessage(message, ChatModel.TypeMyMessage);
            JSONObject messageToSend = null;
            try
            {
                messageToSend = new JSONObject().Put("message", message)
                    .Put("username", Utils.GetDefaults("Email", this))
                    .Put("room", RoomName);
            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }

            // perform the sending message attempt.
            WebSocketClient.Client.Emit("send message", messageToSend);
        }

        public static void AddMessage(string message, int type)
        {
            Ctx.RunOnUiThread(() =>
            {
                mAdapter.AddMessage(new ChatModel {Message = message, Type = type});
                mAdapter.NotifyDataSetChanged();
                ScrollToBottom();
            });
        }

        private static void ScrollToBottom()
        {
            //_recyclerView.ScrollToPosition(mAdapter.ItemCount-1);
            RecyclerView.SmoothScrollToPosition(mAdapter.ItemCount - 1);
            //layoutManager.ReverseLayout = true;
            Log.Error("positiooooooooooooon: ", mAdapter.ItemCount - 1 + "");
        }
    }
}