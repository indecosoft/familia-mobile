using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Animation;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Yuyakaido.Android.CardStackView;
using Familia.Helpers;
using Familia.Location;
using Familia.WebSocket;
using Org.Json;

namespace Familia.Chat {
	public class FindUsersFragment : Fragment {
		private UserCardAdapter _adapter;
		private CardStackView _cardStackView;
		private LottieAnimationView _animationView;
		private TextView _lbNobody;
		private Button _leftButton, _rightButton, _searchButton;
		private List<UserCard> _people = new List<UserCard>();
		private readonly LocationManager _location = LocationManager.Instance;
		private bool _swipeFinished = true;

		private void Setup() {
			_cardStackView.CardSwiped += async (sender, args) => {
				if (_adapter.Count == 0) {
					HideUi();
					return;
				}

				var el = _adapter.GetItem(_cardStackView.TopIndex - 1);
				_adapter.Remove(_adapter.GetItem(_cardStackView.TopIndex - 1));
				_adapter.NotifyDataSetChanged();
				Log.Error("Count ", _adapter.Count + "");
				if (args.Direction.ToString() == "Right") {
					await Task.Run(() => {
						try {
							JSONObject emailObject = new JSONObject().Put("dest", el.Email)
								.Put("from", Utils.GetDefaults("Email"));
							WebSocketClient.Client.Emit("chat request", emailObject);
						} catch (Exception e) {
							Log.Error("Chat Request Err: ", e.Message);
						}
					});
					//
					
				} else if (_cardStackView.TopIndex == _adapter.Count) {
					HideUi();
				}

				_swipeFinished = true;
				if (_cardStackView.TopIndex != _adapter.Count) return;
				HideUi();
			};
		}

		private void HideUi(string message = "Nimeni nu se afla in jurul tau") {
			_rightButton.Visibility = ViewStates.Gone;
			_leftButton.Visibility = ViewStates.Gone;
			_searchButton.Visibility = ViewStates.Visible;
			_lbNobody.Text = message;
		}

