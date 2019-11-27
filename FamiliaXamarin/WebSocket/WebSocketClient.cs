using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Util;
using EngineIO;
using EngineIO.Client;
using Familia.DataModels;
using FamiliaXamarin.Chat;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.JsonModels;
using Java.Lang;
using Newtonsoft.Json;
using Org.Json;
using SocketIO.Client;
using static EngineIO.Emitter;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Socket = SocketIO.Client.Socket;

namespace FamiliaXamarin
{
    public class WebSocketClient : Object, IWebSocketClient, IListener
    {
        Socket _socket;
        public static Socket Client;

        Context _context;
        private SqlHelper<ConversationsRecords> _conversationsRecords;


        public async Task Connect(string hostname, int port, Context context)
        {
            _conversationsRecords = await SqlHelper<ConversationsRecords>.CreateAsync();
            _context = context;
            try
            {

                var options = new IO.Options
                {
                    ForceNew = true,
                    Reconnection = true,
                    //Secure = false,
                    //Transports = new string[] { EngineIO.Client.Transports.PollingXHR.TransportName},
                    Query = $"token={Utils.GetDefaults("Token")}&imei={Utils.GetDeviceIdentificator(Application.Context)}"
                    
                };
                _socket = IO.Socket(hostname,options);

                _socket.On(Socket.EventConnect, OnConnect);
                _socket.On(Socket.EventDisconnect, OnDisconnect);
                _socket.On(Socket.EventConnectError, OnConnectError);
                _socket.On(Socket.EventConnectTimeout, OnConnectTimeout);
                //_socket.On(Manager.EventTransport, OnTransport);

                _socket.On("conversation", OnConversation);
                _socket.On("chat request", OnChatRequest);
                _socket.On("chat accepted", OnChatAccepted);
                _socket.On("chat rejected", OnChatRejected);
                _socket.On("Error", OnError);

                _socket.Connect();
                Client = _socket;
            }
            catch (Exception e)
            {     
                Log.Error("WSConnectionError: ", e.ToString());
            }
        }

        private void OnError(Object[] obj)
        {

        }


        public static void Disconect()
        {      
            Client?.Disconnect();
        }

        public void Emit(string eventName, JSONObject value)
        {
            _socket?.Emit(eventName, value);
        }

        private  void OnConnect(Object[] obj)
        { 
            Log.Error("WebSocket", "Client Connected");
        }

        private  void OnDisconnect(Object[] obj)
        {
            Log.Error("WebSocket", "Client Diconnected");
        }

        private  void OnConnectError(Object[] obj)
        {
            Log.Error("WebSocket", "Connection Error" + obj[0]);
        }

        private  void OnConnectTimeout(Object[] obj)
        {
            Log.Error("WebSocket", "Connection Timeout");
        }
        private void OnTransport(Object[] obj)
        {
            Transport transport = (Transport)obj[0];
            transport.On(Transport.EventRequestHeaders, this);

        }

