using System;
using System.Collections.Generic;
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

namespace FamiliaXamarin.Medicatie
{
    [Activity(Label = "BoalaActivity")]
    public class BoalaActivity : AppCompatActivity, View.IOnClickListener, CustomDialogMedicamentDetails.IMedSaveListener, OnMedicamentClickListener, CustomDialogDeleteMedicament.ICustomDialogDeleteMedicamentListener
    {
        public static string MED_ID = "medId";
        public static string BOALA_ID = "boalaId";
        public static string MED_NAME = "medName";
        private Button save;
        private Button update;
        private EditText etNumeBoala;

        private MedicamentAdapter medicamentAdapter;

        private Boala boala;
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
            save.SetOnClickListener(this);

            update = FindViewById<Button>(Resource.Id.btn_update);
            update.SetOnClickListener(this);

            FloatingActionButton addMed = FindViewById<FloatingActionButton>(Resource.Id.fab_add_med);
            addMed.SetOnClickListener(this);
            etNumeBoala = FindViewById<EditText>(Resource.Id.et_nume_boala);

            setRecyclerView();
        }

        private void setRecyclerView()
        {
            RecyclerView rvMeds = FindViewById<RecyclerView>(Resource.Id.rv_meds);
            LinearLayoutManager layoutManager = new LinearLayoutManager(this);
            rvMeds.SetLayoutManager(layoutManager);
            medicamentAdapter = new MedicamentAdapter();
            medicamentAdapter.setListener(this);

            rvMeds.SetAdapter(medicamentAdapter);
        }


        private void setMode()
        {
            Intent intent = Intent;
            if (intent.HasExtra(MedicineFragment.IdBoala))
            {
                string idBoala = intent.GetStringExtra(MedicineFragment.IdBoala);
                boala = Storage.getInstance().getBoala(idBoala);
                medicamentAdapter.setMedicaments(boala.MedicamentList);
                medicamentAdapter.NotifyDataSetChanged();
                etNumeBoala.Text = boala.NumeBoala;
                save.Visibility = ViewStates.Gone;
            }
            else
            {
                boala = new Boala();
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
        private CustomDialogMedicamentDetails openMedDialog(Medicament medicament)
        {
            CustomDialogMedicamentDetails cdd = new CustomDialogMedicamentDetails(this, medicament);
            cdd.SetListener(this);
            cdd.Show();

            return cdd;
        }

        private void addNewMed(Medicament medicament)
        {
            boala.addMedicament(medicament);
            medicamentAdapter.addMedicament(medicament);
            medicamentAdapter.NotifyDataSetChanged();
        }

        private void updateBoala()
        {
            string numeBoala = etNumeBoala.Text;
            
            boala.MedicamentList = medicamentAdapter.getMedicaments();
            boala.NumeBoala = numeBoala;
            Storage.getInstance().updateBoala(this, boala);

            setupAlarm();

            Intent intent = new Intent(Application.Context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            MainActivity.FromBoala = true;
            StartActivity(intent);

            //Finish();
        }

        private void addNewBoala()
        {
            string numeBoala = etNumeBoala.Text;
            if (numeBoala.Equals(string.Empty))
            {
                Toast.MakeText(this, "Nu ati introdus numele BOLII", ToastLength.Short).Show();
                return;
            }
            boala.MedicamentList = medicamentAdapter.getMedicaments();
            boala.NumeBoala = numeBoala;
            Storage.getInstance().addBoala(this, boala);

            setupAlarm();

            var intent = new Intent(Application.Context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            MainActivity.FromBoala = true;
            StartActivity(intent);

            //Finish();
        }

        private void setupAlarm()
        {
            List<Medicament> meds = boala.MedicamentList;
            foreach (Medicament med in meds)
            {
                List<Hour> hours = med.Hours;
                foreach (Hour itemHour in hours)
                {
                    setAlarm(itemHour, med, boala);
                }
            }
        }

        private void setAlarm(Hour hour, Medicament med, Boala boala)
        {

            AlarmManager am = (AlarmManager)GetSystemService(AlarmService);

            Intent i = new Intent(this, typeof(AlarmBroadcastReceiver));
            i.PutExtra(BOALA_ID, boala.Id);
            i.PutExtra(MED_ID, med.IdMed);

            int _id = DateTime.Now.Millisecond;
            PendingIntent pi = PendingIntent.GetBroadcast(this, _id, i, PendingIntentFlags.OneShot);


            if (am != null)
            {
                string hourString = hour.Nume;
                string[] parts = hourString.Split(':');
                int timeHour = int.Parse(parts[0]);
                int timeMinute = int.Parse(parts[1]);
                Calendar calendar = Calendar.Instance;
                Log.Error("DATAAAA", med.Date);
                Calendar setcalendar = Calendar.Instance;
                setcalendar.Set(CalendarField.HourOfDay, timeHour);
                setcalendar.Set(CalendarField.Minute, timeMinute);
                setcalendar.Set(CalendarField.Second, 0);

                if (setcalendar.Before(calendar))
                    setcalendar.Add(CalendarField.Date, 1);

                am.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, setcalendar.TimeInMillis, AlarmManager.IntervalDay, pi);
            }
        }

        public void onMedSaved(Medicament medicament)
        {
            addNewMed(medicament);
        }

        public void onMedUpdated(Medicament medicament)
        {
            medicamentAdapter.updateMedicament(medicament, medicament.IdMed);
            medicamentAdapter.NotifyDataSetChanged();
        }

        public void OnMedicamentClick(Medicament medicament)
        {
            openMedDialog(medicament);
        }

        public void OnMedicamentDeleteClick(Medicament medicament)
        {
            CustomDialogDeleteMedicament cddb = new CustomDialogDeleteMedicament(this);
            cddb.setListener(this);
            cddb.setMedicament(medicament);
            cddb.Show();
        }

        public void onYesClicked(string result, Medicament medicament)
        {
            if (result.Equals("yes"))
            {
                boala.removeMedicament(medicament);
                medicamentAdapter.removeMedicament(medicament);
                medicamentAdapter.NotifyDataSetChanged();
            }
        }
    }
}