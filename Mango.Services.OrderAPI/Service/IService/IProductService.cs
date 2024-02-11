using eComm.Services.OrderAPI.Models.Dto;

namespace eComm.Services.OrderAPI.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProducts();
    }
}
