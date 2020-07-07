using System.Threading.Tasks;
using Android.Content;
using Org.Json;

namespace Familia.WebSocket {
    public interface IWebSocketClient {
        Task ConnectAsync(string hostname, int port, Context context) => null;
        void Connect(string hostname, int port, Context context) {
        }
        void Disconect();

        void Emit(string eventName, JSONObject value);
    }
}