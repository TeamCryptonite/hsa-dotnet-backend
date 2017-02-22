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
                cfg.CreateMap<Receipt, ReceiptDto>().ReverseMap();
                cfg.CreateMap<LineItem, LineItemDto>().ReverseMap();
                cfg.CreateMap<Product, ProductDto>().ReverseMap();
                cfg.CreateMap<Store, StoreDto>().ReverseMap();
                cfg.CreateMap<ShoppingList, ShoppingListDto>().ReverseMap();
                cfg.CreateMap<ShoppingListItem, ShoppingListItemDto>().ReverseMap();
                cfg.CreateMap<Category, CategoryDto>().ReverseMap();
            });
            //amConfig.AssertConfigurationIsValid();
            //Mapper.Initialize(amConfig);
        }
    }
}
