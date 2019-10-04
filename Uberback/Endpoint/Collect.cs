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
            base.Post("/", x =>
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
                    datas = GetContent(Program.P.db.GetImageAsync().GetAwaiter().GetResult(), Uberback.Response.DataType.Image).ToList();

                // Get only datas corresponding to an id
                if (!string.IsNullOrEmpty(args.Get("id")))
                {
                    datas.RemoveAll(y => y.UserId != args.Get("id"));
                }

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
                            Message = "To must be in the format yyyyMMdd"
                        }, HttpStatusCode.BadRequest));
                    }
                    datas.RemoveAll(y => DateTime.ParseExact(y.DateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture) > to);
                }

                var paginationError = ExtractDataFromPagination(args, datas);
                if (paginationError != null)
                    return paginationError;

                Dictionary<string, double> flags = new Dictionary<string, double>();
                int counter = 0;
                foreach (var elem in datas)
                {
                    foreach (string s in elem.Flags.Split(','))
                    {
                        if (!flags.ContainsKey(s))
                            flags.Add(s, 1);
                        else
                            flags[s]++;
                    }
                    counter++;
                }
                Dictionary<string, double> finalFlags = new Dictionary<string, double>();
                foreach (var elem in flags)
                {
                    finalFlags.Add(elem.Key, elem.Value * 100 / counter);
                }
                return (Response.AsJson(new Response.Collect()
                {
                    Data = datas.ToArray(),
                    FlagsPercentage = finalFlags
                }));
            });
        }

        private Nancy.Response ExtractDataFromPagination(System.Collections.Specialized.NameValueCollection args, List<Response.Data> datas)
        {
            int nbElem = -1;
            int page = 1;
            if (!string.IsNullOrEmpty(args.Get("nbelem")))
            {
                if (!int.TryParse(args.Get("nbelem"), out nbElem) || (nbElem <= 0 && nbElem != -1))
                {
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "Nbelem must be a positive integer"
                    }, HttpStatusCode.BadRequest));
                }
            }
            if (!string.IsNullOrEmpty(args.Get("page")))
            {
                if (!int.TryParse(args.Get("page"), out page) || page <= 0)
                {
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "Page must be a positive integer"
                    }, HttpStatusCode.BadRequest));
                }
                if (nbElem == -1)
                    nbElem = 20;
                if (datas.Count() <= (page - 1) * nbElem)
                {
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "Page is Invalid"
                    }, HttpStatusCode.BadRequest));
                }
            }
            if (nbElem == -1)
                return null;

            datas.RemoveRange(0, (page - 1) * nbElem);
            if (datas.Count() > nbElem)
                datas.RemoveRange(nbElem, datas.Count() - nbElem);
            return null;
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
