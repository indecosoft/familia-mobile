using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Android.Util;
using Familia.DataModels;
using Familia.Helpers;
using Familia.Location;
using Familia.Medicatie.Alarm;
using Java.Lang;
using Org.Json;
using Exception = System.Exception;
using Android.App;

namespace Familia.Services {
	[Service]
	internal class SmartBandService : Service {
		private const int ServiceRunningNotificationId = 10000;
		private string _token;
		private readonly Handler _handler = new Handler();
		private int _refreshTime = 1000;
		private readonly Runnable _runnable;
		private SqlHelper<SmartBandRecords> _sqlHelper;
		private readonly LocationManager _location = LocationManager.Instance;

		private async void HandlerRunnable() {
			if (Utils.CheckIfLocationIsEnabled() && Utils.CheckNetworkAvailability()) {
				await RefreshToken();
				SentData();
				_handler.PostDelayed(_runnable, _refreshTime * 3600 * 6);
			} else {
				Log.Error("SmartBand Service", "Operation Aborted because Location or Network is disabled");
				_handler.PostDelayed(_runnable, _refreshTime * 3600 * 3);
			}
		}

		public SmartBandService() {
			_runnable = new Runnable(HandlerRunnable);
		}

		public override IBinder OnBind(Intent intent) {
			throw new NotImplementedException();
		}

		public override void OnCreate() {
			Log.Error("Service:", "SmartBandService STARTED");

			try {
				if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
				Notification notification = new NotificationCompat.Builder(this, App.NonStopChannelIdForServices)
					.SetContentTitle("Familia").SetContentText("Serviciul fitbit ruleaza in fundal")
					.SetSmallIcon(Resource.Drawable.logo).SetOngoing(true).Build();
				StartForeground(ServiceRunningNotificationId, notification);
			} catch (Exception e) {
				Log.Error("SmartBand Service Error occurred", e.Message);
			}
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
			StartCommands();
			if (intent.HasExtra("IsShouldStop")) {
				StopForeground(true);
				StopSelf();
			}
			return StartCommandResult.Sticky;
		}

		private async void StartCommands() {
			_sqlHelper = await SqlHelper<SmartBandRecords>.CreateAsync();
			await RefreshToken();
			_token = Utils.GetDefaults(GetString(Resource.String.smartband_device));

			_handler.PostDelayed(_runnable, _refreshTime * 5);
		}

		private async Task RefreshToken() {
			string refreshToken = Utils.GetDefaults("FitbitRefreshToken");
			await Task.Run(async () => {
				try {
					string storedToken = Utils.GetDefaults(GetString(Resource.String.smartband_device));
					if (!string.IsNullOrEmpty(storedToken)) {
						var dict = new Dictionary<string, string> {
							{"grant_type", "refresh_token"}, {"refresh_token", refreshToken}
						};
						// TODO: Check this cal after refactoring requests
						string response = await WebServices.WebServices.Post("https://api.fitbit.com/oauth2/token", dict);
						if (response != null) {
							var obj = new JSONObject(response);
							string token = obj.GetString("access_token");
							string newRefreshToken = obj.GetString("refresh_token");
							string userId = obj.GetString("user_id");
							Utils.SetDefaults(GetString(Resource.String.smartband_device), token);
							Utils.SetDefaults("FitbitToken", token);
							Utils.SetDefaults("FitbitRefreshToken", newRefreshToken);
							Utils.SetDefaults("FitbitUserId", userId);
						}
					}
				} catch (Exception e) {
					Log.Error("SmartBand Service Error occurred", e.Message);
					StopSelf();
				}
			});
		}

		private void GetSteps() {
			_location.LocationRequested += RequestedLocationForSteps;
		}

		private void RequestedLocationForSleep(object source, LocationEventArgs args) {
			_location.LocationRequested -= RequestedLocationForSleep;

			Task.Run(async () => {
				// ToDo: Check this Request too
				string data = await WebServices.WebServices.Get("https://api.fitbit.com/1.2/user/-/sleep/date/today.json", _token);

				if (string.IsNullOrEmpty(data)) return;
				try {
					var jsonObject = new JSONObject(data);
					if (!Utils.CheckNetworkAvailability()) {
						await _sqlHelper.Insert(new SmartBandRecords {
							DataObject = jsonObject.ToString(), Type = "Sleep"
						});
					} else {
						var recordEnumerable =
							await _sqlHelper.QueryValuations("SELECT * FROM SmartBandRecords WHERE Type  = 'Sleep'");
						var jsonArray = new JSONArray();
						foreach (SmartBandRecords element in recordEnumerable) {
							jsonArray.Put(new JSONObject(element.DataObject));
							await _sqlHelper.Delete(element);
						}

						jsonArray.Put(jsonObject);
						var obj = new JSONObject();
						Log.Error("FitBitData", Utils.GetDeviceIdentificator(this) + "");
						Log.Error("FitBit IdClient", Utils.GetDefaults("IdClient") + "");
						Log.Error("FitBit IdPersoana", Utils.GetDefaults("IdPersoana") + "");
						Log.Error("FitBit Data", jsonArray + "");
						Log.Error("FitBit Latitude", args.Location.Latitude.ToString().Replace(',', '.') + "");
						Log.Error("FitBit Longitude",args.Location.Longitude.ToString().Replace(',', '.') + "");

						obj.Put("imei", Utils.GetDeviceIdentificator(this))
							.Put("idClient", Utils.GetDefaults("IdClient"))
							.Put("idPersoana", Utils.GetDefaults("IdPersoana")).Put("data", jsonArray)
							.Put("laﬁﬁtitude", ((LocationEventArgs) args).Location.Latitude.ToString().Replace(',', '.'))
							.Put("longitude",
								((LocationEventArgs) args).Location.Longitude.ToString().Replace(',', '.'));
						string result = await WebServices.WebServices.Post("/api/smartband/sleep", obj,
							Utils.GetDefaults("Token"));
						if (!string.IsNullOrEmpty(result)) {
							try {
								var obj1 = new JSONObject(result);
								if (obj1.GetString("data") != "done") {
									await _sqlHelper.Insert(new SmartBandRecords {
										DataObject = jsonObject.ToString(), Type = "Sleep"
									});
								} else {
									await _sqlHelper.QueryValuations(
										"DELETE FROM SmartBandRecords WHERE Type = 'Sleep'");
								}
							} catch (Exception e) {
								Log.Error("SmartBand Service Error occurred", e.Message);
							}
						} else {
							try {
								await _sqlHelper.Insert(new SmartBandRecords {
									DataObject = jsonObject.ToString(), Type = "Sleep"
								});
							} catch (Exception e) {
								Log.Error("SmartBand Service Error occurred", e.Message);
							}
						}
					}
				} catch (JSONException e) {
					Log.Error("SmartBand Service Error occurred", e.Message);
					StopSelf();
				}
			});
		}

