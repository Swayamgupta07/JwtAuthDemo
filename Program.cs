using AutoMapper;
using JwtAuthDemo.Data;
using JwtAuthDemo.Mapping;
using JwtAuthDemo.Middleware;
using JwtAuthDemo.Model.Entity;
using JwtAuthDemo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
namespace JwtAuthDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args); 
            
            Log.Logger = new LoggerConfiguration()
                                        .MinimumLevel.Debug()
                                        .WriteTo.Console()
                                        .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                                        .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<ITokenService, TokenService>();
            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'JwtAuthDemoContext' not found.")));


            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes("ThisIsMySuperSecretKey12345"))
                    };
                });


            builder.Services.AddAutoMapper(typeof(AutoMapperProfile)); 

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseExceptionalHandlingMiddleware();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
