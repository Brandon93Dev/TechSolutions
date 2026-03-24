using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TechSolutions_IPS_HW.Data;
using TechSolutions_IPS_HW.Models.User;
using TechSolutions_IPS_HW.Repositories;
using TechSolutions_IPS_HW.Repositories.Interfaces;
using TechSolutions_IPS_HW.Security.Interfaces;
using TechSolutions_IPS_HW.Security.Services;
using TechSolutions_IPS_HW.Services.AdminServices;
using TechSolutions_IPS_HW.Services.AuditServices;
using TechSolutions_IPS_HW.Services.AuthenticationServices;
using TechSolutions_IPS_HW.Services.CustomerServices;
using TechSolutions_IPS_HW.Services.Interfaces;
using TechSolutions_IPS_HW.Services.MailerServices;
using TechSolutions_IPS_HW.Services.NotificationServices;

namespace TechSolutions_IPS_HW.Services;

//Quite like this way of work, isolating DI from Program.cs, worked on projects where these lines absolutely flood
// the progarm.cs file, making it harder to add new registrations and manage regitration order
public static class DependencyInjectionService
{
    public static IServiceCollection AddAppServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        //Db
        var connectionString = configuration.
            GetConnectionString("DefaultConnection") ?? 
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();

        // Microsoft's identity privider
        services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                // Brute-force protection: lock account after 3 failed attempts for 24 hours
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(24);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Custom sign in manager (for external clients)
        services.AddScoped<SignInManager<ApplicationUser>, AppSignInManagerService>();

        // MVC / Razor
        services.AddControllersWithViews();
        services.AddRazorPages();

        // JWT Bearer for API clients  
        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];
        if (!string.IsNullOrEmpty(jwtKey) && !string.IsNullOrEmpty(jwtIssuer) && !string.IsNullOrEmpty(jwtAudience))
        {
            services.AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                });
        }

        // Authorization (reads approver roles from configuration)
        var approverRoles = configuration.GetSection("ApproverRoles").Get<string[]>() ?? new[] { "Administrator", "Management" };
        services.AddAuthorization(options =>
        {
            //Tells system these above uisers need to be assigned to the CanApprovUsers policy
            options.AddPolicy("CanApproveUsers", policy => policy.RequireRole(approverRoles));
        });

        // Nasty nasty nasty stuff, but we needed to allow cors for a testing external client i was using to auth to the API
        //We although limit what external apis can access and call, NO UGLY SQL INJECTION here
        // CORS - allow origins configured Cors:AllowedOrigins (array). Defaults to AllowAnyOrigin for dev.
        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        services.AddCors(options =>
        {
            if (corsOrigins != null && corsOrigins.Length > 0)
            {
                options.AddPolicy("DefaultCorsPolicy", builder =>
                    builder.WithOrigins(corsOrigins)
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials());
            }
            else
            {
                // Development fallback: allow any origin (no credentials)
                options.AddPolicy("DefaultCorsPolicy", builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod());
            }
        });

        // Infrastructure
        services.AddHttpContextAccessor();

        // Repositories
        services.AddScoped<IAdminNotificationRepository, AdminNotificationRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IEmailQueueRepository, EmailQueueRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICountryDialCodeRepository, CountryDialCodeRepository>();
        services.AddScoped<IEmployeeEmailTemplateRepository, EmployeeEmailTemplateRepository>();

        // Services
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerEmailService, CustomerEmailService>();
        services.AddScoped<IEmployeeEmailTemplateService, EmployeeEmailTemplateService>();
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
        services.AddScoped<ICustomerReferenceDataService, CustomerReferenceDataService>();
        services.AddScoped<ICustomerAccessService, CustomerAccessService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IEmployeeDashboardService, EmployeeDashboardService>();
        services.AddScoped<IAdminNotificationService, AdminNotificationService>();

        // RSA crypto service 
        //make sure you have PrivatePEM and PublicPEM set in the configuration
        services.AddSingleton<RsaCryptoService>();

        // Email queue and sender
        services.AddScoped<EmailQueue>();
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        services.AddHostedService<EmailDispatcherService>();

        // User registration notification service
        services.AddScoped<IUserRegistrationNotifier, UserRegistrationNotifier>();

        return services;
    }
}