        private async void OnConversation(Object[] obj)
        {
            Log.Error("WebSocket", "OnConversation");
            var data = (JSONObject)obj[0];
            string room;
            string message;
            string username;
            try
            {
                username = data.GetString("username");
                message = data.GetString("message");
                room = data.GetString("room");
            }
            catch (JSONException e)
            {
                Log.Error("message send error: ", e.Message);
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    await _conversationsRecords.Insert(new ConversationsRecords
                    {
                        Message = message,
                        Room = room,
                        MessageDateTime = DateTime.Now,
                        MessageType = 1

                    });
                }
                Utils.CreateChannels("1", "1");
                //CAZUL 1 chat simplu intre 2 useri
                var c = Utils.IsActivityRunning(Class.FromType(typeof(ChatActivity)));
                if (c && ChatActivity.RoomName.Equals(room))
                {
                    Log.Error("Caz 1", "*********************");
                    ChatActivity.AddMessage(message, ChatModel.TypeMessage);
                }
                //CAZUL 2 user offline primeste chat
                else if (!c && ChatActivity.RoomName.Equals(room))
                {
                    Log.Error("Caz 2", "*********************");

                    var nb = Utils.CreateChatNotification(username, message, username, room, _context,3, "Vizualizare");
                    
                    var ids = room.Split(':');
                    Utils.GetManager().Notify(ids[0] == Utils.GetDefaults("IdClient")? int.Parse(ids[1]) : int.Parse(ids[0]), nb);
                }
                //CAZUL 3 user 1, user 2 converseaza, al3lea se baga in seama
                else if (!ChatActivity.RoomName.Equals(room))
                {

                    Log.Error("Caz 3", "*********************");
                    var nb = Utils.CreateChatNotification(username, message, username, room, _context,3, "Vizualizare");
                    var ids = room.Split(':');
                    Utils.GetManager().Notify(ids[0] == Utils.GetDefaults("IdClient")? int.Parse(ids[1]) : int.Parse(ids[0]), nb);
                }

            }
            catch (Exception ex)
            {
                Log.Error("Eroare da nu ii bai", ex.ToString());
            }
        }
        void OnChatAccepted(Object[] obj)
        {
            Log.Error("WebSocket", "Chat Accepted");
            var data = (JSONObject)obj[0];
            string email;
            string room;
            try
            {
                //username = data.getString("username");
                email = data.GetString("from");
                room = data.GetString("room");
            }
            catch (JSONException e)
            {
                Log.Error("on Join: ", e.Message);
                return;
            }
            Utils.CreateChannels(email, email);
            // aici adaugi in array-ul de room-uri
            try
            {
                string SharedRooms = Utils.GetDefaults("Rooms");
                if (SharedRooms != null)
                {
                    var model = JsonConvert.DeserializeObject<List<ConverstionsModel>>(SharedRooms);
                    var currentModel = new ConverstionsModel { Username = email, Room = room };
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
                    Utils.SetDefaults("Rooms", serialized);
                }
                else
                {
                    List<ConverstionsModel> model = new List<ConverstionsModel>();
                    var currentModel = new ConverstionsModel { Username = email, Room = room };
                    model.Add(currentModel);
                   

                    string serialized = JsonConvert.SerializeObject(model);
                    Utils.SetDefaults("Rooms", serialized);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error OnConversation", e.Message);
            }
            var nb = Utils.CreateChatNotification("Cerere acceptata", $"{email} ti-a acceptat cererea de chat!", email, room, _context, 2);
            var ids = room.Split(':');
            Utils.GetManager().Notify(ids[0] == Utils.GetDefaults("IdClient")? int.Parse(ids[1]) : int.Parse(ids[0]), nb);
        }

        private void OnChatRequest(Object[] obj)
        {
            Log.Error("WebSocket", "Chat Request");
            
            var data = (JSONObject)obj[0];
            string email;
            string room;
            try
            {
                email = data.GetString("from");
                room = data.GetString("room");
            }
            catch (JSONException e)
            {
                Log.Error("onStartChat: ", e.Message);
                return;
            }

            Utils.CreateChannels(email, email);

            //var nb = Utils.GetAndroidChannelNotification("Cerere de convorbire", $"{email} doreste sa ia legatura cu tine!", "Accept", 1, _context, room);
            var nb = Utils.CreateChatNotification("Cerere de convorbire", $"{email} doreste sa ia legatura cu tine!", email, room, _context);
            var ids = room.Split(':');
            Utils.GetManager().Notify(ids[0] == Utils.GetDefaults("IdClient")? int.Parse(ids[1]) : int.Parse(ids[0]), nb);
        }
        private void OnChatRejected(Object[] obj)
        {
            Log.Error("WebSocket", "Chat Rejected");

            var data = (JSONObject)obj[0];
            string email;
            try
            {
                email = data.GetString("from");
            }
            catch (JSONException e)
            {
                Log.Error("onChatRejected: ", e.Message);
                return;
            }

            Utils.CreateChannels(email, email);

            //var nb = Utils.GetAndroidChannelNotification("Cerere de convorbire", $"{email} doreste sa ia legatura cu tine!", "Accept", 1, _context, room);
            var nb = Utils.CreateChatNotification(email, "Ti-a respins cererea de convorbire!", email, null, _context, 1);
            Utils.GetManager().Notify(1000, nb);
        }

        public void Call(params Object[] p0)
        {
            Log.Error("Call", "Caught EVENT_REQUEST_HEADERS after EVENT_TRANSPORT, adding headers");
            //Dictionary<string, List<string>> mHeaders = (Dictionary<string, List<string>>)p0[0];
            //mHeaders.Add("Authorization", new List<string>{"Basic bXl1c2VyOm15cGFzczEyMw=="});
        }
    }
}