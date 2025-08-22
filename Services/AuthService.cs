using MES.Data;
using MES.Models;


namespace MES.Services
{
    public class AuthService
    {
        public User Login(string username, string password)
        {
            using (var context = new MESDbContext())
            {
                return context.Users
                    .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);
            }
        }
    }
}
