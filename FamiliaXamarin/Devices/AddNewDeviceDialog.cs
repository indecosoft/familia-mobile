using System;
using Familia;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Devices.GlucoseDevice;
using FamiliaXamarin.Devices.PressureDevice;
using FamiliaXamarin.Helpers;

namespace FamiliaXamarin.Devices
{
    public class AddNewDeviceDialog : Dialog
    {
        private Button _btnBloodPressure;
        private Button _btnBloodGlucose;
        private Context _context;
        public EventHandler<DialogStateEventArgs> DialogState;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.dialog_add_new_device);
            InitUi();

        }
        
        private void InitUi()
        {
            _btnBloodPressure = FindViewById<Button>(Resource.Id.btn_blood_pressure);
            _btnBloodGlucose = FindViewById<Button>(Resource.Id.btn_blood_glucose);
            _btnBloodPressure.Click += async delegate(object sender, EventArgs args)
            {
                var intent = new Intent(_context, typeof(AddNewBloodPressureDeviceActivity));
                intent.PutExtra("RegisterOnly", true);
                _context.StartActivity(intent);
                Dismiss();
            };
            _btnBloodGlucose.Click += delegate(object sender, EventArgs args)
            {
                var intent = new Intent(_context, typeof(AddNewGucoseDeviceActivity));
                intent.PutExtra("RegisterOnly", true);
                _context.StartActivity(intent);
                Dismiss();
            };
        }

        public override void Dismiss()
        {
            base.Dismiss();
            DialogState.Invoke(this, new DialogStateEventArgs{Status = DialogStateEventArgs.DialogStatus.Dismissed});
        }

        public override void Show()
        {
            base.Show();
            DialogState.Invoke(this, new DialogStateEventArgs { Status = DialogStateEventArgs.DialogStatus.Showing });
        }

        protected AddNewDeviceDialog(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public AddNewDeviceDialog(Context context) : base(context)
        {
            _context = context;
        }

        protected AddNewDeviceDialog(Context context, bool cancelable, IDialogInterfaceOnCancelListener cancelListener) : base(context, cancelable, cancelListener)
        {
        }

        public AddNewDeviceDialog(Context context, int themeResId) : base(context, themeResId)
        {
        }

        protected AddNewDeviceDialog(Context context, bool cancelable, EventHandler cancelHandler) : base(context, cancelable, cancelHandler)
        {
        }
    }
}