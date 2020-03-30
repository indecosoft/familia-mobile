using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;

namespace Familia.Games
{
    class MyWebViewClient: WebViewClient
    {
        private WebView mWebView;
        private Context context;

        public MyWebViewClient(Context context, WebView mWebView)
        {
            this.mWebView = mWebView;
            this.context = context;
        }

        public override bool OnRenderProcessGone(WebView view, RenderProcessGoneDetail detail)
        {
            if (!detail.DidCrash())
            {
                // Renderer was killed because the system ran out of memory.
                // The app can recover gracefully by creating a new WebView instance
                // in the foreground.
                Log.Error("MY_APP_TAG", "System killed the WebView rendering process " +
                        "to reclaim memory. Recreating...");

                if (mWebView != null)
                {
                    ViewGroup webViewContainer = view.FindViewById<ViewGroup>(Resource.Id.rl_game);
                    webViewContainer.RemoveView(mWebView);
                    mWebView.Destroy();
                    mWebView = null;
                }

                // By this point, the instance variable "mWebView" is guaranteed
                // to be null, so it's safe to reinitialize it.

                return true; // The app continues executing.
            }
            return false;

        }
    }
}