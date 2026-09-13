using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Academic_tracker.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int UserID { get; set; }
        public string Username { get; set; }
        [Unique]
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        // BCrypt hash of the recovery code shown at registration. Null for accounts created before recovery codes existed.
        public string? RecoveryCodeHash { get; set; }
    }
}
