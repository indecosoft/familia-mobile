using System;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Widget;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;

namespace Familia.Helpers {
	class SharingDatePickerFragment : DialogFragment, DatePickerDialog.IOnDateSetListener {
		// TAG can be any string of your choice.
		public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();

		// Initialize this value to prevent NullReferenceExceptions.
		Action<DateTime> _dateSelectedHandler = delegate { };

		public static SharingDatePickerFragment NewInstance(Action<DateTime> onDateSelected) {
			var frag = new SharingDatePickerFragment {_dateSelectedHandler = onDateSelected};
			return frag;
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState) {
			DateTime currently = DateTime.Now;
			var dialog = new DatePickerDialog(Activity, this, currently.Year, currently.Month, currently.Day);

			var origin = new DateTime(1970, 1, 1);

			var minDate = new DateTime(DateTime.Now.Year, 1, 1);
			var max = (long) (DateTime.Now.Date - origin.Date).TotalMilliseconds;
			var min = (long) (minDate - origin.Date).TotalMilliseconds;

			dialog.DatePicker.MaxDate = max;
			dialog.DatePicker.MinDate = min;
			return dialog;
		}

		public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth) {
			// Note: monthOfYear is a value between 0 and 11, not 1 and 12!
			var selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
			Log.Debug(TAG, selectedDate.ToLongDateString());
			_dateSelectedHandler(selectedDate);
		}
	}
}