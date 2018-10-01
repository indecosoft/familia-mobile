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
        private UserCardAdapter adapter;

        // TODO: Rename and change types of parameters
        private CardStackView cardStackView;
        private LottieAnimationView animationView;
        private TextView lbNobody;
        private Button leftButton;
        private Button rightButton;
        public static View mView;
        List<UserCard> people;
        private readonly IWebServices _webServices = new WebServices();

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
        private UserCardAdapter createUserCardAdapter()
        {
            UserCardAdapter adapter = new UserCardAdapter(Activity);
            adapter.AddAll(people);
            return adapter;
        }
        private void setup(View view)
        {
            //progressBar = (ProgressBar)findViewById(R.id.activity_main_progress_bar);

            cardStackView = (CardStackView)view.FindViewById(Resource.Id.activity_main_card_stack_view);
            cardStackView.CardDragging += delegate(object sender, CardStackView.CardDraggingEventArgs args)
            {
                Log.Error("CardStackView", "onCardDragging");
            };
            cardStackView.CardSwiped += delegate(object sender, CardStackView.CardSwipedEventArgs args)
            {
                Log.Error("CardStackView", "onCardSwiped: " + args.Direction);
                Log.Error("CardStackView", "topIndex: " + cardStackView.TopIndex);
                if (cardStackView.TopIndex == adapter.Count - 5)
                {
                    Log.Error("CardStackView", "Paginate: " + cardStackView.TopIndex);
                    //paginate();
                }
            };
        }

        private void reload()
        {

            adapter = createUserCardAdapter();
            cardStackView.SetAdapter(adapter);
            cardStackView.Visibility = ViewStates.Visible;
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
            var response = await _webServices.Post(Constants.PUBLIC_SERVER_ADDRESS + "api/nearMe", dataToSent,
                Utils.GetDefaults("Token", Activity));
            Log.Error("Response: ", "" + response);
            try
            {
                if (response != null)
                {

                    JSONArray nearMe = new JSONArray(response);
                    people = new List<UserCard>();
                    Random r = new Random();
                    if (nearMe.Length() != 0)
                    {
                        for (int i = 0; i < nearMe.Length(); i++)
                        {
                            int imageID = r.Next(1, 10);
                            JSONObject NearMeObj = (JSONObject) (nearMe.Get(i));
                            switch (imageID)
                            {
                                case 1:
                                    people.Add(new UserCard(NearMeObj.GetString("nume"), NearMeObj.GetString("email"),
                                        "probleme de sanatate", NearMeObj.GetString("avatar"),
                                        Activity.Resources.GetDrawable(Resource.Drawable.image_1)));
                                    break;
                                case 2:
                                    people.Add(new UserCard(NearMeObj.GetString("nume"), NearMeObj.GetString("email"),
                                        "probleme de sanatate", NearMeObj.GetString("avatar"),
                                        Activity.Resources.GetDrawable(Resource.Drawable.image_2)));
                                    break;
                                case 3:
                                    people.Add(new UserCard(NearMeObj.GetString("nume"), NearMeObj.GetString("email"),
                                        "probleme de sanatate", NearMeObj.GetString("avatar"),
                                        Activity.Resources.GetDrawable(Resource.Drawable.image_3)));
                                    break;
                                case 4:
                                    people.Add(new UserCard(NearMeObj.GetString("nume"), NearMeObj.GetString("email"),
                                        "probleme de sanatate", NearMeObj.GetString("avatar"),
                                        Activity.Resources.GetDrawable(Resource.Drawable.image_4)));
                                    break;
                                case 5:
                                    people.Add(new UserCard(NearMeObj.GetString("nume"), NearMeObj.GetString("email"),
                                        "probleme de sanatate", NearMeObj.GetString("avatar"),
                                        Activity.Resources.GetDrawable(Resource.Drawable.image_5)));
                                    break;
                                case 6:
                                    people.Add(new UserCard(NearMeObj.GetString("nume"), NearMeObj.GetString("email"),
                                        "probleme de sanatate", NearMeObj.GetString("avatar"),
                                        Activity.Resources.GetDrawable(Resource.Drawable.image_6)));
                                    break;
                                case 7:
                                    people.Add(new UserCard(NearMeObj.GetString("nume"), NearMeObj.GetString("email"),
                                        "probleme de sanatate", NearMeObj.GetString("avatar"),
                                        Activity.Resources.GetDrawable(Resource.Drawable.image_7)));
                                    break;
                                case 8:
                                    people.Add(new UserCard(NearMeObj.GetString("nume"), NearMeObj.GetString("email"),
                                        "probleme de sanatate", NearMeObj.GetString("avatar"),
                                        Activity.Resources.GetDrawable(Resource.Drawable.image_8)));
                                    break;
                                case 9:
                                    people.Add(new UserCard(NearMeObj.GetString("nume"), NearMeObj.GetString("email"),
                                        "probleme de sanatate", NearMeObj.GetString("avatar"),
                                        Activity.Resources.GetDrawable(Resource.Drawable.image_9)));
                                    break;
                                case 10:
                                    people.Add(new UserCard(NearMeObj.GetString("nume"), NearMeObj.GetString("email"),
                                        "probleme de sanatate", NearMeObj.GetString("avatar"),
                                        Activity.Resources.GetDrawable(Resource.Drawable.image_10)));
                                    break;
                            }

                        }
                    }

                    setup(mView);
                    reload();

                }
                else
                {
                    //Utils.DisplayNotification(Activity, "Eroare", "A fost intampinata o eroare in timpul conectarii la server!");
                }


            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.find_users_fragment, container, false);
            cardStackView = view.FindViewById<CardStackView>(Resource.Id.activity_main_card_stack_view);
            mView = view;
            animationView = view.FindViewById<LottieAnimationView>(Resource.Id.animation_view);
            lbNobody = view.FindViewById<TextView>(Resource.Id.lbNobody);
            rightButton = view.FindViewById<Button>(Resource.Id.btnRight);
            leftButton = view.FindViewById<Button>(Resource.Id.btnLeft);

            leftButton.Click += delegate { };
            rightButton.Click += delegate { };

            animationView.AddAnimatorListener(this);
            //start annimation
            animationView.PlayAnimation();
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
            rightButton.Enabled = true;
            leftButton.Enabled = true;
            cardStackView.Enabled = true;
            SearchPeople();

        }
    }
}