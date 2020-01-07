using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Yuyakaido.Android.CardStackView;
using Familia.Location;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using Org.Json;

namespace Familia.Chat {
    public class FindUsersFragment : Android.Support.V4.App.Fragment {
        private UserCardAdapter _adapter;
        private CardStackView _cardStackView;
        private LottieAnimationView _animationView;
        private TextView _lbNobody;
        private Button _leftButton, _rightButton, _searchButton;
        private List<UserCard> _people = new List<UserCard>();
        private readonly LocationManager location = LocationManager.Instance;

        private void Setup() {

            _cardStackView.CardSwiped += async (sender, args) => {
                if (args.Direction.ToString() == "Right" && _adapter.Count != 0) {
                    await Task.Run(() => {
                        try {
                            var emailObject = new JSONObject().Put("dest", _adapter.GetItem(_cardStackView.TopIndex - 1).Email)
                                .Put("from", Utils.GetDefaults("Email"));
                            WebSocketClient.Client.Emit("chat request", emailObject);
                        } catch (Exception e) {
                            Log.Error("Chat Request Err: ", e.Message);
                        }
                    });


                    if (_cardStackView.TopIndex != _adapter.Count) return;
                    HideUI();
                } else if (_cardStackView.TopIndex == _adapter.Count) {
                    HideUI();
                }

            };
        }
        private void HideUI(string message= "Nimeni nu se afla in jurul tau") {
            _rightButton.Visibility = ViewStates.Gone;
            _leftButton.Visibility = ViewStates.Gone;
            _cardStackView.Visibility = ViewStates.Gone;
            _searchButton.Visibility = ViewStates.Visible;
            _lbNobody.Text = message;
        }
        private async Task<List<UserCard>> GetListOfPeople() {
            var list = new List<UserCard>();
            await Task.Run(async () => {
                try {
                    var dataToSent = new JSONObject().Put("id", Utils.GetDefaults("IdClient")).Put("distance", 3000);
                    var response = await WebServices.Post(Constants.PublicServerAddress + "/api/nearMe", dataToSent, Utils.GetDefaults("Token"));
                    if (response != null) {
                        var nearMe = new JSONArray(response);
                        for (var i = 0; i < nearMe.Length(); i++) {
                            var nearMeObj = (JSONObject)nearMe.Get(i);
                            list.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                string.Empty, nearMeObj.GetString("avatar"),
                                Resource.Drawable.card));
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
                    HideUI("Nu se poate realiza conexiunea la server");
                } else if (_people.Count > 0) {
                    _rightButton.Visibility = ViewStates.Visible;
                    _leftButton.Visibility = ViewStates.Visible;
                    _cardStackView.Visibility = ViewStates.Visible;
                    _searchButton.Visibility = ViewStates.Gone;
                    _adapter.Clear();
                    _adapter.AddAll(_people);
                    _lbNobody.Text = string.Empty;
                    _adapter.NotifyDataSetChanged();
                } else {
                    HideUI();
                }

            } catch (Exception ex) {
                Log.Error("Error occurred in " + nameof(FindUsersFragment), ex.Message);
                HideUI("Nu se poate realiza conexiunea la server");
            }
        }

        public override void OnStart() {
            base.OnStart();
            Log.Debug("NearMe Start", "Now");
            _ = location.StopRequestionLocationUpdates();
        }
        public override void OnPause() {
            Log.Debug("NearMe Pause", "Now");
            _ = location.StopRequestionLocationUpdates();
            _ = location.StartRequestingLocation();
            base.OnPause();

        }
        public override void OnResume() {
            _ = location.StartRequestingLocation(1000 * 60 * 5);
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
            var view = inflater.Inflate(Resource.Layout.find_users_fragment, container, false);
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
                _cardStackView.Visibility = ViewStates.Visible;
                _searchButton.Visibility = ViewStates.Gone;
                _lbNobody.Text = "Cautare";
                _ = SearchPeople();
            };
            _lbNobody.Text = string.Empty;
            _leftButton.Click += delegate { SwipeLeft(); };
            _rightButton.Click += delegate { SwipeRight(); };
            _ = SearchPeople();
            return view;
        }
        private void SwipeLeft() {
            var spots = ExtractRemainingUserCards();
            if (spots.Count == 0) {
                return;
            }
            View target = _cardStackView.TopView;
            View targetOverlay = _cardStackView.TopView.OverlayContainer;

            ValueAnimator rotation = ObjectAnimator.OfPropertyValuesHolder(
                    target, PropertyValuesHolder.OfFloat("rotation", -10f));
            rotation.SetDuration(200);
            ValueAnimator translateX = ObjectAnimator.OfPropertyValuesHolder(
                    target, PropertyValuesHolder.OfFloat("translationX", 0f, -2000f));
            ValueAnimator translateY = ObjectAnimator.OfPropertyValuesHolder(
                    target, PropertyValuesHolder.OfFloat("translationY", 0f, 500f));
            translateX.StartDelay = 100;
            translateY.StartDelay = 100;
            translateX.SetDuration(500);
            translateY.SetDuration(500);
            AnimatorSet cardAnimationSet = new AnimatorSet();
            cardAnimationSet.PlayTogether(rotation, translateX, translateY);

            ObjectAnimator overlayAnimator = ObjectAnimator.OfFloat(targetOverlay, "alpha", 0f, 1f);
            overlayAnimator.SetDuration(200);
            AnimatorSet overlayAnimationSet = new AnimatorSet();
            overlayAnimationSet.PlayTogether(overlayAnimator);

            _cardStackView.Swipe(SwipeDirection.Left, cardAnimationSet);
        }

        private void SwipeRight() {
            var spots = ExtractRemainingUserCards();
            if (spots.Count == 0) {
                return;
            }
            View target = _cardStackView.TopView;
            View targetOverlay = _cardStackView.TopView.OverlayContainer;

            ValueAnimator rotation = ObjectAnimator.OfPropertyValuesHolder(
                    target, PropertyValuesHolder.OfFloat("rotation", 10f));
            rotation.SetDuration(200);
            ValueAnimator translateX = ObjectAnimator.OfPropertyValuesHolder(
                    target, PropertyValuesHolder.OfFloat("translationX", 0f, 2000f));
            ValueAnimator translateY = ObjectAnimator.OfPropertyValuesHolder(
                    target, PropertyValuesHolder.OfFloat("translationY", 0f, 500f));
            translateX.StartDelay = 100;
            translateY.StartDelay = 100;
            translateX.SetDuration(500);
            translateY.SetDuration(500);
            AnimatorSet cardAnimationSet = new AnimatorSet();
            cardAnimationSet.PlayTogether(rotation, translateX, translateY);

            ObjectAnimator overlayAnimator = ObjectAnimator.OfFloat(targetOverlay, "alpha", 0f, 1f);
            overlayAnimator.SetDuration(200);
            AnimatorSet overlayAnimationSet = new AnimatorSet();
            overlayAnimationSet.PlayTogether(overlayAnimator);

            _cardStackView.Swipe(SwipeDirection.Right, cardAnimationSet);
        }
        private List<UserCard> ExtractRemainingUserCards() {
            List<UserCard> spots = new List<UserCard>();
            for (int i = _cardStackView.TopIndex; i < _adapter.Count; i++) {
                spots.Add(_adapter.GetItem(i));
            }
            return spots;
        }
    }
}