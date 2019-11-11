using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Graphics;
using Android.Locations;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.App;
using Android.Telephony;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Familia.Helpers;
using FamiliaXamarin.Chat;
using Java.Lang;
using Java.Text;
using Java.Util;
using Org.Json;
using System;
using System.Text.RegularExpressions;
using Android.Widget;
using ZXing;
using ZXing.Common;
using ZXing.Mobile;
using Exception = System.Exception;
using Math = System.Math;
using Orientation = Android.Media.Orientation;
using Resource = Familia.Resource;

namespace FamiliaXamarin.Helpers
{
    public static class Utils
    {
        public static bool util = false;
        public static bool IsActivityRunning(Class activityClass)
        {
            ActivityManager activityManager = (ActivityManager)Application.Context.GetSystemService(Context.ActivityService);
            //var tasks = activityManager.GetRunningTasks(Integer.MaxValue);
            var tasks = activityManager.AppTasks;

            foreach (var task in tasks)
            {
                if (activityClass.CanonicalName.Equals(task.TaskInfo.BaseActivity.ClassName))
                    return true;
            }

            return false;
        }
        
        public static void CloseRunningActivity(Type activityType)
        {
            ActivityManager activityManager = (ActivityManager)Application.Context.GetSystemService(Context.ActivityService);
            //var tasks = activityManager.GetRunningTasks(Integer.MaxValue);
            var tasks = activityManager.AppTasks;

            foreach (var task in tasks)
            {
                if (Class.FromType(activityType).CanonicalName.Equals(task.TaskInfo.BaseActivity.ClassName))
                    task.FinishAndRemoveTask();
            }
        }

        public static void SetDefaults(string key, string value)
        {
            var preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var editor = preferences.Edit();
            editor.PutString(key, value);
            editor.Apply();
        }
        public static void RemoveDefaults()
        {
            Application.Context.GetSharedPreferences(PreferenceManager.GetDefaultSharedPreferencesName(Application.Context), 0).Edit().Clear().Commit();
        }

        public static string GetDefaults(string key)
        {
            var preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            return preferences.GetString(key, null);
        }

        public static bool PasswordValidator(string s)
        {
            var regex = new Regex("^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,100}$");
            var match = regex.Match(s);
            return match.Success;
        }

        public static bool EmailValidator(string s)
        {
            var regex = new Regex("^(([^<>()\\[\\]\\\\.,;:\\s@\"]+(\\.[^<>()\\[\\]\\\\.,;:\\s@\"]+)*)|(\".+\"))@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}])|(([a-zA-Z\\-0-9]+\\.)+[a-zA-Z]{2,}))$");
            var match = regex.Match(s);
            return match.Success;
        }


      
        public static string GetImei(Context ctx)
        {

        /**
         * return imei if android version is 9 or below
         * return android_id if android version is bigger than 9*
         */
            TelephonyManager mgr = ctx.GetSystemService(Context.TelephonyService) as TelephonyManager;
            if (mgr.Imei == null)
            {
                var android_id = testAndroidIdForAndroid10("android10imei", ctx);
                if (android_id != null)
                {
                    return android_id;
                }
            }

            return mgr?.Imei;
        }

        public static string testAndroidIdForAndroid10(string imei, Context ctx)
        {
            if (Build.VERSION.SdkInt > BuildVersionCodes.P)
            {
                Log.Error("UTILS IMEI", "hello android 10");
                var android_id = Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                Log.Error("UTILS IMEI", android_id);
                return android_id;
            }

            return null;
        }

        public static Bitmap CheckRotation(string photoPath, Bitmap bitmap)
        {
            var ei = new ExifInterface(photoPath);
            var orientation = ei.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Undefined);

            Bitmap rotatedBitmap;
            switch (orientation)
            {

                case (int)Orientation.Rotate90:
                    rotatedBitmap = RotateImage(bitmap, 90);
                    break;

                case (int)Orientation.Rotate180:
                    rotatedBitmap = RotateImage(bitmap, 180);
                    break;

                case (int)Orientation.Rotate270:
                    rotatedBitmap = RotateImage(bitmap, 270);
                    break;
                case (int)Orientation.FlipHorizontal:
                    return Flip(bitmap, true, false);

                case (int)Orientation.FlipVertical:
                    return Flip(bitmap, false, true);
                default:
                    rotatedBitmap = bitmap;
                    break;
            }

