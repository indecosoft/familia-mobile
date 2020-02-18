using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Familia.DataModels;
using Familia.Helpers;
using Familia.Medicatie.Data;
using Org.Json;
using SQLite;
using Uri = Android.Net.Uri;

namespace Familia.Medicatie.Alarm {
    [BroadcastReceiver(Enabled = true, Exported = true)]
    class AlarmBroadcastReceiverServer : BroadcastReceiver {

        //        public virtual Android.OS.PowerManager.WakeLock NewWakeLock(Android.OS.WakeLockFlags levelAndFlags, string tag);

        public const string Uuid = "uuid";
        public const string Title = "title";
        public const string Content = "content";
        public const string Postpone = "postpone";
        public const string IdPendingIntent = "idPendingIntent";

        private const string Ok = "OK";
        private const string ActionOk = "actionOk";
        public const string ActionReceive = "actionReceive";
        private static int NotifyId = Constants.NotifId;
        private SQLiteAsyncConnection _db;
        private Intent _medicationServiceIntent;
        public static readonly string FROM_SERVER = "from_server";
        public static readonly string MEDICATION_NAME = "med_name";

        private string oldUuid;


        public override async void OnReceive(Context context, Intent intent) {
            //            var action = intent.Action;
            //            if (action == null) return;
            //            var now = DateTime.Now;

            if (string.IsNullOrEmpty(Utils.GetDefaults("Token"))) return;

            string uuid = intent.GetStringExtra(Uuid);
            string title = intent.GetStringExtra(Title);
            string content = intent.GetStringExtra(Content);
            int postpone = intent.GetIntExtra(Postpone, 5);
            int idPi = intent.GetIntExtra(IdPendingIntent, 1);

            if (await Storage.GetInstance().isHere(uuid) == false) {
                Log.Error("MSS Alarm BRECEIVER", "med is not here anymore");
                return;
            }


            if (oldUuid == null || oldUuid.Equals(""))
            {
                oldUuid = uuid;
            }
            else {
                if (oldUuid.Equals(uuid)) {
                    return;
                }
            }

            Log.Error("MSS Alarm BRECEIVER", "id pi.. " + idPi);

            //const string channel = "channelabsolut_alarm_medication";
            Log.Error("MSS Alarm BRECEIVER", title + ", " + content + ", " + postpone);

            //CreateNotificationChannel(channel, title, content);

            var random = new Random();
            int randomNumber = random.Next(1, 3333) * random.Next(1, 3333);

            NotifyId += randomNumber;
            Log.Error("MSS Alarm BRECEIVER", "notification id: " + NotifyId);
            var alarmIntent = new Intent(context, typeof(AlarmActivity));
            //            alarmIntent.AddFlags(ActivityFlags.ClearTop);
            alarmIntent.PutExtra(Uuid, uuid);
            alarmIntent.PutExtra("notifyId", NotifyId);
            alarmIntent.PutExtra("message", FROM_SERVER);
            alarmIntent.PutExtra(MEDICATION_NAME, title);
            alarmIntent.PutExtra(Postpone, postpone);
            alarmIntent.PutExtra(Content, content);
            alarmIntent.PutExtra(IdPendingIntent, idPi);
            alarmIntent.SetFlags(ActivityFlags.NewTask);
            context.StartActivity(alarmIntent);

            BuildNotification(context, NotifyId, App.AlarmMedicationChannelId, title, content, alarmIntent);

            try {
                var powerManager = (PowerManager)context.GetSystemService(Context.PowerService);
                PowerManager.WakeLock wakeLock = powerManager.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, "server tag");
                wakeLock.Acquire();
                wakeLock.Release();
            } catch (Exception e) {
                Log.Error("MSS Alarm BRECEIVER ERR", e.ToString());
            }
        }

        private static bool IsServiceRunning(Type classTypeof, Context context) {
            var manager = (ActivityManager)context.GetSystemService(Context.ActivityService);
#pragma warning disable 618
            return manager.GetRunningServices(int.MaxValue).Any(service =>
                service.Service.ShortClassName == classTypeof.ToString());
        }

        private static void BuildNotification(Context context, int notifyId, string channel, string title, string content, Intent intent) {

            Log.Error("MSS Alarm BRECEIVER", "build notification");

            PendingIntent piNotification = PendingIntent.GetActivity(context, notifyId, intent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder mBuilder =
                new NotificationCompat.Builder(context, channel)
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetContentText(content)
                    .SetContentTitle(title)
                    .SetAutoCancel(false)
                    //.SetContentIntent(piNotification)
                    .SetPriority(NotificationCompat.PriorityHigh);
                    //.SetOngoing(true);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);

            notificationManager.Notify(notifyId, mBuilder.Build());
            Log.Error("MSS Alarm BRECEIVER", "notify");
        }

    }
}