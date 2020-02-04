using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Familia.Devices.DevicesManagement.Dialogs.Events;
using Familia.Devices.DevicesManagement.Dialogs.Helpers;
using Familia.Devices.Helpers;

namespace Familia.Devices.DevicesManagement.Dialogs {
	public class AddNewDeviceDialog : Dialog {
		private Button _btnBloodPressure;
		private Button _btnBloodGlucose;
		public EventHandler<DialogStateEventArgs> DialogState;
		DeviceType _deviceType = DeviceType.Unknown;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			RequestWindowFeature((int) WindowFeatures.NoTitle);
			SetContentView(Resource.Layout.dialog_add_new_device);
			InitUi();
		}

		private void InitUi() {
			_btnBloodPressure = FindViewById<Button>(Resource.Id.btn_blood_pressure);
			_btnBloodGlucose = FindViewById<Button>(Resource.Id.btn_blood_glucose);
			_btnBloodPressure.Click += (sender, args) => {
				_deviceType = DeviceType.BloodPressure;
				Dismiss();
			};
			_btnBloodGlucose.Click += (sender, args) => {
				_deviceType = DeviceType.Glucose;
				Dismiss();
			};
		}


		public override void Dismiss() {
			base.Dismiss();
			DialogState.Invoke(this,
				new DialogStateEventArgs {Status = DialogStatuses.Dismissed, DeviceType = _deviceType});
		}

		public override void Show() {
			base.Show();
			DialogState.Invoke(this,
				new DialogStateEventArgs {Status = DialogStatuses.Showing, DeviceType = _deviceType});
		}

		protected AddNewDeviceDialog(IntPtr javaReference, JniHandleOwnership transfer) :
			base(javaReference, transfer) { }

		public AddNewDeviceDialog(Context context) : base(context) { }

		protected AddNewDeviceDialog(Context context, bool cancelable, IDialogInterfaceOnCancelListener cancelListener)
			: base(context, cancelable, cancelListener) { }

		public AddNewDeviceDialog(Context context, int themeResId) : base(context, themeResId) { }

		protected AddNewDeviceDialog(Context context, bool cancelable, EventHandler cancelHandler) : base(context,
			cancelable, cancelHandler) { }
	}
}