using System.Threading.Tasks;
using Org.Json;

namespace FamiliaXamarin
{
    public interface IWebServices
    {
        Task<string> Get(string url);
        Task<string> Get(string url, string token);
        Task<string> Post(string url, JSONObject obj);
        Task<string> Post(string url, JSONObject obj, string token);

    }
}