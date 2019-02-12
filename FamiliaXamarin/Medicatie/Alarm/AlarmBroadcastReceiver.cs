using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Widget;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;
using Java.Lang;
using Java.Util;
using Calendar = Java.Util.Calendar;
using Exception = Java.Lang.Exception;
using Process = Android.OS.Process;

namespace FamiliaXamarin.Medicatie.Alarm
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
   
    class AlarmBroadcastReceiver : BroadcastReceiver
    {
        private Hour _mHour;
        private Disease _mDisease;
        private Medicine _mMed;

        public override void OnReceive(Context context, Intent intent)
        {   
            var medId = intent.GetStringExtra(DiseaseActivity.MED_ID);
            var boalaId = intent.GetStringExtra(DiseaseActivity.BOALA_ID);
            var hourId = intent.GetStringExtra(DiseaseActivity.HOUR_ID);

            Storage.GetInstance().GetListOfDiseasesFromFile(context);
            _mDisease = Storage.GetInstance().GetDisease(boalaId);

            if (_mDisease == null) return;
            _mMed = _mDisease.GetMedicineById(medId);

            if (_mMed == null) return;
            _mHour = _mMed.FindHourById(hourId);
            Log.Error("MEDICAMENT_RECEIVER",_mMed.Name);
            
            LaunchAlarm(context, medId, boalaId);

        }

       

        private void LaunchAlarm(Context context, string medId, string boalaId)
        {
                Process.KillProcess(Process.MyPid());
                var i = new Intent(context, typeof(AlarmActivity));
               // var intentNotification = new Intent(context, typeof(MedicineFragment));
                //context.startActivity(new Intent(context, AlarmActivity.class));
                i.PutExtra(DiseaseActivity.MED_ID, medId);
                i.PutExtra(DiseaseActivity.BOALA_ID, boalaId);
                i.SetFlags(ActivityFlags.NewTask);
                //i.AddCategory(Intent.CategoryHome);
                context.StartActivity(i);
                
        }
    }
}