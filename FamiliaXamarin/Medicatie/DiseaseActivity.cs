using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Medicatie.Alarm;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using Java.Util;
using Calendar = Java.Util.Calendar;

namespace FamiliaXamarin.Medicatie
{
    [Activity(Label = "BoalaActivity", Theme = "@style/AppTheme.Dark")]
    public class DiseaseActivity : AppCompatActivity, View.IOnClickListener, CustomDialogMedicamentDetails.IMedSaveListener, OnMedicamentClickListener, CustomDialogDeleteMedicament.ICustomDialogDeleteMedicamentListener
    {
        public static readonly string MED_ID = "medId";
        public static readonly string BOALA_ID = "boalaId";
        public static readonly string ALARM_ID = "alarmId";
        public static readonly string HOUR_ID = "hourId";
        private Button save;
        private Button update;
        private EditText etNumeBoala;

        private MedicineAdapter medicamentAdapter;

        private Disease boala;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_boala);

            SetupViews();

            setMode();
            // Create your application here
        }
        private void SetupViews()
        {
            save = FindViewById<Button>(Resource.Id.btn_save);
            //save.SetOnClickListener(this);
            save.Click += delegate (object sender, EventArgs args) { addNewBoala(); };

            update = FindViewById<Button>(Resource.Id.btn_update);
            //update.SetOnClickListener(this);
            update.Click += delegate (object sender, EventArgs args) { updateBoala(); };

            FloatingActionButton addMed = FindViewById<FloatingActionButton>(Resource.Id.fab_add_med);
            //addMed.SetOnClickListener(this);
            addMed.Click += delegate (object sender, EventArgs args) { openMedDialog(null); };
            etNumeBoala = FindViewById<EditText>(Resource.Id.et_nume_boala);

            setRecyclerView();

        }

        private void setRecyclerView()
        {
            RecyclerView rvMeds = FindViewById<RecyclerView>(Resource.Id.rv_meds);
            LinearLayoutManager layoutManager = new LinearLayoutManager(this);
            rvMeds.SetLayoutManager(layoutManager);
            medicamentAdapter = new MedicineAdapter();
            medicamentAdapter.setListener(this);

            rvMeds.SetAdapter(medicamentAdapter);
        }


        private void setMode()
        {
            Intent intent = Intent;
            if (intent.HasExtra(MedicineFragment.IdBoala))
            {
                string idBoala = intent.GetStringExtra(MedicineFragment.IdBoala);
                boala = Storage.GetInstance().GetDisease(idBoala);
                medicamentAdapter.setMedicaments(boala.ListOfMedicines);
                medicamentAdapter.NotifyDataSetChanged();
                etNumeBoala.Text = boala.DiseaseName;
                save.Visibility = ViewStates.Gone;
            }
            else
            {
                boala = new Disease();
                update.Visibility = ViewStates.Gone;
            }
        }
        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_save:
                    addNewBoala();
                    break;
                case Resource.Id.btn_update:
                    updateBoala();
                    break;
                case Resource.Id.fab_add_med:
                    openMedDialog(null);
                    break;
            }
        }
        private CustomDialogMedicamentDetails openMedDialog(Medicine medicament)
        {
            CustomDialogMedicamentDetails cdd = new CustomDialogMedicamentDetails(this, medicament);
            cdd.SetListener(this);
            cdd.Show();

            return cdd;
        }

        private void addNewMed(Medicine medicament)
        {
            boala.AddMedicine(medicament);
            medicamentAdapter.addMedicament(medicament);
            medicamentAdapter.NotifyDataSetChanged();
        }

        private void updateBoala()
        {
            string numeBoala = etNumeBoala.Text;

            boala.ListOfMedicines = medicamentAdapter.getMedicaments();
            boala.DiseaseName = numeBoala;
            Storage.GetInstance().updateBoala(this, boala);

            setupAlarm();

            //            Intent intent = new Intent(Application.Context, typeof(MainActivity));
            //            intent.AddFlags(ActivityFlags.ClearTop);
            //            MainActivity.FromBoala = true;
            //            StartActivity(intent);

            Finish();
        }

        private void addNewBoala()
        {
            string numeBoala = etNumeBoala.Text;
            if (numeBoala.Equals(string.Empty))
            {
                Toast.MakeText(this, "Nu ati introdus numele BOLII", ToastLength.Short).Show();
                return;
            }
            boala.ListOfMedicines = medicamentAdapter.getMedicaments();
            boala.DiseaseName = numeBoala;
            Storage.GetInstance().AddDisease(this, boala);

            setupAlarm();

            Finish();
        }

        private void setupAlarm()
        {
            List<Medicine> meds = boala.ListOfMedicines;
            foreach (Medicine med in meds)
            {
                List<Hour> hours = med.Hours;
                List<int> alarms = new List<int>();
                for (int i = 0; i < hours.Count; i++)
                {
                    setAlarm(hours[i], med, boala, ref alarms, i);
                }

                med.Alarms = alarms;

            }
        }
        private readonly DateTime Jan1st1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public int CurrentTimeMillis()
        {
            return (int)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }
        private void setAlarm(Hour hour, Medicine med, Disease boala, ref List<int> alarms, int position)
        {
            var id = CurrentTimeMillis(); ;
            var am = (AlarmManager)GetSystemService(AlarmService);

            var i = new Intent(this, typeof(AlarmBroadcastReceiver));
            i.PutExtra(BOALA_ID, boala.Id);
            i.PutExtra(MED_ID, med.IdMed);
            i.PutExtra(HOUR_ID, hour.Id);
            if (med.Alarms != null)
            {
                id = med.Alarms[position];

            }
            
            alarms.Add(id);
            i.PutExtra(ALARM_ID, id);

            var pi = PendingIntent.GetBroadcast(this, id, i, PendingIntentFlags.OneShot);

            if (am == null) return;
            var hourString = hour.HourName;
            var parts = hourString.Split(':');
            var timeHour = Convert.ToInt32(parts[0]);
            var timeMinute = Convert.ToInt32(parts[1]);
            var calendar = Calendar.Instance;
            var setCalendar = Calendar.Instance;
            setCalendar.Set(CalendarField.HourOfDay, timeHour);
            setCalendar.Set(CalendarField.Minute, timeMinute);
            setCalendar.Set(CalendarField.Second, 0);

            

            var dateString = med.Date;
            Log.Error("MY DATE", med.Date);
            parts = dateString.Split('.');
            var day = Convert.ToInt32(parts[0]);
            var month = Convert.ToInt32(parts[1]) - 1;
            var year = Convert.ToInt32(parts[2]);


            setCalendar.Set(CalendarField.Year, year);
            setCalendar.Set(CalendarField.Month, month);
            setCalendar.Set(CalendarField.DayOfMonth, day);

            

            if (setCalendar.Before(calendar))
            {
                setCalendar.Add(CalendarField.Date, 1);
            }


            am.SetInexactRepeating(AlarmType.RtcWakeup, setCalendar.TimeInMillis, AlarmManager.IntervalDay, pi);


        }

        public void onMedSaved(Medicine medicament)
        {
            addNewMed(medicament);
        }

        public void onMedUpdated(Medicine medicament)
        {
            medicamentAdapter.updateMedicament(medicament, medicament.IdMed);
            medicamentAdapter.NotifyDataSetChanged();
        }

        public void OnMedicamentClick(Medicine medicament)
        {
            openMedDialog(medicament);
        }

        public void OnMedicamentDeleteClick(Medicine medicament)
        {
            CustomDialogDeleteMedicament cddb = new CustomDialogDeleteMedicament(this);
            cddb.setListener(this);
            cddb.setMedicament(medicament);
            cddb.Show();
        }

        public void onYesClicked(string result, Medicine medicament)
        {
            if (result.Equals("yes"))
            {
                boala.RemoveMedicine(medicament);
                medicamentAdapter.removeMedicament(medicament);
                medicamentAdapter.NotifyDataSetChanged();
            }
        }
    }
}