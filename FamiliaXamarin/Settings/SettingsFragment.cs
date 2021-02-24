using System;
using System.Diagnostics.Contracts;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Hardware.Fingerprint;
using AndroidX.Fragment.App;
using Familia.Devices.DevicesManagement;
using Familia.Helpers;
using Familia.OngBenefits.GenerateCardQR;
using Familia.OngBenefits.ShowBenefits;

namespace Familia.Settings
{
    public class SettingsFragment : Fragment, View.IOnClickListener
    {
        private Spinner spinner;
        private int optionOfSnooze;
//        private string key;
        private Switch enablefingerprint;
        private Switch enablePin;
        private TextView _tvDevicesManagement;
        private TextView _version;
        private TextView _tvMedicineTitle;
        private RelativeLayout _rlMedicineTitle;

        private TextView _tvDeviceTitle;
        private Button _btnDailyTargetEdit;
        private Button _btnDailyTargetSave;
        private TextView _tvDailyTargetValue;
        private EditText _etDailyTargetValue;
        private TextView _tvDailyTargetLabel;
        private Switch showBenefitsSwitch;
        private Switch cardQRBenefitsSwitch;
        private BenefitsChangedListener benefitsListener;


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.fragment_settings, container, false);
            spinner = (Spinner)v.FindViewById(Resource.Id.alarmSpinner);
            SetupSpinner(v);
            enablefingerprint = v.FindViewById<Switch>(Resource.Id.fingerPrintSwitch);
            enablePin = v.FindViewById<Switch>(Resource.Id.pin_switch);
            _version = v.FindViewById<TextView>(Resource.Id.tv_version);
            string ver = Context.PackageManager.GetPackageInfo(Context.PackageName, 0).VersionName;
            _version.Text = "Versiunea " + ver;
            _tvDevicesManagement = v.FindViewById<TextView>(Resource.Id.devices);
            _rlMedicineTitle = v.FindViewById<RelativeLayout>(Resource.Id.medicine_relative);
            _tvDeviceTitle = v.FindViewById<TextView>(Resource.Id.tv_devices);
            _tvMedicineTitle = v.FindViewById<TextView>(Resource.Id.tv_medicine);
            _tvDevicesManagement.Click += (sender, args) =>
                Activity.StartActivity(typeof(DevicesManagementActivity));
            FingerprintManagerCompat checkHardware;

            checkHardware = FingerprintManagerCompat.From(Activity);

            showBenefitsSwitch = v.FindViewById<Switch>(Resource.Id.showBenefitsSwitch);
            showBenefitsSwitch.CheckedChange += ShowBenefitsSwitch_CheckedChange;

            cardQRBenefitsSwitch = v.FindViewById<Switch>(Resource.Id.QRCardBenefitsSwitch);
            cardQRBenefitsSwitch.CheckedChange += CardQRSwitch_CheckedChange;

            var showBenefits = Utils.GetDefaults(ShowBenefitsFragment.KEY_SHOW_BENEFITS);
            if (showBenefits != null)
            {
                showBenefitsSwitch.Checked = true;
            }

            var showGenerateQRCardBenefits = Utils.GetDefaults(GenerateCardQRFragment.KEY_GENERATE_CARD_QR_BENEFITS);
            if (showGenerateQRCardBenefits != null)
            {
                cardQRBenefitsSwitch.Checked = true;
            }


            var fingerprint = Convert.ToBoolean(Utils.GetDefaults("fingerprint"));

            if (!checkHardware.IsHardwareDetected)
                enablefingerprint.Enabled = false;

            enablefingerprint.Checked = fingerprint;
            enablePin.Checked = !string.IsNullOrEmpty(Utils.GetDefaults("UserPin"));
            enablefingerprint.CheckedChange += Enablefingerprint_CheckedChange;
            enablePin.CheckedChange += EnablePin_CheckedChange;
            if (int.Parse(Utils.GetDefaults("UserType")) == 2)
            {
                _tvDevicesManagement.Visibility = ViewStates.Visible;
                _tvDeviceTitle.Visibility = ViewStates.Visible;

            }
            if(int.Parse(Utils.GetDefaults("UserType")) == 1) {
                _tvDevicesManagement.Visibility = ViewStates.Gone;
                _tvDeviceTitle.Visibility = ViewStates.Gone;
            }
            if (int.Parse(Utils.GetDefaults("UserType")) == 2)
            {
                spinner.Visibility = ViewStates.Gone;
                _rlMedicineTitle.Visibility = ViewStates.Gone;
                _tvMedicineTitle.Visibility = ViewStates.Gone;
            }

