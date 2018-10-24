using Android.Content;
using Java.Lang;
using Org.Json;
using SocketIO.Client;

namespace FamiliaXamarin
{
    public interface IWebSocketClient
    {
        void Connect(string hostname, int port, Context context);

        void Emit(string eventName, JSONObject value);
        //Socket Client();
    }
}