using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WeatherAPI
{
    public class UsersRepository
    {
        private readonly List<User> _users;

        public UsersRepository()
        {
            // Read user credentials from a file "users.txt", this is just for demonstration purpose
            _users = File.ReadLines("users.txt")
                .Select(line => line.Split(','))
                .Select(parts => new User { Username = parts[0], Password = parts[1] })
                .ToList();
        }

        public User GetUser(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }
    }
}
