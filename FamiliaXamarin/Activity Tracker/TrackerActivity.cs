using System;
using Android.Animation;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Airbnb.Lottie.Model;
using Com.Airbnb.Lottie.Value;
using FamiliaXamarin.Helpers;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Activity_Tracker
{
    [Activity(Label = "TrackerActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class TrackerActivity : AppCompatActivity, StepCounterSensor.IStepCounterSensorChangedListener, Animator.IAnimatorListener
    {
        private TextView tvSteps;
        private TextView tvDailyTarget;
        private TextView tvHHT;
        private TextView tvProgress;
        private StepCounterSensor sensor;
        private LottieAnimationView walkingLottieAnimationView;
        private LottieAnimationView progressLottieAnimationView;
        private bool isAnimating;
        private long currentSteps;
        private int dailyTarget;
        private int currentProgres;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_tracker);
            SetToolbar();

            InitWalkingAnimation();
            
            tvSteps = FindViewById<TextView>(Resource.Id.tv_steps_from_sensor);
            tvDailyTarget = FindViewById<TextView>(Resource.Id.tv_steps_daily_target);
            tvHHT = FindViewById<TextView>(Resource.Id.tv_steps_hht_target);
            tvProgress = FindViewById<TextView>(Resource.Id.tv_progress);
            
            dailyTarget = getDailyTarget();
            tvDailyTarget.Text = dailyTarget + "";
            var hht = dailyTarget / 15;
            tvHHT.Text = hht + "";

            InitProgressAnimation();

            sensor = new StepCounterSensor(this);
            sensor.SetListener(this);
        }

        private int getDailyTarget()
        {
            int DailyTarget;
            try
            {
                DailyTarget = int.Parse(Utils.GetDefaults("ActivityTrackerDailyTarget"));
            }
            catch (Exception )
            {
                //daily target not setted, default is 5000
                DailyTarget = 5000;
                Utils.SetDefaults("ActivityTrackerDailyTarget", DailyTarget + "");
            }
            return DailyTarget;
        }

        private int getProgressInPercent(int target, int currentValue)
        {
            return currentValue * 100 / target;
        }

        private void InitProgressAnimation()
        {
            progressLottieAnimationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view_steps_progress);
            currentProgres = 0;
        }

        private void InitWalkingAnimation()
        {
            walkingLottieAnimationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view_walking);
            walkingLottieAnimationView.AddAnimatorListener(this);
            var filter =
                new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.accent));
            walkingLottieAnimationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter,
                new LottieValueCallback(filter));
            isAnimating = false;
            currentSteps = 0;
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

        public void OnStepCounterSensorChanged(long count)
        {
            tvSteps.Text = count + "";
            var progress = getProgressInPercent(dailyTarget, (int) count);

            if (currentProgres != progress)
            {
                if (progress >= 100)
                {
                    currentProgres = 0;
                    progress = 100;
                    progressLottieAnimationView.SetMinAndMaxProgress(1.0f, 1.0f);
                    var filter =
                        new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.accent));
                    progressLottieAnimationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter,
                        new LottieValueCallback(filter));
                    progressLottieAnimationView.PlayAnimation();
                    
                }
                else
                {
                    progressLottieAnimationView.SetMinAndMaxProgress((float)currentProgres / 100, (float)progress / 100);
                    progressLottieAnimationView.PlayAnimation();
                }
                currentProgres = progress;
            }

            tvProgress.Text = progress + "";

            if (!isAnimating)
            {
                walkingLottieAnimationView.Speed = 3;
                walkingLottieAnimationView.PlayAnimation();
            }
        }
    
    public override void OnBackPressed()
        {
            base.OnBackPressed();
            sensor.CloseListener();
        }

        public void OnAnimationCancel(Animator animation)
        {
            Log.Error("TrackerActivity", "animation cancel");
        }

        public void OnAnimationEnd(Animator animation)
        {
            Log.Error("TrackerActivity", "animation end");
            isAnimating = false;
        }

        public void OnAnimationRepeat(Animator animation)
        {
            Log.Error("TrackerActivity", "animation repeat");
        }

        public void OnAnimationStart(Animator animation)
        {
            Log.Error("TrackerActivity", "animation start");
            isAnimating = true;
        }
    }
}