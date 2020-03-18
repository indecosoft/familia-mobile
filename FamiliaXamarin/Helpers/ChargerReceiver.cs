using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Familia.DataModels;
using Org.Json;
using SQLite;
using Environment = System.Environment;

namespace Familia.Helpers {
	[BroadcastReceiver(Enabled = true, Exported = true)]
	[IntentFilter(new[] {Intent.ActionHeadsetPlug})]
	public class ChargerReceiver : BroadcastReceiver {
		private SQLiteAsyncConnection _db;

		public override async void OnReceive(Context context, Intent intent) {
			string action = intent.Action;
			if (action == null || !action.Equals(Intent.ActionHeadsetPlug)) return;
			string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var numeDB = "devices_data.db";
			_db = new SQLiteAsyncConnection(Path.Combine(path, numeDB));
			await _db.CreateTableAsync<DeviceConfigRecords>();

			await Task.Run(async () => {
				try {
					string data = await GetData();
					var obj = new JSONObject(data);
					JSONObject bloodPressure = obj.GetJSONObject("bloodPressureSystolic");
					string intervalBloodPressure = bloodPressure.GetString("interval");
					JSONObject bloodGlucose = obj.GetJSONObject("bloodGlucose");
					string intervalGlucose = bloodGlucose.GetString("interval");
					int idPersoana = obj.GetInt("idPersoana");
					Utils.SetDefaults("IdPersoana", idPersoana.ToString());
					// Log.Error("INTERVAL_BLOOD_PRESSURE", intervalBloodPressure);
					// Log.Error("INTERVAL_GLUCOSE", intervalGlucose);

					AddDeviceConfig(_db, intervalBloodPressure, intervalGlucose);

					LaunchAlarm(context, intervalGlucose, Constants.IntervalGlucose);
					LaunchAlarm(context, intervalBloodPressure, Constants.IntervalBloodPressure);
				} catch (Exception ex) {
					Log.Error("Error la parsarea Jsonului", ex.Message);
				}
			});
		}

		private static void LaunchAlarm(Context context, string interval, string content) {

			if (int.TryParse(interval, out int intervalMilisec))
				intervalMilisec *= 60000;
			else
				intervalMilisec = 60000;

			//for test
			//intervalMilisec = 60000;

			var am = (AlarmManager) context.GetSystemService(Context.AlarmService);
			var i = new Intent(context, typeof(AlarmDeviceReceiver));
			i.PutExtra(AlarmDeviceReceiver.INTERVAL_CONTENT, content);
			i.PutExtra("IntervalMilis", interval);
			PendingIntent pi;
			if (content.Equals("INTERVAL_GLUCOSE")) {
				pi = PendingIntent.GetBroadcast(context, Constants.GlucoseNotifId, i, PendingIntentFlags.UpdateCurrent);
			} else {
				pi = PendingIntent.GetBroadcast(context, Constants.BloodPressureNotifId, i,
					PendingIntentFlags.UpdateCurrent);
			}


			am?.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + intervalMilisec,
				intervalMilisec, pi);
		}


		private static async Task<string> GetData() {
			if (!Utils.CheckNetworkAvailability()) return null;
			string result = await WebServices.WebServices.Get(
				$"https://gisdev.indecosoft.net/chat/api/get-device-config/{Utils.GetDeviceIdentificator(Application.Context)}");
			if (result == null) return null;
			Log.Error("RESULT_FROM_GIS", result);
			return result;
		}


		private static async void AddDeviceConfig(SQLiteAsyncConnection db, string intervalBlood,
			string intervalGlucose) {
			await db.InsertAsync(new DeviceConfigRecords {
				IntervalBloodPresure = intervalBlood, IntervalGlucose = intervalGlucose
			});
		}
	}
}