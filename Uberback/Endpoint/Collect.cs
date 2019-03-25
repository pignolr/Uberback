using Nancy;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
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
                if (string.IsNullOrEmpty(Request.Query["type"]))
                {
                    List<Response.Data> datas = GetContent(Program.P.db.GetImageAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Image).ToList();
                    datas.AddRange(GetContent(Program.P.db.GetTextAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Text).ToList());
                    return (Response.AsJson(new Response.Collect()
                    {
                        Data = datas.ToArray()
                    }));
                }
                switch (Request.Query["type"].ToString())
                {
                    case "text":
                        return (Response.AsJson(new Response.Collect()
                        {
                            Data = GetContent(Program.P.db.GetTextAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Text)
                        }));

                    case "image":
                        return (Response.AsJson(new Response.Collect()
                        {
                            Data = GetContent(Program.P.db.GetImageAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Image)
                        }));

                    default:
                        return (Response.AsJson(new Response.Error()
                        {
                            Message = "Type must be text or image"
                        }, HttpStatusCode.BadRequest));
                }
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
