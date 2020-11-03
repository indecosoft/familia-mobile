using System;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Widget;
using Familia.Helpers;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;

namespace Familia.Medicatie
{
    class DatePickerMedicine : DialogFragment, DatePickerDialog.IOnDateSetListener
    {
        public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();

        Action<DateTime> _dateSelectedHandler = delegate { };

        public static DatePickerMedicine NewInstance(Action<DateTime> onDateSelected)
        {
            var frag = new DatePickerMedicine {_dateSelectedHandler = onDateSelected};
            Log.Error("FRAGUL", frag.ToString());
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime currently = DateTime.Now;
            var dialog = new DatePickerDialog(Activity,
                this,
                currently.Year,
                currently.Month-1,
                currently.Day);

            return dialog;
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            var selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
            Log.Debug(TAG, selectedDate.ToLongDateString());
            _dateSelectedHandler(selectedDate);
        }
    }
}