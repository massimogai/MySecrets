using Microsoft.EntityFrameworkCore;
using MySecrets.Model;
using ScuolaRegionale.Services;

namespace MySecrets.Services
{
    public class UserService
    {
        private SecretsDbContext _context;

        public UserService(SecretsDbContext context)
        {
            _context = context;
        }

        public User? FindByUserName(string username)
        {
            var cursor
                =_context.Users.Where(user => user.UserName == username).Include(user =>user.Secrets );
            return  cursor.FirstOrDefault();
        }
    
        public List<User> ListUsers()
        {
            return _context.Users.ToList();   
        }
        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        public void RemoveUser(User user)
        {
            _context.Users.Remove(user); /* TODO verificare cascade sui secrets*/
            _context.SaveChanges();
        }
        public void SaveUser(User user)
        {
            _context.SaveChanges();
        }

        public void CreateAdmin()
        {
            User? admin=FindByUserName("admin");
            if (admin == null)
            {
                admin = new User();
                admin.Password = "123abc";
                admin.UserName = "admin";
                _context.Users.Add(admin);
                SaveUser(admin);

            }
        }

        public void DeleteSecret(User user, MySecret secret)
        {
            user.Secrets.Remove(secret);
            _context.Secrets.Remove(secret);
            SaveUser(user);
        }

        public void AddSecret(User user, MySecret secret)
        {
            user.Secrets.Add(secret);
            _context.Secrets.Add(secret);
            SaveUser(user);
        }

        public void UpdateSecret(User user, MySecret secret)
        {
            SaveUser(user);
        }
    }
}