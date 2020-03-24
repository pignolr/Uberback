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
            conn = null;
        }

        /// <summary>
        /// Delete the db
        /// Used for unit tests
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync()
        {
            if (conn == null)
                conn = await R.Connection().ConnectAsync();
            R.DbDrop(dbName);
        }

        /// <summary>
        /// Init the db
        /// </summary>
        public async Task InitAsync()
        {
            conn = await R.Connection().ConnectAsync();
            if (!await R.DbList().Contains(dbName).RunAsync<bool>(conn))
                R.DbCreate(dbName).Run(conn);
            if (!await R.Db(dbName).TableList().Contains("Text").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Text").Run(conn);
            if (!await R.Db(dbName).TableList().Contains("Image").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Image").Run(conn);
            if (!await R.Db(dbName).TableList().Contains("AnalysedText").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("AnalysedText").Run(conn);
        }

        /// <summary>
        /// Add text in the db
        /// </summary>
        /// <param name="flags">Flag triggered by the text, SAFE if none</param>
        /// <param name="userId">User id</param>
        public async Task AddTextAsync(string flags, string userId, string service)
        {
            await R.Db(dbName).Table("Text").Insert(R
//                .HashMap("id", await R.Db(dbName).Table("Text").Count().RunAsync(conn))
                .HashMap("UserId", userId)
                .With("Flags", flags)
                .With("DateTime", DateTime.Now.ToString("yyyyMMddHHmmss"))
                .With("Service", service)
                ).RunAsync(conn);
        }

        /// <summary>
        /// Add image in the db
        /// </summary>
        /// <param name="flags">Flag triggered by the image, SAFE if none</param>
        /// <param name="userId">User id</param>
        public async Task AddImageAsync(string flags, string userId, string service)
        {
            await R.Db(dbName).Table("Image").Insert(R
                //.HashMap("id", await R.Db(dbName).Table("Image").Count().RunAsync(conn))
                .HashMap("UserId", userId)
                .With("Flags", flags)
                .With("DateTime", DateTime.Now.ToString("yyyyMMddHHmmss"))
                .With("Service", service)
                ).RunAsync(conn);
        }

        /// <summary>
        /// Add analysed text in the db
        /// </summary>
        /// <param name="flags">Flag triggered by the image, SAFE if none</param>
        /// <param name="userId">User id</param>
        public async Task AddAnalysedTextAsync(string flags, string hashedText)
        {
            await R.Db(dbName).Table("AnalysedText")
                .Insert(new {
                    HashedText = hashedText,
                    Flags = flags,
                    FirstDateTime = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    LastDateTime = DateTime.Now.ToString("yyyyMMddHHmmss")
                }).RunAsync(conn);
        }

        /// <summary>
        /// Get text table
        /// </summary>
        public async Task<Cursor<object>> GetTextAsync()
        {
            return (await R.Db(dbName).Table("Text").RunAsync(conn));
        }

        /// <summary>
        /// Get image table
        /// </summary>
        public async Task<Cursor<object>> GetImageAsync()
        {
            return (await R.Db(dbName).Table("Image").RunAsync(conn));
        }

        /// <summary>
        /// Get image table
        /// </summary>
        public async Task<string> GetFlagsFromAnalysedTextAsync(string hashedText)
        {
            return await R.Db(dbName).Table("AnalysedText")
                .Filter(new{ HashedText = hashedText })
                .GetField("Flags")
                .Nth(0)
                .RunAsync(conn);
        }

        /// <summary>
        /// Get image table
        /// </summary>
        public async Task UpdateLastDateTimeOfAnalysedTextAsync(string hashedText)
        {
            await R.Db(dbName).Table("AnalysedText")
                .Filter(new { HashedText = hashedText })
                .Nth(0)
                .Update(new { LastDateTime = DateTime.Now.ToString("yyyyMMddHHmmss") })
                .RunAsync(conn);
        }

        /// <summary>
        /// Get image table
        /// </summary>
        public async Task<bool> IsTextAnalysedAsync(string hashedText)
        {
            return await R.Db(dbName).Table("AnalysedText")
                .Filter(new { HashedText = hashedText })
                .Count().Gt(0)
                .RunAsync(conn);
        }

    private readonly RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
