using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MemoryVaultAPI.Models;

namespace MemoryVaultAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminRights",
                    policy => policy.RequireRole("Admin"));
                options.AddPolicy("LoginTokens",
                    policy => policy.RequireRole("LoginToken"));
                options.AddPolicy("PasswordTokens",
                    policy => policy.RequireRole("PasswordToken"));
            });

            string corsPolicy = "localhostPolicy";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: corsPolicy,
                                    policy =>
                                    {
                                        policy.WithOrigins("http://localhost:4200");
                                        policy.WithMethods("GET", "POST", "PUT", "DELETE", "HEAD", "OPTIONS");
                                        policy.WithHeaders("Access-Control-Allow-Origin", "Access-Control-Allow-Headers", "Authorization", "Origin", "Accept", "X-Requested-With", "Content-Type", "Access-Control-Request-Method", "Access-Control-Request-Headers");
                                    }
                                 );
            });

            builder.Services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors(corsPolicy);

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();

            
        }
    }
}
