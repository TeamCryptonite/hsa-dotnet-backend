using System.Linq;
using System.Web.Http;
using AutoMapper;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;

namespace HsaDotnetBackend
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            HttpConfiguration httpConfig = GlobalConfiguration.Configuration;

            httpConfig.Formatters.JsonFormatter
                .SerializerSettings
                .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            // AutoMapper
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Receipt, ReceiptDto>();
                cfg.CreateMap<LineItem, LineItemDto>();
                cfg.CreateMap<Product, ProductDto>();
            });
            //amConfig.AssertConfigurationIsValid();
            //Mapper.Initialize(amConfig);
        }
    }
}
