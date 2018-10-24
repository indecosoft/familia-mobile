
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Android.Widget;
using FamiliaXamarin.Medicatie.Entities;
using Java.Util;


namespace FamiliaXamarin.Medicatie.Tasks
{
    class Tasks
    {

        
        public static HttpClient httpClient = new HttpClient();
        public static async Task<string> GetMedicine(string url, string token)
        {
            try
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
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<string> PostMedicine(string url,string uuid, DateTime date, string token)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using (var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("uuid", uuid), new KeyValuePair<string, string>("date", date.ToString("yyyy-MM-dd HH:mm:ss")) })))
                {
                    if (!response.IsSuccessStatusCode) return null;
                    using (var content = response.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }

                }

            }
            catch (Exception)
            {
                return null;
            }

        }


     
    }
}