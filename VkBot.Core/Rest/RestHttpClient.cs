using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VkBot.Core.Rest
{
    public class RestHttpClient : IRestClient, IDisposable
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public byte[] GetBytes(string url)
        {
            return _httpClient.GetAsync(url).Result.Content.ReadAsByteArrayAsync().Result;
        }

        public Task<byte[]> GetBytesAsync(string url)
        {
            return _httpClient.GetAsync(url).Result.Content.ReadAsByteArrayAsync();
        }

        public Stream GetStream(string url)
        {
            return _httpClient.GetAsync(url).Result.Content.ReadAsStreamAsync().Result;
        }

        public Task<Stream> GetStreamAsync(string url)
        {
            return _httpClient.GetAsync(url).Result.Content.ReadAsStreamAsync();
        }

        public string GetString(string url)
        {
            return _httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
        }

        public Task<string> GetStringAsync(string url)
        {
            return _httpClient.GetAsync(url).Result.Content.ReadAsStringAsync();
        }

        public string UploadImage(string url, byte[] data, string imageExtension)
        {
            var requestContent = new MultipartFormDataContent();
            var imageContent = new ByteArrayContent(data);
            switch (imageExtension)
            {
                case "png":
                    imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                    requestContent.Add(imageContent, "photo", "image.png");
                    break;
                case "jpg":
                    imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                    requestContent.Add(imageContent, "photo", "image.jpg");
                    break;
                default:
                    throw new Exception("Unknow the image type of the image");
            }

            var response = _httpClient.PostAsync(url, requestContent).Result;
            requestContent.Dispose();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return response.Content.ReadAsStringAsync().Result;
        }

        public async Task<string> UploadImageAsync(string url, byte[] data, string imageExtension)
        {
            var requestContent = new MultipartFormDataContent();
            var imageContent = new ByteArrayContent(data);
            switch (imageExtension)
            {
                case "png":
                    imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                    requestContent.Add(imageContent, "photo", "image.png");
                    break;
                case "jpg":
                    imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                    requestContent.Add(imageContent, "photo", "image.jpg");
                    break;
                default:
                    throw new Exception("Unknow the image type of the image");
            }

            var response = await _httpClient.PostAsync(url, requestContent).ConfigureAwait(false);
            requestContent.Dispose();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
