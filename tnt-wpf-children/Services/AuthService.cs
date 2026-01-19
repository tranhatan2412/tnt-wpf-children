using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using tnt_wpf_children.Data;

namespace tnt_wpf_children.Services
{
    public class AuthService
    {
        private static AuthService _instance;
        public static AuthService Instance => _instance ??= new AuthService();

        private AuthService() { }

        public string HashPasswordShort(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hex = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

            return hex.Substring(0, Math.Min(20, hex.Length));
        }

        public bool VerifyAdminPassword(string username, string password)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var admin = db.Admins.FirstOrDefault(x => x.Username == username);
                    if (admin == null) return false;

                    string hashPwd = HashPasswordShort(password);
                    return admin.PasswordHash == hashPwd;
                }
            }
            catch
            {
                return false;
            }
        }
        
        public bool VerifyAnyAdminPassword(string password)
        {
             try
            {
                using (var db = new AppDbContext())
                {
                    
                    string hashPwd = HashPasswordShort(password);
                    return db.Admins.Any(a => a.PasswordHash == hashPwd);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
