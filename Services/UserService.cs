using Void.Models;
using Void.Repositories;

namespace Void.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public List<User> GetAll() => _userRepository.GetAll();

        public User? GetById(int id) => _userRepository.GetById(id);

        public List<User> FindByName(string name) => _userRepository.FindByName(name);

        public bool Delete(int id) => _userRepository.Delete(id);

        public bool Update(int id, User updateUser) => _userRepository.Update(id, updateUser);

        public UserRepository Get_userRepository()
        {
            return _userRepository;
        }

        public void Register(User newUser, string confirmPassword, UserRepository _userRepository)
        {
            ValidatePassword(newUser.Password, confirmPassword);
            ValidateEmail(newUser.Email);

            if (_userRepository.UserExists(newUser.UserName))
                throw new ArgumentException("Username already exists");
            _userRepository.AddUser(newUser);
        }

        public bool ValidateLogin(string username, string password)
        {
            var user = _userRepository.FindByName(username).FirstOrDefault();
            return user != null && user.Password == password;
        }


        private void ValidatePassword(string password, string confirmPassword)
        {
            if (password != confirmPassword)
                throw new ArgumentException("Passwords do not match");

            if (password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters");
        }

        private void ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                throw new ArgumentException("Invalid email format");
        }

        internal void AddUser(User user)
        {
            throw new NotImplementedException();
        }


    }
}