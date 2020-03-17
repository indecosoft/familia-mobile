using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Airbnb.Lottie;
using Familia.Games.entities;
using Familia.Helpers;
using System;
using System.Collections.Generic;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Games
{
    [Activity(Label = "GameCenterActivity", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class GameCenterActivity : AppCompatActivity, View.IOnClickListener
    {

        private static string LOG_TAG = "GameCenterActivity";
        private LottieAnimationView emptyAnimation;
        private TextView tvEmpty;
        private ConstraintLayout constraintLayout;

        private static List<Category> categories = new List<Category>() {
            new Category(1, "category1"),
            new Category(2, "category2"),
            new Category(3, "category3"),
            new Category(4, "category4")
        };
        private List<Game> games = new List<Game>() {
            new Game("Planetele Vesele", 1, new List<Category>(){ categories[0], categories[2] }),
            new Game("Joc 2", 2, new List<Category>(){ new Category(4, "category4")})
        };

        private HashSet<Game> list = new HashSet<Game>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_game_center);
            SetToolbar();

            FindViewById<CardView>(Resource.Id.cw_game1).SetOnClickListener(this);
            FindViewById<CardView>(Resource.Id.cw_game2).SetOnClickListener(this);

            emptyAnimation = FindViewById<LottieAnimationView>(Resource.Id.animation_empty_box);
            tvEmpty = FindViewById<TextView>(Resource.Id.tv_empty_games);
            constraintLayout = FindViewById<ConstraintLayout>(Resource.Id.cl_cw);

            selectGamesFromCategories();

        }

        private void selectGamesFromCategories()
        {
            if (categories != null && categories.Count != 0)
            {
                foreach (var game in games)
                {
                    foreach (var gameCategory in game.Categories)
                    {
                        foreach (var category in categories)
                        {
                            if (gameCategory.Id == category.Id)
                            {
                                list.Add(game);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (list.Count != 0)
            {
                emptyAnimation.Visibility = ViewStates.Invisible;
                tvEmpty.Visibility = ViewStates.Gone;
                constraintLayout.Visibility = ViewStates.Visible;
            }
            else
            {
                emptyAnimation.Visibility = ViewStates.Visible;
                tvEmpty.Visibility = ViewStates.Visible;
                constraintLayout.Visibility = ViewStates.Gone;
            }

            showGamesCards();
        }

        private void showGamesCards()
        {
            // this method will be modified to handle a list of games with a list of cardviews
            bool isDisplayedCW1 = false;
            bool isDisplayedCW2 = false;
            foreach (var item in list)
            {
                if (item.Type == 1)
                {
                    isDisplayedCW1 = true;
                }
                if (item.Type == 2)
                {
                    isDisplayedCW2 = true;
                }
            }

            if (!isDisplayedCW1)
            {
                FindViewById<CardView>(Resource.Id.cw_game1).Visibility = ViewStates.Gone;
            }

            if (!isDisplayedCW2)
            {
                FindViewById<CardView>(Resource.Id.cw_game2).Visibility = ViewStates.Gone;
            }
        }

        public void OnClick(View v)
        {
            var intent = new Intent(this, typeof(GameActivity));
            switch (v.Id)
            {
                case Resource.Id.cw_game1:
                    intent.PutExtra("Game", games[0].Type);
                    StartActivity(intent);
                    break;
                case Resource.Id.cw_game2:
                    intent.PutExtra("Game", games[1].Type);
                    StartActivity(intent);
                    break;
            }
        }

        private void SetToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate { OnBackPressed(); };
            Title = "Jocuri";
        }


        private async void GetCategories()
        {
            var dialog = new ProgressBarDialog("Asteptati", "Se preiau datele", this, false);
            try
            {
                dialog.Show();

                string res = await WebServices.WebServices.Get($"{Constants.PublicServerAddress}/api/gamesCategories/", Utils.GetDefaults("Token"));
                if (res != null)
                {
                    Log.Error(LOG_TAG, " RESULT " + res);
                }
                else
                {
                    Log.Error(LOG_TAG, " Res is null");
                }

                RunOnUiThread(() =>
                {
                    Log.Error(LOG_TAG, "uiThread");
                    dialog.Dismiss();
                });
            }
            catch (Exception e)
            {
                Log.Error(LOG_TAG, "ERR " + e.Message);
                dialog.Dismiss();
            }
        }


    }
}