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
using Android.Widget;

namespace FamiliaXamarin.Helpers
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class AlarmDeviceReceiver : BroadcastReceiver
    {
        public static readonly string INTERVAL_CONTENT = "INTERVAL_CONTENT";
        public override void OnReceive(Context context, Intent intent)
        {
            var content = intent.GetStringExtra(INTERVAL_CONTENT);
            Log.Error("INTERVAL_CONTENT", content);
            Toast.MakeText(context, content, ToastLength.Short).Show();
        }
    }
}