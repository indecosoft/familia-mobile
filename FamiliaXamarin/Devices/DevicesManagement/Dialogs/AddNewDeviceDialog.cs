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
using Familia.Devices;
using Newtonsoft.Json;
using System.Collections.Generic;
using Familia.Devices.DevicesManagement.Dialogs.DialogEvents;
using Familia.Devices.Helpers;
using Familia.Devices.DevicesManagement.Dialogs.DialogHelpers;

namespace FamiliaXamarin.Devices {
    public class AddNewDeviceDialog : Dialog
    {
        private Button _btnBloodPressure;
        private Button _btnBloodGlucose;
        private Context _context;
        public EventHandler<DialogStateEventArgs> DialogState;
        DeviceType DeviceType = DeviceType.Unknown;
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
            _btnBloodPressure.Click += delegate(object sender, EventArgs args)
            {
                DeviceType = DeviceType.BloodPressure;
                Dismiss();
            };
            _btnBloodGlucose.Click += delegate(object sender, EventArgs args)
            {
                DeviceType = DeviceType.Glucose;
                Dismiss();
            };
        }

        
        public override void Dismiss()
        {
            base.Dismiss();
            DialogState.Invoke(this, new DialogStateEventArgs{
                Status = DialogStatuses.Dismissed,
                DeviceType = DeviceType
            });
        }

        public override void Show()
        {
            base.Show();
            DialogState.Invoke(this, new DialogStateEventArgs {
                Status = DialogStatuses.Showing,
                DeviceType = DeviceType
            });
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
            _context = context;
        }

        public AddNewDeviceDialog(Context context, int themeResId) : base(context, themeResId)
        {
            _context = context;
        }

        protected AddNewDeviceDialog(Context context, bool cancelable, EventHandler cancelHandler) : base(context, cancelable, cancelHandler)
        {
            _context = context;
        }
    }
}