using System;
using System.Data.Entity.Spatial;
using System.Globalization;
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
            SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Receipt, ReceiptDto>().ReverseMap();
                cfg.CreateMap<LineItem, LineItemDto>().ReverseMap();
                cfg.CreateMap<Product, ProductDto>().ReverseMap();
                cfg.CreateMap<Store, StoreDto>().ReverseMap();
                cfg.CreateMap<Store, StoreDto>()
                    .ForMember(dest => dest.Location,
                        opt =>
                            opt.MapFrom(
                                src =>
                                    new LocationDto()
                                    {
                                        Latitude = src.Location.Latitude.Value,
                                        Longitude = src.Location.Longitude.Value
                                    }));
                cfg.CreateMap<StoreDto, Store>()
                    .ForMember(
                        dest => dest.Location, 
                        opt => opt.MapFrom(src => 
                            src.Location.Longitude.HasValue && src.Location.Latitude.HasValue 
                            ? DbGeography.FromText($"POINT({src.Location.Longitude.Value.ToString(CultureInfo.InvariantCulture)} {src.Location.Latitude.Value.ToString(CultureInfo.InvariantCulture)})") 
                            : null
                ));
                cfg.CreateMap<ShoppingList, ShoppingListDto>().ReverseMap();
                cfg.CreateMap<ShoppingListItem, ShoppingListItemDto>().ReverseMap();
                cfg.CreateMap<Category, CategoryDto>().ReverseMap();
            });
            //amConfig.AssertConfigurationIsValid();
            //Mapper.Initialize(amConfig);
        }
    }
}