            return rotatedBitmap;
        }
        private static Bitmap RotateImage(Bitmap source, float angle)
        {
            var matrix = new Matrix();
            matrix.PostRotate(angle);
            return Bitmap.CreateBitmap(source, 0, 0, source.Width, source.Height,
                matrix, true);
        }
        private static Bitmap Flip(Bitmap bitmap, bool horizontal, bool vertical)
        {
            var matrix = new Matrix();
            matrix.PreScale(horizontal ? -1 : 1, vertical ? -1 : 1);
            return Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
        }
        public static void HideKeyboard(Activity activity)
        {
            var imm = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            //Find the currently focused view, so we can grab the correct window token from it.
            var view = activity.CurrentFocus ?? new View(activity);
            //If no view currently has focus, create a new one, just so we can grab a window token from it
            imm.HideSoftInputFromWindow(view.WindowToken, 0);
        }
        public static Bitmap GenQrCode(Context ctx)
        {
            try
            {

                string token = GetDefaults("Token");
                string email = GetDefaults("Email");


                SimpleDateFormat sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");

                string genDateTime = sdf.Format(new Date());
                Date d1 = sdf.Parse(genDateTime);
                Calendar cal = Calendar.Instance;
                cal.Time = d1;
                cal.Add(CalendarField.Minute, 30);
                string expDateTime = sdf.Format(cal.Time);
                //Log.e("newTime", expDateTime);

                JSONObject qrCodeData = new JSONObject().Put("clientToken", token).Put("generationDateTime", genDateTime).Put("expirationDateTime", expDateTime).Put("email", email).Put("Name", GetDefaults("Name")).Put("Avatar", GetDefaults("Avatar")).Put("Id", GetDefaults("IdClient"));


                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new EncodingOptions { Height = 1000, Width = 1000 }

                };
                var bitmap = writer.Write(Encryption.Encrypt(qrCodeData.ToString()));

                return bitmap;
            }
            catch (Exception e)
            {
                Log.Error("ErrorGeneratingQRCode", e.ToString());
            }
            return null;
        }
        public static bool IsGooglePlayServicesInstalled(Context ctx)
        {
            var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(ctx);
            if (queryResult == ConnectionResult.Success)
            {
                Log.Info("MainActivity", "Google Play Services is installed on this device.");
                return true;
            }

            if (!GoogleApiAvailability.Instance.IsUserResolvableError(queryResult)) return false;
            // Check if there is a way the user can resolve the issue
            var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
            Log.Error("MainActivity", "There is a problem with Google Play Services on this device: {0} - {1}",
                queryResult, errorString);

            return false;
        }
        private static double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
        public static double HaversineFormula(double currentLatitude, double currentLongitude, double destLatitude, double destLongitude)
        {

            //R => raza pamantului
            //a => jumatatea patratului dintre lungimea coardei dintre puncte
            //c => distanta unghiulara in radiani
            //d => distanta finala in metri

            double r = 6371e3;
            //conversie in Radiani

            double radLat1 = ToRadians(currentLatitude);
            double radLat2 = ToRadians(destLatitude);

            //diferenta dintre longitudine si latitudine in radiani
            double difLat = ToRadians(destLatitude - currentLatitude);
            double difLong = ToRadians(destLongitude - currentLongitude);
            //calculare
            double a = Math.Sin(difLat / 2) * Math.Sin(difLat / 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Sin(difLong / 2) * Math.Sin(difLong / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            //(d)
            return r * c;
        }
        public static void CreateChannels(string channelId,string channel)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
            var androidChannel = new NotificationChannel(channelId, channel, NotificationImportance.High);
            androidChannel.EnableLights(true);
            androidChannel.EnableVibration(true);
            androidChannel.LightColor = Color.Green;
            androidChannel.LockscreenVisibility = NotificationVisibility.Private;

            GetManager().CreateNotificationChannel(androidChannel);
        }

        public static NotificationManager GetManager()
        {
            return (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
        }
        /// <summary>
        /// Create Android Channel Notification for chat service
        /// </summary>
        /// <param name="title">Title of notification</param>
        /// <param name="body">Body of notification</param>
        /// <param name="email">Email of person that initiate the conversation</param>
        /// <param name="room">The conversation room name</param>
        /// <param name="type">Notification type (0 - request (default),1 - Request Rejected, 2 - Request Accepted, 3 - Message)</param>
        /// <param name="buttonTitle">Title of Button (oprional)</param>
        /// <returns></returns>
        public static Notification CreateChatNotification(string title, string body, string email,
            string room, Context ctx, int type = 0, string buttonTitle = "Converseaza")
        {
            var chatActivityAcceptedIntent = new Intent(Application.Context, typeof(ChatActivity));
            var chatActivityRejectedIntent = new Intent(Application.Context, typeof(RejectChatBroadcastReceiver));

            chatActivityAcceptedIntent.AddFlags(ActivityFlags.ClearTop);
            chatActivityRejectedIntent.AddFlags(ActivityFlags.ClearTop);

            chatActivityRejectedIntent.PutExtra("EmailFrom", email);
            chatActivityAcceptedIntent.PutExtra("EmailFrom", email);

            chatActivityAcceptedIntent.PutExtra("Room", room);
            chatActivityRejectedIntent.PutExtra("Room", room);

            chatActivityAcceptedIntent.PutExtra("Active", true);
            chatActivityRejectedIntent.PutExtra("Active", true);
            //0
            switch (type)
            {
                case 0:
                    //Notificare daca vrea sa vorbeasca cu celalat user
                    chatActivityAcceptedIntent.PutExtra("AcceptClick", true);
                    chatActivityRejectedIntent.PutExtra("RejectClick", true);
                    var stackBuilderAccept = Android.Support.V4.App.TaskStackBuilder.Create(Application.Context);
                   
                    stackBuilderAccept.AddNextIntentWithParentStack(chatActivityAcceptedIntent);
                   
                    // Get the PendingIntent containing the entire back stack
                    var acceptIntent = stackBuilderAccept.GetPendingIntent(DateTime.Now.Millisecond, (int)PendingIntentFlags.OneShot);

                     var rejectIntent = PendingIntent.GetBroadcast(Application.Context, DateTime.Now.Millisecond, chatActivityRejectedIntent, PendingIntentFlags.OneShot);

                    return new NotificationCompat.Builder(Application.Context, email)
                        .SetContentTitle(title)
                        .SetContentText(body)
                        .SetSmallIcon(Resource.Drawable.logo)
                        .AddAction(Resource.Drawable.logo, "Accepta", acceptIntent)
                        .AddAction(Resource.Drawable.logo, "Refuza", rejectIntent)
                        .SetStyle(new NotificationCompat.BigTextStyle()
                            .BigText(body))
                        .SetPriority(NotificationCompat.PriorityDefault)
                        .SetAutoCancel(true)
                        .SetOngoing(false)
                        .Build();
                        
                case 1:
                    //Notificare daca NU i-a acceptat cererea de chat
                    return new NotificationCompat.Builder(Application.Context, email)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetSmallIcon(Resource.Drawable.logo)
                .SetOngoing(false)
                .SetAutoCancel(true)
                .Build();
                case 2:
                    //Notificare daca i-a acceptat cererea de chat
                    var stackBuilder = Android.Support.V4.App.TaskStackBuilder.Create(ctx);
                    stackBuilder.AddNextIntentWithParentStack(chatActivityAcceptedIntent);

                   // var acceptIntent1 = PendingIntent.GetActivity(Application.Context, 3, chatActivityAcceptedIntent, PendingIntentFlags.OneShot);
                    var acceptIntent1 = stackBuilder.GetPendingIntent(DateTime.Now.Millisecond, (int)PendingIntentFlags.OneShot);


                    return new NotificationCompat.Builder(Application.Context, email)
                            .SetContentTitle(title)
                            .SetContentText(body)
                            .SetSmallIcon(Resource.Drawable.logo)
                            .SetStyle(new NotificationCompat.BigTextStyle()
                                .BigText(body))
                            .SetPriority(NotificationCompat.PriorityDefault)
                            .SetContentIntent(acceptIntent1)
                            .SetOngoing(false)
                            .SetAutoCancel(true)
                            .AddAction(Resource.Drawable.logo, buttonTitle, acceptIntent1)
                            .Build();
                case 3:
                    //Notificare pentru mesaj
                    //chatActivityAcceptedIntent.PutExtra("NewMessage", body);
                    ChatActivity.Messages.Add(new MessagesModel(){Room = room, Message = body});
                    var stackBuilderIntent2 = Android.Support.V4.App.TaskStackBuilder.Create(ctx);
                    stackBuilderIntent2.AddNextIntentWithParentStack(chatActivityAcceptedIntent);

                   // var acceptIntent2 = PendingIntent.GetActivity(Application.Context, 4, chatActivityAcceptedIntent, PendingIntentFlags.OneShot);
                    var acceptIntent2 = stackBuilderIntent2.GetPendingIntent(DateTime.Now.Millisecond, (int)PendingIntentFlags.OneShot);

                    return new NotificationCompat.Builder(Application.Context, title)
                            .SetContentTitle(title)
                            .SetContentText(body)
                            .SetSmallIcon(Resource.Drawable.logo)
                            .SetStyle(new NotificationCompat.BigTextStyle()
                                .BigText(body))
                            .SetOngoing(false)
                            .SetPriority(NotificationCompat.PriorityDefault)
                            .SetContentIntent(acceptIntent2)
                            .SetAutoCancel(true)
                            .AddAction(Resource.Drawable.logo, buttonTitle, acceptIntent2)
                            .Build();
                    
            }

            return null;
        }

        public static bool CheckIfLocationIsEnabled()
        {
            LocationManager lm = (LocationManager)(Application.Context.GetSystemService(Context.LocationService));
            bool gpsEnabled = false;
            bool networkEnabled = false;

            try
            {
                gpsEnabled = lm.IsProviderEnabled(LocationManager.GpsProvider);
            }
            catch
            {
                // ignored
            }

            try
            {
                networkEnabled = lm.IsProviderEnabled(LocationManager.NetworkProvider);
            }
            catch
            {
                // ignored
            }

            return gpsEnabled && networkEnabled;

        }
        public static bool CheckNetworkAvailability()
        {
            ConnectivityManager cm = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            NetworkInfo activeNetwork = cm.ActiveNetworkInfo;
            return activeNetwork != null && activeNetwork.IsConnected;
        }

    }
}