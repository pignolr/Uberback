using Nancy;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Uberback.Response;

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

                Validator.Collect.ValidatorResponse error = null;
                if ((error = Validator.Collect.ValidateRequest(args)) != null)
                    return Response.AsJson(new Response.Error() { Message = error.Message }, error.StatusCode);

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
                if (!string.IsNullOrEmpty(args.Get("userId")))
                {
                    datas.RemoveAll(y => y.UserId != args.Get("userId"));
                }

                // Get only datas corresponding to an id
                if (!string.IsNullOrEmpty(args.Get("services")))
                {
                    string[] services = args.Get("services").Split(';');
                    datas.RemoveAll(y => !services.Any(z => z == y.Service));
                }

                // from/to filters
                if (!string.IsNullOrEmpty(args.Get("from")))
                {
                    DateTime from = DateTime.ParseExact(args.Get("from"), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    datas.RemoveAll(y => DateTime.ParseExact(y.DateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture) < from);
                }
                if (!string.IsNullOrEmpty(args.Get("to")))
                {
                    DateTime to = DateTime.ParseExact(args.Get("to"), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    datas.RemoveAll(y => DateTime.ParseExact(y.DateTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture) > to);
                }

                Dictionary<string, FlagData[]> finalDatas = new Dictionary<string, FlagData[]>();
                Dictionary<string, Dictionary<string, double>> flags = new Dictionary<string, Dictionary<string, double>>();
                Dictionary<string, int> counters = new Dictionary<string, int>();
                counters.Add("All", 0);
                flags.Add("All", new Dictionary<string, double>());
                foreach (var elem in datas)
                {
                    if (!flags.ContainsKey(elem.Service))
                    {
                        flags.Add(elem.Service, new Dictionary<string, double>());
                        counters.Add(elem.Service, 1);
                    }
                    else
                    {
                        counters[elem.Service]++;
                    }
                    counters["All"]++;
                    foreach (string s in elem.Flags.Split(','))
                    {
                        if (!flags[elem.Service].ContainsKey(s))
                            flags[elem.Service].Add(s, 1);
                        else
                            flags[elem.Service][s]++;
                        if (!flags["All"].ContainsKey(s))
                            flags["All"].Add(s, 1);
                        else
                            flags["All"][s]++;
                    }
                }
                foreach (var elem in flags)
                {
                    List<FlagData> tmpDatas = new List<FlagData>();
                    double sum = elem.Value.Sum(y => y.Value);
                    foreach (var elem2 in elem.Value)
                    {
                        tmpDatas.Add(new FlagData()
                        {
                            Name = elem2.Key,
                            Value = elem2.Value / counters[elem.Key] * 100f,
                            PercentValue = elem2.Value / sum * 100f
                        });
                    }
                    finalDatas.Add(elem.Key, tmpDatas.ToArray());
                }
                return (Response.AsJson(new Response.Collect()
                {
                    Datas = finalDatas
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
                    Type = type,
                    Service = elem.Service
                });
            }
            return (datas.ToArray());
        }
    }
}