            SetViewSettingsForTrackerActivity(v);

            return v;
        }

        public void SetBenefitsListener(BenefitsChangedListener benefitsListener) {
            this.benefitsListener = benefitsListener;
        }

        private void ShowBenefitsSwitch_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                benefitsListener.OnShowBenefitsChanged(true);
            }
            else {
                benefitsListener.OnShowBenefitsChanged(false);
            }
        }

        private void CardQRSwitch_CheckedChange (object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                benefitsListener.OnCardQRChanged(true);
            }
            else
            {
                benefitsListener.OnCardQRChanged(false);
            }
        }

        public interface BenefitsChangedListener {
            void OnShowBenefitsChanged(bool visibility);
            void OnCardQRChanged(bool visibility);
        }

        

        private void SetViewSettingsForTrackerActivity(View v)
        {
            _tvDailyTargetLabel = v.FindViewById<TextView>(Resource.Id.tv_daily_target_label);
            _btnDailyTargetEdit = v.FindViewById<Button>(Resource.Id.btn_daily_target_edit);
            _btnDailyTargetSave = v.FindViewById<Button>(Resource.Id.btn_daily_target_save);
            _btnDailyTargetEdit.SetOnClickListener(this);
            _btnDailyTargetSave.SetOnClickListener(this);
            _tvDailyTargetValue = v.FindViewById<TextView>(Resource.Id.tv_daily_target_value_displayed);

            string storedValue = Utils.GetDefaults("ActivityTrackerDailyTarget");
            if (storedValue == null)
            {
                storedValue = 5000 + "";
                Utils.SetDefaults("ActivityTrackerDailyTarget", storedValue);
            }

            _tvDailyTargetValue.Text = storedValue;

            _etDailyTargetValue = v.FindViewById<EditText>(Resource.Id.et_daily_target_value_editable);
            SetVisibilityForTrackerActivity(false);
        }

        private void SetVisibilityForTrackerActivity(bool show)
        {
            _btnDailyTargetSave.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
            _etDailyTargetValue.Visibility = show ? ViewStates.Visible : ViewStates.Gone;


            _tvDailyTargetLabel.Visibility = !show ? ViewStates.Visible : ViewStates.Gone;
            _tvDailyTargetValue.Visibility = !show ? ViewStates.Visible : ViewStates.Gone;
            _btnDailyTargetEdit.Visibility = !show ? ViewStates.Visible : ViewStates.Gone;
        }

        void EnablePin_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (enablePin.Checked)
                Activity.StartActivity(typeof(ActivitySetPin));
            else
                Utils.SetDefaults("UserPin", string.Empty);
        }


        private void SetupSpinner(View v)
        {
           
            spinner.ItemSelected += delegate (object sender, AdapterView.ItemSelectedEventArgs args)
                {
                    Contract.Requires(sender != null);
                    optionOfSnooze = args.Position;
                    

                    switch (optionOfSnooze)
                    {
                        case 0:
                            Utils.SetDefaults("snooze", "5");
                            break;
                        case 1:
                            Utils.SetDefaults("snooze", "10");
                            break;
                        case 2:
                            Utils.SetDefaults("snooze", "15");
                            break;
                        case 3:
                            Utils.SetDefaults("snooze", "30");
                            break;
                    }
                };


            string[] categories = {"5 min", "10 min", "15 min", "30 min"};

            var adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleSpinnerItem, categories);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
        }
        private void Enablefingerprint_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            // this is an Activity
            Utils.SetDefaults("fingerprint",
                enablefingerprint.Checked ? true.ToString() : false.ToString());
            if(enablefingerprint.Checked && !enablePin.Checked)
            {
                enablePin.Checked = true;
            }
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_daily_target_edit:
                    SetVisibilityForTrackerActivity(true);
                    _etDailyTargetValue.Text = _tvDailyTargetValue.Text;
                    break;
                case Resource.Id.btn_daily_target_save:
                    string newValue = _etDailyTargetValue.Text;
                    if (newValue != null)
                    {
                        _tvDailyTargetValue.Text = newValue;
                        SetVisibilityForTrackerActivity(false);

                        Utils.SetDefaults("ActivityTrackerDailyTarget", newValue);
                    }
                    else
                    {
                        Toast.MakeText(Context, "Campul este gol.", ToastLength.Long).Show();
                    }

                    break;
            }
        }
    }
}