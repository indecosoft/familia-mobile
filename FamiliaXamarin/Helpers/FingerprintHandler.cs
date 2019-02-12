using System;
using Android.Hardware.Fingerprints;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android;
using Android.Support.V4.Content;
using FamiliaXamarin.Login_System;

namespace FamiliaXamarin.Helpers
{
    internal class FingerprintHandler: FingerprintManager.AuthenticationCallback
    {
        private readonly LoginActivity _mainActivity;
        private int _error;

        public event EventHandler<FingerprintAuthEventArgs> FingerprintAuth;
        public FingerprintHandler(LoginActivity mainActivity)
        {
            _mainActivity = mainActivity;
        }

        internal void StartAuthentication(FingerprintManager fingerprintManager, FingerprintManager.CryptoObject cryptoObject)
        {
            var cenCancellationSignal = new CancellationSignal();
            if (ContextCompat.CheckSelfPermission(_mainActivity, Manifest.Permission.UseFingerprint) != 
                (int)Android.Content.PM.Permission.Granted)
                return;
            fingerprintManager.Authenticate(cryptoObject, cenCancellationSignal, 0, this, null);

            
        }

        public override void OnAuthenticationFailed() => FingerprintAuth?.Invoke(this,
            new FingerprintAuthEventArgs {ErrorsCount = ++_error, Status = false});

        public override void
            OnAuthenticationSucceeded(FingerprintManager.AuthenticationResult result) =>
            FingerprintAuth?.Invoke(this,
                new FingerprintAuthEventArgs
                    {ErrorsCount = _error = 0, Status = true, Result = result});

        internal class FingerprintAuthEventArgs : EventArgs
        {
            public int ErrorsCount { get; set; }
            public bool Status { get; set; }
            public FingerprintManager.AuthenticationResult Result { get; set; }

        }
    }
}