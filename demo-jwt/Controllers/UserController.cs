using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using demo_jwt.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace demo_jwt.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;

        public UserController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> UserInfo()
        {
            return Ok(new { Message = "Pass authorized" });
        }
    }
}
