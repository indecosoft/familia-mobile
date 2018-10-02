using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Com.Yuyakaido.Android.CardStackView;
using Java.Lang;
using Org.Json;

namespace FamiliaXamarin
{
    public class FindUsersFragment : Android.Support.V4.App.Fragment, Animator.IAnimatorListener
    {
        private UserCardAdapter _adapter;

        // TODO: Rename and change types of parameters
        private CardStackView _cardStackView;
        private LottieAnimationView _animationView;
        private TextView _lbNobody;
        private Button _leftButton;
        private Button _rightButton;
        private View _mView;
        private List<UserCard> _people;
        private readonly IWebServices _webServices = new WebServices();
        private readonly IWebSocketClient _webSocket = new WebSocketClient();

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        //        private List<UserCard> createTouristSpots()
        //        {
        //            List<UserCard> spots = new List<UserCard>();
        //            spots.Add(new UserCard("Yasaka Shrine", "Kyoto", "https://source.unsplash.com/Xq1ntWruZQI/600x800"));
        //            spots.Add(new UserCard("Fushimi Inari Shrine", "Kyoto", "https://source.unsplash.com/NYyCqdBOKwc/600x800"));
        //            spots.Add(new UserCard("Bamboo Forest", "Kyoto", "https://source.unsplash.com/buF62ewDLcQ/600x800"));
        //            spots.Add(new UserCard("Brooklyn Bridge", "New York", "https://source.unsplash.com/THozNzxEP3g/600x800"));
        //            spots.Add(new UserCard("Empire State Building", "New York",
        //                "https://source.unsplash.com/USrZRcRS2Lw/600x800"));
        //            spots.Add(new UserCard("The statue of Liberty", "New York",
        //                "https://source.unsplash.com/PeFk7fzxTdk/600x800"));
        //            spots.Add(new UserCard("Louvre Museum", "Paris", "https://source.unsplash.com/LrMWHKqilUw/600x800"));
        //            spots.Add(new UserCard("Eiffel Tower", "Paris", "https://source.unsplash.com/HN-5Z6AmxrM/600x800"));
        //            spots.Add(new UserCard("Big Ben", "London", "https://source.unsplash.com/CdVAUAddqEc/600x800"));
        //            spots.Add(new UserCard("Great Wall of China", "China", "https://source.unsplash.com/AWh9C-QjhE4/600x800"));
        //            return spots;
        //        }

        //        private UserCardAdapter createTouristSpotCardAdapter()
        //        {
        //            UserCardAdapter adapter = new UserCardAdapter(Application.Context);
        //            adapter.AddAll(createTouristSpots());
        //            return adapter;
        //        }
        private UserCardAdapter CreateUserCardAdapter()
        {
            var userCardAdapter = new UserCardAdapter(Activity);
            userCardAdapter.AddAll(_people);
            return userCardAdapter;
        }
        private void Setup(View view)
        {
            //progressBar = (ProgressBar)findViewById(R.id.activity_main_progress_bar);

            _cardStackView = (CardStackView)view.FindViewById(Resource.Id.activity_main_card_stack_view);
            _cardStackView.CardDragging += delegate
            {
                Log.Error("CardStackView", "onCardDragging");
            };
            _cardStackView.CardSwiped += delegate(object sender, CardStackView.CardSwipedEventArgs args)
            {
                Log.Error("CardStackView", $"onCardSwiped: {args.Direction}");
                Log.Error("CardStackView", $"topIndex: {_cardStackView.TopIndex}");
                if (_cardStackView.TopIndex == _adapter.Count - 5)
                {
                    Log.Error("CardStackView", "Paginate: " + _cardStackView.TopIndex);
                    //paginate();
                }

                if (args.Direction.ToString() == "Right")
                {
                    try
                    {
                        string EmailFrom = Utils.GetDefaults("Email", Activity);
                        Activity.StartActivity(typeof(ChatActivity));
//                        Chat.EmailDest = _people[_cardStackView.TopIndex - 1].Email;
//                        Chat.Avatar = _people[_cardStackView.TopIndex - 1].Avatar;
                        var emailObject = new JSONObject().Put("dest", _people[_cardStackView.TopIndex - 1].Email).Put("from", EmailFrom);
                        WebSocketClient.Client.Emit("chat request", emailObject);
                    }
                    catch (JSONException e)
                    {
                        e.PrintStackTrace();
                    }
                    
                    //startActivity(new Intent(getActivity(), Chat.class));
                }

                if (_cardStackView.TopIndex != _people.Count) return;
                _rightButton.Enabled = false;
                _leftButton.Enabled = false;
                _cardStackView.Enabled = false;
                _lbNobody.Text = "Nimeni nu se afla in jurul tau";
                _animationView.PlayAnimation();

            };
        }

