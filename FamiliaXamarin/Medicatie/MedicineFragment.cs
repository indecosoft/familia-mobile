using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Familia.Medicatie.Alarm;
using Familia.Medicatie.Data;
using Familia.Medicatie.Entities;
using SQLite;
using Fragment = Android.Support.V4.App.Fragment;

namespace Familia.Medicatie
{
    public class MedicineFragment : Fragment ,View.IOnClickListener, IOnBoalaClickListener, CustomDialogDeleteDisease.ICustomDialogDeleteDiseaseListener
    {

//        private ProgressBarDialog _progressBarDialog;
        public static string IdBoala = "id_boala";
        private DiseaseAdapter _boalaAdapter;
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Log.Error("CREATE VIEW", "MEDICINE PERSONALA FRAGMENT");

            View view = inflater.Inflate(Resource.Layout.fragment_medicine, container, false);
            view.FindViewById(Resource.Id.btn_add_disease).SetOnClickListener(this);
            setupRecycleView(view);
            
            return view;
        }


        public override void OnResume()
        {
            base.OnResume();
            SetListForAdapter();
            Log.Error("MEDICINE PERSONALA", "on resume called");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Error("MEDICINE PERSONALA", "on destroy called");

        }

        public override void OnPause()
        {
            base.OnPause();
            Log.Error("MEDICINE PERSONALA", "on pause called");

        }


        private void SetListForAdapter()
        {
            _boalaAdapter.setBoli(Storage.GetInstance().GetListOfDiseasesFromFile(Activity));
            _boalaAdapter.NotifyDataSetChanged();
        }


        private void setupRecycleView(View view)
        {
            var rvBoli = view.FindViewById<RecyclerView>(Resource.Id.rv_notes);
            var layoutManager = new LinearLayoutManager(Activity);
            rvBoli.SetLayoutManager(layoutManager);
            _boalaAdapter = new DiseaseAdapter();
            _boalaAdapter.SetListenerBoala(this);
            rvBoli.SetAdapter(_boalaAdapter);
        }
        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.btn_add_disease:
                    var intent = new Intent(Activity, typeof(DiseaseActivity));
                    StartActivity(intent);
                    break;
            }
        }


        public void OnBoalaClick(Disease boala)
        {
            var intent = new Intent(Application.Context, typeof(DiseaseActivity));
            intent.PutExtra(IdBoala, boala.Id);
            StartActivity(intent);
        }

        public void OnBoalaDelete(Disease boala)
        {
            var cddb = new CustomDialogDeleteDisease(Activity);
            cddb.SetListener(this);
            cddb.SetBoala(boala);
            cddb.Show();
            cddb.Window.SetBackgroundDrawableResource(Resource.Color.colorPrimary);
        }


        public void OnYesClicked(string result, Disease boala)
        {
            if (result.Equals("yes"))
            {
                Storage.GetInstance().removeBoala(Activity, boala);
                var meds = boala.ListOfMedicines;
                CancelAlarms(meds);

                _boalaAdapter.removeBoala(boala);
                _boalaAdapter.NotifyDataSetChanged();

            }
        }

        private void CancelAlarms(List<Medicine> meds)
        {
            foreach (Medicine med in meds)
            {
                if (med.Alarms != null)
                {
                    foreach (int alarm in med.Alarms)
                    {
                        var am = (AlarmManager) Context.GetSystemService(Context.AlarmService);

                        var i = new Intent(Activity, typeof(AlarmBroadcastReceiver));
                        PendingIntent pi =
                            PendingIntent.GetActivity(Context, alarm, i, PendingIntentFlags.UpdateCurrent);
                        am.Cancel(pi);
                    }
                }
            }
        }
    }
}