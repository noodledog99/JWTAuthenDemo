using JWTAuthenDemo.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace JWTAuthenDemo.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _userCollection;
  
        public UserService(IDbConfig dbConfig)
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(dbConfig.ConnectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            var client = new MongoClient(settings);
            var database = client.GetDatabase(dbConfig.DbName);
            _userCollection = database.GetCollection<User>(dbConfig.Users);
        }

        public IEnumerable<User> GetAllUser()
        {
            return _userCollection.Find(it => true).ToEnumerable<User>();
        }

        public User GetUserById(string username)
        {
            return _userCollection.Find(it => it.UserId == username).FirstOrDefault();
        }

        public User AuthorizUser(string userId, string password)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                throw new ApplicationException("Username or Password is empty.");
            }

            var user = _userCollection.Find(it => it.UserId == userId).FirstOrDefault();

            // check if username exists
            if (user == null)
            {
                throw new ApplicationException("Username does not exist.");
            }

            var bPasswordHash = Convert.FromBase64String(user.PasswordHash);
            var bPasswordSalt = Convert.FromBase64String(user.PasswordSalt);

            //check if password is correct
            if (!VerifyPasswordHash(password, bPasswordHash, bPasswordSalt))
            {
                return null;
            }

            return user;
        }

        public void Create(User user, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ApplicationException("Password is requires");
            }
            if (_userCollection.Find(it => it.UserId == user.UserId).FirstOrDefault() != null)
            {
                throw new ApplicationException("Username \"" + user.UserId + "\" is already taken");
            }

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);


            user.PasswordHash = Convert.ToBase64String(passwordHash);
            user.PasswordSalt = Convert.ToBase64String(passwordSalt);

            _userCollection.InsertOne(user);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hhmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hhmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                    {
                        return false;
                    };
                }
                return true;
            }
        }
    }
}
