using Nancy;

namespace Uberback.Endpoint
{
    public class Base : NancyModule
    {
        public Base() : base("/")
        {
            Get("/", x =>
            {
                return (Response.AsJson(new Response.Information()
                {
                    Message = "Refer to https://github.com/Uberschutz/Uberback/wiki for the list of endpoints"
                }, HttpStatusCode.OK));
            });
        }
    }
}
