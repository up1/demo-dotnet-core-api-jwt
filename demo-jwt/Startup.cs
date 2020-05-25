using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using demo_jwt.Data;
using demo_jwt.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using demo_jwt.Configuration;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace demo_jwt
    {
        public class Startup
        {
            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public IConfiguration Configuration { get; }

            public void ConfigureServices(IServiceCollection services)
            {
                // Use In-memory database
                services.AddDbContext<ApplicationDbContext>(config =>
                { 
                   config.UseInMemoryDatabase("MemoryBaseDataBase");
                });

                // Add Identity
                services.AddIdentity<AppUser, IdentityRole>(config =>
                {
                    // User defined password policy settings.  
                    config.Password.RequiredLength = 4;
                    config.Password.RequireDigit = false;
                    config.Password.RequireNonAlphanumeric = false;
                    config.Password.RequireUppercase = false;
                })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

                // Add JWT token
                var jwtSection = Configuration.GetSection("JwtBearerTokenSettings");
                services.Configure<JwtBearerTokenSettings>(jwtSection);
                var jwtBearerTokenSettings = jwtSection.Get<JwtBearerTokenSettings>();
                var key = Encoding.ASCII.GetBytes(jwtBearerTokenSettings.SecretKey);

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = jwtBearerTokenSettings.Issuer,
                        ValidAudience = jwtBearerTokenSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                    };
                });

                services.AddControllers();
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseHttpsRedirection();

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
        }
    }
