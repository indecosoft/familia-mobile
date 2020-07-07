using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Familia.Devices.DevicesManagement.Dialogs.Events;
using Familia.Devices.DevicesManagement.Dialogs.Helpers;
using Familia.Devices.Helpers;
namespace Familia.Devices.PressureDevice.Dialogs
{
    public class BloodPressureManualRegisterDialog : Dialog
    {
		private Button _btnCancel;
		private Button _btnRegister;
		private EditText editTextSystolic;
		private EditText editTextDiastolic;
		private EditText editTextPulse;
		private TextInputLayout _editTextSystolicLayout;
		private TextInputLayout _editTextDiastolicLayout;
		private TextInputLayout _editTextPulseLayout;


		public EventHandler<DialogStateEventArgs> DialogState;
		private BloodPressureData model;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			RequestWindowFeature((int)WindowFeatures.NoTitle);
			SetContentView(Resource.Layout.dialog_manual_blood_pressure);
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
			return checkField(editTextSystolic.Text, ref _editTextSystolicLayout) && checkField(editTextDiastolic.Text, ref _editTextDiastolicLayout) && checkField(editTextPulse.Text, ref _editTextPulseLayout);

		}
		private void InitUi()
		{
			_btnCancel = FindViewById<Button>(Resource.Id.btn_cancel);
			_btnRegister = FindViewById<Button>(Resource.Id.btn_add);
			editTextSystolic = FindViewById<EditText>(Resource.Id.et_sys);
			editTextDiastolic = FindViewById<EditText>(Resource.Id.et_dia);
			editTextPulse = FindViewById<EditText>(Resource.Id.et_puls);
			_editTextSystolicLayout = FindViewById<TextInputLayout>(Resource.Id.et_sys_layout);
			_editTextDiastolicLayout = FindViewById<TextInputLayout>(Resource.Id.et_dia_layout);
			_editTextPulseLayout = FindViewById<TextInputLayout>(Resource.Id.et_puls_layout);



			_btnCancel.Click += (sender, args) => {
				model = null;
				Dismiss();
			};
			_btnRegister.Click += (sender, args) => {
                if (FormIsValid())
                {
					model.Systolic = string.IsNullOrEmpty(editTextSystolic.Text) ? 0 : float.Parse(editTextSystolic.Text);
					model.Diastolic = string.IsNullOrEmpty(editTextDiastolic.Text) ? 0 : float.Parse(editTextDiastolic.Text);
					model.PulseRate = string.IsNullOrEmpty(editTextPulse.Text) ? 0 : float.Parse(editTextPulse.Text);
					model.RecordDateTime = DateTime.Now;
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

		protected BloodPressureManualRegisterDialog(IntPtr javaReference, JniHandleOwnership transfer) :
			base(javaReference, transfer)
		{ }

		public BloodPressureManualRegisterDialog(Context context, BloodPressureData model) : base(context) {
		 this.model =  model;

		}

		protected BloodPressureManualRegisterDialog(Context context, bool cancelable, IDialogInterfaceOnCancelListener cancelListener)
			: base(context, cancelable, cancelListener) { }

		public BloodPressureManualRegisterDialog(Context context, int themeResId) : base(context, themeResId) { }

		protected BloodPressureManualRegisterDialog(Context context, bool cancelable, EventHandler cancelHandler) : base(context,
			cancelable, cancelHandler)
		{ }
	}
}