        private void Reload()
        {

            _adapter = CreateUserCardAdapter();
            _cardStackView.SetAdapter(_adapter);
            _cardStackView.Visibility = ViewStates.Visible;
        }

        private async void SearchPeople()
        {

            //search for people
            //        GetLocation location = new GetLocation(Activity);
            //        //location.getLocation();
            //        location.startGetLocation(5000);
            //end animation
            //            Handler handler = new Handler();
            //            handler.postDelayed(new Runnable() {
            //                @Override
            //                public void run()
            //                {
            //                animationView.cancelAnimation();
            //            }
            //            }, 5000);
            // Acquire a reference to the system Location Manager

            var dataToSent = new JSONObject().Put("email", Utils.GetDefaults("Email", Activity));
            var response = await _webServices.Post(Constants.PublicServerAddress + "api/nearMe", dataToSent,
                Utils.GetDefaults("Token", Activity));
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
                            var nearMeObj = (JSONObject) (nearMe.Get(i));
                            switch (imageId)
                            {
                                case 1:
                                    _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                        "probleme de sanatate", nearMeObj.GetString("avatar"),
                                        Resource.Drawable.image_1));
                                    break;
                                case 2:
                                    _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                        "probleme de sanatate", nearMeObj.GetString("avatar"),
                                        Resource.Drawable.image_2));
                                    break;
                                case 3:
                                    _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                        "probleme de sanatate", nearMeObj.GetString("avatar"),
                                        Resource.Drawable.image_3));
                                    break;
                                case 4:
                                    _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                        "probleme de sanatate", nearMeObj.GetString("avatar"),
                                        Resource.Drawable.image_4));
                                    break;
                                case 5:
                                    _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                        "probleme de sanatate", nearMeObj.GetString("avatar"),
                                        Resource.Drawable.image_5));
                                    break;
                                case 6:
                                    _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                        "probleme de sanatate", nearMeObj.GetString("avatar"),
                                        Resource.Drawable.image_6));
                                    break;
                                case 7:
                                    _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                        "probleme de sanatate", nearMeObj.GetString("avatar"),
                                        Resource.Drawable.image_7));
                                    break;
                                case 8:
                                    _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                        "probleme de sanatate", nearMeObj.GetString("avatar"),
                                        Resource.Drawable.image_8));
                                    break;
                                case 9:
                                    _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                        "probleme de sanatate", nearMeObj.GetString("avatar"),
                                        Resource.Drawable.image_9));
                                    break;
                                case 10:
                                    _people.Add(new UserCard(nearMeObj.GetString("nume"), nearMeObj.GetString("email"),
                                        "probleme de sanatate", nearMeObj.GetString("avatar"),
                                        Resource.Drawable.image_10));
                                    break;
                            }

                        }
                    }

                    Setup(_mView);
                    Reload();

                }
                else
                {
                    _lbNobody.Text = "A fost intampinata o eroare in timpul conectarii la server!";
                    //Utils.DisplayNotification(Activity, "Eroare", "A fost intampinata o eroare in timpul conectarii la server!");
                }


            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
            if(_animationView.IsAnimating)
                _animationView.CancelAnimation();
        }

        private void InitUi(View view)
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
            
            _mView = view;
            InitUi(view);
            _animationView.AddAnimatorListener(this);
            //start annimation
            _animationView.PlayAnimation();
            

            _leftButton.Click += delegate { };
            _rightButton.Click += delegate { };

            
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
    }
}