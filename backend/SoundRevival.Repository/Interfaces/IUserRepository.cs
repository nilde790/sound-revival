using System;
using System.Collections.Generic;
using System.Text;
using SoundRevival.Repository.Entities;

namespace SoundRevival.Repository.Interfaces
{
    public interface IUserRepository
    {
        public Task<User?> FindUserByEmail(string email);

        public void AddUser(User user);

        public Task<bool> SaveChangesAsync();


    }
}
