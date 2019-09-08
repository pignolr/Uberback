﻿using Nancy;
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
                var args = Common.ParseArgs(Request.Body);

                // Error Handling
                Common.Answer? error = Common.BasicCheck(args.Get("token"));
                if (error.HasValue)
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = error.Value.message
                    }, error.Value.code));
                if (!string.IsNullOrEmpty(args.Get("type")) && args.Get("type") != "image" && args.Get("type") != "text")
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "If set, type must be text or image"
                    }, HttpStatusCode.BadRequest));

                // Getting datas
                List<Response.Data> datas;
                if (string.IsNullOrEmpty(args.Get("type")))
                {
                    datas = GetContent(Program.P.db.GetImageAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Image).ToList();
                    datas.AddRange(GetContent(Program.P.db.GetTextAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Text).ToList());
                }
                else if (args.Get("type") == "text")
                    datas = GetContent(Program.P.db.GetTextAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Text).ToList();
                else
                    datas = GetContent(Program.P.db.GetTextAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Image).ToList();

                // from/to filters
                if (!string.IsNullOrEmpty(args.Get("from")))
                {
                    DateTime from;
                    if (!DateTime.TryParseExact(args.Get("from"), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out from))
                    {
                        return (Response.AsJson(new Response.Error()
                        {
                            Message = "From must be in the format yyyyMMdd"
                        }, HttpStatusCode.BadRequest));
                    }
                    datas.RemoveAll(y => DateTime.ParseExact(y.DateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture) < from);
                }
                if (!string.IsNullOrEmpty(args.Get("to")))
                {
                    DateTime to;
                    if (!DateTime.TryParseExact(args.Get("to"), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out to))
                    {
                        return (Response.AsJson(new Response.Error()
                        {
                            Message = "From must be in the format yyyyMMdd"
                        }, HttpStatusCode.BadRequest));
                    }
                    datas.RemoveAll(y => DateTime.ParseExact(y.DateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture) > to);
                }
                int totalToxicity = datas.Where(y => y.Flags != "SAFE").Count();
                return (Response.AsJson(new Response.Collect()
                {
                    Data = datas.ToArray(),
                    TotalToxicity = totalToxicity * 100 / datas.Count
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
