using JustCli;
using JustCli.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ccc
{
    [Command("users", "shows registered users")]
    class ViewUsersCommand : ICommandAsync
    {
      
        [CommandOutput]
        public IOutput Output{ get; set; }

        public async Task<int> ExecuteAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStringAsync(new Uri($"{Program.Config.BaseUrl}cert/GetUsers")).ConfigureAwait(true);

                List<string> users = JsonConvert.DeserializeObject<List<string>>(res);

                if (users.Count > 0)
                {
                    Output.WriteSuccess($"\nFound {users.Count} users");
                }
                else
                {
                    Output.WriteWarning("\nNo users registered (this should be an error");
                }

                foreach (var u in users)
                {
                    Output.WriteInfo(u);
                }
            }
            return ReturnCode.Success;
        }

        
    }

}
