using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ccc
{
    internal class CertRestClient
    {
        internal static async Task<X509Certificate2> DownloadCertAndCache(string username, string thumbprint)
        {
            var path = Path.Combine(Config.ConfigBasePath, thumbprint + ".cer");
            if (File.Exists(path))
            {
                var cachedCert = new X509Certificate2(path);
                return cachedCert;
            }
            X509Certificate2 cert;
            using (var http = new HttpClient())
            {
                var data = await http.GetStringAsync(new Uri($"{Program.Config.BaseUrl }cert/GetUserCert?username={username}&thumbprint={thumbprint}")).ConfigureAwait(true);
                JObject json = JObject.Parse(data);
                var rawData = (string)json.SelectToken("rawData");
                byte[] bytes = Encoding.UTF8.GetBytes(rawData);
                cert = new X509Certificate2(bytes);
                File.WriteAllText(path, rawData);
            }
            return cert;
        }
    }
}
