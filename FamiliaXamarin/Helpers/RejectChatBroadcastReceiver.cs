using Android.Content;
using Android.Support.V4.App;
using Android.Util;
using Familia.WebSocket;
using Org.Json;

namespace Familia.Helpers {
	[BroadcastReceiver(Enabled = true, Exported = true)]
	public class RejectChatBroadcastReceiver : BroadcastReceiver {
		public override void OnReceive(Context context, Intent intent) {
			var ids = intent.GetStringExtra("Room").Split(':');
			NotificationManagerCompat.From(context).Cancel(
				ids[0] == Utils.GetDefaults("Id") ? int.Parse(ids[1]) : int.Parse(ids[0]));
			string emailFrom = Utils.GetDefaults("Email");
			try {
				JSONObject mailObject = new JSONObject().Put("dest", intent.GetStringExtra("EmailFrom"))
					.Put("from", emailFrom).Put("accepted", false);
				Log.Error("aici", mailObject.ToString());
				WebSocketClient.Socket.Emit("chat accepted", mailObject);
			} catch (JSONException e) {
				e.PrintStackTrace();
			}
		}
	}
}