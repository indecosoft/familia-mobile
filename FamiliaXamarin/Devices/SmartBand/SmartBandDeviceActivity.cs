using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.Helpers;
using Java.Util.Concurrent;
using Org.Json;
using Refractored.Controls;
using Square.Picasso;
using Task = System.Threading.Tasks.Task;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace FamiliaXamarin.SmartBand
{
    [Activity(Label = "SmartBandDeviceActivity", Theme = "@style/AppTheme.Dark")]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "fittauth",
        DataHost = "finish")]
    public  class SmartBandDeviceActivity : AppCompatActivity
    {
        string _url = string.Empty;
        string _token = string.Empty;
        TextView _lbBpm;
        TextView _lbSleep;
        TextView _lbSteps;
        TextView _lbDisplayName;
        TextView _lbFullName;
        TextView _lbActivity;
        CircleImageView _avatarImage;
        ConstraintLayout _loadingScreen;
        //readonly IWebServices _webServices = new WebServices();

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        protected override async void OnCreate(Bundle savedInstanceState)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_smart_band_device);
            _lbBpm = FindViewById<TextView>(Resource.Id.lbBpm);
            _lbSleep = FindViewById<TextView>(Resource.Id.lbSleepTime);
            _lbSteps = FindViewById<TextView>(Resource.Id.lbsteps);
            _lbDisplayName = FindViewById<TextView>(Resource.Id.lbDisplayName);
            _lbFullName = FindViewById<TextView>(Resource.Id.lbFullName);
            _lbActivity = FindViewById<TextView>(Resource.Id.lbActivTime);
            _avatarImage = FindViewById<CircleImageView>(Resource.Id.FitBitprofileImage);
            _loadingScreen = FindViewById<ConstraintLayout>(Resource.Id.loading);


            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            };
            Title = "Profil de sanatate";

            OnNewIntent(Intent);
            if (_url != null)
            {
                Log.Error("URL", $"{_url}");

                var token = _url.Substring(_url.IndexOf("&access_token", StringComparison.Ordinal) + 32);
                var userId = _url.Substring(_url.IndexOf("&user_id=", StringComparison.Ordinal) + 9);
                var scope = _url.Substring(_url.IndexOf("&scope=", StringComparison.Ordinal) + 7);
                var tokenType = _url.Substring(_url.IndexOf("&token_type=", StringComparison.Ordinal) + 12);
                var expires = _url.Substring(_url.IndexOf("&expires_in=", StringComparison.Ordinal) + 12);

                tokenType = tokenType.Replace($"&expires_in={expires}", string.Empty);
                token = token.Replace($"&user_id={userId}", string.Empty);
                userId = userId.Replace($"&scope={scope}", string.Empty);
                scope = scope.Replace($"&token_type={tokenType}&expires_in={expires}", string.Empty);

                Log.Error("Token", token);
                Log.Error("Token_Type", tokenType);
                Log.Error("UserId", userId);
                Log.Error("Scope", scope);
                Log.Error("Expires_in", expires);
                Utils.SetDefaults(GetString(Resource.String.smartband_device), token, this);
            }
            else
            {
                _token = Utils.GetDefaults(GetString(Resource.String.smartband_device), this);
                Log.Error("TokenFromShared", _token);
            }
            _loadingScreen.Visibility = ViewStates.Visible;
            await Task.Run(function: async () => await PopulateFields());
            _loadingScreen.Visibility = ViewStates.Gone;

            // Create your application here
        }

        async Task PopulateFields()
        {
            await GetProfileData();
            await GetSteps();
            await GetSleepData();
            await GetHeartRatePulse();
        }


        private async Task GetProfileData()
        {
            var data = await WebServices.Get("https://api.fitbit.com/1/user/-/profile.json", _token);
            if (!string.IsNullOrEmpty(data))
            {
                Log.Error("Profile result", data);

                try
                {
                    var displayName = new JSONObject(data).GetJSONObject("user").GetString("displayName");
                    
                    var fullName = new JSONObject(data).GetJSONObject("user").GetString("fullName");


                    var avatarUrl = new JSONObject(data).GetJSONObject("user").GetString("avatar640");
                    Log.Error("Fitbit Avatar", avatarUrl);
                    
                    RunOnUiThread(() =>
                    {
                        _lbDisplayName.Text = displayName;
                        _lbFullName.Text = fullName;
                        Picasso.With(this)
                            .Load(avatarUrl)
                            .Resize(640, 640)
                            .CenterCrop()
                            .Into(_avatarImage);
                    });
                    
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
            }
        }
        async Task GetSteps()
        {
            var data = await WebServices.Get("https://api.fitbit.com/1.2/user/-/sleep/date/today.json", _token);
            if (!string.IsNullOrEmpty(data))
            {
                Log.Error("Steps Result", data);

                try
                {
                    var fairlyActiveMinutes = new JSONObject(data).GetJSONObject("summary").GetInt("fairlyActiveMinutes");
                    var veryActiveMinutes = new JSONObject(data).GetJSONObject("summary").GetInt("veryActiveMinutes");
                    var activeMinutes = fairlyActiveMinutes + veryActiveMinutes;

                    var steps = new JSONObject(data).GetJSONObject("summary").GetInt("steps");

                    RunOnUiThread(() =>
                    {
                        _lbSteps.Text = $"{steps}";
                        _lbActivity.Text = $"{activeMinutes}";
                    });
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
            }
        }
        async Task GetSleepData()
        {
            var data = await WebServices.Get("https://api.fitbit.com/1/user/-/activities/date/today.json", _token);
            Log.Error("Sleep Result", data);
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    var totalMinutesAsleep = new JSONObject(data).GetJSONObject("summary").GetInt("totalMinutesAsleep");
                    var h = (int)TimeUnit.Minutes.ToHours(totalMinutesAsleep);
                    var min = (int)(((float)totalMinutesAsleep / 60 - h) * 100) * 60 / 100;


                    RunOnUiThread(() =>
                    {
                        _lbSleep.Text = $"{h} hr {min} min";
                    });
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
            }
        }
        async Task GetHeartRatePulse()
        {
            var data = await WebServices.Get("https://api.fitbit.com/1/user/-/activities/heart/date/today/1d.json", _token);
            if (!string.IsNullOrEmpty(data))
            {
                Log.Error("Heartrate Result", data);
                try
                {
                    var bpm = ((JSONObject)new JSONObject(data).GetJSONArray("activities-heart").Get(0)).GetJSONObject("value").GetInt("restingHeartRate");
                    var bmpText = $"{bpm} bpm";

                    RunOnUiThread(() =>
                    {
                        _lbBpm.Text = bmpText;
                    });
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            _url = intent.DataString;
        }
        public override void OnBackPressed()
        {
            base.OnBackPressed();
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
        }
}
}