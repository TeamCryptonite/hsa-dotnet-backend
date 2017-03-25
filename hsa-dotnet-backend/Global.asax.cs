using System;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.Web;
using System.Web.Http;
using AutoMapper;
using HsaDotnetBackend.Helpers;
using HsaDotnetBackend.Models;
using HsaDotnetBackend.Models.DTOs;
using Newtonsoft.Json;
using SqlServerTypes;

namespace HsaDotnetBackend
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var httpConfig = GlobalConfiguration.Configuration;

            httpConfig.Formatters.JsonFormatter
                .SerializerSettings
                .ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // DbGeography assemblies
            Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));

            // TODO: Consider moving to it's own config file
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Receipt, ReceiptDto>()
                    .ForMember(dest => dest.ImageUrl,
                        opt => opt.MapFrom(src =>
                            AzureBlobHelper.GetReceiptImageUrl(src.ImageRef)
                        ));
                cfg.CreateMap<ReceiptDto, Receipt>()
                    .ForMember(dest => dest.Provisional, opt => opt.Ignore())
                    .ForMember(dest => dest.WaitingForOcr, opt => opt.Ignore());
                cfg.CreateMap<Receipt, ReceiptDto>();
                cfg.CreateMap<LineItem, LineItemDto>().ReverseMap();
                cfg.CreateMap<Product, ProductDto>().ReverseMap();
                cfg.CreateMap<Store, StoreDto>().ReverseMap();
                cfg.CreateMap<Store, StoreDto>()
                    .ForMember(dest => dest.Location,
                        opt => opt.MapFrom(src =>
                            new LocationDto
                            {
                                Latitude = src.Location.Latitude.Value,
                                Longitude = src.Location.Longitude.Value
                            }));
                cfg.CreateMap<StoreDto, Store>()
                    .ForMember(
                        dest => dest.Location,
                        opt => opt.MapFrom(src =>
                            src.Location.Longitude.HasValue && src.Location.Latitude.HasValue
                                ? DbGeography.FromText(
                                    $"POINT({src.Location.Longitude.Value.ToString(CultureInfo.InvariantCulture)} {src.Location.Latitude.Value.ToString(CultureInfo.InvariantCulture)})")
                                : null
                        ));
                cfg.CreateMap<ShoppingList, ShoppingListDto>().ReverseMap();
                cfg.CreateMap<ShoppingListItem, ShoppingListItemDto>().ReverseMap();
                cfg.CreateMap<Category, CategoryDto>().ReverseMap();
                cfg.CreateMap<User, UserDto>()
                    .ForMember(
                        dest => dest.UserGuid,
                        opt => opt.MapFrom(src =>
                            src.UserObjectId.ToString()));
                cfg.CreateMap<UserDto, User>()
                    .ForMember(
                        dest => dest.UserObjectId,
                        opt => opt.MapFrom(src =>
                            Guid.Parse(src.UserGuid)));
            });

            //amConfig.AssertConfigurationIsValid();
            //Mapper.Initialize(amConfig);
        }
    }
}