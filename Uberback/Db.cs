using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Threading.Tasks;

namespace Uberback
{
    public class Db
    {
        public Db(string dbName = "Uberschutz")
        {
            this.dbName = dbName;
            R = RethinkDB.R;
        }

        public async Task InitAsync()
        {
            conn = await R.Connection().ConnectAsync();
            if (!await R.DbList().Contains(dbName).RunAsync<bool>(conn))
                R.DbCreate(dbName).Run(conn);
        }

        private readonly RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
