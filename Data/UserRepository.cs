using JwtAuthDemo.Data;
using JwtAuthDemo.Model.Entity;
using Microsoft.EntityFrameworkCore;
using System.Net.Security;

namespace JwtAuthDemo.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            return await _context.AppUsers.ToListAsync();
        }

        public async Task<AppUser?> GetUserByEmailAsync(string email)
        {
            return await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<AppUser?> GetUserByUsernameAsync(string username)
        {
            return await _context.AppUsers.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<AppUser> RegisterUserAsync(AppUser user)
        {
            await _context.AppUsers.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<AppUser?> DeleteUserAsync(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user != null)
            {
                _context.AppUsers.Remove(user);
                await _context.SaveChangesAsync();
            }
            return user;
        }

        public async Task<AppUser?> UpdatePasswordAsync(string username, string passwordHash)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserName == username);
            if (user != null)
            {
                user.Passwordhash = passwordHash;
                _context.AppUsers.Update(user);
                await _context.SaveChangesAsync();
            }
            return user;
        }
    }
}
