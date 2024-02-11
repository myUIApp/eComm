using AutoMapper;
using eComm.Services.ProductAPI.Models;
using eComm.Services.ProductAPI.Models.Dto;

namespace eComm.Services.ProductAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
               // config.CreateMap<ProductDto, Product>().ReverseMap();
                config.CreateMap<ProductDto, Product>();
                config.CreateMap<Product, ProductDto>();
            });
            return mappingConfig;
        }

    }
}
