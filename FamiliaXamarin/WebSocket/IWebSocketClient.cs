using Android.Content;
using Org.Json;

namespace FamiliaXamarin
{
    public interface IWebSocketClient
    {
        void Connect(string hostname, int port, Context context);

        void Emit(string eventName, JSONObject value);
    }
}