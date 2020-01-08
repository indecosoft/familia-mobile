using Android.Content;
using Android.Support.V4.App;
using Android.Util;
using Org.Json;

namespace FamiliaXamarin.Helpers
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    public class RejectChatBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var ids = intent.GetStringExtra("Room").Split(':');
            NotificationManagerCompat.From(context).Cancel(
                ids[0] == Utils.GetDefaults("Id")
                    ? int.Parse(ids[1])
                    : int.Parse(ids[0]));
            var emailFrom = Utils.GetDefaults("Email");
            try
            {
                var mailObject = new JSONObject()
                    .Put("dest", intent.GetStringExtra("EmailFrom"))
                    .Put("from", emailFrom)
                    .Put("accepted", false);
                Log.Error("aici", mailObject.ToString());
                WebSocketClient.Client.Emit("chat accepted", mailObject);
            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
        }
    }
}