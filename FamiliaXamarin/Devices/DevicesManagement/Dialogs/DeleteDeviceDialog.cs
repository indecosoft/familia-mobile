using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Familia.DataModels;
using Familia.Devices.DevicesManagement.Dialogs.Events;
using Familia.Devices.DevicesManagement.Dialogs.Helpers;
using Familia.Devices.DevicesManagement.Dialogs.Models;
using Familia.Helpers;

namespace Familia.Devices.DevicesManagement.Dialogs
{
    class DeleteDeviceDialog : Dialog
    {
        private readonly DeviceEditingManagementModel? _model;
        public EventHandler<DialogStateEventArgs> DialogState;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.dialog_delete_device);
            InitUi();
        }

        private void InitUi()
        {
            FindViewById<Button>(Resource.Id.btn_confirm).Click += async (sender, args) => {
                var sqlHelper = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                await sqlHelper.QueryValuations($"DELETE FROM BluetoothDeviceRecords WHERE Id ='{_model?.Device.Id}'");
                Dismiss();
            };
            FindViewById<Button>(Resource.Id.btn_cancel).Click += (sender, args) => Dismiss();
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
        public DeleteDeviceDialog(Context context, DeviceEditingManagementModel? model) : base(context)
        {
            _model = model;
        }
    }
}