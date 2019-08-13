using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Animation;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Yuyakaido.Android.CardStackView;
using FamiliaXamarin;
using FamiliaXamarin.Helpers;
using Java.Security.Interfaces;
using Org.Json;

namespace Familia.Chat
{
    public class FindUsersFragment : Android.Support.V4.App.Fragment
    {
        private UserCardAdapter _adapter;
        private CardStackView _cardStackView;
        private LottieAnimationView _animationView;
        private TextView _lbNobody;
        private Button _leftButton;
        private Button _rightButton;
        private Button _searchButton;
        private List<UserCard> _people = new List<UserCard>();

        private void Setup()
        {

            _cardStackView.CardSwiped += async (sender, args) =>
            {
                if (args.Direction.ToString() == "Right" && _adapter.Count != 0)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            var emailFrom = Utils.GetDefaults("Email");

                            var emailObject = new JSONObject().Put("dest", _adapter.GetItem(_cardStackView.TopIndex - 1).Email)
                                .Put("from", emailFrom);
                            WebSocketClient.Client.Emit("chat request", emailObject);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Chat Request Err: ", e.Message);
                        }
                    });


                    if (_cardStackView.TopIndex != _adapter.Count) return;
                    _rightButton.Visibility = ViewStates.Gone;
                    _leftButton.Visibility = ViewStates.Gone;
                    _cardStackView.Visibility = ViewStates.Gone;
                    _searchButton.Visibility = ViewStates.Visible;
                    _lbNobody.Text = "Nimeni nu se afla in jurul tau";
                }
                else if (_cardStackView.TopIndex == _adapter.Count)
                {
                    _rightButton.Visibility = ViewStates.Gone;
                    _leftButton.Visibility = ViewStates.Gone;
                    _cardStackView.Visibility = ViewStates.Gone;
                    _searchButton.Visibility = ViewStates.Visible;
                    _lbNobody.Text = "Nimeni nu se afla in jurul tau";
                }

            };
        }

        private async Task<List<UserCard>> GetListOfPeople()
        {
            var list = new List<UserCard>();
            await Task.Run(async () =>
            {
                try
                {
                    var dataToSent = new JSONObject().Put("id", Utils.GetDefaults("IdClient")).Put("distance", 3000);
                    var response = await WebServices.Post(Constants.PublicServerAddress + "/api/nearMe", dataToSent, Utils.GetDefaults("Token"));
                    Log.Error("SA VINA RASPUNSURILEEEEE: ", response);

                    if (response != null)
                    {
                        var nearMe = new JSONArray(response);
                        for (var i = 0; i < nearMe.Length(); i++)
                        {
                            var nearMeObj = (JSONObject)nearMe.Get(i);


                            list.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                string.Empty, nearMeObj.GetString("avatar"),
                                Resource.Drawable.card));
                            Log.Error("FORULETUL DE PE NEARME: ", "Ma bucur ca am inserat");

                        }
                    }
                    
                }
                catch (Exception e)
                {
                    Log.Error("Error occurred in " + nameof(FindUsersFragment), e.Message);
                    list = null;
                }
                

            });

            return list;
        }

        private async void SearchPeople()
        {
            try
            {
                _people = await GetListOfPeople();

                Log.Error("Debug Log From " + nameof(FindUsersFragment), "CurrentList Count " + _people.Count);
                if (_people == null)
                {
                    _rightButton.Visibility = ViewStates.Gone;
                    _leftButton.Visibility = ViewStates.Gone;
                    _cardStackView.Visibility = ViewStates.Gone;
                    _searchButton.Visibility = ViewStates.Visible;
                    _lbNobody.Text = "Nu se poate realiza conexiunea la server";
                } else if (_people.Count > 0)
                {
                    _rightButton.Visibility = ViewStates.Visible;
                    _leftButton.Visibility = ViewStates.Visible;
                    _cardStackView.Visibility = ViewStates.Visible;
                    _searchButton.Visibility = ViewStates.Gone;
                    _adapter.Clear();
                    _adapter.AddAll(_people);
                    _lbNobody.Text = string.Empty;
                    _adapter.NotifyDataSetChanged();
                }
                else
                {
                    _rightButton.Visibility = ViewStates.Gone;
                    _leftButton.Visibility = ViewStates.Gone;
                    _cardStackView.Visibility = ViewStates.Gone;
                    _searchButton.Visibility = ViewStates.Visible;
                    _lbNobody.Text = "Nimeni nu se afla in jurul tau";
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error occurred in " + nameof(FindUsersFragment), ex.Message);
                _lbNobody.Text = "Nu se poate realiza conexiunea la server";
            }
        }

        private void InitUi(View view)
        {
            _cardStackView = view.FindViewById<CardStackView>(Resource.Id.activity_main_card_stack_view);
            _animationView = view.FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            _lbNobody = view.FindViewById<TextView>(Resource.Id.lbNobody);
            _rightButton = view.FindViewById<Button>(Resource.Id.btnRight);
            _leftButton = view.FindViewById<Button>(Resource.Id.btnLeft);
            _searchButton = view.FindViewById<Button>(Resource.Id.searchButton);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.find_users_fragment, container, false);

            InitUi(view);
            Setup();
            //start annimation
            _animationView.PlayAnimation();
            _adapter = new UserCardAdapter(Activity, _people);
            _cardStackView.SetAdapter(_adapter);
            _searchButton.Visibility = ViewStates.Gone;
            _searchButton.Click += (sender, args) =>
           {
               _rightButton.Visibility = ViewStates.Visible;
               _leftButton.Visibility = ViewStates.Visible;
               _cardStackView.Visibility = ViewStates.Visible;
               _searchButton.Visibility = ViewStates.Gone;
               _lbNobody.Text = "Cautare";
               SearchPeople();
           };
            _lbNobody.Text = string.Empty;
            SearchPeople();

            _leftButton.Click += delegate { SwipeLeft(); };
            _rightButton.Click += delegate { SwipeRight(); };

            return view;
        }
        private void SwipeLeft()
        {
            var spots = ExtractRemainingUserCards();
            if (spots.Count == 0)
            {
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

        private void SwipeRight()
        {
            var spots = ExtractRemainingUserCards();
            if (spots.Count == 0)
            {
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
        private List<UserCard> ExtractRemainingUserCards()
        {
            List<UserCard> spots = new List<UserCard>();
            for (int i = _cardStackView.TopIndex; i < _adapter.Count; i++)
            {
                spots.Add(_adapter.GetItem(i));
            }
            return spots;
        }
    }
}