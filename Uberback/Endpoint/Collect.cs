using Nancy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private List<dynamic> GetContent(Cursor<object> items)
        {
            List<dynamic> allElems = new List<dynamic>();
            foreach (dynamic elem in items)
            {
                allElems.Add(elem.ToString().Replace(Environment.NewLine, ""));
            }
            return (allElems);
        }
    }
}
