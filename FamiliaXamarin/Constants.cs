using System.Collections.Generic;
using Java.Util;

namespace FamiliaXamarin {
    class Constants {
        public static string BloodPressureFile { get; } = "blood_pressure_data.txt";
        public static string BloodGlucoseFile { get; } = "blood_glucose_data.txt";
        public static UUID UuidBloodPressureService { get; } = UUID.FromString("00001810-0000-1000-8000-00805f9b34fb");

        public static UUID UuidBloodPressureMeasurementChar { get; } =
            UUID.FromString("00002a35-0000-1000-8000-00805f9b34fb");

        public static UUID ClientCharacteristicConfig { get; } =
            UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

        //public static final UUID UUID_GLUC_FEATURE = UUID.FromString("00002a51-0000-1000-8000-00805f9b34fb");
        public static UUID UuidGlucMeasurementChar { get; } = UUID.FromString("00002a18-0000-1000-8000-00805f9b34fb");

        public static UUID UuidGlucMeasurementContextChar { get; } =
            UUID.FromString("00002a34-0000-1000-8000-00805f9b34fb");

        public static UUID UuidGlucRecordAccessControlPointChar { get; } =
            UUID.FromString("00002a52-0000-1000-8000-00805f9b34fb");

        public static UUID UuidGlucServ { get; } = UUID.FromString("00001808-0000-1000-8000-00805f9b34fb");

        public static UUID TransferWearableControlUuid { get; } =
            UUID.FromString("0a0ae00a-0a00-1000-8000-00805f9b34fb");

        public static UUID TransferWearableEkgUuid { get; } = UUID.FromString("0a0ae00c-0a00-1000-8000-00805f9b34fb");

        public static UUID TransferWearableActivityUuid { get; } =
            UUID.FromString("0a0ae00d-0a00-1000-8000-00805f9b34fb");

        public static UUID TransferWearableSleepcontentUuid { get; } =
            UUID.FromString("0a0ae00b-0a00-1000-8000-00805f9b34fb");

        public static string IntervalGlucose { get; } = "INTERVAL_GLUCOSE";
        public static string IntervalBloodPressure { get; } = "INTERVAL_BLOOD_PRESSURE";

        #region Medication Constants
        public static string ChannelId { get; } = "my chanel id";
        public static string NotificationTitle { get; } = "Este timpul sa iti iei medicamentele";
        public static string NotifContent { get; } = "";

        public static int BloodPressureNotifId { get; } = 4000;
        public static int GlucoseNotifId { get; } = 5000;
        public static int NotifId { get; } = 3000;
        public static int NotificationAlarmDevice { get; } = 400;
        public static int NotifChatId { get; set; } = 100;
        public static int NotifMedicationId { get; set; } = 1;
        public static int NotifIdServer { get; } = 668;
        public static string MedicationFile { get; } = "data.txt";
        public static string MedicationServerFile { get; } = "data_server.txt";
        #endregion
        #region Fitbit Constants
        public static string ClientSecret { get; } = "bb4070c932c69d3083aa90dd471c8cf3";
        public static string ClientId { get; } = "22CZRL";
        public static string CallbackUrl { get; } = "fittauth://finish";
        #endregion
        


       public static string SaveDeviceDataUrl { get; } = "https://gisdev.indecosoft.net/devices/save-device-measurements";
         //   public static string SaveDeviceDataUrl { get; } = "http://192.168.101.14/devices/save-device-measurements";
        public static string PublicServerAddress { get; } = "https://gisdev.indecosoft.net/chat";
         // public static string PublicServerAddress { get; } = "http://192.168.101.14:3000";

        //   public static string WebSocketLocationAddress { get; } = "http://192.168.101.14:3000/location";
        public static string WebSocketLocationAddress { get; } = "https://gisdev.indecosoft.net/location";
        //  public static string WebSocketAddress { get; } = "http://192.168.101.14:3000/ws";
        public static string WebSocketAddress { get; } = "https://gisdev.indecosoft.net/ws";
        public static int WebSocketPort { get; } = 3000;
        public static int RequestGallery { get; } = 2;
        public static int RequestCamera { get; } = 1;
    }
}