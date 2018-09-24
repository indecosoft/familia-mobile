using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Telephony;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Java.Text;
using Java.Util;
using Java.Util.Regex;
using Org.Json;
using ZXing;
using ZXing.Common;
using Orientation = Android.Widget.Orientation;


namespace FamiliaXamarin
{
    internal class Utils
    {

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
        public static Bitmap GenQRCode(Context ctx)
        {
            try
            {

                string token = Utils.GetDefaults("Token",ctx);


                SimpleDateFormat sdf = new SimpleDateFormat("dd/MM/yyyy HH:mm:ss");

                string genDateTime = sdf.Format(new Date());
                Date d1 = sdf.Parse(genDateTime);
                Calendar cal = Calendar.Instance;
                cal.Time = d1;
                cal.Add(Calendar.Minute, 30);
                String expDateTime = sdf.Format(cal.Time);
                //Log.e("newTime", expDateTime);

                JSONObject qrCodeData = new JSONObject().Put("clientToken", token).Put("generationDateTime", genDateTime).Put("expirationDateTime", expDateTime);

                //ZXing.BarcodeReader
                //var content = "123456789012345678";
                var options = new EncodingOptions();
                options.Height = 1000;
                options.Width = 1000;
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = options

                };
                var bitmap = writer.Write(qrCodeData.ToString());
//                BarcodeWriter barcodeEncoder = new BarcodeWriter();
//                var bitmap = barcodeEncoder.Encoder.encode(qrCodeData.ToString(), BarcodeFormat.QR_CODE, 1000, 1000);

                return bitmap;
            }
            catch (Exception e)
            {
                Log.Error("ErrorGeneratingQRCode", e.ToString());
            }
            return null;
        }
    }
}