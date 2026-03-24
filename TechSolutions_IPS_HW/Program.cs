using TechSolutions_IPS_HW.Services;

var builder = WebApplication.CreateBuilder(args);

// Bundle service registrations in in a dioffrent file, they can flood program,.cs and
// become hard to manage if we keep adding them here.
builder.Services.AddAppServices(builder.Configuration);

var app = builder.Build();

//Lets keep this in dev, we dont want to anger the gods
if (app.Environment.IsDevelopment())
    app.UseMigrationsEndPoint();
//Fopr the above reason i have commented out the below :D
//else
//{
//    app.UseExceptionHandler("/Home/Error"); 
//    app.UseHsts();
//}

app.UseHttpsRedirection();
app.UseRouting();

// Enable CORS for API calls (this we enabled for external logins that might need to also access the system)
app.UseCors("DefaultCorsPolicy");

// Enable authentication before authorization so Identity works
app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

// Map Api controller routes
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Configure approver roles from configuration (kept in Program.cs because its used later for seeding)
var approverRoles = builder.Configuration.GetSection("ApproverRoles").Get<string[]>() ?? new[] { "Administrator", "Management" };


// Seed initial approver/admin user and role (development convenience)
var seededAdminEmail = builder.Configuration["SeedAdmin:Email"] ?? "admin@localhost";
var seededAdminPassword = builder.Configuration["SeedAdmin:Password"] ?? "Admin123!";

var seededManagerEmail = builder.Configuration["SeedManager:Email"] ?? "manager@localhost";
var seededManagerPassword = builder.Configuration["SeedManager:Password"] ?? "Manager123!";

await DataSeeder.SeedInitialUsersAsync(
    app, 
    seededAdminEmail, 
    seededAdminPassword, 
    seededManagerEmail, 
    seededManagerPassword, 
    approverRoles);

app.Run();
