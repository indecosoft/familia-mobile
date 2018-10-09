using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace FamiliaXamarin.Medicatie.Alarm
{
    class AlarmBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, "ALARM !!!", ToastLength.Long).Show();
            string medId = intent.GetStringExtra(BoalaActivity.MED_ID);
            string boalaId = intent.GetStringExtra(BoalaActivity.BOALA_ID);
            Intent i = new Intent(context, typeof(AlarmActivity));
            Intent intentNotification = new Intent(context, typeof(BoalaActivity));
            //context.startActivity(new Intent(context, AlarmActivity.class));
            i.PutExtra(BoalaActivity.MED_ID, medId);
            i.PutExtra(BoalaActivity.BOALA_ID, boalaId);

            context.StartActivity(i);
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intentNotification, 0);

            NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(context, Constants.ChannelId)
                .SetSmallIcon(Resource.Mipmap.ic_launcher_round)
                .SetWhen(DateTime.Now.Millisecond)
                .SetContentTitle(Constants.NotificationTitle)
                .SetContentText(Constants.NotifContent)
                .SetAutoCancel(true)
                .SetPriority(NotificationCompat.PriorityDefault)
                .SetContentIntent(pendingIntent);

            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(context);


            // notificationId is a unique int for each notification that you must define
            notificationManager.Notify(Constants.NotifId, mBuilder.Build());
        }
    }
}