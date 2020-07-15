using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Widget;
using Familia.Helpers;
using Familia.Location;
using Familia.Medicatie.Alarm;
using Exception = System.Exception;
using Math = System.Math;

namespace Familia.Services {
	[Service]
	internal class DistanceCalculator : Service {
		private double _pacientLatitude, _pacientLongitude;
		private NotificationManager _mNotificationManager;
		private int _verifications = 15;
		private int _refreshTime = 15000;
		private LocationManager location = LocationManager.Instance;

		public override IBinder OnBind(Intent intent) {
			throw new NotImplementedException();
		}

		public override void OnCreate() {
			CreateChannels();
		}

		public override async void OnDestroy() {
			await location.StopRequestionLocationUpdates();
			location.LocationRequested -= Location_LocationRequested;
			_pacientLatitude = 0;
			_pacientLongitude = 0;

		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
            _pacientLatitude = double.Parse(intent.GetStringExtra("Latitude"));
            _pacientLongitude = double.Parse(intent.GetStringExtra("Longitude"));
            Log.Error("PatientLocation", _pacientLatitude.ToString());
            Log.Error("PatientLocation", _pacientLongitude.ToString());
            init();
			return StartCommandResult.Sticky;
		}
        private async void init()
        {
			try
			{
				Notification notification = new NotificationCompat.Builder(this, App.SimpleChannelIdForServices)
					.SetContentTitle("Familia").SetContentText("Asistenta la domiciliu in curs de desfasurare")
					.SetSmallIcon(Resource.Drawable.logo).SetOngoing(true).Build();

				StartForeground(App.SimpleNotificationIdForServices, notification);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				StopSelf();
			}
			// Enlist this instance of the service as a foreground service

			location.LocationRequested += Location_LocationRequested;

			await location.StartRequestingLocation(_refreshTime);
		}

		private void Location_LocationRequested(object source, EventArgs args) {
			try {
				//if (_pacientLatitude == 0 ||_pacientLongitude == 0)
				//            {
				//	_pacientLatitude = ((LocationEventArgs)args).Location.Latitude;
				//	_pacientLongitude = ((LocationEventArgs)args).Location.Longitude;
				//}
				Log.Error("Patient" , $"{_pacientLatitude},{_pacientLongitude}");
				Log.Error("Asistent" , $"{((LocationEventArgs)args).Location.Latitude},{((LocationEventArgs)args).Location.Longitude}");


				if (!Utils.GetDefaults("ActivityStart").Equals(string.Empty)) {
					double distance = Utils.HaversineFormula(_pacientLatitude, _pacientLongitude,
						((LocationEventArgs) args).Location.Latitude, ((LocationEventArgs) args).Location.Longitude);
					Log.Error("Distance", "" + distance);
					Toast.MakeText(this, Math.Round(distance) + " metri.", ToastLength.Short)
							.Show();
					if (distance > 180 && distance < 220) {
						Log.Warn("Distance warning",
							"mai mult de 200 metri. Esti la " + Math.Round(distance) + " metri distanta");
						_verifications = 0;
						NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment",
							"Ai plecat de la pacient? Esti la " + Math.Round(distance) + " metri distanta");

						GetManager().Notify(2, nb.Build());
						if (_refreshTime == 15000) return;
						_refreshTime = 15000;
						location.ChangeInterval(_refreshTime);
					} else if (distance > 220) {
						if (_verifications == 0) {
							NotificationCompat.Builder nb =
								GetAndroidChannelNotification("Avertisment", "Vizita a fost anulata automat!");
							GetManager().Notify(2 , nb.Build());
							_verifications = 15;

							//trimitere date la server
							Utils.SetDefaults("ActivityStart", string.Empty);
							Utils.SetDefaults("QrId", string.Empty);
							Utils.SetDefaults("QrCode", string.Empty);
							Utils.SetDefaults("readedQR", string.Empty);
							StopSelf();
						} else {
							NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment",
								"Vizita va fi anulata automat deoarece te afli la " + (Math.Round(distance) > 1000 ? Math.Round(distance)/1000 + " kilometri" : Math.Round(distance) + " metri") +
								" distanta de pacient! Mai ai " + (_verifications) +
								" minute sa te intorci!");
							GetManager().Notify(2, nb.Build());
							_verifications--;
							if (_refreshTime == 60000) return;
							_refreshTime = 60000;
							location.ChangeInterval(_refreshTime);
						}
					} else {
						
						_verifications = 15;

						if (_refreshTime == 15000) return;
						_refreshTime = 15000;
						location.ChangeInterval(_refreshTime);
					}
				} else {
					StopSelf();
				}
			} catch (Exception e) {
				Log.Error($"Error occurred in {nameof(DistanceCalculator)} service: ", e.Message);
				StopSelf();
			}
		}

		private void CreateChannels() {
			if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
			var androidChannel = new NotificationChannel("Distance_Calculation_ID" , "Distance Calculation" ,
				NotificationImportance.High);
			androidChannel.EnableLights(true);
			androidChannel.EnableVibration(true);
			androidChannel.LightColor = Android.Resource.Color.HoloRedLight;
			androidChannel.LockscreenVisibility = NotificationVisibility.Private;

			GetManager().CreateNotificationChannel(androidChannel);
		}

		private NotificationManager GetManager() =>
			_mNotificationManager ??
			(_mNotificationManager = (NotificationManager) GetSystemService(NotificationService));

		private NotificationCompat.Builder GetAndroidChannelNotification(string title, string body) {
			var intent = new Intent(this, typeof(MainActivity));

			PendingIntent acceptIntent = PendingIntent.GetActivity(this, 1, intent, PendingIntentFlags.OneShot);

			return new NotificationCompat.Builder(ApplicationContext, "Distance_Calculation_ID").SetContentTitle(title)
				.SetContentText(body).SetSmallIcon(Resource.Drawable.alert)
				.SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
				.SetPriority(NotificationCompat.PriorityHigh).SetContentIntent(acceptIntent).SetAutoCancel(true);
		}
	}
}