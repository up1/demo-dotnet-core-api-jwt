using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using demo_jwt.Configuration;
using demo_jwt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace demo_jwt.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtBearerTokenSettings jwtBearerTokenSettings;
        private readonly UserManager<AppUser> userManager;

        public AuthController(IOptions<JwtBearerTokenSettings> jwtTokenOptions, UserManager<AppUser> userManager)
        {
            this.jwtBearerTokenSettings = jwtTokenOptions.Value;
            this.userManager = userManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] AppUser appUser)
        {
            if (appUser == null)
            {
                return new BadRequestObjectResult(new { Message = "User validation failed" });
            }
            var newUser = new AppUser
            {
                Id = new Random().Next(1, 100).ToString(),
                Name = appUser.Name,
                UserName = appUser.Name,
                Password = appUser.Password,
                Email = appUser.Email
            };

            var result = await userManager.CreateAsync(newUser, appUser.Password);
            if(!result.Succeeded)
            {
                var dictionary = new ModelStateDictionary();
                foreach (IdentityError error in result.Errors)
                {
                    dictionary.AddModelError(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new { Message = "User registration failed", Errors = dictionary });
            }

            return Ok(new { Message = "User registeration successful"});
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] AppUser appUser)
        {
            if (appUser == null)
            {
                return new BadRequestObjectResult(new { Message = "Login validate failed" });
            }

            var identityUser = await userManager.FindByNameAsync(appUser.Name);
            if(identityUser == null)
            {
                return new BadRequestObjectResult(new { Message = "User not found" });
            }

            var result = userManager.PasswordHasher.VerifyHashedPassword(identityUser, identityUser.PasswordHash, appUser.Password);
            if(result == PasswordVerificationResult.Failed)
            {
                return new BadRequestObjectResult(new { Message = "Password incorrect" });
            }

            var token = GenerateToken(identityUser);
            return Ok(new { Token = token, Message = "Success" });
        }

        private object GenerateToken(AppUser identityUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtBearerTokenSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, identityUser.UserName.ToString()),
                    new Claim(ClaimTypes.Email, identityUser.Email)
                }),

                Expires = DateTime.UtcNow.AddSeconds(jwtBearerTokenSettings.ExpiryTimeInSeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = jwtBearerTokenSettings.Audience,
                Issuer = jwtBearerTokenSettings.Issuer
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
