using eComm.Services.ShoppingCartAPI.Models.Dto;

namespace eComm.Services.ShoppingCartAPI.Service.IService
{
    public interface ICouponService
    {
        Task<CouponDto> GetCoupon(string couponCode);
    }
}
