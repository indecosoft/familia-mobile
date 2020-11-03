using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.Core.Content;
using Com.Airbnb.Lottie;
using Com.Airbnb.Lottie.Model;
using Com.Airbnb.Lottie.Value;
using Com.Bumptech.Glide;
using Familia.DataModels;
using Familia.Devices.Helpers;
using Familia.Helpers;
using Java.Util.Concurrent;
using Org.Json;
using Refractored.Controls;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;


namespace Familia.Devices.SmartBand
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
            string refreshToken = Utils.GetDefaults("FitbitRefreshToken");
            Task.Run(async () =>
            {
                try
                {
                    string storedToken = Utils.GetDefaults("FitbitToken");
                    if (!string.IsNullOrEmpty(storedToken))
                    {
                        _token = storedToken;
                        var dict = new Dictionary<string, string>
                            {
                                {"grant_type", "refresh_token"}, {"refresh_token", refreshToken}
                            };
                        string response = await WebServices.WebServices.Post("https://api.fitbit.com/oauth2/token", dict);
                        if (response != null)
                        {
                            var obj = new JSONObject(response);
                            _token = obj.GetString("access_token");
                            string newRefreshToken = obj.GetString("refresh_token");
                            string userId = obj.GetString("user_id");
                            Utils.SetDefaults(GetString(Resource.String.smartband_device), _token);
                            Utils.SetDefaults("FitbitToken", _token);
                            Utils.SetDefaults("RitbitRefreshToken", newRefreshToken);
                            Utils.SetDefaults("FitbitUserId", userId);
                        }
                    }
                    else
                    {
                        Utils.SetDefaults(GetString(Resource.String.smartband_device), null);
                        Utils.SetDefaults("FitbitToken", null);
                        Utils.SetDefaults("RitbitRefreshToken", null);
                        Utils.SetDefaults("FitbitUserId", null);
                        var sqlHelper =
                       await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                        await sqlHelper.QueryValuations($"DELETE FROM BluetoothDeviceRecords WHERE DeviceType ='{GetString(Resource.String.smartband_device)}'");
                        Finish();
                    }
                }
                catch (Exception e)
                {
                    Log.Error("FitbitServiceError", e.Message);
                    Utils.SetDefaults(GetString(Resource.String.smartband_device), null);
                    Utils.SetDefaults("FitbitToken", null);
                    Utils.SetDefaults("RitbitRefreshToken", null);
                    Utils.SetDefaults("FitbitUserId", null);
                    var sqlHelper =
                   await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                    await sqlHelper.QueryValuations($"DELETE FROM BluetoothDeviceRecords WHERE DeviceType ='{GetString(Resource.String.smartband_device)}'");
                    Finish();
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
            var animationView = FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            var filter =
                new SimpleColorFilter(ContextCompat.GetColor(this, Resource.Color.colorAccent));
            animationView.AddValueCallback(new KeyPath("**"), LottieProperty.ColorFilter,
                new LottieValueCallback(filter));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate { OnBackPressed(); };
            Title = "Profil de sanatate";

            OnNewIntent(Intent);
            if (_url != null)
            {

                string code = _url.Substring(_url.IndexOf("&access_token", StringComparison.Ordinal) + 24).Replace("#_=_", string.Empty);

                await Task.Run(async () =>
                {
                    var dict = new Dictionary<string, string>
                    {
                        {"code", code}, {"grant_type", "authorization_code"}, {"redirect_uri", Constants.CallbackUrl}
                    };
                    string response = await WebServices.WebServices.Post("https://api.fitbit.com/oauth2/token", dict);
                    if (response != null)
                    {
                        var obj = new JSONObject(response);
                        _token = obj.GetString("access_token");
                        string refreshToken = obj.GetString("refresh_token");
                        string userId = obj.GetString("user_id");
                        var bleDevicesRecords = await SqlHelper<BluetoothDeviceRecords>.CreateAsync();
                        await bleDevicesRecords.Insert(
                            new BluetoothDeviceRecords
                            {
                                Name = "SmartBand", 
                                Address = _token,
                                DeviceType = DeviceType.SmartBand
                            });
                        Utils.SetDefaults(GetString(Resource.String.smartband_device), _token);
                        Utils.SetDefaults("FitbitToken", _token);
                        Utils.SetDefaults("FitbitRefreshToken", refreshToken);
                        Utils.SetDefaults("FitbitUserId", userId);
                        Utils.SetDefaults("FitbitAuthCode", code);
                    }

                });
            }
            else
            {
                RefreshToken();
                _token = Utils.GetDefaults(GetString(Resource.String.smartband_device));
               // Log.Error("TokenFromShared", _token);
            }
            _loadingScreen.Visibility = ViewStates.Visible;
            await Task.Run(async () => await PopulateFields());
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
            string data = await WebServices.WebServices.Get("https://api.fitbit.com/1/user/-/profile.json", _token);
            if (!string.IsNullOrEmpty(data))
            {
                Log.Error("Profile result", data);

                try
                {
                    string displayName = new JSONObject(data).GetJSONObject("user").GetString("displayName");

                    string fullName = new JSONObject(data).GetJSONObject("user").GetString("fullName");


                    string avatarUrl = new JSONObject(data).GetJSONObject("user").GetString("avatar640");
                    Log.Error("Fitbit Avatar", avatarUrl);

                    RunOnUiThread(() =>
                    {
                        _lbDisplayName.Text = displayName;
                        _lbFullName.Text = fullName;
                        try
                        {
                            Glide.With(this).Load(avatarUrl).Into(_avatarImage);
                        }
                        catch
                        {
                            //ignored
                        }

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
            string data = await WebServices.WebServices.Get("https://api.fitbit.com/1/user/-/activities/date/today.json", _token);
            if (!string.IsNullOrEmpty(data))
            {
                Log.Error("Steps Result", data);

                try
                {
                    int fairlyActiveMinutes = new JSONObject(data).GetJSONObject("summary").GetInt("fairlyActiveMinutes");
                    int veryActiveMinutes = new JSONObject(data).GetJSONObject("summary").GetInt("veryActiveMinutes");
                    int activeMinutes = fairlyActiveMinutes + veryActiveMinutes;

                    int steps = new JSONObject(data).GetJSONObject("summary").GetInt("steps");

                    RunOnUiThread(() =>
                    {
                        _lbSteps.Text = $"{steps}";
                        _lbActivity.Text = $"{activeMinutes} min";
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
            string data = await WebServices.WebServices.Get("https://api.fitbit.com/1.2/user/-/sleep/date/today.json", _token);
            Log.Error("Sleep Result", data);
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    int totalMinutesAsleep = new JSONObject(data).GetJSONObject("summary").GetInt("totalMinutesAsleep");
                    var h = (int)TimeUnit.Minutes.ToHours(totalMinutesAsleep);
                    int min = (int)(((float)totalMinutesAsleep / 60 - h) * 100) * 60 / 100;


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
            string data = await WebServices.WebServices.Get("https://api.fitbit.com/1/user/-/activities/heart/date/today/1d.json", _token);
            if (!string.IsNullOrEmpty(data))
            {
                Log.Error("Heartrate Result", data);
                try
                {
                    int bpm = ((JSONObject)new JSONObject(data).GetJSONArray("activities-heart").Get(0)).GetJSONObject("value").GetInt("restingHeartRate");
                    string bmpText = $"{bpm} bpm";

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
            intent.PutExtra("FromSmartband", true);
            StartActivity(intent);
        }
    }
}