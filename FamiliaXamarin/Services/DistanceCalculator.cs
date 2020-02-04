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
		private int _verifications;
		private int _refreshTime = 15000;
		private LocationManager location = LocationManager.Instance;

		public override IBinder OnBind(Intent intent) {
			throw new NotImplementedException();
		}

		public override void OnCreate() {
			CreateChannels();
			_pacientLongitude = double.Parse(Utils.GetDefaults("ConsultLong"));
			_pacientLatitude = double.Parse(Utils.GetDefaults("ConsultLat"));
		}

		public override async void OnDestroy() {
			await location.StopRequestionLocationUpdates();
			location.LocationRequested -= Location_LocationRequested;
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
			try {
				Notification notification = new NotificationCompat.Builder(this, App.SimpleChannelIdForServices)
					.SetContentTitle("Familia").SetContentText("Asistenta la domiciliu in curs de desfasurare")
					.SetSmallIcon(Resource.Drawable.logo).SetOngoing(true).Build();

				StartForeground(App.SimpleNotificationIdForServices, notification);
			} catch (Exception e) {
				Console.WriteLine(e);
				StopSelf();
			}
			// Enlist this instance of the service as a foreground service

			location.LocationRequested += Location_LocationRequested;

			Task.Run(async () => await location.StartRequestingLocation(_refreshTime));
			return StartCommandResult.Sticky;
		}

		private void Location_LocationRequested(object source, EventArgs args) {
			try {
				if (!Utils.GetDefaults("ActivityStart").Equals(string.Empty)) {
					double distance = Utils.HaversineFormula(_pacientLatitude, _pacientLongitude,
						((LocationEventArgs) args).Location.Latitude, ((LocationEventArgs) args).Location.Longitude);
					Log.Error("Distance", "" + distance);
					Toast.MakeText(this, "" + distance + " metri", ToastLength.Short).Show();
					if (distance > 180 && distance < 220) {
						Log.Warn("Distance warning",
							"mai mult de 200 metri.Esti la " + Math.Round(distance) + " metrii distanta");
						Toast.MakeText(this, "" + distance, ToastLength.Short).Show();
						_verifications = 0;
						NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment",
							"Ai plecat de la pacient? Esti la " + Math.Round(distance) + " metrii distanta");

						GetManager().Notify(100, nb.Build());
						if (_refreshTime == 15000) return;
						_refreshTime = 15000;
						location.ChangeInterval(_refreshTime);
					} else if (distance > 220) {
						Toast.MakeText(this, distance + " metri " + _verifications, ToastLength.Short).Show();

						if (_verifications == 15) {
							NotificationCompat.Builder nb =
								GetAndroidChannelNotification("Avertisment", "Vizita a fost anulata automat!");
							GetManager().Notify(1000, nb.Build());
							_verifications = 0;

							//trimitere date la server
							Utils.SetDefaults("ActivityStart", string.Empty);
							StopSelf();
						} else {
							NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment",
								"Vizita va fi anulata automat deoarece te afli la " + Math.Round(distance) +
								" metrii distanta de pacient! Mai ai " + (15 - _verifications) +
								" minute sa te intorci!");
							GetManager().Notify(1000, nb.Build());
							_verifications++;
							if (_refreshTime == 60000) return;
							_refreshTime = 60000;
							location.ChangeInterval(_refreshTime);
						}
					} else {
						Toast.MakeText(this, Math.Round(distance) + " metri. Inca esti bine ;)", ToastLength.Short)
							.Show();
						_verifications = 0;

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
			var androidChannel = new NotificationChannel("ANDROID_CHANNEL_ID", "ANDROID_CHANNEL_NAME",
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

			return new NotificationCompat.Builder(ApplicationContext, "ANDROID_CHANNEL_ID").SetContentTitle(title)
				.SetContentText(body).SetSmallIcon(Resource.Drawable.alert)
				.SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
				.SetPriority(NotificationCompat.PriorityHigh).SetContentIntent(acceptIntent).SetAutoCancel(true);
		}
	}
}