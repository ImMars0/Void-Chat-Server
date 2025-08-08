using Void.Database;
using Void.Models;


namespace Void.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseContext _context;

        public UserRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public User? GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.UserName == username);
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User? GetById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public bool Delete(int id)
        {
            var user = GetById(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            _context.SaveChanges();
            return true;
        }

        public bool Update(int id, User updateUser)
        {
            var user = GetById(id);
            if (user == null) return false;
            user.UserName = updateUser.UserName;
            user.Password = updateUser.Password;
            user.Email = updateUser.Email;
            _context.Users.Update(user);
            _context.SaveChanges();
            return true;
        }

        public bool UserExists(string username)
        {
            return _context.Users.Any(u => u.UserName == username);
        }
        public bool EmailExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public bool ValidateUser(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));

            return user != null && BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password);
        }

        public List<User> FindByName(string name)
        {
            return _context.Users.Where(u => u.UserName.Contains(name)).ToList();
        }


    }
}
