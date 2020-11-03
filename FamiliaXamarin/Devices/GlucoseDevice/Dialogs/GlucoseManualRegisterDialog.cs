using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Familia.Devices.DevicesManagement.Dialogs.Events;
using Familia.Devices.DevicesManagement.Dialogs.Helpers;
using Familia.Devices.PressureDevice;
using Google.Android.Material.TextField;

namespace Familia.Devices.GlucoseDevice.Dialogs
{
    public class GlucoseManualRegisterDialog : Dialog
	{
		private Button _btnCancel;
		private Button _btnRegister;
		private EditText editTextGlucose;
		private TextInputLayout _editTextGlucoseLayout;


		public EventHandler<DialogStateEventArgs> DialogState;
		public float? Glucose { get; set; }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			RequestWindowFeature((int)WindowFeatures.NoTitle);
			SetContentView(Resource.Layout.dialog_manual_glucose);
			InitUi();
		}

		private bool checkField(string value, ref TextInputLayout field)
		{
			if (string.IsNullOrEmpty(value))
			{
				field.Error = "Camp obligatoriu";
				return false;
			}
			else
			{
				field.Error = null;
				return true;

			}
		}
		private bool FormIsValid()
		{
			return checkField(editTextGlucose.Text, ref _editTextGlucoseLayout);

		}
		private void InitUi()
		{
			_btnCancel = FindViewById<Button>(Resource.Id.btn_cancel);
			_btnRegister = FindViewById<Button>(Resource.Id.btn_add);
			editTextGlucose = FindViewById<EditText>(Resource.Id.et_glucose);
			_editTextGlucoseLayout = FindViewById<TextInputLayout>(Resource.Id.et_glucose_layout);

			_btnCancel.Click += (sender, args) => {
				Glucose = null;
				Dismiss();
			};
			_btnRegister.Click += (sender, args) => {
				if (FormIsValid())
				{
					Glucose = string.IsNullOrEmpty(editTextGlucose.Text) ? 0 : float.Parse(editTextGlucose.Text);
					Dismiss();
				}

			};
		}


		public override void Dismiss()
		{
			base.Dismiss();
			DialogState.Invoke(this,
				new DialogStateEventArgs { Status = DialogStatuses.Dismissed });
		}

		public override void Show()
		{
			base.Show();
			DialogState.Invoke(this,
				new DialogStateEventArgs { Status = DialogStatuses.Showing });
		}

		protected GlucoseManualRegisterDialog(IntPtr javaReference, JniHandleOwnership transfer) :
			base(javaReference, transfer)
		{ }

		public GlucoseManualRegisterDialog(Context context) : base(context)
		{

		}

		protected GlucoseManualRegisterDialog(Context context, bool cancelable, IDialogInterfaceOnCancelListener cancelListener)
			: base(context, cancelable, cancelListener) { }

		public GlucoseManualRegisterDialog(Context context, int themeResId) : base(context, themeResId) { }

		protected GlucoseManualRegisterDialog(Context context, bool cancelable, EventHandler cancelHandler) : base(context,
			cancelable, cancelHandler)
		{ }
	}
}
