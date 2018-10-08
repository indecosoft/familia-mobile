using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Medicatie.Entities;
using Java.Util;

namespace FamiliaXamarin.Medicatie
{
    class CustomDialogMedicamentDetails : Dialog, View.IOnClickListener, DatePickerDialog.IOnDateSetListener, HourAdapter.OnHourClickListener
    {
        private Spinner spinner;
        private EditText etMedicamentName;
        private EditText etNumarZile;
        private RadioGroup rgDurata;
        private RadioButton rbContinuu;
        private RadioButton rbNrZile;
        private HourAdapter hourAdapter;
        private IMedSaveListener listener;
        private Medicament medicament;
        private IMode mode;
        private int intervalZi;
        private Activity activity;
        private string timeSelected;
        private DatePickerDialog datePickerDialog;
        private DatePickerDialog.IOnDateSetListener mDateSetListener;
        private TextView tvStartDate;
        private bool listmode = true;

        public CustomDialogMedicamentDetails(Context context, Medicament medicament) : base(context)
        {
            this.activity = (Activity)context;
            mode = medicament == null ? IMode.SAVE : IMode.UPDATE;
            this.medicament = medicament;
        }
        public void SetListener(IMedSaveListener listener)
        {
            this.listener = listener;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.custom_dialog);
            listmode = true;
            setupViews();
            //setDateListener();

        }
        private void setCurrentDate()
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
            return day + "/" + month + "/" + year;
        }
       

        private void setupViews()
    {
        etMedicamentName = FindViewById<EditText>(Resource.Id.et_medicament_name);
        etNumarZile = FindViewById<EditText>(Resource.Id.et_numar_zile);
        etNumarZile.Visibility = ViewStates.Invisible;
        rgDurata = FindViewById<RadioGroup>(Resource.Id.rg_durata);
        rbContinuu = FindViewById<RadioButton>(Resource.Id.rb_continuu);
        rbContinuu.SetOnClickListener(this);
        rbNrZile = FindViewById<RadioButton>(Resource.Id.rb_numar_zile);
        rbNrZile.SetOnClickListener(this);
        tvStartDate = FindViewById<TextView>(Resource.Id.tv_start_date);
        tvStartDate.SetOnClickListener(this);

        setupSpinner();
        setupRvHours();

        FindViewById(Resource.Id.btn_save_med_dialog).SetOnClickListener(this);

        if (medicament != null)
        {
            etMedicamentName.Text = medicament.Name;
            hourAdapter.SetList(medicament.Hours);
        }

        switch (mode)
        {
            case IMode.UPDATE:
                setViewOnUpdate();
                break;
            case IMode.SAVE:
                setCurrentDate();
                break;
        }
    }

    private void setViewOnUpdate()
    {
        listmode = false;
        spinner.SetSelection(medicament.IntervalZi - 1);

        hourAdapter.SetList(medicament.Hours);
        hourAdapter.NotifyDataSetChanged();
        if (medicament.NrZile != 0)
        {
            rbNrZile.Checked =true;
            etNumarZile.Visibility = ViewStates.Visible;
            etNumarZile.Text = medicament.NrZile + "";

        }
        else
        {
            rbContinuu.Checked = true;
        }
        tvStartDate.Text = medicament.Date;

    }
        private void setIntervalOfHours(int i)
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

        private void setupSpinner()
    {
        spinner = FindViewById<Spinner>(Resource.Id.spinner);
        spinner.ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs args)
        {
            intervalZi = args.Position + 1;
            if (listmode)
            {
                setIntervalOfHours(args.Position);
            }

            if (!listmode)
            {
                listmode = true;
            }

            if (medicament == null)
            {
                medicament = new Medicament(etMedicamentName.Text);
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
        ArrayAdapter<string> dataAdapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleSpinnerItem, categories);
        dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
        spinner.Adapter = dataAdapter;
    }

    private void setupRvHours()
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
            void onMedSaved(Medicament medicament);

            void onMedUpdated(Medicament medicament);

        }

        private enum IMode
        {
            SAVE,
            UPDATE
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_save_med_dialog:
                    onSaveClicked();
                    break;
                case Resource.Id.rb_continuu:
                    setFirstRadioButton(rbContinuu.Checked, (int)ViewStates.Invisible);
                    break;
                case Resource.Id.rb_numar_zile:
                    setSecondRadioButton(rbNrZile.Checked, (int)ViewStates.Visible);
                    break;
                case Resource.Id.tv_start_date:
                    onDateClick();
                    break;

            }
        }
        private void setSecondRadioButton(bool isChecked, int visible)
        {
            if (isChecked) {
                etNumarZile.Visibility = (ViewStates)visible;
            }
        }

        private void setFirstRadioButton(bool isChecked, int invisible)
        {
            setSecondRadioButton(isChecked, invisible);
        }

        private void onDateClick()
        {
//            Calendar cal = Calendar.Instance;
//            int year = cal.Get(Calendar.Year);
//            int month = cal.Get(Calendar.Month);
//            int day = cal.Get(Calendar.DayOfMonth);
//
//            datePickerDialog = new DatePickerDialog(Context, Android.Resource.Style.ThemeHoloDialog, mDateSetListener, year, month, day);
//            datePickerDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
//            datePickerDialog.Show();

            var frag = DatePickerFragment.NewInstance(delegate (DateTime time)
            {
                tvStartDate.Text = time.ToShortDateString();
            });
            frag.Show(activity.FragmentManager, DatePickerFragment.TAG);
        }

        private void onSaveClicked()
        {
            if (listener != null)
            {
                string name = etMedicamentName.Text;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    notifyListener(name);
                    Dismiss();
                }
                else
                {
                    Toast.MakeText(Context, "Nu ati introdus numele MEDICAMENTULUI", ToastLength.Short).Show();
                }

            }

        }
        private void notifyListener(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (medicament == null)
                {
                    medicament = new Medicament(name);
                }
                else
                {
                    medicament.Name = name;
                    medicament.Hours = hourAdapter.GetList();
                    medicament.IntervalZi= intervalZi;
                    string zile = etNumarZile.Text;
                    if (!zile.Equals(string.Empty))
                    {
                        int nrZile = int.Parse(zile);
                        medicament.NrZile = nrZile;
                        
                    }
                    else
                    {
                        medicament.NrZile = 0;
                    }
                }
                switch (mode)
                {
                    case IMode.SAVE:
                        saveNewMed();
                        break;
                    case IMode.UPDATE:
                        updateMed();
                        break;
                }
            }
            else
            {
                Toast.MakeText(Context, "Nu ati introdus numele MEDICAMENTULUI", ToastLength.Short).Show();
            }
        }
        private void updateMed()
        {
            medicament.Hours = hourAdapter.GetList();
            listener.onMedUpdated(medicament);

        }

        private void saveNewMed()
        {
            medicament.Hours = hourAdapter.GetList();
            //logHourAdapterList();
            listener.onMedSaved(medicament);
        }
        private void onTimeClicked(Hour myHour)
        {
            Calendar mcurrentTime = Calendar.Instance;
            int hour = mcurrentTime.Get(Calendar.HourOfDay);
            int minute = mcurrentTime.Get(Calendar.Minute);

            TimePickerDialog mTimePicker = new TimePickerDialog(
                Context,
                delegate(object sender, TimePickerDialog.TimeSetEventArgs args)
                {
                    onTimeSelected(sender as TimePicker, args.HourOfDay, args.Minute, myHour);
                }, hour,minute,true);
            mTimePicker.SetTitle("Select Time");
            mTimePicker.Show();
        }

        private void onTimeSelected(TimePicker timePicker, int selectedHour, int selectedMinute, Hour myHour)
        {
            timeSelected = selectedHour + ":" + selectedMinute;
            myHour.Nume = timeSelected;
            hourAdapter.updateHour(myHour);
            hourAdapter.NotifyDataSetChanged();
            Calendar calendar = getCalendar(timePicker, selectedHour, selectedMinute);
            // setAlarm(calendar.getTimeInMillis());
        }

        private Calendar getCalendar(TimePicker timePicker, int selectedHour, int selectedMinute)
        {
            Calendar calendar = Calendar.Instance;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                calendar.Set(calendar.Get(Calendar.Year), calendar.Get(Calendar.Month), calendar.Get(Calendar.DayOfMonth),
                    timePicker.Hour, timePicker.Minute, 0);
            }
            else
            {
                calendar.Set(calendar.Get(Calendar.Year), calendar.Get(Calendar.Month), calendar.Get(Calendar.DayOfMonth),
                    selectedHour, selectedMinute, 0);
            }
            return calendar;
        }

        public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            string dateSaved = $"{dayOfMonth}/ {(month + 1)} /{year}";
            tvStartDate.Text = dateSaved;
            this.medicament.Date = dateSaved;
        }

        public void onHourClicked(Hour hour)
        {
            Toast.MakeText(activity, hour.Id + " clicked", ToastLength.Short).Show();
            onTimeClicked(hour);
        }
    }
}