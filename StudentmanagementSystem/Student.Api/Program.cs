using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Student.Api.Middleware;
using Student.Core.Interfaces;
using Student.Core.Services;
using Student.Infrastructure.Data;
using Student.Infrastructure.Repositories;

namespace Student.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configure Serilog Logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            // 2. Add Controllers
            builder.Services.AddControllers();

            // 3. Configure Database Context (SQL Server with SQLite fallback for seamless execution)
            bool useSqlite = builder.Configuration.GetValue<bool>("UseSqlite");
            string defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
            string sqliteConnectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=student_management.db";

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (useSqlite)
                {
                    options.UseSqlite(sqliteConnectionString);
                }
                else
                {
                    try
                    {
                        options.UseSqlServer(defaultConnectionString, sqlOptions =>
                        {
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 3,
                                maxRetryDelay: TimeSpan.FromSeconds(5),
                                errorNumbersToAdd: null);
                        });
                    }
                    catch
                    {
                        // Fallback to SQLite if SQL Server connection setup fails
                        options.UseSqlite(sqliteConnectionString);
                    }
                }
            });

            // 4. Register Dependency Injection Services & Repositories
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // 5. Configure JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"] ?? "ZestIndiaTechnicalAssignmentSecretKeyForJWTAuth2026!";
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "StudentApi";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "StudentApiClients";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // 6. Configure Swagger with JWT Bearer Token Security
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Student Management API",
                    Version = "v1",
                    Description = "RESTful API for Student Management System - Zest India Technical Assignment"
                });

                // Add JWT Security Definition to Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste your raw JWT token below (do NOT include 'Bearer ' prefix)"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // 7. Enable CORS for Web UI
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // 8. Auto-migrate/Ensure Database Created and Seeded with 10 Students
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.EnsureCreated();

                    // Seed all 10 students if not present
                    var seedStudents = new List<Core.Entities.Student>
                    {
                        new Core.Entities.Student { Name = "Aarav Sharma", Email = "aarav.sharma@example.com", Age = 21, Course = "Computer Science", CreatedDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
                        new Core.Entities.Student { Name = "Priya Patel", Email = "priya.patel@example.com", Age = 22, Course = "Information Technology", CreatedDate = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc) },
                        new Core.Entities.Student { Name = "Rohan Verma", Email = "rohan.verma@example.com", Age = 20, Course = "Electronics Engineering", CreatedDate = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc) },
                        new Core.Entities.Student { Name = "Ananya Iyer", Email = "ananya.iyer@example.com", Age = 23, Course = "Data Science", CreatedDate = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc) },
                        new Core.Entities.Student { Name = "Vikramaditya Deshmukh", Email = "vikram.deshmukh@example.com", Age = 21, Course = "Artificial Intelligence", CreatedDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc) },
                        new Core.Entities.Student { Name = "Sneha Kulkarni", Email = "sneha.kulkarni@example.com", Age = 22, Course = "Computer Science", CreatedDate = new DateTime(2026, 4, 18, 0, 0, 0, DateTimeKind.Utc) },
                        new Core.Entities.Student { Name = "Aditya Reddi", Email = "aditya.reddi@example.com", Age = 24, Course = "Cyber Security", CreatedDate = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc) },
                        new Core.Entities.Student { Name = "Kavya Nair", Email = "kavya.nair@example.com", Age = 20, Course = "Information Technology", CreatedDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc) },
                        new Core.Entities.Student { Name = "Siddharth Malhotra", Email = "siddharth.m@example.com", Age = 22, Course = "Cloud Computing", CreatedDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc) },
                        new Core.Entities.Student { Name = "Meera Joshi", Email = "meera.joshi@example.com", Age = 21, Course = "Data Science", CreatedDate = new DateTime(2026, 6, 25, 0, 0, 0, DateTimeKind.Utc) }
                    };

                    bool addedNew = false;
                    foreach (var student in seedStudents)
                    {
                        if (!dbContext.Students.Any(s => s.Email.ToLower() == student.Email.ToLower()))
                        {
                            dbContext.Students.Add(student);
                            addedNew = true;
                        }
                    }

                    if (addedNew)
                    {
                        dbContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while initializing/seeding the database.");
                }
            }

            // 9. Exception Handling Middleware
            app.UseMiddleware<ExceptionMiddleware>();

            // 10. HTTP Pipeline Configuration
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Management API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseStaticFiles(); // For serving Web UI
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            Log.Information("Starting Student Management System Web API...");
            app.Run();
        }
    }
}
