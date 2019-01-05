using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Animation;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Yuyakaido.Android.CardStackView;
using FamiliaXamarin.Helpers;
using Org.Json;

namespace FamiliaXamarin.Chat
{
    public class FindUsersFragment : Android.Support.V4.App.Fragment, Animator.IAnimatorListener
    {
        private UserCardAdapter _adapter;
        private CardStackView _cardStackView;
        private LottieAnimationView _animationView;
        private TextView _lbNobody;
        private Button _leftButton;
        private Button _rightButton;
        private List<UserCard> _people;

        UserCardAdapter CreateUserCardAdapter()
        {
            var userCardAdapter = new UserCardAdapter(Activity);
            userCardAdapter.AddAll(_people);
            return userCardAdapter;
        }
        private void Setup() => _cardStackView.CardSwiped += delegate (object sender, CardStackView.CardSwipedEventArgs args)
                      {
                          if (args.Direction.ToString() == "Right" && _people.Count != 0)
                          {
                              try
                              {
                                  var emailFrom = Utils.GetDefaults("Email", Activity);

                                  var emailObject = new JSONObject().Put("dest", _people[_cardStackView.TopIndex - 1].Email).Put("from", emailFrom);
                                  WebSocketClient.Client.Emit("chat request", emailObject);
                              }
                              catch (Exception e)
                              {
                                  Console.WriteLine(e.Message);
                              }

                          }
                          else if (_people.Count == 0)
                          {
                              _rightButton.Enabled = false;
                              _leftButton.Enabled = false;
                              _cardStackView.Enabled = false;
                              _lbNobody.Text = "Nimeni nu se afla in jurul tau";
                              _cardStackView.SetAdapter(_adapter);
                              _animationView.PlayAnimation();
                          }

                          if (_cardStackView.TopIndex != _people.Count) return;
                          _rightButton.Enabled = false;
                          _leftButton.Enabled = false;
                          _cardStackView.Enabled = false;
                          _lbNobody.Text = "Nimeni nu se afla in jurul tau";
                          _cardStackView.SetAdapter(_adapter);
                          _animationView.PlayAnimation();

                      };

        private void Reload()
        {
            _adapter = CreateUserCardAdapter();
            _cardStackView.SetAdapter(_adapter);
            _cardStackView.Visibility = ViewStates.Visible;
        }

        private async void SearchPeople()
        {
            await Task.Run(async () =>
            {
                var dataToSent = new JSONObject().Put("id", Utils.GetDefaults("IdClient", Activity)).Put("distance", 3000);
                var response = await WebServices.Post(Constants.PublicServerAddress + "/api/nearMe", dataToSent, Utils.GetDefaults("Token", Activity));
                Log.Error("Response: ", "" + response);
                try
                {
                    if (response != null)
                    {

                        var nearMe = new JSONArray(response);
                        _people = new List<UserCard>();
                        var r = new Random();
                        if (nearMe.Length() != 0)
                        {
                            for (var i = 0; i < nearMe.Length(); i++)
                            {
                                var imageId = r.Next(1, 10);
                                var nearMeObj = (JSONObject)(nearMe.Get(i));
                                switch (imageId)
                                {
                                    case 1:
                                        _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                            string.Empty, nearMeObj.GetString("avatar"),
                                            Resource.Drawable.image_1));
                                        break;
                                    case 2:
                                        _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                            string.Empty, nearMeObj.GetString("avatar"),
                                            Resource.Drawable.image_2));
                                        break;
                                    case 3:
                                        _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                            string.Empty, nearMeObj.GetString("avatar"),
                                            Resource.Drawable.image_3));
                                        break;
                                    case 4:
                                        _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                            string.Empty, nearMeObj.GetString("avatar"),
                                            Resource.Drawable.image_4));
                                        break;
                                    case 5:
                                        _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                            string.Empty, nearMeObj.GetString("avatar"),
                                            Resource.Drawable.image_5));
                                        break;
                                    case 6:
                                        _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                            string.Empty, nearMeObj.GetString("avatar"),
                                            Resource.Drawable.image_6));
                                        break;
                                    case 7:
                                        _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                            string.Empty, nearMeObj.GetString("avatar"),
                                            Resource.Drawable.image_7));
                                        break;
                                    case 8:
                                        _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                            string.Empty, nearMeObj.GetString("avatar"),
                                            Resource.Drawable.image_8));
                                        break;
                                    case 9:
                                        _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                            string.Empty, nearMeObj.GetString("avatar"),
                                            Resource.Drawable.image_9));
                                        break;
                                    case 10:
                                        _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                            string.Empty, nearMeObj.GetString("avatar"),
                                            Resource.Drawable.image_10));
                                        break;
                                }
                            }
                        }

                        Activity.RunOnUiThread(Reload);
                        if (_people.Count == 0)
                        {
                            
                            Activity.RunOnUiThread(() =>
                            {
                                _animationView.PlayAnimation();
                                _lbNobody.Text = "Nimeni nu se afla in jurul tau";
                            });

                            
                        }

                    }
                    else
                    {
                        Activity.RunOnUiThread(() => { _lbNobody.Text = "A fost intampinata o eroare in timpul conectarii la server!"; });

                        //Utils.DisplayNotification(Activity, "Eroare", "A fost intampinata o eroare in timpul conectarii la server!");
                    }


                }
                catch (JSONException e)
                {
                    e.PrintStackTrace();
                }
            });
            _animationView.Progress = 1f;


            if (_animationView.IsAnimating)
                _animationView.CancelAnimation();
        }

        void InitUi(View view)
        {
            _cardStackView = view.FindViewById<CardStackView>(Resource.Id.activity_main_card_stack_view);
            _animationView = view.FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            _lbNobody = view.FindViewById<TextView>(Resource.Id.lbNobody);
            _rightButton = view.FindViewById<Button>(Resource.Id.btnRight);
            _leftButton = view.FindViewById<Button>(Resource.Id.btnLeft);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.find_users_fragment, container, false);
            InitUi(view);
            Setup();
            _animationView.AddAnimatorListener(this);
            //start annimation
            _animationView.PlayAnimation();


            _leftButton.Click += delegate { SwipeLeft(); };
            _rightButton.Click += delegate { SwipeRight(); };

            return view;
        }

        public void OnAnimationCancel(Animator animation)
        {
            //throw new NotImplementedException();
        }

        public void OnAnimationEnd(Animator animation)
        {
            //throw new NotImplementedException();
        }

        public void OnAnimationRepeat(Animator animation)
        {
            //throw new NotImplementedException();
        }

        public void OnAnimationStart(Animator animation)
        {  
                _rightButton.Enabled = true;
                _leftButton.Enabled = true;
                _cardStackView.Enabled = true;
                _lbNobody.Text = string.Empty;
   
            SearchPeople();

        }
        private void SwipeLeft()
        {
            List<UserCard> spots = ExtractRemainingUserCards();
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
            List<UserCard> spots = ExtractRemainingUserCards();
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