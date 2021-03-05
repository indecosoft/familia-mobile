using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Android.Util;

namespace Familia.WebServices {
    public static class WebServices {
        public static async Task<string> Get(string url, string token = null) {
            Log.Error("Get Token Request on", url);

            using var client = new HttpClient();
            try {
                if (!string.IsNullOrEmpty(token)) {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                using HttpResponseMessage response = await client.GetAsync(url);
                using HttpContent content = response.Content;
                return await content.ReadAsStringAsync();
            } catch (Exception ex) {
                Log.Error("WEB SERVER ERR", ex.Message);

                return null;
            }
        }

        public static async Task<string> Post<T>(string route, T obj, string token = null)
            where T : Java.Lang.Object, new() {
            try {
                Log.Error("Post Request on", Constants.PublicServerAddress + route);

                var httpWebRequest = (HttpWebRequest) WebRequest.Create(Constants.PublicServerAddress + route);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                if (!string.IsNullOrEmpty(token)) {
                    httpWebRequest.Headers.Add("Authorization", "Bearer " + token);
                }

                await using var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync());
                var json = obj.ToString();

                await streamWriter.WriteAsync(json);
                await streamWriter.FlushAsync();
                streamWriter.Close();
                var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                using var streamReader =
                    new StreamReader(httpResponse.GetResponseStream() ?? throw new InvalidOperationException());
                return await streamReader.ReadToEndAsync();
            } catch (Exception e) {
                Console.WriteLine(e);
                return null;
            }
        }
       
        public static async Task<string> Post(string url, Dictionary<string, string> dict) {
            try {
                using var client = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes($"{Constants.ClientId}:{Constants.ClientSecret}");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsync(url, new FormUrlEncodedContent(dict));
                return await response.Content.ReadAsStringAsync();
            } catch (Exception e) {
                Console.WriteLine(e);
                return null;
            }
        }
        public static async Task<string> Post<T>(string url, MultipartFormDataContent form) {
            try {
                using var client = new HttpClient();
                // var byteArray = Encoding.ASCII.GetBytes(url);
                // client.DefaultRequestHeaders.Authorization =
                //     new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsync(url, form);
                return await response.Content.ReadAsStringAsync();
            } catch (Exception e) {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
