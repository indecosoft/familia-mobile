using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Hardware.Fingerprint;

namespace FamiliaXamarin.Helpers {
    public class BiometricUtils
    {


        public static bool IsBiometricPromptEnabled()
        {
            return (Build.VERSION.SdkInt >= BuildVersionCodes.P);
        }


        /*
         * Condition I: Check if the android version in device is greater than
         * Marshmallow, since fingerprint authentication is only supported
         * from Android 6.0.
         * Note: If your project's minSdkversion is 23 or higher,
         * then you won't need to perform this check.
         *
         * */
        public static bool IsSdkVersionSupported()
        {
            return (Build.VERSION.SdkInt >= BuildVersionCodes.M);
        }



        /*
         * Condition II: Check if the device has fingerprint sensors.
         * Note: If you marked android.hardware.fingerprint as something that
         * your app requires (android:required="true"), then you don't need
         * to perform this check.
         * 
         * */
        public static bool IsHardwareSupported(Context context)
        {
            FingerprintManagerCompat fingerprintManager = FingerprintManagerCompat.From(context);
            return fingerprintManager.IsHardwareDetected;
        }



        /*
         * Condition III: Fingerprint authentication can be matched with a 
         * registered fingerprint of the user. So we need to perform this check
         * in order to enable fingerprint authentication
         * 
         * */
        public static bool IsFingerprintAvailable(Context context)
        {
            FingerprintManagerCompat fingerprintManager = FingerprintManagerCompat.From(context);
            return fingerprintManager.HasEnrolledFingerprints;
        }



        /*
         * Condition IV: Check if the permission has been added to
         * the app. This permission will be granted as soon as the user
         * installs the app on their device.
         * 
         * */
        public static bool IsPermissionGranted(Context context)
        {
            return Android.Support.V4.Content.ContextCompat.CheckSelfPermission(context, Manifest.Permission.UseBiometric) ==
                    Permission.Granted;
        }
    }
}