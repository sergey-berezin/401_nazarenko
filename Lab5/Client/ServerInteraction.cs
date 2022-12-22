using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using Newtonsoft.Json;
using Polly;

namespace Client
{
    public class ClientFunctions
    {
        private HttpClient Client { get; set; }

        string connetion { get; set; }

        public ClientFunctions()
        {
            Client = new HttpClient();
            connetion = "https://localhost:7156";
        }

        async public Task<int?> PostImageToServer(Image_post img)
        {

            var jitterer = new Random();
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                  + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));

            var SerializedData = new StringContent(JsonConvert.SerializeObject(img), Encoding.UTF8, "application/json");

            var result = await retryPolicy.ExecuteAsync(async () => {
                var response = await Client.PostAsync($"{connetion}/images", SerializedData);
                return response;
            });

            string responseString = result.Content.ReadAsStringAsync().Result;
            return Int32.Parse(responseString);
        }


        async public Task<List<List<Image_get>>> GetArraysFromServer()
        {
            var jitterer = new Random();
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                  + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));


            var result = await retryPolicy.ExecuteAsync(async () => {
                var response = await Client.GetAsync($"{connetion}/images");
                return response;
            });

            List<List<Image_get>> ids = JsonConvert.DeserializeObject<List<List<Image_get>>>(result.Content.ReadAsStringAsync().Result);
            return ids;
        }

    }
}
