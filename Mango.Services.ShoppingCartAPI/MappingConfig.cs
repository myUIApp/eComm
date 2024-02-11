using AutoMapper;
using eComm.Services.ShoppingCartAPI.Models;
using eComm.Services.ShoppingCartAPI.Models.Dto;

namespace eComm.Services.ShoppingCartAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<CartHeader, CartHeaderDto>().ReverseMap();
                config.CreateMap<CartDetails, CartDetailsDto>().ReverseMap();
            });
            return mappingConfig;
        }

    }
}
