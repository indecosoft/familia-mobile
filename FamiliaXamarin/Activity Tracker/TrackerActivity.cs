using System;
using Android;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Airbnb.Lottie.Model;
using Com.Airbnb.Lottie.Value;
using Familia.Helpers;
using Familia.Services;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Activity_Tracker
{
    [Activity(Label = "TrackerActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class TrackerActivity : AppCompatActivity, TrackerActivityService.IStepsChangeListener
    {
        private TextView tvSteps;
        private TextView tvDailyTarget;
        private TextView tvHHT;
        private TextView tvProgress;
        private LottieAnimationView progressLottieAnimationView;
        private int dailyTarget;
        private int currentProgres;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_tracker);
            SetToolbar();

            tvSteps = FindViewById<TextView>(Resource.Id.tv_steps_from_sensor);
            tvDailyTarget = FindViewById<TextView>(Resource.Id.tv_steps_daily_target);
            tvHHT = FindViewById<TextView>(Resource.Id.tv_steps_hht_target);
            tvProgress = FindViewById<TextView>(Resource.Id.tv_progress);

            TrackerActivityService.SetListener(this);
            SetSteps();
            dailyTarget = GetDailyTarget();
            tvDailyTarget.Text = dailyTarget + "";
            int hht = dailyTarget / 15;
            tvHHT.Text = hht + "";

            InitProgressAnimation();
            SetUIForProgress();

        }

        private void SetSteps()
        {
            try
            {
                tvSteps.Text = int.Parse(Utils.GetDefaults("ActivityTrackerDailySteps")) + "";
            }
            catch (Exception e)
            {
                Log.Error("TrackerActivity Error", e.Message);
                tvSteps.Text = 0 + "";
            }
        }

        private int GetDailyTarget()
        {
            int DailyTarget;
            try
            {
                DailyTarget = int.Parse(Utils.GetDefaults("ActivityTrackerDailyTarget"));
            }
            catch (Exception)
            {
                //daily target not setted, default is 5000
                DailyTarget = 5000;
                Utils.SetDefaults("ActivityTrackerDailyTarget", DailyTarget + "");
            }
            return DailyTarget;
        }

        private int GetProgressInPercent(int target, int currentValue)
        {
            return currentValue * 100 / target;
        }

        private void InitProgressAnimation()
        {
            progressLottieAnimationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view_steps_progress);
            currentProgres = 0;
        }

        private void SetToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate { OnBackPressed(); };
            Title = "Consiliere Activitate";
        }
        
        private void SetUIForProgress100()
        {
            var filter = new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.accent));
            progressLottieAnimationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter, new LottieValueCallback(filter));
            progressLottieAnimationView.PlayAnimation();
        }

        public void OnStepsChanged(long steps)
        {
            tvSteps.Text = steps + "";
            SetUIForProgress();
        }

        private void SetUIForProgress()
        {
            int progress = GetProgressInPercent(dailyTarget, (int)TrackerActivityService.TotalDailySteps);
            if (currentProgres != progress)
            {
                if (progress >= 100)
                {
                    currentProgres = 0;
                    progress = 100;
                    SetUIForProgress100();
                }
                else
                {
                    progressLottieAnimationView.SetMinAndMaxProgress((float)currentProgres / 100, (float)progress / 100);
                    progressLottieAnimationView.PlayAnimation();
                }
                currentProgres = progress;
            }
            tvProgress.Text = progress + "";
        }
    }
}