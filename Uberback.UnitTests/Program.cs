using Nancy;
using Nancy.Testing;
using System.Threading.Tasks;
using Xunit;

namespace Uberback.UnitTests
{
    public class Program
    {
        /// <summary>
        /// Reset the db for each tests
        /// </summary>
        private async Task Init()
        {
            Db db = new Db();
            await db.DeleteAsync();
            await db.InitAsync();
        }

        [Fact]
        public async Task MainEndpoint()
        {
            await Init();
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper);
            var result = await browser.Get("/", with => {
                with.HttpRequest();
            });
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }
    }
}
