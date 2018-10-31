using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.Util;
using FamiliaXamarin.JsonModels;
using Newtonsoft.Json;
using Org.Json;
using SocketIO.Client;
using Object = Java.Lang.Object;
using Socket = SocketIO.Client.Socket;

namespace FamiliaXamarin
{
    public class WebSocketClient : IWebSocketClient
    {
        Socket _socket;
        public static Socket Client;

        Context _context;
        public void Connect(string hostname, int port, Context context)
        {
            
            _context = context;
            try
            {

                var options = new IO.Options
                {
                    ForceNew = true,
                    Reconnection = true,
                    Query = $"token={Utils.GetDefaults("Token", Application.Context)}&imei={Utils.GetImei(Application.Context)}"
                    
                };
                _socket = IO.Socket(Constants.WebSocketAddress, options);

                _socket.On(Socket.EventConnect, OnConnect);
                _socket.On(Socket.EventDisconnect, OnDisconnect);
                _socket.On(Socket.EventConnectError, OnConnectError);
                _socket.On(Socket.EventConnectTimeout, OnConnectTimeout);

                _socket.On("conversation", OnConversation);
                _socket.On("chat request", OnChatRequest);
                _socket.On("chat accepted", OnChatAccepted);

                _socket.Connect();
                Client = _socket;
            }
            catch (Exception e)
            {     


                Log.Error("WSConnectionError: ", e.ToString());
            }
        }                                      

        public static void Disconect()
        {
            
            Client?.Disconnect();
        }

        public void Emit(string eventName, JSONObject value)
        {
            _socket?.Emit(eventName, value);
        }

        static void OnConnect(Object[] obj)
        {
            Log.Error("WebSocket", "Client Connected");
        }
        static void OnDisconnect(Object[] obj)
        {
            Log.Error("WebSocket", "Client Diconnected");
        }
        static void OnConnectError(Object[] obj)
        {
            Log.Error("WebSocket", "Connection Error" + obj[0]);
        }
        static void OnConnectTimeout(Object[] obj)
        {
            Log.Error("WebSocket", "Connection Timeout");
        }
        void OnConversation(Object[] obj)
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
            Utils.CreateChannels(username, username);

            try
            {

                //CAZUL 1 chat simplu intre 2 useri
                if (ChatActivity.Active && ChatActivity.RoomName.Equals(room))
                {
                    Log.Error("Caz 1", "*********************");
                    ChatActivity.AddMessage(message, ChatModel.TypeMessage);
                }
                //CAZUL 2 user offline primeste chat
                else if (!ChatActivity.Active && ChatActivity.RoomName.Equals(room))
                {
                    Log.Error("Caz 2", "*********************");

                    NotificationCompat.Builder nb = Utils.GetAndroidChannelNotification(username, message, "Vizualizare", 3, _context, room);
                    Utils.GetManager().Notify(100, nb.Build());
                }
                //CAZUL 3 user 1, user 2 converseaza, al3lea se baga in seama
                else if (!ChatActivity.RoomName.Equals(room))
                {

                    Log.Error("Caz 3", "*********************");

                    NotificationCompat.Builder nb = Utils.GetAndroidChannelNotification(username, message, "Vizualizare", 3, _context, room);
                    Utils.GetManager().Notify(100, nb.Build());
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
                string SharedRooms = Utils.GetDefaults("Rooms", _context);
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
                    Utils.SetDefaults("Rooms", serialized, _context);
                }
                else
                {
                    List<ConverstionsModel> model = new List<ConverstionsModel>();
                    var currentModel = new ConverstionsModel { Username = email, Room = room };
                    model.Add(currentModel);
                   

                    string serialized = JsonConvert.SerializeObject(model);
                    Utils.SetDefaults("Rooms", serialized, _context);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error OInConversatyion", e.Message);
            }



            var nb = Utils.GetAndroidChannelNotification("Cerere acceptata", email + " ti-a acceptat cererea de chat!", "Converseaza", 2, _context, room);
            Utils.GetManager().Notify(100, nb.Build());
        }
        void OnChatRequest(Object[] obj)
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

            var nb = Utils.GetAndroidChannelNotification("Cerere de chat", $"{email} doreste sa ia legatura cu tine!", "Accept", 1, _context, room);
            Utils.GetManager().Notify(100, nb.Build());
        }
    }
}