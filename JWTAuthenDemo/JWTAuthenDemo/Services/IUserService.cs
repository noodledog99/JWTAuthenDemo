using JWTAuthenDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTAuthenDemo.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAllUser();
        User GetUserById(string id);
        void Create(User user, string password);
        User AuthorizUser(string username, string password);
    }
}
