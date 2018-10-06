using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Widget;
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
    public class SmartBandDeviceActivity : AppCompatActivity
    {
        private string _url = string.Empty;
        private string _token = string.Empty;
        private TextView _lbBpm;
        private TextView _lbSleep;
        private TextView _lbSteps;
        private TextView _lbDisplayName;
        private TextView _lbFullName;
        private TextView _lbActivity;
        private CircleImageView _avatarImage;
        private readonly IWebServices _webServices = new WebServices();

        protected override void OnCreate(Bundle savedInstanceState)
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

            PopulateFields();

            // Create your application here
        }

        private async void PopulateFields()
        {
            await GetProfileData();
            await GetSteps();
            await GetSleepData();
            await GetHeartRatePulse();
        }

        
        private async Task GetProfileData()
        {
            var data = await _webServices.Get("https://api.fitbit.com/1/user/-/profile.json", _token);
            if (!string.IsNullOrEmpty(data))
            {
                Log.Error("Profile result", data);

                try
                {
                    var displayName = new JSONObject(data).GetJSONObject("user").GetString("displayName");
                    _lbDisplayName.Text = displayName;
                    var fullName = new JSONObject(data).GetJSONObject("user").GetString("fullName");
                    _lbFullName.Text = fullName;

                    var avatarUrl = new JSONObject(data).GetJSONObject("user").GetString("avatar640");
                    Log.Error("Fitbit Avatar", avatarUrl);
                    Picasso.With(this)
                        .Load(avatarUrl)
                        .Resize(640, 640)
                        .CenterCrop()
                        .Into(_avatarImage);
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
            }
        }
        private async Task GetSteps()
        {
            var data = await _webServices.Get("https://api.fitbit.com/1.2/user/-/sleep/date/today.json", _token);
            if (!string.IsNullOrEmpty(data))
            {
                Log.Error("Steps Result", data);

                try
                {
                    var fairlyActiveMinutes = new JSONObject(data).GetJSONObject("summary").GetInt("fairlyActiveMinutes");
                    var veryActiveMinutes = new JSONObject(data).GetJSONObject("summary").GetInt("veryActiveMinutes");
                    var activeMinutes = fairlyActiveMinutes + veryActiveMinutes;
                    _lbActivity.Text = $"{activeMinutes}";
                    var steps = new JSONObject(data).GetJSONObject("summary").GetInt("steps");
                    _lbSteps.Text = $"{steps}";
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
            }
        }
        private async Task GetSleepData()
        {
            var data = await _webServices.Get("https://api.fitbit.com/1/user/-/activities/date/today.json", _token);
            Log.Error("Sleep Result", data);
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    var totalMinutesAsleep = new JSONObject(data).GetJSONObject("summary").GetInt("totalMinutesAsleep");
                    var h = (int)TimeUnit.Minutes.ToHours(totalMinutesAsleep);
                    var min = (int)(((float)totalMinutesAsleep / 60 - h) * 100) * 60 / 100;

                    _lbSleep.Text = $"{h} hr {min} min";
                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
            }
        }
        private async Task GetHeartRatePulse()
        {
            var data = await _webServices.Get("https://api.fitbit.com/1/user/-/activities/heart/date/today/1d.json", _token);
            if (!string.IsNullOrEmpty(data))
            {
                Log.Error("Heartrate Result", data);
                try
                {
                    var bpm = ((JSONObject)new JSONObject(data).GetJSONArray("activities-heart").Get(0)).GetJSONObject("value").GetInt("restingHeartRate");
                    var bmpText = $"{bpm} bpm";
                    _lbBpm.Text = bmpText;
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