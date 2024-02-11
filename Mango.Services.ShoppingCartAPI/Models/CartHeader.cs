using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eComm.Services.ShoppingCartAPI.Models
{
    public class CartHeader
    {
        [Key]
        public int CartHeaderId { get; set; }
        public string? UserId { get; set; }
        public string? CouponCode { get; set; }

        [NotMapped] // Do not need this field to add to db
        public double Discount { get; set; }
        [NotMapped]
        public double CartTotal { get; set;}
    }
}
