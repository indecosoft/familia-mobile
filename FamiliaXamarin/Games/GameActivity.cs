﻿using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Familia.Games.Sensors;
using Java.Interop;
using Object = Java.Lang.Object;
using String = Java.Lang.String;

namespace Familia.Games {
	[Activity(Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Landscape)]
	public class GameActivity : AppCompatActivity, IGyroSensorChangedListener {
		public float rotationOY;
		public float rotationOX;
		public float rotationOZ;
		public int score;
		public int highScore;

		public RelativeLayout rlGame;
		private GyroSensor gyroSensor;
		private WebView webView;


		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_game);
			Window.AddFlags(WindowManagerFlags.Fullscreen);
			Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
			rlGame = FindViewById<RelativeLayout>(Resource.Id.rl_game);

			webView = FindViewById<WebView>(Resource.Id.wv_game);

			webView.Settings.JavaScriptEnabled = true;
			webView.SetWebViewClient(new MyWebViewClient(this, webView));
			//webView.SetWebChromeClient(new WebChromeClient());
			webView.SetRendererPriorityPolicy(RendererPriority.Bound, true);

			WebSettings webSettings = webView.Settings;
			webSettings.JavaScriptEnabled = true;
			webSettings.DomStorageEnabled = true;
			webSettings.LoadWithOverviewMode = true;
			webSettings.UseWideViewPort = true;

			webView.AddJavascriptInterface(new WebViewJavascriptInterface(this), "AndroidJSHandler");

			
			gyroSensor = new GyroSensor(this);
			gyroSensor.SetGyroListener(this);

			webView.LoadUrl("file:///android_asset/joc1/index.html");
		}

		public void OnGyroSensorChanged(float rotOY, float rotOX, float rotOZ) {
			rotationOY = rotOY;
			rotationOX = rotOX;
			rotationOZ = rotOZ;
		}

		public override void OnBackPressed() {
			base.OnBackPressed();
			gyroSensor.Dispose();
			Finish();
		}
	}


	public class WebViewJavascriptInterface : Object, IDisposable {
		private Context context;

		public WebViewJavascriptInterface(Context _context) {
			context = _context;
		}

		public WebViewJavascriptInterface(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }

		[Export]
		[JavascriptInterface]
		public void receiveMessageFromJS(String message) {
			Log.Error("WebViewJavascriptInterface", "receiving from html.." + message);
		}

		[Export]
		[JavascriptInterface]
		public string getFromAndroid() {
			Log.Error("WebViewJavascriptInterface", "getting from android..");
			return "from android";
		}

		[Export]
		[JavascriptInterface]
		public void saveScore(String score) {
			((GameActivity) context).score = int.Parse((string) score);
			if (((GameActivity)context).score > ((GameActivity)context).highScore) {
			((GameActivity)context).highScore = ((GameActivity)context).score;
				Log.Error("GameActivity", "high score: " + ((GameActivity)context).highScore);
			}
			((GameActivity)context).score = 0;
			Log.Error("GameActivity", "score: " + ((GameActivity) context).score);
		}

		[Export]
		[JavascriptInterface]
		public string getScore() {
			return ((GameActivity) context).score + "";
		}

		[Export]
		[JavascriptInterface]
		public string getScreenDimension() {
			return ((GameActivity) context).rlGame.Width + "/" + ((GameActivity) context).rlGame.Height;
		}

		[Export]
		[JavascriptInterface]
		public string getXYFromSensor() {
			Log.Error("GameActivity", "get xy from sensor " + ((GameActivity)context).rotationOY + "/" + ((GameActivity)context).rotationOX + "/" +
				   ((GameActivity)context).rotationOZ);
			return ((GameActivity) context).rotationOY + "/" + ((GameActivity) context).rotationOX + "/" +
			       ((GameActivity) context).rotationOZ;
		}
	}
}