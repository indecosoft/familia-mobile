using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using FamiliaXamarin.Medicatie.Alarm;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;

namespace FamiliaXamarin.Medicatie
{
    public class MedicineFragment : Android.Support.V4.App.Fragment ,View.IOnClickListener, IOnBoalaClickListener, CustomDialogDeleteBoala.ICustomDialogDeleteBoalaListener
    {
        public static string IdBoala = "id_boala";
        private BoalaAdapter _boalaAdapter;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.fragment_medicine, container, false);
            view.FindViewById(Resource.Id.btn_add).SetOnClickListener(this);
            setupRecycleView(view);
            return view;
        }
        public override void OnResume()
        {
            base.OnResume();
            SetListForAdapter();
        }

        private void SetListForAdapter()
        {
            _boalaAdapter.setBoli(Storage.getInstance().getBoliTest(Activity));
            _boalaAdapter.NotifyDataSetChanged();
        }


        private void setupRecycleView(View view)
        {
            RecyclerView rvBoli = view.FindViewById<RecyclerView>(Resource.Id.rv_notes);
            LinearLayoutManager layoutManager = new LinearLayoutManager(Activity);
            rvBoli.SetLayoutManager(layoutManager);
            _boalaAdapter = new BoalaAdapter();
            _boalaAdapter.SetListenerBoala(this);
            rvBoli.SetAdapter(_boalaAdapter);
        }
        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_add:
                    Activity.StartActivity(typeof(BoalaActivity));
                    break;
            }
        }


        public void OnBoalaClick(Boala boala)
        {
            Intent intent = new Intent(Application.Context, typeof(BoalaActivity));
            intent.PutExtra(IdBoala, boala.Id);
            StartActivity(intent);
        }

        public void OnBoalaDelete(Boala boala)
        {
            CustomDialogDeleteBoala cddb = new CustomDialogDeleteBoala(Activity);
            cddb.SetListener(this);
            cddb.SetBoala(boala);
            cddb.Show();
        }


        public void OnYesClicked(string result, Boala boala)
        {
            if (result.Equals("yes"))
            {
                Storage.getInstance().removeBoala(Activity, boala);
                List<Medicament> meds = boala.MedicamentList;
                CancelAlarms(meds);

                _boalaAdapter.removeBoala(boala);
                _boalaAdapter.NotifyDataSetChanged();

            }
        }

        private void CancelAlarms(List<Medicament> meds)
        {
            foreach (var med in meds)
            {
                if (med.Alarms != null)
                {
                    foreach (var alarm in med.Alarms)
                    {
                        AlarmManager am = (AlarmManager) Context.GetSystemService(Context.AlarmService);

                        Intent i = new Intent(Activity, typeof(AlarmBroadcastReceiver));
                        PendingIntent pi =
                            PendingIntent.GetActivity(Context, alarm, i, PendingIntentFlags.UpdateCurrent);
                        am.Cancel(pi);
                    }
                }
            }
        }
//        public override void OnMedicationScheduleLoaded(List<MedicationSchedule> msList)
//        {
//            foreach (var ms in  msList)
//            {
//
//                //TODO parseaza din timestamp-ul de pe ms ca sa iei ora si data pt setarea alarmei
// 
//
//            }
//        }

        
    }
}