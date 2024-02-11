using Microsoft.AspNetCore.Identity;

namespace eComm.Services.AuthAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
