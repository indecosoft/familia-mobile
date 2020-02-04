using Java.Util;

namespace Familia.Devices.Helpers {
	public class BLEHelpers {
		#region CaresensGlucometer

		public static UUID BLE_CHAR_CUSTOM_TIME_MC = UUID.FromString("01020304-0506-0708-0900-0A0B0C0D0E0F");
		public static UUID BLE_CHAR_CUSTOM_TIME_TI = UUID.FromString("0000FFF1-0000-1000-8000-00805f9b34fb");
		public static UUID BLE_CHAR_CUSTOM_TIME_TI_NEW = UUID.FromString("C4DEA3BC-5A9D-11E9-8647-D663BD873D93");
		public static UUID BLE_CHAR_GLUCOSE_CONTEXT = UUID.FromString("00002A34-0000-1000-8000-00805f9b34fb");
		public static UUID BLE_CHAR_GLUCOSE_MANUFACTURE = UUID.FromString("00002A29-0000-1000-8000-00805f9b34fb");
		public static UUID BLE_CHAR_GLUCOSE_MEASUREMENT = UUID.FromString("00002A18-0000-1000-8000-00805f9b34fb");
		public static UUID BLE_CHAR_GLUCOSE_RACP = UUID.FromString("00002A52-0000-1000-8000-00805f9b34fb");
		public static UUID BLE_CHAR_GLUCOSE_SERIALNUM = UUID.FromString("00002A25-0000-1000-8000-00805f9b34fb");
		public static UUID BLE_CHAR_SOFTWARE_REVISION = UUID.FromString("00002A28-0000-1000-8000-00805f9b34fb");
		public static UUID BLE_DESCRIPTOR = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
		public static UUID BLE_SERVICE_CUSTOM_TIME_MC = UUID.FromString("11223344-5566-7788-9900-AABBCCDDEEFF");
		public static UUID BLE_SERVICE_CUSTOM_TIME_TI = UUID.FromString("0000FFF0-0000-1000-8000-00805f9b34fb");
		public static UUID BLE_SERVICE_CUSTOM_TIME_TI_NEW = UUID.FromString("C4DEA010-5A9D-11E9-8647-D663BD873D93");
		public static UUID BLE_SERVICE_DEVICE_INFO = UUID.FromString("0000180A-0000-1000-8000-00805f9b34fb");
		public static UUID BLE_SERVICE_GLUCOSE = UUID.FromString("00001808-0000-1000-8000-00805f9b34fb");

		#endregion

		#region MedisanaBloodPressure

		public static UUID UuidBloodPressureService { get; } = UUID.FromString("00001810-0000-1000-8000-00805f9b34fb");

		public static UUID UuidBloodPressureMeasurementChar { get; } =
			UUID.FromString("00002a35-0000-1000-8000-00805f9b34fb");

		public static UUID ClientCharacteristicConfig { get; } =
			UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

		#endregion

		#region MedisanaGlucometer

		public static UUID UuidGlucMeasurementChar { get; } = UUID.FromString("00002a18-0000-1000-8000-00805f9b34fb");

		public static UUID UuidGlucMeasurementContextChar { get; } =
			UUID.FromString("00002a34-0000-1000-8000-00805f9b34fb");

		public static UUID UuidGlucRecordAccessControlPointChar { get; } =
			UUID.FromString("00002a52-0000-1000-8000-00805f9b34fb");

		public static UUID UuidGlucServ { get; } = UUID.FromString("00001808-0000-1000-8000-00805f9b34fb");

		#endregion
	}
}