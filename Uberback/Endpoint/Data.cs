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
                // Error Handling
                Common.Answer? error = Common.BasicCheck(Request.Query["token"]);
                if (error.HasValue)
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = error.Value.message
                    }, error.Value.code));
                if (string.IsNullOrEmpty(Request.Query["type"]) || string.IsNullOrEmpty(Request.Query["userId"]) || string.IsNullOrEmpty(Request.Query["flags"]) || string.IsNullOrEmpty(Request.Query["token"]))
                    return (Response.AsJson(new Response.Error()
                    {
                        Message = "Missing arguments"
                    }, HttpStatusCode.BadRequest));

                // Add information to the db
                switch (Request.Query["type"].ToString())
                {
                    case "text":
                        Program.P.db.AddTextAsync(Request.Query["flags"], Request.Query["userId"]);
                        break;

                    case "image":
                        Program.P.db.AddImageAsync(Request.Query["flags"], Request.Query["userId"]);
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
