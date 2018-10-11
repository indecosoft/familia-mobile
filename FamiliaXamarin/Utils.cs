using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Telephony;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Java.Lang;
using Java.Text;
using Java.Util;
using Java.Util.Regex;
using Org.Json;
using ZXing;
using ZXing.Common;
using ZXing.Mobile;
using Exception = System.Exception;
using Math = System.Math;
using Orientation = Android.Widget.Orientation;


namespace FamiliaXamarin
{
    internal class Utils
    {
        private static NotificationManager _notificationManager;
        public static void SetDefaults(string key, string value, Context context)
        {
            var preferences = PreferenceManager.GetDefaultSharedPreferences(context);
            var editor = preferences.Edit();
            editor.PutString(key, value);
            editor.Apply();
        }


        public static string GetDefaults(string key, Context context)
        {
            var preferences = PreferenceManager.GetDefaultSharedPreferences(context);
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
            TelephonyManager mgr = ctx.GetSystemService(Context.TelephonyService) as TelephonyManager;
            return mgr?.Imei;
        }
        public static Bitmap CheckRotation(string photoPath, Bitmap bitmap)
        {
            var ei = new ExifInterface(photoPath);
            var orientation = ei.GetAttributeInt(ExifInterface.TagOrientation, (int)Android.Media.Orientation.Undefined);

            Bitmap rotatedBitmap;
            switch (orientation)
            {

                case (int)Android.Media.Orientation.Rotate90:
                    rotatedBitmap = RotateImage(bitmap, 90);
                    break;

                case (int)Android.Media.Orientation.Rotate180:
                    rotatedBitmap = RotateImage(bitmap, 180);
                    break;

                case (int)Android.Media.Orientation.Rotate270:
                    rotatedBitmap = RotateImage(bitmap, 270);
                    break;
                case (int)Android.Media.Orientation.FlipHorizontal:
                    return Flip(bitmap, true, false);

                case (int)Android.Media.Orientation.FlipVertical:
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

                string token = Utils.GetDefaults("Token", ctx);


                SimpleDateFormat sdf = new SimpleDateFormat("dd/MM/yyyy HH:mm:ss");

                string genDateTime = sdf.Format(new Date());
                Date d1 = sdf.Parse(genDateTime);
                Calendar cal = Calendar.Instance;
                cal.Time = d1;
                cal.Add(CalendarField.Minute, 30);
                string expDateTime = sdf.Format(cal.Time);
                //Log.e("newTime", expDateTime);

                JSONObject qrCodeData = new JSONObject().Put("clientToken", token).Put("generationDateTime", genDateTime).Put("expirationDateTime", expDateTime);


                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new EncodingOptions { Height = 1000, Width = 1000 }

                };
                var bitmap = writer.Write(qrCodeData.ToString());

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

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                // Check if there is a way the user can resolve the issue
                var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error("MainActivity", "There is a problem with Google Play Services on this device: {0} - {1}",
                    queryResult, errorString);

                // Alternately, display the error to the user.
            }

            return false;
        }
        private static double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
        public static double HaversineFormula(double CurrentLatitude, double CurrentLongitude, double DestLatitude, double DestLongitude)
        {

            //R => raza pamantului
            //a => jumatatea patratului dintre lungimea coardei dintre puncte
            //c => distanta unghiulara in radiani
            //d => distanta finala in metri

            double r = 6371e3;
            //conversie in Radiani

            double radLat1 = ToRadians(CurrentLatitude);
            double radLat2 = ToRadians(DestLatitude);

            //diferenta dintre longitudine si latitudine in radiani
            double difLat = ToRadians(DestLatitude - CurrentLatitude);
            double difLong = ToRadians(DestLongitude - CurrentLongitude);
            //calculare
            double a = Math.Sin(difLat / 2) * Math.Sin(difLat / 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Sin(difLong / 2) * Math.Sin(difLong / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            //(d)
            return r * c;
        }
        public static void CreateChannels()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
            var androidChannel = new NotificationChannel("ANDROID_CHANNEL_ID", "ANDROID_CHANNEL_NAME", NotificationImportance.High);
            androidChannel.EnableLights(true);
            androidChannel.EnableVibration(true);
            androidChannel.LightColor = Color.Green;
            androidChannel.LockscreenVisibility = NotificationVisibility.Private;

            GetManager().CreateNotificationChannel(androidChannel);
        }

        public static NotificationManager GetManager()
        {
            return _notificationManager ??
                   (_notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService));
        }
        public static NotificationCompat.Builder GetAndroidChannelNotification(string title, string body, string buttonTitle, int type, Context context, string room)
        {
            var intent = new Intent(context, typeof(ChatActivity));
            var rejectintent = new Intent(context, typeof(ChatActivity));
            //intent.setAction("ro.indecosoft.familia_ingrijire_paleativ");
            //intent.putExtra("100", 0);
            
            switch (type)
            {
                //1 => request
                //2 => accept
                //3 => message
                case 1:
                   
                    intent.PutExtra("AcceptClick", true);
                    intent.PutExtra("EmailFrom", body.Replace(" doreste sa ia legatura cu tine!", ""));
                    intent.PutExtra("Room", room);
                    rejectintent.PutExtra("RejectClick", true);
                    var acceptIntent = PendingIntent.GetActivity(context, 1, intent, PendingIntentFlags.OneShot);
                    var rejectIntent = PendingIntent.GetActivity(context, 1, rejectintent, PendingIntentFlags.OneShot);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        return new NotificationCompat.Builder(context, "ANDROID_CHANNEL_ID")
                            .SetContentTitle(title)
                            .SetContentText(body)
                            .SetSmallIcon(Resource.Drawable.logo)
                            .SetStyle(new NotificationCompat.BigTextStyle()
                                .BigText(body))
                            .SetPriority(NotificationCompat.PriorityDefault)
                            .SetContentIntent(acceptIntent)
                            .SetAutoCancel(true)
                            .AddAction(Resource.Drawable.logo, "Refuza", rejectIntent)
                            .AddAction(Resource.Drawable.logo, "Accepta", acceptIntent);
                    }

                    break;

                case 2:
                    intent.PutExtra("Conv", true);
                    intent.PutExtra("Room", room);
                    intent.PutExtra("EmailFrom", body.Replace(" ti-a acceptat cererea de chat!", ""));

                    var acceptIntent1 = PendingIntent.GetActivity(context, 1, intent, PendingIntentFlags.OneShot);
                    //var rejectIntent1 = PendingIntent.GetActivity(context, 1, rejectintent, PendingIntentFlags.OneShot);
                    //intent.putExtra("ConversationClick",true);
                    
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {

                        return new NotificationCompat.Builder(context, "ANDROID_CHANNEL_ID")
                            .SetContentTitle(title)
                            .SetContentText(body)
                            .SetSmallIcon(Resource.Drawable.logo)
                            .SetStyle(new NotificationCompat.BigTextStyle()
                                .BigText(body))
                            .SetPriority(NotificationCompat.PriorityDefault)
                            .SetContentIntent(acceptIntent1)
                            .SetAutoCancel(true)
                            .AddAction(Resource.Drawable.logo, buttonTitle, acceptIntent1);
                    }

                    break;
            }

            return null;
        }


    }
}