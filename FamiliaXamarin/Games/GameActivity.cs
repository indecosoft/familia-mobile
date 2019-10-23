using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

namespace Familia.Games
{
    [Activity( Theme = "@style/AppTheme.Dark",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class GameActivity : AppCompatActivity, IGyroSensorChangedListener
    {
        public static float currentX;
        public static float currentY;
        public static int score = 0;

        public static RelativeLayout rlGame;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_game);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            rlGame = FindViewById<RelativeLayout>(Resource.Id.rl_game);

            WebView webView = FindViewById<WebView>(Resource.Id.wv_game);
            
            webView.Settings.JavaScriptEnabled = true;
            webView.SetWebChromeClient(new WebChromeClient());

            WebSettings webSettings = webView.Settings;
            webSettings.JavaScriptEnabled = true;
            webSettings.DomStorageEnabled = true;

            webView.AddJavascriptInterface(new WebViewJavascriptInterface(this), "JSHandler");

            webView.LoadUrl("file:///android_asset/test.html");

            var gyroSensor = new GyroSensor(this);
            gyroSensor.SetGyroListener(this);

        }

        public void OnGyroSensorChanged(float x, float y)
        {

//            Log.Error("GameActivity", " x " + x + " y " + y);
            currentX = x;
            currentY = y;
        }


    }

    public class WebViewJavascriptInterface: Java.Lang.Object
    {

        private Context context;

        public WebViewJavascriptInterface(Context _context)
        {
            context = _context;
        }

        public WebViewJavascriptInterface(IntPtr handle, JniHandleOwnership transfer): base(handle, transfer)
        {}

        [Export]
        [JavascriptInterface]
        public void receiveMessageFromJS(Java.Lang.String message)
        {
            Log.Error("WebViewJavascriptInterface", "receiving from html.." + message);

//            Toast.MakeText(context, message, ToastLength.Long).Show();

        }

        [Export]
        [JavascriptInterface]
        public string getFromAndroid()
        {
            Log.Error("WebViewJavascriptInterface", "getting from android..");
            return "from android";
        }

        [Export]
        [JavascriptInterface]
        public void saveScore(Java.Lang.String score)
        {
           
            GameActivity.score = int.Parse((string)score);
            Log.Error("GameActivity", "score: " + GameActivity.score);
        }

        [Export]
        [JavascriptInterface]
        public string getScore()
        {
            
            return GameActivity.score + "";
        }

        [Export]
        [JavascriptInterface]
        public string getScreenDimension()
        {
            return GameActivity.rlGame.Width + "/" + GameActivity.rlGame.Height;
        }

        [Export]
        [JavascriptInterface]
        public string getXYFromGyro()
        {
            return GameActivity.currentX + "/" + GameActivity.currentY;
        }

    }
}