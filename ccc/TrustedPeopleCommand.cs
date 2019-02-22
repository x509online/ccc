using JustCli;
using JustCli.Attributes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ccc
{
    [Command("trustedpeople", "View certs in LocalMachine/TrustedPeople")]
    class TrustedPeopleCommand : ICommandAsync
    {
        [CommandOutput]
        public IOutput Output { get; set; }

        public async Task<int> ExecuteAsync()
        {
            X509Store store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            Output.WriteInfo($"{store.Certificates.Count} certificates found in LocalMachine/TrustedPeople" + System.Environment.NewLine);
            HttpClient client = new HttpClient();
            foreach (var cert in store.Certificates)
            {
                string userName = null;
                try
                {
                    userName = await client.GetStringAsync($"{Program.Config.BaseUrl}cert/Thumbprint/{cert.Thumbprint.ToUpperInvariant()}");
                }
                catch { }
                if (userName == null)
                {
                    Output.WriteWarning($"{cert.SubjectName.Name} [{cert.Thumbprint.ToUpperInvariant()}]");
                    Output.WriteWarning("Not Found\n" );
                }
                else
                {
                    Output.WriteSuccess($"{cert.SubjectName.Name} [{cert.Thumbprint.ToUpperInvariant()}]");
                    Output.WriteSuccess($"Linked to https://github.com/{userName}\n");
                }
            }
            return ReturnCode.Success;
        }
    }
}
