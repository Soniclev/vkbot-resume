using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace VkBot.Core.Rest
{
    public interface IRestClient
    {
        byte[] GetBytes(string url);
        Task<byte[]> GetBytesAsync(string url);

        string GetString(string url);
        Task<string> GetStringAsync(string url);

        Stream GetStream(string url);
        Task<Stream> GetStreamAsync(string url);

        string UploadImage(string url, byte[] data, string imageExtension);
        Task<string> UploadImageAsync(string url, byte[] data, string imageExtension);
    }
}
