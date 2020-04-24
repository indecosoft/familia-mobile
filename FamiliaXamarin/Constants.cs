using Android;
using Familia.Activity_Tracker;

namespace Familia {
	 internal class Constants {

		public  const string IntervalGlucose  = "INTERVAL_GLUCOSE";
		public  const string IntervalBloodPressure  = "INTERVAL_BLOOD_PRESSURE";

		#region Medication 

		public static string ChannelId { get; } = "my chanel id";
		public static string NotificationTitle { get; } = "Este timpul sa iti iei medicamentele";
		public static string NotifContent { get; } = "";
		public static int BloodPressureNotifId { get; } = 88886;
		public static int GlucoseNotifId { get; } = 88887;
		public static int NotifId { get; } = 3000;
		public static int NotificationAlarmDevice { get; } = 400;
		public static int NotifChatId { get; set; } = 100;
		public static int NotifMedicationId { get; set; } = 1;
		public static int NotifIdServer { get; } = 668;
		public static string MedicationFile { get; } = "data.txt";
		public static string MedicationServerFile { get; } = "data_server.txt";

		#endregion

		#region Fitbit 

		public static string ClientSecret { get; } = "bb4070c932c69d3083aa90dd471c8cf3";
		public static string ClientId { get; } = "22CZRL";
		public static string CallbackUrl { get; } = "fittauth://finish";

		#endregion

		public static readonly string[] PermissionsArray = {
			Manifest.Permission.ReadPhoneState, Manifest.Permission.AccessCoarseLocation,
			Manifest.Permission.AccessFineLocation, Manifest.Permission.Camera, Manifest.Permission.ReadExternalStorage,
			Manifest.Permission.WriteExternalStorage };

		public static string Config { get; } = "https://gisdev.indecosoft.net/chat/api/get-device-config";

		// public static string SaveDeviceDataUrl { get; } = "http://192.168.101.107/devices/save-device-measurements";
		// public static string PublicServerAddress { get; } = "http://192.168.101.107:3000";
		// public static string WebSocketLocationAddress { get; } = "http://192.168.101.107:3000/location";
		//public static string WebSocketAddress { get; } = "http://192.168.101.107:3000/ws";

		public static string SaveDeviceDataUrl { get; } =
			"https://gisdev.indecosoft.net/devices/save-device-measurements";
        
		public static string PublicServerAddress { get; } = "https://gisdev.indecosoft.net/chat";
		public static string WebSocketLocationAddress { get; } = "https://gisdev.indecosoft.net/location";
		public static string WebSocketAddress { get; } = "https://gisdev.indecosoft.net/ws";
		public static int WebSocketPort { get; } = 3000;
		public static int RequestGallery { get; } = 2;
		public static int RequestCamera { get; } = 1;
	}
}