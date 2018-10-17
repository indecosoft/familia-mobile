
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace FamiliaXamarin.Medicatie.Tasks
{
    class Tasks
    {
        public static HttpClient httpClient = new HttpClient();
        public static async Task<string> GetMedicine(string url, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using (var res = await httpClient.GetAsync(url))
            {
                if (!res.IsSuccessStatusCode) return null;

                using (var content = res.Content)
                {
                    return await content.ReadAsStringAsync();
                }

            }
        }
    }
}