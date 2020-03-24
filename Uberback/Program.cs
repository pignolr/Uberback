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

        public Program(string backToken) // Used by unit tests
        {
            InitVariables(backToken).GetAwaiter().GetResult();
        }

        public static Program P;
        public Db db { private set; get; }

        public string token { private set; get; }
        public ImageAnnotatorClient imageClient { private set; get; }
        public TranslationClient translationClient { private set; get; }
        public string perspectiveApi { private set; get; }

        public API.ITranslator Translator { private set; get; }
        public API.ITextAnalyser TextAnalyser { private set; get; }
        public API.IImageAnalyser ImageAnalyser { private set; get; }


        static async Task Main(string[] args)
            => await new Program().InitAsync();

        private async Task InitVariables(string backToken)
        {
            P = this;
            db = new Db();
            token = backToken;
            await db.InitAsync();
        }

        private async Task InitAsync()
        {
            if (!File.Exists("Keys/token.txt") || !File.Exists("Keys/perspectiveAPI.txt") || !File.Exists("Keys/imageAPI.json") || !File.Exists("Keys/googleAPI.json"))
                throw new FileNotFoundException("Missing Key file");

            await InitVariables(File.ReadAllText("Keys/token.txt"));
            perspectiveApi = File.ReadAllText("Keys/perspectiveAPI.txt");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Keys/imageAPI.json");
            imageClient = ImageAnnotatorClient.Create();
            translationClient = TranslationClient.Create();

            Translator = new API.GoogleTranslator("Keys/googleAPI.json");
            TextAnalyser = new API.PerspectiveTextAnalyser("Keys/perspectiveAPI.txt", "Config/GoogleVisionV1ImageAnalyserConfig.xml", Translator);
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
