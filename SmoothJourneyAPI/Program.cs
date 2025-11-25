using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
//using SmoothJourneyAPI.Data;
//using SmoothJourneyAPI.Interfaces;
//using SmoothJourneyAPI.Repositories;
//using SmoothJourneyAPI.Services;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SmoothJourneyAPI.Data;

namespace SmoothJourneyAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<SmoothJourneyDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SmoothJourneyDbContext") ?? throw new InvalidOperationException("Connection string 'SmoothJourneyDbContext' not found.")));
            //builder.Services.AddDbContext<SmoothJourneyDbContext>(options =>
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("SmoothJourneyDbContext") ?? throw new InvalidOperationException("Connection string 'SmoothJourneyDbContext' not found.")));
            //builder.Services.AddDbContext<SmoothJourneyDbContext>(options =>
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("SmoothJourneyDbContext") ?? throw new InvalidOperationException("Connection string 'SmoothJourneyDbContext' not found.")));
            
            //builder.Services.AddScoped<IUserRepository, UserRepository>();
            //builder.Services.AddScoped<IAuthService, AuthService>();

            //// Password service injection
            //builder.Services.AddSingleton<PasswordService>(); 

            // AutoMapper
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // JWT setup
            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // true in prod
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true
                };
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
