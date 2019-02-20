using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Familia;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;

namespace FamiliaXamarin.Devices
{
    public class DeviceManagementDialog : Dialog
    {

        private EditText _etDeviceName;
        private Button _btnSave;
        private Button _btnCancel;
        private DevicesManagementModel _deviceModel;
        private Context _context;
        
        public EventHandler<DialogStateEventArgs> DialogState;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.dialog_devices_management);
            InitUi();

        }

        private void InitUi()
        {
            _etDeviceName = FindViewById<EditText>(Resource.Id.et_device_name);
            _btnSave = FindViewById<Button>(Resource.Id.btn_save);
            _btnCancel = FindViewById<Button>(Resource.Id.btn_cancel);
            _etDeviceName.Text = _deviceModel.Device.Name;
            _btnSave.Click += async delegate(object sender, EventArgs args)
            {
                SqlHelper<BluetoothDeviceRecords> db = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                await db.QueryValuations(
                    $"Update BluetoothDeviceRecords set Name = '{_etDeviceName.Text}' where Id = '{_deviceModel.Device.Id}'");
                Dismiss();
            };
            _btnCancel.Click += delegate(object sender, EventArgs args)
            {
                Dismiss();
            };
        }

        protected DeviceManagementDialog(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public DeviceManagementDialog(Context context, DevicesManagementModel model) : base(context)
        {
            
            _deviceModel = model;
            _context = context;
            SetTitle("Editare dispozitiv");
        }

        protected DeviceManagementDialog(Context context, bool cancelable, IDialogInterfaceOnCancelListener cancelListener) : base(context, cancelable, cancelListener)
        {
        }

        public override void Dismiss()
        {
            base.Dismiss();
            DialogState.Invoke(this, new DialogStateEventArgs
                {Status = DialogStateEventArgs.DialogStatus.Dismissed});
        }
       
        public override void Show()
        {
            base.Show();
            DialogState.Invoke(this,new DialogStateEventArgs
                {Status = DialogStateEventArgs.DialogStatus.Showing});
        } 

        public DeviceManagementDialog(Context context, int themeResId) : base(context, themeResId)
        {
        }

        protected DeviceManagementDialog(Context context, bool cancelable, EventHandler cancelHandler) : base(context, cancelable, cancelHandler)
        {
        }
    }

    public class DialogStateEventArgs : EventArgs
    {
        public DialogStatus Status { get;  set; }
        public enum DialogStatus
        {
            Dismissed = 1,
            Showing = 2,
            Hidden = 3,
            Canceled = 4
        }
    }
}