		private async Task<List<UserCard>> GetListOfPeople() {
			var list = new List<UserCard>();
			await Task.Run(async () => {
				try {
					JSONObject dataToSent = new JSONObject().Put("id", Utils.GetDefaults("Id")).Put("distance", 3000);
					string response = await WebServices.WebServices.Post(Constants.PublicServerAddress + "/api/nearMe",
						dataToSent, Utils.GetDefaults("Token"));
					Log.Error("FindUsers", response + "");
					if (response != null) {
						var nearMe = new JSONArray(response);
						for (var i = 0; i < nearMe.Length(); i++) {
							var nearMeObj = (JSONObject) nearMe.Get(i);
							list.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
								string.Empty, nearMeObj.GetString("avatar"), Resource.Drawable.card));
						}
					}
				} catch (Exception e) {
					Log.Error("Error occurred in " + nameof(FindUsersFragment), e.Message);
					list = null;
				}
			});
			return list;
		}

		private async Task SearchPeople() {
			try {
				_people = await GetListOfPeople();

				Log.Error("Debug Log From " + nameof(FindUsersFragment), "CurrentList Count " + _people.Count);
				if (_people == null) {
					HideUi("Nu se poate realiza conexiunea la server");
				} else if (_people.Count > 0) {
					_rightButton.Visibility = ViewStates.Visible;
					_leftButton.Visibility = ViewStates.Visible;
					_searchButton.Visibility = ViewStates.Gone;
					_adapter.Clear();
					_adapter.AddAll(_people);
					_lbNobody.Text = string.Empty;
					_adapter.NotifyDataSetChanged();
				} else {
					HideUi();
				}
			} catch (Exception ex) {
				Log.Error("Error occurred in " + nameof(FindUsersFragment), ex.Message);
				HideUi("Nu se poate realiza conexiunea la server");
			}
		}

		public override void OnResume() {
			Log.Debug("NearMe Resume", "Now");
			// Task.Run(async () => await _location.StartRequestingLocation());
			_location.ChangeInterval();
			base.OnResume();
		}

		private void InitUi(View view) {
			_cardStackView = view.FindViewById<CardStackView>(Resource.Id.activity_main_card_stack_view);
			_animationView = view.FindViewById<LottieAnimationView>(Resource.Id.animation_view);
			_lbNobody = view.FindViewById<TextView>(Resource.Id.lbNobody);
			_rightButton = view.FindViewById<Button>(Resource.Id.btnRight);
			_leftButton = view.FindViewById<Button>(Resource.Id.btnLeft);
			_searchButton = view.FindViewById<Button>(Resource.Id.searchButton);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			View view = inflater.Inflate(Resource.Layout.find_users_fragment, container, false);
			InitUi(view);
			Setup();
			//start annimation
			_animationView.PlayAnimation();
			_adapter = new UserCardAdapter(Activity, _people);
			_cardStackView.SetAdapter(_adapter);
			_searchButton.Visibility = ViewStates.Gone;
			_searchButton.Click += (sender, args) => {
				_rightButton.Visibility = ViewStates.Visible;
				_leftButton.Visibility = ViewStates.Visible;
				_searchButton.Visibility = ViewStates.Gone;
				_lbNobody.Text = "Cautare";
				_location.LocationRequested += OnLocationRequested;
				_location.ChangeInterval();
			};
			_lbNobody.Text = string.Empty;
			_leftButton.Click += (sender, args) => SwipeLeft();
			_rightButton.Click += (sender, args) => SwipeRight();

			_location.LocationRequested += OnLocationRequested;
			return view;
		}

		private void OnLocationRequested(object source, EventArgs args) {
			_location.LocationRequested -= OnLocationRequested;
			_ = SearchPeople();
		}

		private void SwipeLeft() {
			try {
				if (!_swipeFinished || _adapter.Count == 0) return;
				_swipeFinished = false;
				View target = _cardStackView.TopView;
				View targetOverlay = _cardStackView.TopView.OverlayContainer;

				ValueAnimator rotation = ObjectAnimator.OfPropertyValuesHolder(
					target, PropertyValuesHolder.OfFloat("rotation", -10f));
				rotation.SetDuration(200);
				ValueAnimator translateX = ObjectAnimator.OfPropertyValuesHolder(target,
					PropertyValuesHolder.OfFloat("translationX", 0f, -2000f));
				ValueAnimator translateY = ObjectAnimator.OfPropertyValuesHolder(target,
					PropertyValuesHolder.OfFloat("translationY", 0f, 500f));
				translateX.StartDelay = 100;
				translateY.StartDelay = 100;
				translateX.SetDuration(500);
				translateY.SetDuration(500);
				var cardAnimationSet = new AnimatorSet();
				cardAnimationSet.PlayTogether(rotation, translateX, translateY);

				ObjectAnimator overlayAnimator = ObjectAnimator.OfFloat(targetOverlay, "alpha", 0f, 1f);
				overlayAnimator.SetDuration(200);
				var overlayAnimationSet = new AnimatorSet();
				overlayAnimationSet.PlayTogether(overlayAnimator);

				_cardStackView.Swipe(SwipeDirection.Left, cardAnimationSet);
			} catch (Exception e) {
				Console.WriteLine(e);
				throw;
			}
		}
		


		private void SwipeRight() {
			try {
				if (!_swipeFinished || _adapter.Count == 0) return;
				_swipeFinished = false;
				View target = _cardStackView.TopView;
				View targetOverlay = _cardStackView.TopView.OverlayContainer;

				ValueAnimator rotation = ObjectAnimator.OfPropertyValuesHolder(
					target, PropertyValuesHolder.OfFloat("rotation", 10f));
				rotation.SetDuration(200);
				ValueAnimator translateX = ObjectAnimator.OfPropertyValuesHolder(target,
					PropertyValuesHolder.OfFloat("translationX", 0f, 2000f));
				ValueAnimator translateY = ObjectAnimator.OfPropertyValuesHolder(target,
					PropertyValuesHolder.OfFloat("translationY", 0f, 500f));
				translateX.StartDelay = 100;
				translateY.StartDelay = 100;
				translateX.SetDuration(500);
				translateY.SetDuration(500);
				var cardAnimationSet = new AnimatorSet();
				cardAnimationSet.PlayTogether(rotation, translateX, translateY);

				ObjectAnimator overlayAnimator = ObjectAnimator.OfFloat(targetOverlay, "alpha", 0f, 1f);
				overlayAnimator.SetDuration(200);
				var overlayAnimationSet = new AnimatorSet();
				overlayAnimationSet.PlayTogether(overlayAnimator);

				_cardStackView.Swipe(SwipeDirection.Right, cardAnimationSet);
			} catch (Exception e) {
				Console.WriteLine(e);
				throw;
			}
		}

		private List<UserCard> ExtractRemainingUserCards() {
			var spots = new List<UserCard>();
			for (int i = _cardStackView.TopIndex; i < _adapter.Count; i++) {
				spots.Add(_adapter.GetItem(i));
			}

			return spots;
		}
	}
}