using JustCli;
using JustCli.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ccc
{
    //Command("samplecert", "downloads a sample cert")]
    class SampleCertCommand : ICommandAsync
    {

        [CommandArgument("tp", "thumbprint", Description ="certificate thumbprint SHA1")]
        public string Thumbprint { get; set; }

        [CommandOutput]
        public IOutput Output{ get; set; }

        public async Task<int> ExecuteAsync()
        {
            Output.WriteInfo("CCC means CertCentralClient");
            Output.WriteInfo(Program.Config.BaseUrl);
            Output.WriteInfo(Thumbprint);

            X509Certificate2 cert = await ParseCert().ConfigureAwait(true);

            Output.WriteInfo(cert.SubjectName.Name);

            bool trusted = IsTrusted(cert);

            if (trusted)
            {
                Output.WriteSuccess("Certificate Trusted !!");
            }
            else
            {
                AddCertToStoreIfNotExist(cert);
            }
            return ReturnCode.Success;
        }

        private bool IsTrusted(X509Certificate2 cert)
        {
            bool trusted = true;
            X509Chain chain = new X509Chain(true);
            if (!chain.Build(cert))
            {
                foreach (var st in chain.ChainStatus)
                {
                    Output.WriteWarning(st.StatusInformation);
                    Output.WriteWarning(st.Status.ToString());
                    if (st.Status == X509ChainStatusFlags.UntrustedRoot)
                    {
                        trusted = false;
                    }
                }
            }

            return trusted;
        }

        private void AddCertToStoreIfNotExist(X509Certificate2 cert)
        {
            Console.WriteLine("Add cert to Root? [Y/n]");
            var add = Console.ReadLine();
            if (add.ToUpper(CultureInfo.InvariantCulture) == "Y")
            {
                X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                var foundCerts = store.Certificates.Find(X509FindType.FindBySerialNumber, cert.SerialNumber, false);
                if (foundCerts.Count > 0) // should not happen
                {
                    store.Close();
                    Output.WriteWarning("Certificate found. SN: " + cert.SerialNumber);
                }
                else
                {
                    store.Close();
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                    Output.WriteSuccess("CERT Added successfully.");
                }
            }
        }

        private async Task<X509Certificate2> ParseCert()
        {
            var http = new HttpClient();
            var data = await http.GetStringAsync(Program.Config.BaseUrl + "cert/" + Thumbprint);
            JObject json = JObject.Parse(data);
            var rawData = (string)json.SelectToken("rawData");
            byte[] bytes = Encoding.UTF8.GetBytes(rawData);
            X509Certificate2 cert = new X509Certificate2(bytes);
            return cert;
        }
    }

}
