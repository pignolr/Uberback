using Nancy;

namespace Uberback.Endpoint
{
    public class Data : NancyModule
    {
        /// <summary>
        /// Post informationx
        /// </summary>
        public Data() : base("/data")
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
                if (string.IsNullOrEmpty(args.Get("type")) || string.IsNullOrEmpty(args.Get("userId")) || string.IsNullOrEmpty(args.Get("flags")) || string.IsNullOrEmpty(args.Get("token")))
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "Missing arguments"
                    }, HttpStatusCode.BadRequest));

                // Add information to the db
                switch (args.Get("type").ToString())
                {
                    case "text":
                        Program.P.db.AddTextAsync(args.Get("flags"), args.Get("userId")).GetAwaiter().GetResult();
                        break;

                    case "image":
                        Program.P.db.AddImageAsync(args.Get("flags"), args.Get("userId")).GetAwaiter().GetResult();
                        break;

                    default:
                        return (Response.AsJson(new Response.Error()
                        {
                            Message = "Type must be text or image"
                        }, HttpStatusCode.BadRequest));
                }
                return (Response.AsJson(new Response.Empty(), HttpStatusCode.NoContent));
            });
        }
    }
}
