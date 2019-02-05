using Nancy;

namespace Uberback.Endpoint
{
    public class Base : NancyModule
    {
        public Base() : base("/")
        {
            Get("/", x =>
            {
                return (Response.AsJson(new Response.Error()
                {
                    Code = 200,
                    Message = "Ok"
                }));
            });
        }
    }
}
