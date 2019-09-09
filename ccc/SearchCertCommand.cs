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
    [Command("search", "Search certs by thumbprint")]
    class SearchCertsCommand : ICommandAsync
    {
        [CommandArgument("t", "thumbprint", Description = "Thumbprint")]
        public string Thumbprint { get; set; }

        [CommandOutput]
        public IOutput Output { get; set; }

        public async Task<int> ExecuteAsync()
        {
            HttpClient client = new HttpClient();
            var userName = await client.GetStringAsync(new Uri($"{Program.Config.BaseUrl}cert/Thumbprint/{Thumbprint.ToUpperInvariant()}")).ConfigureAwait(true);
            client.Dispose();            
            if (userName!=null)
            {
                Output.WriteInfo($"The certificate with thumbprint {Thumbprint.ToUpperInvariant()} is registered with user https://github.com/{userName}");
                Output.WriteInfo($"\n Query the certificates of user '{userName}' by running:");
                Output.WriteInfo($"ccc certs -u {userName}\n");
            }
            else
            {
                Output.WriteWarning("Thumbprint not found");
            }
            
            return ReturnCode.Success;
        }
    }
}
