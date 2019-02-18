using Android.Content;
using FamiliaXamarin.Medicatie.Data;
using FamiliaXamarin.Medicatie.Entities;

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
            
            LaunchAlarm(context, medId, boalaId);

        }

       

        private void LaunchAlarm(Context context, string medId, string boalaId)
        {
               
                var i = new Intent(context, typeof(AlarmActivity));
                i.AddFlags(ActivityFlags.ClearTop);
                i.PutExtra(DiseaseActivity.MED_ID, medId);
                i.PutExtra(DiseaseActivity.BOALA_ID, boalaId);
                i.SetFlags(ActivityFlags.NewTask);
                context.StartActivity(i);
                
        }
    }
}