using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FamiliaXamarin.Medicatie.Alarm
{

    

    class App : Application
    {
        public override void OnCreate()
        {
            base.OnCreate();

            createNotificationChannel();
        }

        private void createNotificationChannel()
        {
            // Create the NotificationChannel, but only on API 26+ because
            // the NotificationChannel class is new and not in the support library
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                string name = Constants.ChannelId;
                string description = "my channel desc";
#pragma warning disable CS0618 // Type or member is obsolete
                var importance = NotificationManager.ImportanceDefault;
#pragma warning restore CS0618 // Type or member is obsolete
                NotificationChannel channel = new NotificationChannel(Constants.ChannelId, name, importance)
                {
                    Description = description
                };
                // Register the channel with the system; you can't change the importance
                // or other notification behaviors after this
                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);

            }
        }
    }
}