using Android.Content;
using SocketIO.Client;

namespace FamiliaXamarin
{
    public interface IWebSocketClient
    {
        void Connect(string hostname, int port, Context context);
        //Socket Client();
    }
}