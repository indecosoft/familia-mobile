using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Org.Json;

namespace FamiliaXamarin
{
    public class WebServices
    {
        public static async Task<string> Get(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(url))
                    {
                        using (var content = response.Content)
                        {
                            var httpContent = await content.ReadAsStringAsync();
                            return httpContent;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }  
        }

        public static async Task<string> Get(string url, string token)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    using (var response = await client.GetAsync(url))
                    {

                        using (var content = response.Content)
                        {
                            var httpContent = await content.ReadAsStringAsync();

                            return httpContent;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
            }
        }

        public static async Task<string> Post(string url, JSONObject obj)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = obj.ToString();

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    var result = await streamReader.ReadToEndAsync();
                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static async Task<string> Post(string url, JSONObject obj, string token)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                httpWebRequest.Headers.Add("Authorization", "Bearer " + token);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = obj.ToString();

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    var result = await streamReader.ReadToEndAsync();
                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}