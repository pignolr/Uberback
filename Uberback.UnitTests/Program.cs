using Nancy;
using Nancy.Testing;
using System.Threading.Tasks;
using Xunit;

namespace Uberback.UnitTests
{
    public class Program
    {
        [Fact]
        public async Task MainEndpoint()
        {
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper);
            var result = await browser.Get("/", with => {
                with.HttpRequest();
            });
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task SendData()
        {

            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper);
            var result = await browser.Post("/data", with =>
            {

                with.HttpRequest();
            });
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
