using JwtAuthDemo.Model.Entity;

namespace JwtAuthDemo.Data
{
    public interface IUserRepository
    {
        Task<AppUser?> GetUserByUsernameAsync(string username);
        Task<AppUser?> GetUserByEmailAsync(string email);
        Task<AppUser> RegisterUserAsync(AppUser user);
        Task<AppUser> DeleteUserAsync(int id);
        Task<AppUser> UpdatePasswordAsync(string username, string passwordHash);
        Task<IEnumerable<AppUser>> GetAllUsersAsync();
    }

}