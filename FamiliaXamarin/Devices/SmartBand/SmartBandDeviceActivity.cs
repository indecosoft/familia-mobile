using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
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

namespace FamiliaXamarin.Devices.SmartBand
{
    [Activity(Label = "SmartBandDeviceActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
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
        private ConstraintLayout _loadingScreen;


        private void RefreshToken()
        {
            var refreshToken = Utils.GetDefaults("FitbitRefreshToken", this);
            Task.Run(async () =>
            {
                try
                {
                    string storedToken = Utils.GetDefaults("FitbitToken", this);
                    if (!string.IsNullOrEmpty(storedToken))
                    {
                        _token = storedToken;
                        var dict = new Dictionary<string, string>
                            {
                                {"grant_type", "refresh_token"}, {"refresh_token", refreshToken}
                            };
                        var response = await WebServices.Post("https://api.fitbit.com/oauth2/token", dict);
                        if (response != null)
                        {
                            var obj = new JSONObject(response);
                            _token = obj.GetString("access_token");
                            var newRefreshToken = obj.GetString("refresh_token");
                            var userId = obj.GetString("user_id");
                            Utils.SetDefaults(GetString(Resource.String.smartband_device), _token, this);
                            Utils.SetDefaults("FitbitToken", _token, this);
                            Utils.SetDefaults("RitbitRefreshToken", newRefreshToken, this);
                            Utils.SetDefaults("FitbitUserId", userId, this);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("FitbitServiceError", e.Message);
                }

            });
        }
        protected override async void OnCreate(Bundle savedInstanceState)
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

                var code = _url.Substring(_url.IndexOf("&access_token", StringComparison.Ordinal) + 24).Replace("#_=_", string.Empty);

                await Task.Run(async () =>
                {
                    var dict = new Dictionary<string, string>
                    {
                        {"code", code}, {"grant_type", "authorization_code"}, {"redirect_uri", Constants.CallbackUrl}
                    };
                    var response = await WebServices.Post("https://api.fitbit.com/oauth2/token", dict);
                    if (response != null)
                    {
                        var obj = new JSONObject(response);
                        _token = obj.GetString("access_token");
                        var refreshToken = obj.GetString("refresh_token");
                        var userId = obj.GetString("user_id");
                        Utils.SetDefaults(GetString(Resource.String.smartband_device), _token, this);
                        Utils.SetDefaults("FitbitToken", _token, this);
                        Utils.SetDefaults("FitbitRefreshToken", refreshToken, this);
                        Utils.SetDefaults("FitbitUserId", userId, this);
                        Utils.SetDefaults("FitbitAuthCode", code, this);
                    }

                });
            }
            else
            {
                RefreshToken();
                _token = Utils.GetDefaults(GetString(Resource.String.smartband_device), this);
                Log.Error("TokenFromShared", _token);
            }
            _loadingScreen.Visibility = ViewStates.Visible;
            await Task.Run(function: async () => await PopulateFields());
            _loadingScreen.Visibility = ViewStates.Gone;
        }

        private async Task PopulateFields()
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

        private async Task GetSteps()
        {
            var data = await WebServices.Get("https://api.fitbit.com/1/user/-/activities/date/today.json", _token);
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

        private async Task GetSleepData()
        {
            var data = await WebServices.Get("https://api.fitbit.com/1.2/user/-/sleep/date/today.json", _token);
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

        private async Task GetHeartRatePulse()
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