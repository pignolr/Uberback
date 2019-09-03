using Nancy;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Uberback.Endpoint
{
    public class Collect : NancyModule
    {
        /// <summary>
        /// Get information
        /// </summary>
        public Collect() : base("/collect")
        {
            Post("/", x =>
            {
                // Error Handling
                Common.Answer? error = Common.BasicCheck(Request.Query["token"]);
                if (error.HasValue)
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = error.Value.message
                    }, error.Value.code));
                if (!string.IsNullOrEmpty(Request.Query["type"]) && Request.Query["type"] != "image" && Request.Query["type"] != "text")
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "If set, type must be text or image"
                    }, HttpStatusCode.BadRequest));

                // Getting datas
                List<Response.Data> datas;
                if (string.IsNullOrEmpty(Request.Query["type"]))
                {
                    datas = GetContent(Program.P.db.GetImageAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Image).ToList();
                    datas.AddRange(GetContent(Program.P.db.GetTextAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Text).ToList());
                }
                else if (Request.Query["type"] == "text")
                    datas = GetContent(Program.P.db.GetTextAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Text).ToList();
                else
                    datas = GetContent(Program.P.db.GetTextAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Image).ToList();

                // from/to filters
                if (!string.IsNullOrEmpty(Request.Query["from"]))
                {
                    DateTime from;
                    if (!DateTime.TryParseExact(Request.Query["from"], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out from))
                    {
                        return (Response.AsJson(new Response.Error()
                        {
                            Message = "From must be in the format yyyyMMdd"
                        }, HttpStatusCode.BadRequest));
                    }
                    datas.RemoveAll(y => DateTime.ParseExact(y.DateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture) < from);
                }
                if (!string.IsNullOrEmpty(Request.Query["to"]))
                {
                    DateTime to;
                    if (!DateTime.TryParseExact(Request.Query["to"], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out to))
                    {
                        return (Response.AsJson(new Response.Error()
                        {
                            Message = "From must be in the format yyyyMMdd"
                        }, HttpStatusCode.BadRequest));
                    }
                    datas.RemoveAll(y => DateTime.ParseExact(y.DateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture) > to);
                }
                return (Response.AsJson(new Response.Collect()
                {
                    Data = datas.ToArray()
                }));
            });
        }

        /// <summary>
        /// Format content properly for json output
        /// </summary>
        private Response.Data[] GetContent(Cursor<object> items, Response.DataType type)
        {
            List<Response.Data> datas = new List<Response.Data>();
            foreach (dynamic elem in items)
            {
                datas.Add(new Response.Data()
                {
                    DateTime = elem.DateTime,
                    Flags = elem.Flags,
                    UserId = elem.UserId,
                    Type = type
                });
            }
            return (datas.ToArray());
        }
    }
}
