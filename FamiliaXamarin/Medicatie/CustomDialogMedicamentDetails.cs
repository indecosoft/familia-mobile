using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia;
using FamiliaXamarin.Medicatie.Entities;
using Java.Util;

namespace FamiliaXamarin.Medicatie
{
    class CustomDialogMedicamentDetails : Dialog, View.IOnClickListener,
        DatePickerDialog.IOnDateSetListener, HourAdapter.OnHourClickListener
    {
        private Spinner spinner;
        private EditText etMedicamentName;

        private EditText etNumarZile;

        //private RadioGroup rgDurata;
        private RadioButton rbContinuu;
        private RadioButton rbNrZile;
        private HourAdapter hourAdapter;
        private IMedSaveListener listener;
        private Medicine medicament;
        private Mode mode;
        private int intervalZi;
        private DiseaseActivity activity;
        private string timeSelected;
        private TextView tvStartDate;
        private bool listmode = true;
        private bool _isEdited;
        private string currentMed = string.Empty;

        public CustomDialogMedicamentDetails(Context context, Medicine medicament) : base(context)
        {
            activity = (DiseaseActivity) context;
            mode = medicament == null ? Mode.Save : Mode.Update;
            this.medicament = medicament;
        }

        public void SetListener(IMedSaveListener saveMedListener)
        {
            listener = saveMedListener;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int) WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.custom_dialog);
            listmode = true;
            SetupViews();
        }

        public override void OnBackPressed()
        {
            if (_isEdited)
            {
                Android.Support.V7.App.AlertDialog.Builder alert =
                    new Android.Support.V7.App.AlertDialog.Builder(activity);
                alert.SetTitle("Avertisment");
                alert.SetMessage("Esti pe cale sa renunti la modificarile facute. Renuntati?");
                alert.SetPositiveButton("Da", (senderAlert, args) => { base.OnBackPressed(); });

                alert.SetNegativeButton("Nu", (senderAlert, args) => { });

                Dialog dialog = alert.Create();
                dialog.Show();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        private void SetCurrentDate()
        {
            string dateSaved = getCurrentDate();
            tvStartDate.Text = dateSaved;
        }

        private string getCurrentDate()
        {
            Calendar cal = Calendar.Instance;
            int year = cal.Get(CalendarField.Year);
            int month = cal.Get(CalendarField.Month);
            int day = cal.Get(CalendarField.DayOfMonth);
            return $"{day}.{(month + 1)}.{year}";
        }


        private void SetupViews()
        {
            etMedicamentName = FindViewById<EditText>(Resource.Id.et_medicament_name);
            etNumarZile = FindViewById<EditText>(Resource.Id.et_numar_zile);
            etNumarZile.Visibility = ViewStates.Gone;
            //rgDurata = FindViewById<RadioGroup>(Resource.Id.rg_durata);
            rbContinuu = FindViewById<RadioButton>(Resource.Id.rb_continuu);
            rbContinuu.SetOnClickListener(this);
            rbNrZile = FindViewById<RadioButton>(Resource.Id.rb_numar_zile);
            rbNrZile.SetOnClickListener(this);
            tvStartDate = FindViewById<TextView>(Resource.Id.tv_start_date);
            tvStartDate.SetOnClickListener(this);
            rbContinuu.Checked = true;

            SetupSpinner();
            SetupRvHours();

            FindViewById(Resource.Id.btn_save_med_dialog).SetOnClickListener(this);

            if (medicament != null)
            {
                etMedicamentName.Text = medicament.Name;
                etMedicamentName.TextChanged += delegate
                {
                    try
                    {
                        _isEdited = !currentMed.Equals(etMedicamentName.Text);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                };
                hourAdapter.SetList(medicament.Hours);
            }

            switch (mode)
            {
                case Mode.Update:
                    SetViewOnUpdate();
                    break;
                case Mode.Save:
                    SetCurrentDate();
                    break;
            }
        }

        private void SetViewOnUpdate()
        {
            listmode = false;
            spinner.SetSelection(medicament.IntervalOfDay - 1);

            hourAdapter.SetList(medicament.Hours);
            hourAdapter.NotifyDataSetChanged();
            if (medicament.NumberOfDays != 0)
            {
                rbNrZile.Checked = true;
                etNumarZile.Visibility = ViewStates.Visible;
                etNumarZile.Text = medicament.NumberOfDays + "";
            }
            else
            {
                rbContinuu.Checked = true;
            }

            tvStartDate.Text = medicament.Date;
        }

        private void SetIntervalOfHours(int i)
        {
            hourAdapter.ClearList();
            int idHour = 0;
            int inceput = 6;
            int interval = 24 / (i + 1);
            if (i == 0)
            {
                hourAdapter.AddHour(new Hour(inceput + ":00", idHour + ""));
            }
            else
            {
                for (int j = 1; j < i + 2; j++)
                {
                    idHour++;
                    if (j == 1)
                    {
                        hourAdapter.AddHour(new Hour(inceput + ":00", idHour + ""));
                    }
                    else
                    {
                        inceput += interval;
                        if (inceput > 24)
                        {
                            inceput = 0;
                        }

                        hourAdapter.AddHour(new Hour(inceput + ":00", idHour + ""));
                    }
                }
            }
        }

        private void SetupSpinner()
        {
            spinner = FindViewById<Spinner>(Resource.Id.spinner);
            spinner.ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs args)
            {
                // Contract.Requires(sender != null);
                intervalZi = args.Position + 1;
                if (listmode)
                {
                    SetIntervalOfHours(args.Position);
                }

                if (!listmode)
                {
                    listmode = true;
                }

                if (medicament == null)
                {
                    medicament = new Medicine(etMedicamentName.Text);
                    medicament.Date = getCurrentDate();
                }

                hourAdapter.NotifyDataSetChanged();
            };

            List<string> categories = new List<string>();
            categories.Add("o data pe zi");
            for (int i = 2; i < 13; i++)
            {
                categories.Add("de " + i + " ori pe zi");
            }

            ArrayAdapter<string> dataAdapter = new ArrayAdapter<string>(Context,
                Android.Resource.Layout.SimpleSpinnerItem, categories);
            dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = dataAdapter;
        }

        private void SetupRvHours()
        {
            RecyclerView rvHours = FindViewById<RecyclerView>(Resource.Id.rv_hours);
            LinearLayoutManager layoutManager = new LinearLayoutManager(Context);
            rvHours.SetLayoutManager(layoutManager);

            hourAdapter = new HourAdapter();
            rvHours.SetAdapter(hourAdapter);
            hourAdapter.SetListener(this);
        }

        public interface IMedSaveListener
        {
            void OnMedSaved(Medicine medicament);

            void OnMedUpdated(Medicine medicament);
        }

        private enum Mode
        {
            Save,
            Update
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_save_med_dialog:
                    OnSaveClicked();
                    break;
                case Resource.Id.rb_continuu:
                    SetSecondRadioButton(rbContinuu.Checked, ViewStates.Gone);
                    break;
                case Resource.Id.rb_numar_zile:
                    SetSecondRadioButton(rbNrZile.Checked, ViewStates.Visible);
                    break;
                case Resource.Id.tv_start_date:
                    OnDateClick();
                    break;
            }
        }

        private void SetSecondRadioButton(bool isChecked, ViewStates visibility)
        {
            if (isChecked)
            {
                etNumarZile.Visibility = visibility;
            }
        }

        private void OnDateClick()
        {
            var frag = DatePickerMedicine.NewInstance(delegate(DateTime time)
            {
                tvStartDate.Text = time.ToShortDateString();
            });
            frag.Show(activity.SupportFragmentManager, DatePickerMedicine.TAG);
        }

        private void OnSaveClicked()
        {
            if (listener != null)
            {
                string name = etMedicamentName.Text;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    NotifyListener(name);
                    Dismiss();
                }
                else
                {
                    Toast.MakeText(Context, "Introduceti denumirea medicmentului!",
                        ToastLength.Long).Show();
                }
            }
        }

        private void NotifyListener(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (medicament == null)
                {
                    medicament = new Medicine(name);
                }
                else
                {
                    medicament.Name = name;
                    medicament.Hours = hourAdapter.GetList();
                    medicament.IntervalOfDay = intervalZi;
                    medicament.Date = tvStartDate.Text;
                    string zile = etNumarZile.Text;
                    if (!zile.Equals(string.Empty))
                    {
                        var canParse = int.TryParse(zile, out int nrZile);
                        if (canParse)
                        {
                            medicament.NumberOfDays = nrZile;
                        }
                        else
                        {
                            Toast.MakeText(Application.Context,
                                "Numarul de zile este prea mare", ToastLength.Short).Show();
                        }
                    }
                    else
                    {
                        medicament.NumberOfDays = 0;
                    }
                }

                switch (mode)
                {
                    case Mode.Save:
                        SaveNewMed();
                        break;
                    case Mode.Update:
                        UpdateMed();
                        break;
                }
            }
            else
            {
                Toast.MakeText(Context, "Nu ati introdus numele MEDICAMENTULUI", ToastLength.Short)
                    .Show();
            }
        }

        private void UpdateMed()
        {
            medicament.Hours = hourAdapter.GetList();
            listener.OnMedUpdated(medicament);
        }

        private void SaveNewMed()
        {
            medicament.Hours = hourAdapter.GetList();
            listener.OnMedSaved(medicament);
        }

        private void OnTimeClicked(Hour myHour)
        {
            Calendar mcurrentTime = Calendar.Instance;
            int hour = mcurrentTime.Get(CalendarField.HourOfDay);
            int minute = mcurrentTime.Get(CalendarField.Minute);

            TimePickerDialog mTimePicker = new TimePickerDialog(Context,
                delegate(object sender, TimePickerDialog.TimeSetEventArgs args)
                {
                    OnTimeSelected(sender as TimePicker, args.HourOfDay, args.Minute, myHour);
                }, hour, minute, true);

            mTimePicker.SetTitle("Select Time");
            mTimePicker.Show();
        }

        private void OnTimeSelected(TimePicker timePicker, int selectedHour, int selectedMinute,
            Hour myHour)
        {
            timeSelected = selectedHour + ":" + selectedMinute;
            myHour.HourName = timeSelected;
            hourAdapter.updateHour(myHour);
            hourAdapter.NotifyDataSetChanged();
            //Calendar calendar = GetCalendar(timePicker, selectedHour, selectedMinute);
        }

//        private Calendar GetCalendar(TimePicker timePicker, int selectedHour, int selectedMinute)
//        {
//            Calendar calendar = Calendar.Instance;
//            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
//            {
//                calendar.Set(calendar.Get(CalendarField.Year), calendar.Get(CalendarField.Month),
//                    calendar.Get(CalendarField.DayOfMonth),
//                    timePicker.Hour, timePicker.Minute, 0);
//            }
//            else
//            {
//                calendar.Set(calendar.Get(CalendarField.Year), calendar.Get(CalendarField.Month),
//                    calendar.Get(CalendarField.DayOfMonth),
//                    selectedHour, selectedMinute, 0);
//            }
//
//            return calendar;
//        }

        public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            string dateSaved = $"{dayOfMonth}.{month + 1}.{year}";
            Log.Error("DATE SAVED", dateSaved);
            tvStartDate.Text = dateSaved;
            medicament.Date = dateSaved;
        }

        public void onHourClicked(Hour hour)
        {
            OnTimeClicked(hour);
            _isEdited = true;
        }
    }
}