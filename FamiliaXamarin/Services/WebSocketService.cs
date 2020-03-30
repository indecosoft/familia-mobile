using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.Helpers;
using Familia.Medicatie.Alarm;
using Familia.WebSocket;
using Org.Json;

namespace Familia.Services {
	[Service]
	internal class WebSocketService : Service {
		private readonly IWebSocketClient _socketClient = new WebSocketClient();
		private readonly IWebSocketClient _webSocketLocation = new WebSocketLocation();
		//private readonly ChargerReceiver _charger = new ChargerReceiver();
		public override IBinder OnBind(Intent intent) => throw new NotImplementedException();

		public override void OnDestroy() {
			base.OnDestroy();
			//UnregisterReceiver(_charger);
		}

		public override async void OnCreate() {
			base.OnCreate();
			Log.Error("Service:", "WebSocketService STARTED");

			try {
				if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {
					using Notification notification =
						new NotificationCompat.Builder(this, App.NonStopChannelIdForServices).SetContentTitle("Familia")
							.SetContentText("Ruleaza in fundal").SetSmallIcon(Resource.Drawable.logo).SetOngoing(true)
							.Build();

					StartForeground(App.NonstopNotificationIdForServices, notification);
				}

				//RegisterReceiver(_charger, new IntentFilter(Intent.ActionHeadsetPlug));

				bool ok = int.TryParse(Utils.GetDefaults("UserType"), out int type);
				Log.Error("ok", ok.ToString());
				Log.Error("type", type.ToString());
				if (ok) {
					if (type != 2)
						await _socketClient.ConnectAsync(Constants.WebSocketAddress, Constants.WebSocketPort, this);
				}

				_webSocketLocation.Connect(Constants.WebSocketLocationAddress, Constants.WebSocketPort, this);
				await Task.Run(async () => {
					try {
						string result = await GetData();
						if (result == "{}") throw new Exception("Error geting configuration from GIS");
						Utils.SetDefaults("IdPersoana", new JSONObject(result).GetInt("idPersoana").ToString());
					} catch (Exception ex) {
						Log.Error("Configuration Error", ex.Message);
					}
				});
			} catch (Exception e) {
				Console.WriteLine(e);
				//throw;
			}
		}


		private static async Task<string> GetData() {
			if (!Utils.CheckNetworkAvailability()) return "{}";
			return await WebServices.WebServices.Get(
				       $"https://gis.indecosoft.net/devices/get-device-config/{Utils.GetDeviceIdentificator(Application.Context)}") ??
			   "{}";
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) =>
			StartCommandResult.Sticky;
	}
}