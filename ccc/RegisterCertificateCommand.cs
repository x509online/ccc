using JustCli;
using JustCli.Attributes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ccc
{
    [Command("push", "Register a certificate from a signature.")]
    class RegisterCertificateCommand : ICommandAsync
    {
        public RegisterCertificateCommand()
        {


        }

        [CommandArgument("s", "certificate-subject", Description = "certificate subject to search", DefaultValue = "")]
        public string certSubject { get; set; }

        [CommandArgument("c", "certificate", Description = "certificate file with private key", DefaultValue = "")]
        public string certFile { get; set; }

        [CommandArgument("p", "certPwd", Description = "certificate Password", DefaultValue = "")]
        public string certPwd { get; set; }

        [CommandOutput]
        public IOutput Output { get; set; }

        public async Task<int> ExecuteAsync()
        {
            if (String.IsNullOrEmpty(Program.Config.UserName) ||
                String.IsNullOrEmpty(Program.Config.ApiKey))
            {
                Output.WriteError("Please login first");
                return ReturnCode.Failure;
            }

            Output.WriteInfo("Select a Certificate from CurrentUser/My for publishing to CertCentral");
            X509Certificate2 cert = LoadCertFromCUMy();
            var http = new HttpClient();

            http.DefaultRequestHeaders.Add("ApiKey", $"{Program.Config.UserName}#{Program.Config.ApiKey}");
            var data = await http.GetStringAsync(Program.Config.BaseUrl + "cert/rnd");
            //Output.WriteInfo(data);

            var signature = Sign(data, cert);

            string fullUrl = $"{Program.Config.BaseUrl}cert/verify?data={data}&signature={WebUtility.UrlEncode(signature)}";

            var res = await http.GetAsync(fullUrl);

            if (res.IsSuccessStatusCode)
            {
                Output.WriteSuccess("Certificate Registered:" + cert.SubjectName.Name);
                Output.WriteSuccess($"Availiable at: \n{Program.Config.BaseUrl}cert/getusercert?username={Program.Config.UserName}&thumbprint={cert.Thumbprint.ToUpperInvariant()}");

                return ReturnCode.Success;
            }
            else
            {
                Output.WriteError("Cannot register certificate: " + res.ReasonPhrase);
                return ReturnCode.Failure;
            }
        }

        private X509Certificate2 LoadCertFromCUMy()
        {
            if (string.IsNullOrEmpty(certFile) && string.IsNullOrEmpty(certPwd))
            {
                #if NET47
                var cert = CertPicker.X509CertPicker.ShowCertPicker(certSubject);
                return cert;
                #else //Console Cert Picker
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                var codecerts = store.Certificates.Find(X509FindType.FindByApplicationPolicy, "1.3.6.1.5.5.7.3.3", false);
                var certs = codecerts.Find(X509FindType.FindBySubjectName, certSubject, false);
                for (int i = 0; i < certs.Count; i++)
                {
                    var c = certs[i];
                    Output.WriteInfo($"{i+1}. {c.SubjectName.Name} [{c.Thumbprint}]");
                }
                if (int.TryParse(Console.ReadLine(), out int selectedId))
                {
                    var c = certs[selectedId-1];
                    return c;
                }
                else
                {
                    throw new ArgumentException("No cert selected");
                }
                #endif
            }
            else
            {
                return new X509Certificate2(certFile, certPwd);
            }
        }

        private string Sign(string data, X509Certificate2 cert)
        {
            var signedCms = new SignedCms(new ContentInfo(Encoding.UTF8.GetBytes(data)), true);
            signedCms.ComputeSignature(new CmsSigner(cert));
            return Convert.ToBase64String(signedCms.Encode());
        }
    }
}
