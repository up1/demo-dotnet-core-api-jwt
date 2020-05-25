using System;
using Microsoft.AspNetCore.Identity;

namespace demo_jwt.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
