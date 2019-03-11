using JustCli;
using JustCli.Attributes;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ccc
{
    [Command("trust", "Adds cert to TrustedPeople")]
    class TrustCertCommand : ICommandAsync
    {
        static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        [CommandArgument("u", "userName", Description = "User Name")]
        public string Name { get; set; }

        [CommandArgument("t", "thumbPrint", Description = "Thumbprint")]
        public string Thumbprint { get; set; }

        [CommandOutput]
        public IOutput Output { get; set; }

        public async Task<int> ExecuteAsync()
        {
            if (!IsElevated)
            {
                Output.WriteWarning("To add certificates to your LocalMachine Store you need to run CCC as Admin.");
                return ReturnCode.Failure;
            }
            else
            {
                var cert = await CertRestClient.DownloadCertAndCache(Name, Thumbprint);
                Output.WriteInfo($"Adding Certificate {cert.SubjectName.Name} [{cert.Thumbprint}]");
                CertTrustValidator.AddCertToStoreIfNotExist(cert);

            }
            return ReturnCode.Success;

        }
    }
}
