using eComm.Services.ShoppingCartAPI.Models.Dto;

namespace eComm.Services.ShoppingCartAPI.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProducts();
    }
}
