using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SoundRevival.Repository.Entities;
using SoundRevival.Repository.Interfaces;

namespace SoundRevival.Repository.Repository
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
            var result = await _context.SaveChangesAsync();

            if(result != 0)
            {
                return true;
            }

            return false;
            
        }
    }
}
