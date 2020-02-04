using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.Helpers;
using Familia.Medicatie.Data;
using Familia.Medicatie.Entities;
using Uri = Android.Net.Uri;

namespace Familia.Medicatie.Alarm {
    [BroadcastReceiver(Enabled = true, Exported = true)]

    class AlarmBroadcastReceiver : BroadcastReceiver {
        private Hour _mHour;
        private Disease _mDisease;
        private Medicine _mMed;
        public static readonly string FROM_APP = "from_app";
        public static readonly string NOTIFICATION_ID = "notification_id";

        private static int NotifyId = Constants.NotifId;


        public override void OnReceive(Context context, Intent intent) {
            string medId = intent.GetStringExtra(DiseaseActivity.MED_ID);
            string boalaId = intent.GetStringExtra(DiseaseActivity.BOALA_ID);
            string hourId = intent.GetStringExtra(DiseaseActivity.HOUR_ID);
            Storage.GetInstance().GetListOfDiseasesFromFile(context);
            _mDisease = Storage.GetInstance().GetDisease(boalaId);

            if (_mDisease == null) return;
            _mMed = _mDisease.GetMedicineById(medId);
            if (_mMed == null) return;
            _mHour = _mMed.FindHourById(hourId);

            // if (Utils.GetDefaults("Token", context) == null) return;

            if (string.IsNullOrEmpty(Utils.GetDefaults("Token"))) return;

            LaunchAlarm(context, medId, boalaId);

        }

        private async void LaunchAlarm(Context context, string medId, string boalaId) {
            const string channel = "channelabsolut";
            DateTime now = DateTime.Now;


            CreateNotificationChannel(channel, "title from app", "content from app");

            NotificationManagerCompat notificationManager =
                NotificationManagerCompat.From(context);

            var random = new Random();
            int randomNumber = random.Next(0, 5000);

            NotifyId += randomNumber;

            Log.Error("ID NOTIFICARE", NotifyId + "");

            var okIntent = new Intent(context, typeof(AlarmActivity));
            //            okIntent.AddFlags(ActivityFlags.ClearTop);
            okIntent.PutExtra(DiseaseActivity.MED_ID, medId);
            okIntent.PutExtra(DiseaseActivity.BOALA_ID, boalaId);
            okIntent.PutExtra("message", FROM_APP);
            okIntent.PutExtra(NOTIFICATION_ID, NotifyId);
            okIntent.SetFlags(ActivityFlags.NewTask);

            context.StartActivity(okIntent);

            var boli = Storage.GetInstance().GetListOfDiseasesFromFile(context);
            Disease mBoala = Storage.GetInstance().GetDisease(boalaId);
            if (mBoala != null) {
                Medicine mMed = mBoala.GetMedicineById(medId);
            }

            BuildNotification(context, NotifyId, channel, _mMed.Name, "medicament", okIntent);

            try {
                var powerManager = (PowerManager)context.GetSystemService(Context.PowerService);
                PowerManager.WakeLock wakeLock = powerManager.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "simple tag");
                wakeLock.Acquire();
                await Task.Delay(1000);
                wakeLock.Release();
            } catch (Exception e) {
                Log.Error("ERR", e.ToString());
            }
        }

        private static void CreateNotificationChannel(string mChannel, string mTitle, string mContent) {
            string description = mContent;
            long[] vibrationPattern = { 100, 200, 300, 400, 500, 400, 300, 200, 400 };
            Uri sound = Uri.Parse(ContentResolver.SchemeAndroidResource + "://" + Application.Context.PackageName + "/" + Resource.Raw.alarm);  //Here is FILE_NAME is the name of file that you want to play
            AudioAttributes attributes = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Alarm)
                .Build();

            var channel =
                new NotificationChannel(mChannel, mTitle, NotificationImportance.Default) {
                    Description = description

                };
            channel.EnableVibration(true);
            channel.SetSound(sound, attributes);
            channel.SetVibrationPattern(vibrationPattern);
            var notificationManager =
                (NotificationManager)Application.Context.GetSystemService(
                    Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        private static void BuildNotification(Context context, int notifyId, string channel, string title, string content, Intent intent) {
            PendingIntent piNotification = PendingIntent.GetActivity(context, notifyId, intent, PendingIntentFlags.UpdateCurrent);
            NotificationCompat.Builder mBuilder =
                new NotificationCompat.Builder(context, channel)
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetContentText(content)
                    .SetContentTitle(title)
                    .SetAutoCancel(true)
                    .SetContentIntent(piNotification)
                    .SetPriority(NotificationCompat.PriorityHigh);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);
            notificationManager.Notify(notifyId, mBuilder.Build());
        }
    }
}