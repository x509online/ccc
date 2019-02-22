using JustCli;
using JustCli.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace ccc
{
    public class CertInfo
    {
        public string SubjectName { get; set; }
        public string ThumbPrint { get; set; }
        public string Issuer { get; set; }
    }

    [Command("certs", "Query certs from other user")]
    class ViewUserCertsCommand : ICommandAsync
    {

        static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        [CommandArgument("u", "userName", Description = "User Name")]
        public string Name { get; set; }

        [CommandOutput]
        public IOutput Output { get; set; }

        public async Task<int> ExecuteAsync()
        {
            bool allTrusted = true;
            List<X509Certificate2> parsedCerts = new List<X509Certificate2>();
            HttpClient http = new HttpClient();
            var jsonCerts = await http.GetStringAsync(
                $"{Program.Config.BaseUrl}cert/GetCertsFromUser/{Name}");

            var certs = JsonConvert.DeserializeObject<IList<CertInfo>>(jsonCerts);
            Output.WriteSuccess($"Found {certs.Count} certs for user {Name}");
            Output.WriteInfo("\n");
            for (int i = 0; i < certs.Count; i++)
            {
                var c = certs[i];
                var cert = await CertRestClient.DownloadCertAndCache(Name, c.ThumbPrint);
                parsedCerts.Add(cert);
                (bool trusted, IList<string> reasons) = CertTrustValidator.IsTrusted(cert);

                if (trusted)
                {
                    Output.WriteSuccess($"{i + 1}.- {c.SubjectName}");
                    Output.WriteSuccess($"\t{c.ThumbPrint}");
                    Output.WriteSuccess($"\tTrusted in this machine.\n");
                }
                else
                {
                    allTrusted = false;
                    Output.WriteWarning($"{i + 1}.- {c.SubjectName}");
                    Output.WriteWarning($"\t{c.ThumbPrint}\n");
                    foreach (var r in reasons)
                    {
                        Output.WriteWarning($"\t{r}");
                    }
                    Output.WriteInfo("\n");
                }
            }

            if (allTrusted)
            {
                Output.WriteInfo($"All certificates are trusted.");
            }
            else
            {
                if (!IsElevated)
                {
                    Output.WriteWarning("To add certificates to your LocalMachine Store you need to run CCC as Admin.");
                    return ReturnCode.Failure;
                }
                else
                {

                    Output.WriteInfo("Select certificate to trust. (Empty to skip).");
                    var numCert = Console.ReadLine();
                    if (int.TryParse(numCert, out int n))
                    {
                        CertTrustValidator.AddCertToStoreIfNotExist(parsedCerts[n - 1]);
                    }
                }
            }
            return ReturnCode.Success;
        }
    }
}
