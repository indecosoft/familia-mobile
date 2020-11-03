using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using Familia.Helpers;
using Familia.Location;
using Familia.Medicatie.Alarm;
using AndroidX.Core.App;
using Exception = System.Exception;
using Math = System.Math;

namespace Familia.Services {


	[Service]
	internal class DistanceCalculator : Service {
		private double _pacientLatitude, _pacientLongitude;
		private NotificationManager _mNotificationManager;
		private int _verifications = 15;
		private int _refreshTime = 15000;
		private readonly LocationManager location = LocationManager.Instance;
		private static IServiceStoppedListener listener;

		public override IBinder OnBind(Intent intent) {
			throw new NotImplementedException();
		}

		public override void OnCreate() {
			Log.Error("Distance Calculator Service Started" , "here");
			CreateChannels();
		}

		public override async void OnDestroy() {
			Log.Error("Distance Service" , "is Destroyed");
			location.LocationRequested -= Location_LocationRequested;
			await location.StopRequestionLocationUpdates();
			_pacientLatitude = 0;
			_pacientLongitude = 0;
			if (listener is null) return;
			listener.OnServiceStopped();
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
            try {
				Log.Error("PatientLocation from shared" , intent.GetStringExtra("Latitude"));
				Log.Error("PatientLocation from shared" , intent.GetStringExtra("Longitude"));
				_pacientLatitude = double.Parse(intent.GetStringExtra("Latitude") , CultureInfo.InvariantCulture);
				_pacientLongitude = double.Parse(intent.GetStringExtra("Longitude") , CultureInfo.InvariantCulture);
				Log.Error("PatientLocation" , _pacientLatitude.ToString());
				Log.Error("PatientLocation" , _pacientLongitude.ToString());
				init();
			} catch (Exception ex) {
				Log.Error("Distance Calculator Service" , ex.Message);
				StopSelf();
            }
			
			return StartCommandResult.Sticky;
		}
        private async void init()
        {
			Log.Error("Distance Calculator Service" , "init");
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
            
			location.LocationRequested += Location_LocationRequested;

			await location.StartRequestingLocation(_refreshTime);
		}
		/// <summary>
		/// pentru citirile cu succes a locatiei se va calcula distanta dintre pacient si asistent si notifica utilizatorul in cazul in care distanta este prea mare
		/// </summary>
		/// <param name="source"></param>
		/// <param name="args"></param>
		private void Location_LocationRequested(object source, LocationEventArgs args) {
			Log.Error("Locatie preluata in service" , "here");
			try {
                if (_pacientLatitude == 0 || _pacientLongitude == 0) {
                    _pacientLatitude = args.Location.Latitude;
                    _pacientLongitude = args.Location.Longitude;
                }
                Log.Error("Patient" , $"{_pacientLatitude},{_pacientLongitude}");
				Log.Error("Asistent" , $"{args.Location.Latitude},{args.Location.Longitude}");


				if (!Utils.GetDefaults("ActivityStart").Equals(string.Empty)) {
					double distance = Utils.HaversineFormula(_pacientLatitude, _pacientLongitude,
						 args.Location.Latitude, args.Location.Longitude);
					Log.Error("Distance", "" + distance);
					Toast.MakeText(this, Math.Round(distance) + " metri.", ToastLength.Short)
							.Show();
					if (distance > 450 && distance < 550) {
						Log.Warn("Distance warning",
							"mai mult de 200 metri. Esti la " + Math.Round(distance) + " metri distanta");
						_verifications = 0;
						NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment",
							"Ai plecat de la pacient? Esti la " + Math.Round(distance) + " metri distanta");

						GetManager().Notify(2, nb.Build());
						if (_refreshTime == 15000) return;
						_refreshTime = 15000;
						location.ChangeInterval(_refreshTime);
					} else if (distance >= 550) {
						if (_verifications == 0) {
							NotificationCompat.Builder nb =
								GetAndroidChannelNotification("Avertisment", "Vizita a fost anulata automat!");
							GetManager().Notify(2 , nb.Build());
							//trimitere date la server
							Utils.SetDefaults("ActivityStart", string.Empty);
							Utils.SetDefaults("QrId", string.Empty);
							Utils.SetDefaults("QrCode", string.Empty);
							Utils.SetDefaults("readedQR", string.Empty);
							StopSelf();
						} else {
							NotificationCompat.Builder nb = GetAndroidChannelNotification("Avertisment",
								"Vizita va fi anulata automat deoarece te afli la " + (Math.Round(distance) > 1000 ? (Math.Round(distance)/1000).ToString().Replace('.', ',') + " kilometri" : Math.Round(distance) + " metri") +
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
					Utils.SetDefaults("ActivityStart" , string.Empty);
					Utils.SetDefaults("QrId" , string.Empty);
					Utils.SetDefaults("QrCode" , string.Empty);
					Utils.SetDefaults("readedQR" , string.Empty);
					StopSelf();
				}
			} catch (Exception e) {
				Log.Error($"Error occurred in {nameof(DistanceCalculator)} service: ", e.Message);
				Utils.SetDefaults("ActivityStart" , string.Empty);
				Utils.SetDefaults("QrId" , string.Empty);
				Utils.SetDefaults("QrCode" , string.Empty);
				Utils.SetDefaults("readedQR" , string.Empty);
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
		public static void SetListener(IServiceStoppedListener mlistener) {
			listener = mlistener;
		}

		public interface IServiceStoppedListener {
			public void OnServiceStopped();
		}

    }
}