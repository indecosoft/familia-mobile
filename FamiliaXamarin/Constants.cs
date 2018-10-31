using Java.Util;

namespace FamiliaXamarin
{
    class Constants
    {
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

        #region Medication Constants
        public static string ChannelId { get; } = "my chanel id";
        public static string NotificationTitle { get; } = "Este timpul sa iti iei medicamentele";
        public static string NotifContent { get; } = "";
        public static int NotifId { get; } = 667;
        public static int NotifIdServer { get; } = 668;
        public static string MedicationFile { get; } = "data.txt";
        public static string MedicationServerFile { get; } = "data_server.txt";
        #endregion

        public static string DataUrl { get; } = "https://devgis.sigma.team/devices/save-device-measurements";

        public static string ServerAddress { get; } = "http://192.168.101.192:3000/";

        //public static string PublicServerAddress { get; } = "https://gisdev.indecosoft.net/chat";
        //public static string PublicServerAddress { get; } = "https://gisdev.indecosoft.net/";
        //public static string PublicServerAddress { get; } = "http://192.168.101.140:3000/api/userMeds/1";
        public static string PublicServerAddress { get; } = "http://192.168.101.107:3000";
        //public static string WebSocketAddress { get; } = "https://gisdev.indecosoft.net/ws";
        public static string WebSocketAddress { get; } = "http://192.168.101.107:3000/ws";
        public static int WebSocketPort { get; } = 3000;
        public static string ImageDirectory { get; } = "/demonuts";
        public static int RequestGallery { get; } = 2;
        public static int RequestCamera { get; } = 1;
    }
}