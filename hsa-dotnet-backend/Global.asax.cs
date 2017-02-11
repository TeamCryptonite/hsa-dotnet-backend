using System.Web.Http;

namespace HsaDotnetBackend
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            HttpConfiguration config = GlobalConfiguration.Configuration;

            config.Formatters.JsonFormatter
                .SerializerSettings
                .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }
    }
}
