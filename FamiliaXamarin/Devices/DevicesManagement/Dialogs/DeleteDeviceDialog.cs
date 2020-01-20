using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Familia;
using Familia.Devices.DevicesManagement.Dialogs.DialogEvents;
using Familia.Devices.DevicesManagement.Dialogs.DialogHelpers;
using FamiliaXamarin.DataModels;
using FamiliaXamarin.Helpers;

namespace FamiliaXamarin.Devices
{
    class DeleteDeviceDialog : Dialog
    {
        private Context _context;
        private readonly DeviceEditingManagementModel _model;
        public EventHandler<DialogStateEventArgs> DialogState;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.dialog_delete_device);
            InitUi();
        }

        private void InitUi()
        {
            FindViewById<Button>(Resource.Id.btn_confirm).Click += async delegate(object sender, EventArgs args)
            {
                var sqlHelper =
                    await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                await sqlHelper.QueryValuations($"DELETE FROM BluetoothDeviceRecords WHERE Id ='{_model.Device.Id}'");
                Dismiss();
            };
            FindViewById<Button>(Resource.Id.btn_cancel).Click += delegate (object sender, EventArgs args)
            {
                Dismiss();
            };
        }

        public override void Dismiss()
        {
            base.Dismiss();
            DialogState.Invoke(this, new DialogStateEventArgs
                { Status = DialogStatuses.Dismissed });
        }

        public override void Show()
        {
            base.Show();
            DialogState.Invoke(this, new DialogStateEventArgs
                { Status = DialogStatuses.Showing });
        }
        public DeleteDeviceDialog(Context context, DeviceEditingManagementModel model) : base(context)
        {
            _context = context;
            _model = model;
        }
    }
}