		private void RequestedLocationForSteps(object source, EventArgs args) {
			try {
				_location.LocationRequested -= RequestedLocationForSteps;
				Task.Run(async () => {
					// ToDo: Check this Request too
					string data = await WebServices.WebServices.Get("https://api.fitbit.com/1/user/-/activities/date/today.json",
						_token);
					if (!string.IsNullOrEmpty(data)) {
						Log.Error("Steps Result", data);

						try {
							int fairlyActiveMinutes = new JSONObject(data).GetJSONObject("summary")
								.GetInt("fairlyActiveMinutes");
							int veryActiveMinutes = new JSONObject(data).GetJSONObject("summary")
								.GetInt("veryActiveMinutes");
							int activeMinutes = fairlyActiveMinutes + veryActiveMinutes;

							int steps = new JSONObject(data).GetJSONObject("summary").GetInt("steps");

							var jsonObject = new JSONObject();
							var date = DateTime.Now.ToString("yyyy-MM-dd");
							jsonObject.Put("steps", steps).Put("activity", activeMinutes).Put("dateTime", date);

							if (!Utils.CheckNetworkAvailability()) {
								await _sqlHelper.Insert(new SmartBandRecords {
									DataObject = jsonObject.ToString(), Type = "Activity"
								});
							} else {
								var recordEnumerable =
									await _sqlHelper.QueryValuations(
										"SELECT * FROM SmartBandRecords WHERE Type  = 'Activity'");
								var jsonArray = new JSONArray();
								foreach (SmartBandRecords element in recordEnumerable) {
									jsonArray.Put(new JSONObject(element.DataObject));
									await _sqlHelper.Delete(element);
								}

								jsonArray.Put(jsonObject);
								var obj = new JSONObject();
								obj.Put("imei", Utils.GetDeviceIdentificator(this))
									.Put("idClient", Utils.GetDefaults("IdClient"))
									.Put("idPersoana", Utils.GetDefaults("IdPersoana")).Put("data", jsonArray)
									.Put("latitude",
										((LocationEventArgs) args).Location.Latitude.ToString().Replace(',', '.'))
									.Put("longitude",
										((LocationEventArgs) args).Location.Longitude.ToString().Replace(',', '.'));

								string result = await WebServices.WebServices.Post(
									"/api/smartband/activity", obj,
									Utils.GetDefaults("Token"));

								if (!string.IsNullOrEmpty(result)) {
									try {
										var obj1 = new JSONObject(result);
										if (obj1.GetString("data") != "done") {
											await _sqlHelper.Insert(new SmartBandRecords {
												DataObject = jsonObject.ToString(), Type = "Activity"
											});
										} else {
											await _sqlHelper.QueryValuations(
												"DELETE FROM SmartBandRecords WHERE Type = 'Activity'");
										}
									} catch (Exception e) {
										Log.Error("SmartBand Service Error occurred", e.Message);
									}
								} else {
									try {
										await _sqlHelper.Insert(new SmartBandRecords {
											DataObject = jsonObject.ToString(), Type = "Activity"
										});
									} catch (Exception e) {
										Log.Error("SmartBand Service Error occurred", e.Message);
									}
								}
							}

							await _location.StopRequestionLocationUpdates();
						} catch (JSONException e) {
							Log.Error("SmartBand Service Error occurred", e.Message);
							StopSelf();
						}
					}
				});
			} catch (Exception e) {
				Log.Error("SmartBand Service Error occurred", e.Message);
			}

			_location.LocationRequested -= RequestedLocationForSleep;
		}

		private void GetSleepData() {
			_location.LocationRequested += RequestedLocationForSleep;
		}

		private async void SentData() {
			if (!Utils.CheckNetworkAvailability()) return;
			GetSleepData();
			GetSteps();
			await _location.StartRequestingLocation();
		}
	}
}