using System;
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

        async public Task<Image?> GetImageFromServer(int id)
        {
            var jitterer = new Random();
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                  + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));
                                  
                                  
            var response = await retryPolicy.ExecuteAsync(async () => {
                var response = await Client.GetAsync($"{connetion}/images/{id}");
                return response;
            });                      
            
            Image? image = JsonConvert.DeserializeObject<Image?>(response.Content.ReadAsStringAsync().Result);
            return image;
        }

        async public Task<int?> PostImageToServer(Image img)
        {
            byte[] data = File.ReadAllBytes(img.Name);
            img.Blob = Convert.ToBase64String(data);
            img.Hash = Image.GetHash(data);
            var SerializedData = new StringContent(JsonConvert.SerializeObject(img), Encoding.UTF8, "application/json");
            
            
            var jitterer = new Random();
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                  + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));            
            
            
            var response = await retryPolicy.ExecuteAsync(async () => {
                var response =  await Client.PostAsync($"{connetion}/images", SerializedData);
                return response;
            });
            
            string responseString = response.Content.ReadAsStringAsync().Result;
            return Int32.Parse(responseString);
        }

        async public Task<int[]?> GetIdsFromServer()
        {
        
            var jitterer = new Random();
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                  + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));           
        
        
            var response = await retryPolicy.ExecuteAsync(async () => {
                var response = await Client.GetAsync($"{connetion}/images");
                return response;
            });        
            
            int[]? ids = JsonConvert.DeserializeObject<int[]?>(response.Content.ReadAsStringAsync().Result);
            return ids;
        }

        async public Task DeleteAllServer()
        {
            var jitterer = new Random();
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                  + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));

            var response = await retryPolicy.ExecuteAsync(async () => {
                var response = await Client.DeleteAsync($"{connetion}/images");
                return response;
            });     
        }

        async public Task DeleteImageServer(int id)
        {
            var jitterer = new Random();
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                  + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));

            var response = await retryPolicy.ExecuteAsync(async () => {
                var response  = await Client.DeleteAsync($"{connetion}/images/{id}");
                return response;
            });             
        }

    }
}
