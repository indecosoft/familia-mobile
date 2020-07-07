using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Familia.Helpers;
using Familia.Location;
using Org.Json;
using SocketIO.Client;
using Object = Java.Lang.Object;

namespace Familia.WebSocket {
    public class WebSocketLocation : IWebSocketClient {
        public static Socket Socket;
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
                Socket = IO.Socket(hostname, options);

                Socket.On(Socket.EventConnect, OnConnect);
                Socket.On(Socket.EventDisconnect, OnDisconnect);
                Socket.On(Socket.EventConnectError, OnConnectError);
                Socket.On(Socket.EventConnectTimeout, OnConnectTimeout);

                Socket.On("get-location", OnGetLocation);

                Socket.Connect();
            } catch (Exception e) {
                Log.Error("WSConnectionError: ", e.ToString());
            }
        }


        public void Emit(string eventName, JSONObject value) {
            Socket?.Emit(eventName, value);
        }

        public void Disconect() {
            Socket?.Disconnect();
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
            using var locationObj = new JSONObject();
            locationObj.Put("latitude", (args as LocationEventArgs).Location.Latitude);
            locationObj.Put("longitude", (args as LocationEventArgs).Location.Longitude);
            Socket.Emit("send-location", locationObj);
            location.LocationRequested -= LocationRequested;
        }
    }
}
