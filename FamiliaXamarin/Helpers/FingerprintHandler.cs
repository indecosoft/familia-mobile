using Android.Hardware.Fingerprints;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android;
using Android.Support.V4.Content;
using FamiliaXamarin.Login_System;

namespace FamiliaXamarin.Helpers
{
    internal class FingerprintHandler:FingerprintManager.AuthenticationCallback
    {
        private readonly LoginActivity _mainActivity;
        private int _error;

        public FingerprintHandler(LoginActivity mainActivity)
        {
            _mainActivity = mainActivity;
        }

        internal void StartAuthentication(FingerprintManager fingerprintManager, FingerprintManager.CryptoObject cryptoObject)
        {
            CancellationSignal cenCancellationSignal = new CancellationSignal();
            if (ContextCompat.CheckSelfPermission(_mainActivity, Manifest.Permission.UseFingerprint) != (int)Android.Content.PM.Permission.Granted)
                return;
            fingerprintManager.Authenticate(cryptoObject, cenCancellationSignal, 0, this, null);
        }

        public override void OnAuthenticationFailed()
        {
            Vibrator vibrator = (Vibrator)_mainActivity.GetSystemService(Context.VibratorService);
            vibrator?.Vibrate(VibrationEffect.CreateOneShot(100, VibrationEffect.DefaultAmplitude));
            Toast.MakeText(_mainActivity, "Amprenta nerecunoscuta!", ToastLength.Long).Show();
            _error++;
            if (_error != 5) return;
            Toast.MakeText(_mainActivity, "5 incercari gresite de verificare a amprentelor!", ToastLength.Long).Show();
            _error = 0;
        }

        public override void OnAuthenticationSucceeded(FingerprintManager.AuthenticationResult result)
        {
            Intent intent = new Intent(_mainActivity, typeof(MainActivity));
            _mainActivity.StartActivity(intent);
            _mainActivity.Finish();
        }
    }
}