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
using Org.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Games
{
    [Activity(Label = "GameCenterActivity", Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class GameCenterActivity : AppCompatActivity
    {
        private static string LOG_TAG = "GameCenterActivity";
        private LottieAnimationView emptyAnimation;
        private TextView tvEmpty;

        private static List<Category> categories = new List<Category>();
      
        private List<Game> games = new List<Game>() {
            new Game("Planetele Vesele", 1, new List<Category>(){ new Category(4, "Coordonare") }),
            new Game("Logică", 2, new List<Category>(){ new Category(1, "Gandire Logica"), new Category(2, "Gandire Matematica")}),
            new Game("Jurnal de activități", 3, new List<Category>(){ new Category(3, "Orientare in timp si spatiu")})
        };

        private HashSet<Game> list = new HashSet<Game>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_game_center);
            SetToolbar();

            emptyAnimation = FindViewById<LottieAnimationView>(Resource.Id.animation_empty_box);
            tvEmpty = FindViewById<TextView>(Resource.Id.tv_empty_games);
            
            selectGamesFromCategories();
        }

        private async void selectGamesFromCategories()
        {
            categories = await GetCategories();

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

            Log.Error(LOG_TAG, "games count " + list.Count);

            if (list.Count != 0)
            {
                emptyAnimation.Visibility = ViewStates.Invisible;
                tvEmpty.Visibility = ViewStates.Gone;
            }
            else
            {
                emptyAnimation.Visibility = ViewStates.Visible;
                tvEmpty.Visibility = ViewStates.Visible;
            }

            showGamesCards();
        }

        private void showGamesCards()
        {
            var gridview = FindViewById<GridView>(Resource.Id.gridview);
            gridview.Adapter = new GamesGridViewAdapter(this, list);

            gridview.ItemClick += delegate (object sender, AdapterView.ItemClickEventArgs args) {
                var intent = new Intent(this, typeof(GameActivity));
                intent.PutExtra("Game", games[args.Position].Type);
                StartActivity(intent);
            };
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


        private async Task<List<Category>> GetCategories()
        {
            var dialog = new ProgressBarDialog("Asteptati", "Se preiau datele", this, false);
            var categories = new List<Category>();
            try
            {
                dialog.Show();
                string res = await WebServices.WebServices.Get($"{Constants.PublicServerAddress}/api/gamesCategories/", Utils.GetDefaults("Token"));
                if (res != null)
                {
                    Log.Error(LOG_TAG, " RESULT " + res);
                    try {
                        var array = new JSONArray(res);
                        for (var i = 0; i < array.Length(); i++)
                        {
                            var obj = (JSONObject)array.Get(i);
                            categories.Add(new Category(obj.GetInt("id"), obj.GetString("categorie")));
                        }
                    } catch (Exception e) {
                        Log.Error(LOG_TAG, "ERR" + e.Message);
                    }
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

            return categories;
        }


    }
}