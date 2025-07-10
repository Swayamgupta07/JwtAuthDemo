using JwtAuthDemo.Model.Entity;

namespace JwtAuthDemo.Services
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
