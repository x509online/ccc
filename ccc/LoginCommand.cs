using JustCli;
using JustCli.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ccc
{
    [Command("login", "Configures API Key for auth", 1)]
    class LoginCommand : ICommandAsync
    {
        [CommandArgument("u", "userName", Description = "User Name")]
        public string Name { get; set; }

        [CommandArgument("k", "apiKey", Description = "Api Key")]
        public string ApiKey { get; set; }

        [CommandOutput]
        public IOutput Output { get; set; }

        public async Task<int> ExecuteAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("ApiKey", $"{Name}#{ApiKey}");
                var res = await client.GetStringAsync(new Uri($"{Program.Config.BaseUrl}cert/login")).ConfigureAwait(true);

                Output.WriteSuccess("User Logged in successfully !!");
                Output.WriteInfo("Saving credentials");

                SaveCredentials();

                Output.WriteSuccess("Credentials Saved Successfully.");
            }
            return ReturnCode.Success;
        }

        private void SaveCredentials()
        {
            Program.Config.UpdateCrendentials(Name, ApiKey);
            Program.Config.Flush();
        }
    }
}
