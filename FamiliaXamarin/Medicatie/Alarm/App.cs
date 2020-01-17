using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace FamiliaXamarin.Medicatie.Alarm
{

    

    class App : Application
    {
        public static readonly string NonStopChannelIdForServices = "nonstop_channel_id_for_continuous_services";
        public static readonly string SimpleChannelIdForServices = "simple_channel_id_for_services_with_a_simple_task";
        public static readonly int NonstopNotificationIdForServices = 33331;
        public static readonly int SimpleNotificationIdForServices = 33330;
        public static readonly string AlarmMedicationChannelId = "alarm_medication_channel_id";
        
        public override void OnCreate()
        {
            base.OnCreate();
            createNotificationChannel();
            Log.Error("App CreateChannel", "onCreate called");
           // createSimpleChannelForServices();
            //createNonstopChannelForServices();
            //createAlarmMedicationChannel();
        }

        private void createNotificationChannel()
        {

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                string name = Constants.ChannelId;
                string description = "my channel desc";
#pragma warning disable CS0618 // Type or member is obsolete
                var importance = NotificationManager.ImportanceHigh;
#pragma warning restore CS0618 // Type or member is obsolete
                NotificationChannel channel = new NotificationChannel(Constants.ChannelId, name, importance)
                {
                    Description = description
                };

                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);

            }
        }

        private void createSimpleChannelForServices() {
            NotificationChannel channel = new NotificationChannel(SimpleChannelIdForServices, "Test simple channel",
                NotificationImportance.Default);
            ((NotificationManager)GetSystemService(NotificationService))
                .CreateNotificationChannel(channel);
            Log.Error("App CreateChannel", "Test simple channel created");
        }

        private void createNonstopChannelForServices() {
            NotificationChannel channel = new NotificationChannel(NonStopChannelIdForServices, "Test nonstop channel",
                  NotificationImportance.Default);
            ((NotificationManager)GetSystemService(NotificationService))
                .CreateNotificationChannel(channel);
            Log.Error("App CreateChannel", "Test nonstop channel created");
        }

        private void createAlarmMedicationChannel() {
            NotificationChannel channel = new NotificationChannel(AlarmMedicationChannelId, "Test alarm medication channel",
               NotificationImportance.High);
            ((NotificationManager)GetSystemService(NotificationService))
                .CreateNotificationChannel(channel);
            Log.Error("App CreateChannel", "Test alarm medication channel created");
        }



    }
}