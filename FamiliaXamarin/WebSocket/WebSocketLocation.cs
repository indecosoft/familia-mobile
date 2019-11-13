using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Support.V4.Content;
using Android.Util;
using Android.Widget;
using EngineIO.Client;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using FamiliaXamarin.Location;
using Org.Json;
using SocketIO.Client;
using Exception = System.Exception;
using Object = Java.Lang.Object;
using Socket = SocketIO.Client.Socket;

namespace Familia.WebSocket
{
    public class WebSocketLocation : LocationCallback, IWebSocketClient
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
                    Query = $"token={Utils.GetDefaults("Token")}&imei={Utils.GetDeviceIdentificator(Application.Context)}"

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
                _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(context);
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
        private FusedLocationProviderClient _fusedLocationProviderClient;
        private LocationRequest locationRequest;
        private LocationCallback locationCallback;
        private bool _isRequestingLocationUpdates;
        private void OnGetLocation(Object[] obj)
        {
            if (!Utils.CheckIfLocationIsEnabled())
            {
                Toast.MakeText(Application.Context, "Nu aveti locatia activata", ToastLength.Long).Show();
                //_socket.Emit("send-location", new JSONObject($"{{latitude: '{Utils.GetDefaults("Latitude").Replace(',', '.')}', longitude: '{Utils.GetDefaults("Longitude").Replace(',', '.')}'}}"));
            } else
            {
                bool isGooglePlayServicesInstalled = Utils.IsGooglePlayServicesInstalled(Application.Context);

                if (!isGooglePlayServicesInstalled) return;
                locationRequest = new LocationRequest()
                    .SetPriority(LocationRequest.PriorityBalancedPowerAccuracy)
                    .SetInterval(1000 * 60)
                    .SetMaxWaitTime(1000 * 60 * 2);
                locationCallback = new FusedLocationProviderCallback(Application.Context);

                _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(Application.Context);
                RequestLocationUpdatesButtonOnClick();
                _socket.Emit("send-location", new JSONObject($"{{latitude: '{Utils.GetDefaults("Latitude").Replace(',', '.')}', longitude: '{Utils.GetDefaults("Longitude").Replace(',', '.')}'}}"));
                StopRequestionLocationUpdates();
            }


            
            
        }
        private async void RequestLocationUpdatesButtonOnClick()
        {
            // No need to request location updates if we're already doing so.
            if (_isRequestingLocationUpdates)
            {
                StopRequestionLocationUpdates();
                _isRequestingLocationUpdates = false;
            }
            else
            {
                if (ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.AccessFineLocation) !=
                    Permission.Granted) return;
                await StartRequestingLocationUpdates();
                _isRequestingLocationUpdates = true;
            }
        }

        private async Task StartRequestingLocationUpdates()
        {
            await _fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);
        }

        private async void StopRequestionLocationUpdates()
        {

            if (_isRequestingLocationUpdates)
            {
                await _fusedLocationProviderClient.RemoveLocationUpdatesAsync(locationCallback);
            }
        }
    }
}
