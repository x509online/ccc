using JustCli;
using JustCli.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ccc
{
    [Command("logout", "Removes auth from user profile", 1)]
    class Logout : ICommand
    {
       
        [CommandOutput]
        public IOutput Output { get; set; }

        public int Execute()
        {
            if (string.IsNullOrEmpty(Program.Config.UserName))
            {
                Output.WriteWarning("User not authenticated");                
            }
            else
            {
                Program.Config.UpdateCrendentials(string.Empty, string.Empty);
                Program.Config.Flush();
                
                Output.WriteSuccess("Logout Ok.");
            }

            return ReturnCode.Success;
        }
    }
}
