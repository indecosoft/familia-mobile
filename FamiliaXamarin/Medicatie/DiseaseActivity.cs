using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Medicatie.Alarm;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using Java.Util;
using Toolbar = Android.Support.V7.Widget.Toolbar;
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
        private string currentDisease = string.Empty;

        private MedicineAdapter medicamentAdapter;
        private bool _isEdited = false;

        private Disease disease;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_boala);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            };

            Title = "Tratament";

            SetupViews();

            setMode();
        }
        private void SetupViews()
        {
            save = FindViewById<Button>(Resource.Id.btn_save);
            save.Click += delegate (object sender, EventArgs args) { addNewBoala(); };
            update = FindViewById<Button>(Resource.Id.btn_update);
            update.Click += delegate (object sender, EventArgs args) { updateBoala(); };
            FloatingActionButton addMed = FindViewById<FloatingActionButton>(Resource.Id.fab_add_med);
            addMed.Click += delegate (object sender, EventArgs args) { openMedDialog(null); };
            etNumeBoala = FindViewById<EditText>(Resource.Id.et_nume_boala);
         
            setRecyclerView();

        }

        public override void OnBackPressed()
        {
            if (_isEdited)
            {
                Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(this);
                alert.SetTitle("Avertisment");
                alert.SetMessage("Esti pe cale sa renunti la modificarile facute. Renuntati?");
                alert.SetPositiveButton("Da", (senderAlert, args) => {
                    base.OnBackPressed();
                });

                alert.SetNegativeButton("Nu", (senderAlert, args) => {
                });

                Dialog dialog = alert.Create();
                dialog.Show();
            }
            else
            {
                base.OnBackPressed();
                var intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.PutExtra("FromDisease", true);
                StartActivity(intent);
            }
            
        }

        private void setRecyclerView()
        {
            RecyclerView rvMeds = FindViewById<RecyclerView>(Resource.Id.rv_meds);
            LinearLayoutManager layoutManager = new LinearLayoutManager(this);
            rvMeds.SetLayoutManager(layoutManager);
            medicamentAdapter = new MedicineAdapter();
            medicamentAdapter.SetListener(this);

            rvMeds.SetAdapter(medicamentAdapter);
        }


        private void setMode()
        {
            Intent intent = Intent;
            if (intent.HasExtra(MedicineFragment.IdBoala))
            {
                string idBoala = intent.GetStringExtra(MedicineFragment.IdBoala);
                disease = Storage.GetInstance().GetDisease(idBoala);
                medicamentAdapter.SetMedicaments(disease.ListOfMedicines);
                medicamentAdapter.NotifyDataSetChanged();
                etNumeBoala.Text = disease.DiseaseName;
                currentDisease = disease.DiseaseName;
                etNumeBoala.TextChanged += delegate (object sender, TextChangedEventArgs args)
                {
                    try
                    {
                        if (!currentDisease.Equals(etNumeBoala.Text))
                            _isEdited = true;
                        else
                            _isEdited = false;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);

                    }

                };
                save.Visibility = ViewStates.Gone;
            }
            else
            {
                disease = new Disease();
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
            cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimary);
            return cdd;
        }

        private void addNewMed(Medicine medicament)
        {
            disease.AddMedicine(medicament);
            medicamentAdapter.AddMedicament(medicament);
            medicamentAdapter.NotifyDataSetChanged();
        }

        private void updateBoala()
        {
            string numeBoala = etNumeBoala.Text;

            disease.ListOfMedicines = medicamentAdapter.GetMedicaments();
            disease.DiseaseName = numeBoala;
            Storage.GetInstance().updateBoala(this, disease);

            setupAlarm();

            Finish();
        }

        private void addNewBoala()
        {
            string numeBoala = etNumeBoala.Text;
            if (numeBoala.Equals(string.Empty))
            {
                Toast.MakeText(this, "Introduceti denumirea afectiunii!", ToastLength.Long).Show();
                return;
            }
            disease.ListOfMedicines = medicamentAdapter.GetMedicaments();
            disease.DiseaseName = numeBoala;
            Storage.GetInstance().AddDisease(this, disease);

            setupAlarm();

            Finish();
        }

        private void setupAlarm()
        {
            List<Medicine> meds = disease.ListOfMedicines;
            foreach (Medicine med in meds)
            {
                List<Hour> hours = med.Hours;
                List<int> alarms = new List<int>();
                for (int i = 0; i < hours.Count; i++)
                {
                    setAlarm(hours[i], med, disease, ref alarms, i);
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
            var idAlarm = DateTime.Now.Millisecond ;
            var am = (AlarmManager)GetSystemService(AlarmService);
            var id = 0;
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
           // Log.Error("MEDICAMENT", med.Name);
            var pi = PendingIntent.GetBroadcast(this, idAlarm, i, PendingIntentFlags.OneShot);

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
           // Log.Error("MY DATE", med.Date);
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
            medicamentAdapter.UpdateMedicament(medicament, medicament.IdMed);
            medicamentAdapter.NotifyDataSetChanged();
        }

        public void OnMedicamentClick(Medicine medicament)
        {
            openMedDialog(medicament);
            _isEdited = true;
        }

        public void OnMedicamentDeleteClick(Medicine medicament)
        {
            CustomDialogDeleteMedicament cddb = new CustomDialogDeleteMedicament(this);
            cddb.setListener(this);
            cddb.setMedicament(medicament);
            cddb.Show();
            cddb.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimary);
        }

        public void onYesClicked(string result, Medicine medicament)
        {
            if (!result.Equals("yes")) return;
            disease.RemoveMedicine(medicament);
            medicamentAdapter.RemoveMedicament(medicament);
            medicamentAdapter.NotifyDataSetChanged();
        }
    }
}