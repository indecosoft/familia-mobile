using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace FamiliaXamarin.Helpers
{
    class SharingDatePickerFragment : Android.Support.V4.App.DialogFragment,
        DatePickerDialog.IOnDateSetListener
    {
        // TAG can be any string of your choice.
        public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();

        // Initialize this value to prevent NullReferenceExceptions.
        Action<DateTime> _dateSelectedHandler = delegate { };

        public static SharingDatePickerFragment NewInstance(Action<DateTime> onDateSelected)
        {
            SharingDatePickerFragment frag = new SharingDatePickerFragment
            {
                _dateSelectedHandler = onDateSelected
            };
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime currently = DateTime.Now;
            DatePickerDialog dialog = new DatePickerDialog(Activity,
                this,
                currently.Year,
                currently.Month,
                currently.Day);

            DateTime origin = new DateTime(1970, 1, 1);

            DateTime minDate = new DateTime(DateTime.Now.Year,1,1);
            long max = (long)(DateTime.Now.Date - origin.Date).TotalMilliseconds;
            long min = (long)(minDate - origin.Date).TotalMilliseconds;

            dialog.DatePicker.MaxDate = max;
            dialog.DatePicker.MinDate = min;
            return dialog;
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
            DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
            Log.Debug(TAG, selectedDate.ToLongDateString());
            _dateSelectedHandler(selectedDate);
        }
    }
}