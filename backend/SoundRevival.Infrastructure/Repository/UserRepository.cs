using Microsoft.EntityFrameworkCore;
using Npgsql;
using SoundRevival.Core.Entities;
using SoundRevival.Core.Exceptions;
using SoundRevival.Core.Interfaces;

namespace SoundRevival.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }


        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        public async Task<User?> FindUserByEmail(string email)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            return existingUser;       
        }

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();

                return result != 0;
              
            }
            catch(DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new UniqueConstraintViolationException("A unique constraint was violated while saving changes.", ex);
            }
        }
    }
}
