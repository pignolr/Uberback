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
        public void A()
        {

        }
    }
}
