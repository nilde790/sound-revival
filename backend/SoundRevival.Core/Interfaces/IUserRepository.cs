using System;
using System.Collections.Generic;
using System.Text;
using SoundRevival.Core.Entities;

namespace SoundRevival.Core.Interfaces
{
    public interface IUserRepository
    {
        public Task<User?> FindUserByEmail(string email);

        public void AddUser(User user);

        public Task<bool> SaveChangesAsync();


    }
}
