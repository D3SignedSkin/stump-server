using System.Web.Http;
using Owin;

namespace Stump.Server.WorldServer.WebAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            app.Use<Microsoft.Owin.Host.HttpListener.OwinHttpListener>();
            app.UseWebApi(config);
        }
    }
}