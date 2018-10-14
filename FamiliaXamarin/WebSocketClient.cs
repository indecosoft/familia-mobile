using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Support.V4.App;
using Android.Util;
using FamiliaXamarin.JsonModels;
using Java.Net;
using Newtonsoft.Json;
using Org.Json;
using SocketIO.Client;
using Object = Java.Lang.Object;
using Socket = SocketIO.Client.Socket;

namespace FamiliaXamarin
{
    internal class WebSocketClient : IWebSocketClient
    {
        private Socket _socket;
        public static Socket Client;

//        private JSONArray _rooms = new JSONArray();
//        private JSONObject _newRoom = new JSONObject();
        private Context _context;
        public void Connect(string hostname, int port, Context context)
        {
            Utils.CreateChannels();
            _context = context;
            try
            {

                var options = new IO.Options
                {
                    ForceNew = true,
                    Reconnection = true,
                    Query = $"token={Utils.GetDefaults("Token", Application.Context)}&imei={Utils.GetImei(Application.Context)}"
                    
                };
                //URI url = new URI(Constants.WebSocketAddress);
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
                //_socket = IO.Socket($"{hostname});:ket;
            }
            catch (Exception e)
            {     
                                                                           
                
 //               _socket = IO.Socket("http://192.168.100.52:ocket(n3000");

                Log.Error("WSConnectionError: ", e.ToString());
            }
        }                                      

        public void Emit(string eventName, JSONObject value)
        {
            _socket.Emit(eventName, value);
        }

        private static void OnConnect(Object[] obj)
        {
            Log.Error("WebSocket", "Client Connected");
        }
        private static void OnDisconnect(Object[] obj)
        {
            Log.Error("WebSocket", "Client Diconnected");
        }
        private static void OnConnectError(Object[] obj)
        {
            Log.Error("WebSocket", "Connection Error" + obj[0]);
        }
        private static void OnConnectTimeout(Object[] obj)
        {
            Log.Error("WebSocket", "Connection Timeout");
        }
        private void OnConversation(Object[] obj)
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

                //Log.Error("Active", "" + Chat.active);
                if (!ChatActivity.RoomName.Equals(room))
                {

                    NotificationCompat.Builder nb = Utils.GetAndroidChannelNotification(username, message, "Vizualizare", 3, _context, room);
                    Utils.GetManager().Notify(100, nb.Build());
                }
                else if (!ChatActivity.Active)
                {
                    Log.Error("Caz 2", "*********************");

                    NotificationCompat.Builder nb = Utils.GetAndroidChannelNotification(username, message, "Vizualizare", 3, _context, room);
                    Utils.GetManager().Notify(100, nb.Build());
                }
                else if (ChatActivity.RoomName.Equals(room) && ChatActivity.Active)
                {

                    Log.Error("Caz 3", "*********************");

                    //String[] data2 = message.split(" ");
                    //removeTyping(username);
                    Log.Error("Mesaj: ", message);
                    //if(!data2[0].replace(":","").equals(Email))
                    ChatActivity.addMessage(message, ChatModel.TypeMessage);

                }
            }
            catch (Exception ex)
            {
                Log.Error("Eroare da nu ii bai", ex.ToString());
            }
        }
        private void OnChatAccepted(Object[] obj)
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

//            Chat.RoomName = room;
//            Chat.Email = email;
//            Chat.FromNotify = false;
            // aici adaugi in array-ul de room-uri
            try
            {
                string SharedRooms = Utils.GetDefaults("Rooms", _context);
                if (string.IsNullOrEmpty(SharedRooms))
                {
                    var model = JsonConvert.DeserializeObject<List<ConverstionsModel>>(SharedRooms);
                    var currentModel = new ConverstionsModel { Username = email, Room = room };
                    if (!model.Contains(currentModel))
                    {
                        model.Add(currentModel);   
                    }

                    string serialized = JsonConvert.SerializeObject(model);
                    Utils.SetDefaults("Rooms", serialized, _context);
                }
            }
            catch
            {
                //ignored
            }
           
            
            
            var nb = Utils.GetAndroidChannelNotification("Cerere acceptata", email + " ti-a acceptat cererea de chat!", "Converseaza", 2, _context, room);
            Utils.GetManager().Notify(100, nb.Build());
        }
        private void OnChatRequest(Object[] obj)
        {
            Log.Error("WebSocket", "Chat Request");
            var data = (JSONObject)obj[0];
            string email;
            string room;
            //String avatar;
            try
            {
                //username = data.getString("username");
                email = data.GetString("from");
                room = data.GetString("room");
                //avatar = data.getString("avatar");
            }
            catch (JSONException e)
            {
                Log.Error("onStartChat: ", e.Message);
                return;
            }

//            Chat.Email = email;
//            Chat.RoomName = room;
            // Chat.Avatar = avatar;
//            var mPrefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
//            var sharedRooms = mPrefs.GetString("Rooms", "[]");
//            try
//            {
//                _newRoom.Put("dest", email).Put("room", room);
//                _rooms = new JSONArray(sharedRooms);
//                _rooms.Put(_newRoom);
//
//                var editor = mPrefs.Edit();
//                editor.PutString("Rooms", _rooms.ToString());
//                editor.Apply();
//            }
//            catch (JSONException e)
//            {
//                e.PrintStackTrace();
//            }


            var nb = Utils.GetAndroidChannelNotification("Cerere de chat", $"{email} doreste sa ia legatura cu tine!", "Accept", 1, _context, room);
            Utils.GetManager().Notify(100, nb.Build());
        }
    }
}