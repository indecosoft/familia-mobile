using System.Threading.Tasks;
using Android.Content;
using Org.Json;

namespace FamiliaXamarin {
    public interface IWebSocketClient {
        Task ConnectAsync(string hostname, int port, Context context) => null;
        void Connect(string hostname, int port, Context context) {
        }

        void Emit(string eventName, JSONObject value);
    }
}