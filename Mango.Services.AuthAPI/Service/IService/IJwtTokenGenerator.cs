using eComm.Services.AuthAPI.Models;

namespace eComm.Services.AuthAPI.Service.IService
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser applicationUser,IEnumerable<string> roles);
    }
}
