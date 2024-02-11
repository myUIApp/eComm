using AutoMapper;
using eComm.MessageBus;
using eComm.Services.ShoppingCartAPI.Data;
using eComm.Services.ShoppingCartAPI.Models;
using eComm.Services.ShoppingCartAPI.Models.Dto;
using eComm.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace eComm.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private IMapper _mapper;
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private ResponseDto _response;

        public CartAPIController(AppDbContext db, IMapper mapper,
            IProductService productService,
            ICouponService couponService,
            IConfiguration configuration,
            IMessageBus messageBus)
        {
            _db = db;
            _mapper = mapper;
            _productService = productService;
            _couponService = couponService;
            _configuration = configuration;
            _messageBus = messageBus;
            _response = new ResponseDto();
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                CartDto cart = new()
                {
                    CartHeader = _mapper.Map<CartHeaderDto>(_db.CartHeaders.First(c => c.UserId == userId))
                };
                cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(_db.CartDetails
                    .Where(e => e.CartHeaderId == cart.CartHeader.CartHeaderId));

                IEnumerable<ProductDto> productDtos = await _productService.GetProducts();

                foreach (var item in cart.CartDetails)
                {
                    item.Product = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
                    cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }
                //apply Coupon code if any
                //if (cart.CartHeader.CouponCode != null)
                //{
                //    CouponDto couponDto = await _couponService.GetCoupon(cart.CartHeader.CouponCode);
                //    if (couponDto != null && cart.CartHeader.CartTotal > couponDto.MinAmount)
                //    {
                //        cart.CartHeader.CartTotal -= couponDto.DiscountAmount;
                //        cart.CartHeader.Discount = couponDto.DiscountAmount;
                //    }
                //}


                _response.Result = cart;

            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();                
            }
            return _response;
        }
        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
                _db.CartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();
                _response.Result = true; 
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message.ToString();
            }
            return _response;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponCode = "";
                _db.CartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message.ToString();
            }
            return _response;
        }

        [HttpPost("Cartupsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                //var cartHeaderFromDb = await _db.CartHeaders.FirstOrDefaultAsync(e => e.UserId == cartDto.CartHeader.UserId);
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == cartDto.CartHeader.UserId);
                if (cartHeaderFromDb == null)
                {
                    // Create Cart header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    _db.CartHeaders.Add(cartHeader);
                    await _db.SaveChangesAsync();
                    cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                    _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // if header is not null
                    //Check if details has same product
                    var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(u =>
                            u.ProductId == cartDto.CartDetails.First().ProductId &&
                            u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                    if (cartDetailsFromDb == null)
                    {
                        //create Cartdetails
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        // Update count in Cart details
                        cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                        _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();

                    }
                }
                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
        {
            try
            {
                CartDetails cartDetailsFromDb = _db.CartDetails.First(e => e.CartDetailsId == cartDetailsId);

                int totalCountOfCartItem = _db.CartDetails.Where(u =>
                        u.CartHeaderId == cartDetailsFromDb.CartHeaderId).Count();
                _db.CartDetails.Remove(cartDetailsFromDb);
                if (totalCountOfCartItem == 1)
                {
                    var cartHeaderToRemove = await _db.CartHeaders.FirstOrDefaultAsync(e =>
                    e.CartHeaderId == cartDetailsFromDb.CartHeaderId);
                    
                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }
                await _db.SaveChangesAsync();
                
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpPost("EmailCartRequest")]
        public async Task<object> EmailCartRequest([FromBody] CartDto cartDto)
        {
            try
            {
                await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));
                
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message.ToString();
            }
            return _response;
        }

    }
}
