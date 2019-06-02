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
        public Collect() : base("/collect")
        {
            Post("/", x =>
            {
                if (string.IsNullOrEmpty(Request.Query["token"]))
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "Missing arguments"
                    }, HttpStatusCode.BadRequest));
                if (Request.Query["token"] != Program.P.token)
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "Bad token"
                    }, HttpStatusCode.Unauthorized));
                if (!string.IsNullOrEmpty(Request.Query["type"]) && Request.Query["type"] != "image" && Request.Query["type"] != "text")
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "If set, type must be text or image"
                    }, HttpStatusCode.BadRequest));
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
