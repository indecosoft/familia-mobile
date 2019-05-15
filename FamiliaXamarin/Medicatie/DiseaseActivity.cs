using System;
using System.Collections.Generic;
using Familia;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.OS;
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
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Calendar = Java.Util.Calendar;

namespace FamiliaXamarin.Medicatie
{
    [Activity(Label = "BoalaActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
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
        private bool _isEdited;

        private Disease disease;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_boala);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate { OnBackPressed(); };
            Log.Error("AAAAAAAAAAAA","DISEASE ACTIVITY CREATED");
            Title = "Tratament";

            SetupViews();

            SetMode();
        }
        private void SetupViews()
        {
            save = FindViewById<Button>(Resource.Id.btn_save);
            save.Click += delegate { AddNewBoala(); };
            update = FindViewById<Button>(Resource.Id.btn_update);
            update.Click += delegate { UpdateBoala(); };
            FloatingActionButton addMed = FindViewById<FloatingActionButton>(Resource.Id.fab_add_med);
            addMed.Click += delegate { OpenMedDialog(null); };
            etNumeBoala = FindViewById<EditText>(Resource.Id.et_nume_boala);
         
            SetRecyclerView();

        }

        public override void OnBackPressed()
        {
            if (_isEdited)
            {
                var alert = new Android.Support.V7.App.AlertDialog.Builder(this);
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
                //base.OnBackPressed();
                var intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.PutExtra("FromMedicine", true);
                StartActivity(intent);
            }
            
        }

        private void SetRecyclerView()
        {
            RecyclerView rvMeds = FindViewById<RecyclerView>(Resource.Id.rv_meds);
            LinearLayoutManager layoutManager = new LinearLayoutManager(this);
            rvMeds.SetLayoutManager(layoutManager);
            medicamentAdapter = new MedicineAdapter();
            medicamentAdapter.SetListener(this);

            rvMeds.SetAdapter(medicamentAdapter);
        }


        private void SetMode()
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
                etNumeBoala.TextChanged += delegate
                {
                    try
                    {
                        _isEdited = !currentDisease.Equals(etNumeBoala.Text);
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
                    AddNewBoala();
                    break;
                case Resource.Id.btn_update:
                    UpdateBoala();
                    break;
                case Resource.Id.fab_add_med:
                    OpenMedDialog(null);
                    break;
            }
        }
        private void OpenMedDialog(Medicine medicament)
        {
            CustomDialogMedicamentDetails cdd = new CustomDialogMedicamentDetails(this, medicament);
            cdd.SetListener(this);
            
            cdd.Show();
            cdd.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimary);
        }

        private void AddNewMed(Medicine medicament)
        {
            disease.AddMedicine(medicament);
            medicamentAdapter.AddMedicament(medicament);
            medicamentAdapter.NotifyDataSetChanged();
        }

        private void UpdateBoala()
        {
            string numeBoala = etNumeBoala.Text;

            disease.ListOfMedicines = medicamentAdapter.GetMedicaments();
            disease.DiseaseName = numeBoala;
            Storage.GetInstance().updateBoala(this, disease);

            SetupAlarm();

            Finish();
        }

        private void AddNewBoala()
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

            SetupAlarm();
            
            Finish();
            
            
        
        }

        private void SetupAlarm()
        {
            List<Medicine> meds = disease.ListOfMedicines;
            foreach (Medicine med in meds)
            {
                List<Hour> hours = med.Hours;
                List<int> alarms = new List<int>();
                for (int i = 0; i < hours.Count; i++)
                {
                    SetAlarm(hours[i], med, disease, ref alarms, i);
                    Log.Error("AAAAAAAAAAAA", "setupd alarm " + med.Name + " hour: " +  hours[i].HourName);
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
        private void SetAlarm(Hour hour, Medicine med, Disease boala, ref List<int> alarms, int position)
        {
            var am = (AlarmManager)GetSystemService(AlarmService);
            var idAlarm = DateTime.Now.Millisecond ;
            var id = 0;
            var days = 0;
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

            var date = DateTime.Parse(med.Date);
            var setDt = new DateTime(date.Year, date.Day, date.Month,timeHour,timeMinute,0);

            var calendar = Calendar.Instance;
            var setCalendar = Calendar.Instance;
//            setCalendar.Set(CalendarField.Year, setDt.Year);
//            setCalendar.Set(CalendarField.Month, setDt.Month);
//            setCalendar.Set(CalendarField.DayOfMonth, setDt.Day);
//            setCalendar.Set(CalendarField.HourOfDay, setDt.Hour);
//            setCalendar.Set(CalendarField.Minute, setDt.Minute);
//            setCalendar.Set(CalendarField.Second, 0);
            
            if (setDt < DateTime.Now)
            {
                TimeSpan difference = DateTime.Now.Subtract(setDt);
                days = (int) difference.TotalDays + 1;
                setDt = setDt.AddDays(days);
            }

            if (med.NumberOfDays != 0)
            {
                days = med.NumberOfDays - days;
                for (int j = 0; j < days; j++)
                {
                        idAlarm = DateTime.Now.Millisecond ;
                        pi = PendingIntent.GetBroadcast(this, idAlarm, i, PendingIntentFlags.OneShot);
                        setDt = setDt.AddDays(j);
                   
                    var mili = (long) setDt.ToUniversalTime()
                        .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                        .TotalMilliseconds;
                    
                        am.SetInexactRepeating(AlarmType.RtcWakeup, mili, 0, pi);
                       
                }
            }
            else
            {
                var mili = (long) setDt.ToUniversalTime()
                    .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                    .TotalMilliseconds;
                am.SetInexactRepeating(AlarmType.RtcWakeup, mili, AlarmManager.IntervalDay, pi);
               

            }
        }

        public void OnMedSaved(Medicine medicament)
        {
            AddNewMed(medicament);
        }

        public void OnMedUpdated(Medicine medicament)
        {
            medicamentAdapter.UpdateMedicament(medicament, medicament.IdMed);
            medicamentAdapter.NotifyDataSetChanged();
        }

        public void OnMedicamentClick(Medicine medicament)
        {
            OpenMedDialog(medicament);
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