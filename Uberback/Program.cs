using Google.Cloud.Translation.V2;
using Google.Cloud.Vision.V1;
using Nancy.Hosting.Self;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Uberback
{
    class Program
    {
        private Program() // Default ctor, used by Main
        { }

        public Program(string backToken, string backSaltForHash) // Used by unit tests
        {
            InitVariables(backToken, backSaltForHash).GetAwaiter().GetResult();
        }

        public static Program P;
        public Db db { private set; get; }

        public string token { private set; get; }
        public string saltForHash { private set; get; }

        public API.GoogleTranslator Translator { private set; get; }
        public API.PerspectiveTextAnalyser TextAnalyser { private set; get; }
        public API.GoogleVisionV1ImageAnalyser ImageAnalyser { private set; get; }

        static async Task Main(string[] args)
            => await new Program().InitAsync();

        private async Task InitVariables(string backToken, string backSaltForHash)
        {
            P = this;
            db = new Db();
            token = backToken;
            saltForHash = backSaltForHash;
            await db.InitAsync();
        }

        private async Task InitAsync()
        {
            if (!File.Exists("Keys/token.txt") || !File.Exists("Keys/perspectiveAPI.txt") || !File.Exists("Keys/googleAPI.json"))
                throw new FileNotFoundException("Missing Key file");
            if (!File.Exists("Config/saltForHash.txt"))
                throw new FileNotFoundException("Missing Config file");

            await InitVariables(File.ReadAllText("Keys/token.txt"), File.ReadAllText("Config/saltForHash.txt"));

            Translator = new API.GoogleTranslator("Keys/googleAPI.json");
            TextAnalyser = new API.PerspectiveTextAnalyser("Keys/perspectiveAPI.txt", "Config/PerspectiveTextAnalyserConfig.xml", Translator);
            ImageAnalyser = new API.GoogleVisionV1ImageAnalyser("Keys/googleAPI.json");

            AutoResetEvent autoEvent = new AutoResetEvent(false);
            LaunchServer(autoEvent);
            autoEvent.WaitOne();
        }

        private void LaunchServer(AutoResetEvent autoEvent)
        {
            HostConfiguration config = new HostConfiguration()
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };
            NancyHost host = new NancyHost(config, new Uri("http://localhost:5412"));
            host.Start();
            Console.WriteLine("Host started... Do ^C to exit.");
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("^C received, exitting...");
                host.Dispose();
                autoEvent.Set();
            };
        }
    }
}
