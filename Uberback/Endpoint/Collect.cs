using Nancy;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;

namespace Uberback.Endpoint
{
    public class Collect : NancyModule
    {
        public Collect() : base("/collect")
        {
            Post("/", x =>
            {
                if (string.IsNullOrEmpty(Request.Query["type"]) || string.IsNullOrEmpty(Request.Query["token"]))
                    return (Response.AsJson(new Response.Error()
                    {
                        Code = 400,
                        Message = "Missing arguments"
                    }, HttpStatusCode.BadRequest));
                if (Request.Query["token"] != Program.P.token)
                    return (Response.AsJson(new Response.Error()
                    {
                        Code = 401,
                        Message = "Bad token"
                    }, HttpStatusCode.Unauthorized));
                switch (Request.Query["type"].ToString())
                {
                    case "text":
                        return (Response.AsJson(new Response.Collect()
                        {
                            Code = 200,
                            Data = GetContent(Program.P.db.GetTextAsync().GetAwaiter().GetResult())
                        }));

                    case "image":
                        return (Response.AsJson(new Response.Collect()
                        {
                            Code = 200,
                            Data = GetContent(Program.P.db.GetImageAsync().GetAwaiter().GetResult())
                        }));

                    default:
                        return (Response.AsJson(new Response.Error()
                        {
                            Code = 400,
                            Message = "Type must be text or image"
                        }, HttpStatusCode.BadRequest));
                }
            });
        }

        private Uberback.Response.Data[] GetContent(Cursor<object> items)
        {
            List<Uberback.Response.Data> datas = new List<Response.Data>();
            foreach (dynamic elem in items)
            {
                datas.Add(new Uberback.Response.Data()
                {
                    DateTime = elem.DateTime,
                    Flags = elem.Flags,
                    UserId = elem.UserId
                });
            }
            return (datas.ToArray());
        }
    }
}
