using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Domain.Enums
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Manager = "Manager";
    }

    public enum UserRole
    {
        User = 1,
        Manager = 2,
        Admin = 3
    }
}
