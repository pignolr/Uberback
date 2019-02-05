using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System;
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
            if (!await R.Db(dbName).TableList().Contains("Text").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Text").Run(conn);
            if (!await R.Db(dbName).TableList().Contains("Image").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Image").Run(conn);
        }

        public async Task AddTextAsync(string flags, string userId)
        {
            await R.Db(dbName).Table("Text").Insert(R.HashMap("id", await R.Db(dbName).Table("Text").Count().RunAsync(conn))
                .With("UserId", userId)
                .With("Flags", flags)
                .With("DateTime", DateTime.Now.ToString("yyyyMMddHHmmss"))
                ).RunAsync(conn);
        }

        public async Task AddImageAsync(string flags, string userId)
        {
            await R.Db(dbName).Table("Image").Insert(R.HashMap("id", await R.Db(dbName).Table("Image").Count().RunAsync(conn))
                .With("UserId", userId)
                .With("Flags", flags)
                .With("DateTime", DateTime.Now.ToString("yyyyMMddHHmmss"))
                ).RunAsync(conn);
        }

        private readonly RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
