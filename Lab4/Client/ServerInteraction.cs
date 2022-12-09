using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using Newtonsoft.Json;

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
            var response = await Client.GetAsync($"{connetion}/images/{id}");
            for (int i = 0; i < 5 && response.StatusCode != System.Net.HttpStatusCode.OK; ++i)
            {
                response = await Client.GetAsync($"{connetion}/images/{id}");
            }
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Failed to get image from \"{connetion}/images/{id}\" in {5} attempts");
            }
            Image? image = JsonConvert.DeserializeObject<Image?>(response.Content.ReadAsStringAsync().Result);
            return image;
        }

        async public Task<int?> PostImageToServer(Image img)
        {
            byte[] data = File.ReadAllBytes(img.Name);
            img.Blob = Convert.ToBase64String(data);
            img.Hash = Image.GetHash(data);
            var SerializedData = new StringContent(JsonConvert.SerializeObject(img), Encoding.UTF8, "application/json");
            var response =  await Client.PostAsync($"{connetion}/images", SerializedData);
            for (int i = 0; i < 5 && response.StatusCode != System.Net.HttpStatusCode.OK; ++i)
            {
                response = await Client.PostAsync($"{connetion}/images", SerializedData);
            }
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Failed to post image to \"{connetion}/images\" in {5} attempts");
            }
            string responseString = response.Content.ReadAsStringAsync().Result;
            return Int32.Parse(responseString);
        }

        async public Task<int[]?> GetIdsFromServer()
        {
            var response = await Client.GetAsync($"{connetion}/images");
            for (int i = 0; i < 5 && response.StatusCode != System.Net.HttpStatusCode.OK; ++i)
            {
                response = await Client.GetAsync($"{connetion}/images");
            }
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Failed to get id's from \"{connetion}/images\" in {5} attempts");
            }
            int[]? ids = JsonConvert.DeserializeObject<int[]?>(response.Content.ReadAsStringAsync().Result);
            return ids;
        }

        async public Task DeleteAllServer()
        {
            var response = await Client.DeleteAsync($"{connetion}/images");
            for (int i = 0; i < 5 && response.StatusCode != System.Net.HttpStatusCode.OK; ++i)
            {
                response = await Client.DeleteAsync($"{connetion}/images");
            }
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Failed to delete images from \"{connetion}/images\" in {5} attempts");
            }
        }

        async public Task DeleteImageServer(int id)
        {
            var response  = await Client.DeleteAsync($"{connetion}/images/{id}");
            for (int i = 0; i < 5 && response.StatusCode != System.Net.HttpStatusCode.OK; ++i)
            {
                response = response = await Client.DeleteAsync($"{connetion}/images/{id}");
            }
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Failed to delete an image from \"{connetion}/images/{id}\" in {5} attempts");
            }
        }

    }
}
