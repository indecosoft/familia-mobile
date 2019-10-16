using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Util;
using EngineIO.Client;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using Org.Json;
using SocketIO.Client;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Socket = SocketIO.Client.Socket;

namespace Familia.WebSocket
{
    public class WebSocketLocation : IWebSocketClient
    {
        public WebSocketLocation()
        {
        }
        Socket _socket;
        public Socket Client;

        public async Task Connect(string hostname, int port, Context context)
        {
            try
            {

                var options = new IO.Options
                {
                    ForceNew = true,
                    Reconnection = true,
                    Secure = false,
                    Query = $"token={Utils.GetDefaults("Token")}&imei={Utils.GetImei(Application.Context)}"

                };
                _socket = IO.Socket(hostname, options);

                _socket.On(Socket.EventConnect, OnConnect);
                _socket.On(Socket.EventDisconnect, OnDisconnect);
                _socket.On(Socket.EventConnectError, OnConnectError);
                _socket.On(Socket.EventConnectTimeout, OnConnectTimeout);

                _socket.On("get-location", OnGetLocation);
                _socket.On("Error", OnError);

                _socket.Connect();
                Client = _socket;
            }
            catch (Exception e)
            {
                Log.Error("WSConnectionError: ", e.ToString());
            }
        }

        public void Emit(string eventName, JSONObject value)
        {
            _socket?.Emit(eventName, value);
        }
        private void OnError(Object[] obj)
        {

        }


        public void Disconect()
        {
            Client?.Disconnect();
        }

        private static void OnConnect(Object[] obj)
        {
            Log.Error("WebSocket Location", "Client Connected");
        }

        private static void OnDisconnect(Object[] obj)
        {
            Log.Error("WebSocket Location", "Client Diconnected");
        }

        private static void OnConnectError(Object[] obj)
        {
            Log.Error("WebSocket Location", "Connection Error" + obj[0]);
        }

        private static void OnConnectTimeout(Object[] obj)
        {
            Log.Error("WebSocket Location", "Connection Timeout");
        }

        private async void OnGetLocation(Object[] obj)
        {
            _socket.Emit("send-location", new JSONObject($"{{latitude: '{Utils.GetDefaults("Latitude")}', longitude: '{Utils.GetDefaults("Longitude")}'}}"));
        }
    }
}
