using System;
using Android.Hardware.Fingerprints;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Widget;
using Android;

namespace FamiliaXamarin.Helpers
{
    internal class FingerprintHandler:FingerprintManager.AuthenticationCallback
    {
        private Context mainActivity;

        public FingerprintHandler(Context mainActivity)
        {
            this.mainActivity = mainActivity;
        }

        internal void StartAuthentication(FingerprintManager fingerprintManager, FingerprintManager.CryptoObject cryptoObject)
        {
            CancellationSignal cenCancellationSignal = new CancellationSignal();
            if (ActivityCompat.CheckSelfPermission(mainActivity, Manifest.Permission.UseFingerprint) != (int)Android.Content.PM.Permission.Granted)
                return;
            fingerprintManager.Authenticate(cryptoObject, cenCancellationSignal, 0, this, null);
        }

        public override void OnAuthenticationFailed()
        {
            Toast.MakeText(mainActivity, "Fingerprint Authentication failed!", ToastLength.Long).Show();
        }

        public override void OnAuthenticationSucceeded(FingerprintManager.AuthenticationResult result)
        {
            mainActivity.StartActivity(new Intent(mainActivity, typeof(MainActivity)));
        }
    }
}