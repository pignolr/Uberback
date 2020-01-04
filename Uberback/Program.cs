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
            await InitVariables(File.ReadAllText("Keys/token.txt"));
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
