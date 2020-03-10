using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Games {
	[Activity(Label = "GameCenterActivity", Theme = "@style/AppTheme.Dark",
		ScreenOrientation = ScreenOrientation.Portrait)]
	public class GameCenterActivity : AppCompatActivity, View.IOnClickListener {
		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_game_center);
			SetToolbar();

			FindViewById<CardView>(Resource.Id.cw_game1).SetOnClickListener(this);
			FindViewById<CardView>(Resource.Id.cw_game2).SetOnClickListener(this);
		}

		public void OnClick(View v) {
			var intent = new Intent(this, typeof(GameActivity));
			switch (v.Id) {
				case Resource.Id.cw_game1:
					intent.PutExtra("Game", 1);
					StartActivity(intent);
					break;
				case Resource.Id.cw_game2:
					intent.PutExtra("Game", 2);
					StartActivity(intent);
					break;
			}
		}

		private void SetToolbar() {
			var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);
			toolbar.NavigationClick += delegate { OnBackPressed(); };
			Title = "Jocuri";
		}
	}
}