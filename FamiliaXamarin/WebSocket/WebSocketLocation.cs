using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Familia.Location;
using FamiliaXamarin.Helpers;
using Org.Json;
using SocketIO.Client;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Socket = SocketIO.Client.Socket;
using FamiliaXamarin;

namespace Familia.WebSocket {
    public class WebSocketLocation : IWebSocketClient {
        Socket _socket;
        public Socket Client;
        private readonly Handler handler = new Handler(Looper.MainLooper);
        private readonly LocationManager location = LocationManager.Instance;
        public void Connect(string hostname, int port, Context context) {
            try {
                var options = new IO.Options {
                    ForceNew = true,
                    Reconnection = true,
                    Secure = false,
                    Query = $"token={Utils.GetDefaults("Token")}&imei={Utils.GetDeviceIdentificator(Application.Context)}"

                };
                _socket = IO.Socket(hostname, options);

                _socket.On(Socket.EventConnect, OnConnect);
                _socket.On(Socket.EventDisconnect, OnDisconnect);
                _socket.On(Socket.EventConnectError, OnConnectError);
                _socket.On(Socket.EventConnectTimeout, OnConnectTimeout);

                _socket.On("get-location", OnGetLocation);

                _socket.Connect();
                Client = _socket;
            } catch (Exception e) {
                Log.Error("WSConnectionError: ", e.ToString());
            }
        }

        public void Emit(string eventName, JSONObject value) {
            _socket?.Emit(eventName, value);
        }

        public void Disconect() {
            Client?.Disconnect();
        }

        private static void OnConnect(Object[] obj) {
            Log.Error("WebSocket Location", "Client Connected");
        }

        private static void OnDisconnect(Object[] obj) {
            Log.Error("WebSocket Location", "Client Diconnected");
        }

        private static void OnConnectError(Object[] obj) {
            Log.Error("WebSocket Location", "Connection Error" + obj[0]);
        }

        private static void OnConnectTimeout(Object[] obj) {
            Log.Error("WebSocket Location", "Connection Timeout");
        }

        private void OnGetLocation(Object[] obj) {
            handler.Post(async () => {
                location.LocationRequested += LocationRequested;
                //await location.StopRequestionLocationUpdates();
                await location.StartRequestingLocation();
            });
        }

        private void LocationRequested(object source, EventArgs args) {
            using JSONObject locationObj = new JSONObject();
            locationObj.Put("latitude", (args as LocationEventArgs).Location.Latitude);
            locationObj.Put("longitude", (args as LocationEventArgs).Location.Longitude);
            _socket.Emit("send-location", locationObj);
            location.LocationRequested -= LocationRequested;
        }
    }
}
