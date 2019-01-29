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
            if (_mMed.NumberOfDays != 0)
            {
                try
                {
                    var hourString = _mHour.HourName;
                    var parts = hourString.Split(':');
                    var timeHour = Convert.ToInt32(parts[0]);
                    var timeMinute = Convert.ToInt32(parts[1]);
                    /*var calendar = Calendar.Instance;
                    var setCalendar = Calendar.Instance;*/
                    
                    /*setCalendar.Set(CalendarField.HourOfDay, timeHour);
                    setCalendar.Set(CalendarField.Minute, timeMinute);
                    setCalendar.Set(CalendarField.Second, 0);
                    */

                    var date = DateTime.Parse(_mMed.Date);
//                    parts = dateString.Split('.');
//                    var day = Convert.ToInt32(parts[0]);
//                    var month = Convert.ToInt32(parts[1]) - 1;
//                    var year = Convert.ToInt32(parts[2]);


                    /*setCalendar.Set(CalendarField.Year, year);
                    setCalendar.Set(CalendarField.Month, month);
                    setCalendar.Set(CalendarField.DayOfMonth, day);


                    setCalendar.Add(CalendarField.Date, _mMed.NumberOfDays);*/

                    var setDt = new DateTime(date.Year, date.Month, date.Day,timeHour,timeMinute,0).AddDays(_mMed.NumberOfDays);
                    if( DateTime.Compare(setDt,DateTime.Now) > 0)
                        LaunchAlarm(context, intent, medId, boalaId);

                    /*if (setCalendar.After(calendar))
                    {

                        LaunchAlarm(context, intent, medId, boalaId);
                    }*/
                }
                catch (Exception e)
                {
                    Log.Error("ERROOOOR", e.ToString());

                }
            }
            else
            {
                LaunchAlarm(context, intent, medId, boalaId);
            }
        }

       

        private void LaunchAlarm(Context context, Intent intent, string medId, string boalaId)
        {
           
                var i = new Intent(context, typeof(AlarmActivity));
                var intentNotification = new Intent(context, typeof(MedicineFragment));
                //context.startActivity(new Intent(context, AlarmActivity.class));
                i.PutExtra(DiseaseActivity.MED_ID, medId);
                i.PutExtra(DiseaseActivity.BOALA_ID, boalaId);
                i.SetFlags(ActivityFlags.NewTask);
                context.StartActivity(i);
        }
    }
}