using JustCli;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ccc
{
    class ThisAssembly
    {
        public static string AssemblyInformationalVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    class Program
    {
        internal const string DEFAULT_BASE_URL  = "https://certcentral.x509.online/api/";
        //internal const string DEFAULT_BASE_URL = "https://localhost:44305/api/";

        public static Config Config { get; set; }

        static async Task Main(string[] args)
        {
            Config = Config.Init();
            PrintWelComeMessage();
            await CommandLineParser.Default.ParseAndExecuteCommandAsync(args);
        }

        private static void PrintWelComeMessage()
        {
            Console.Write($"CCC {ThisAssembly.AssemblyInformationalVersion} | {Config.BaseUrl} | ");
            if (string.IsNullOrEmpty(Config.UserName))
            {
                Console.Write("Not authenticated.");
            }
            else
            {
                Console.Write("Authenticated as:" + Config.UserName);
            }
            Console.WriteLine("\n");
        }
    }
}
