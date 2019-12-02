using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Activity_Tracker
{
    [Activity(Label = "TrackerActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class TrackerActivity : AppCompatActivity, StepCounterSensor.IStepCounterSensorChangedListener
    {
        private TextView tvSteps;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_tracker);
            SetToolbar();

            tvSteps = FindViewById<TextView>(Resource.Id.tv_steps_from_sensor);

            StepCounterSensor sensor = new StepCounterSensor(this);
            sensor.SetListener(this);
        }

        private void SetToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                OnBackPressed();
            };
            Title = "Consiliere Activitate";
        }

        public void OnStepCounterSensorChanged(long count)
        {
            string message = count + "";
//            Toast.MakeText(this, message, ToastLength.Long).Show();
            tvSteps.Text = count + "";
        }
    }
}