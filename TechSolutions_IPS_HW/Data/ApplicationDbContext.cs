using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechSolutions_IPS_HW.Models.Admin;
using TechSolutions_IPS_HW.Models.Audit;
using TechSolutions_IPS_HW.Models.Customer;
using TechSolutions_IPS_HW.Models.Email;
using TechSolutions_IPS_HW.Models.Reference;
using TechSolutions_IPS_HW.Models.User;
using TechSolutions_IPS_HW.Services.Interfaces;

namespace TechSolutions_IPS_HW.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IServiceScopeFactory? _scopeFactory;

    // Constructor used and set in Dependencyynjection
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, 
        IServiceScopeFactory scopeFactory) : base(options)
    {
        _scopeFactory = scopeFactory;
    }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        _scopeFactory = null;
    }

    #region Db Entities
    public DbSet<Customer> Customers { get; set; }

    public DbSet<AuditActor> AuditActors { get; set; } = null!;

    public DbSet<AuditEntry> AuditEntries { get; set; } = null!;

    public DbSet<CustomerChangeLog> CustomerChangeLogs { get; set; } = null!;

    public DbSet<AdminNotification> AdminNotifications { get; set; } = null!;

    public DbSet<EmailQueueEntry> EmailQueueEntries { get; set; } = null!;

    public DbSet<CountryDialCode> CountryDialCodes { get; set; } = null!;

    public DbSet<EmployeeEmailTemplate> EmployeeEmailTemplates { get; set; } = null!;

    #endregion

    #region On Fire save changes event listener
    //Created a handly little event listener to ensure the following:
    // Newly registered users require a approval from admin or manager for their registration to be accepted
    // Admin/Managerial useras are notified when a new user registers     
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // detect newly added users before saving so we can notify approvers after save
        var newUsers = ChangeTracker.Entries<ApplicationUser>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);
        
        if (newUsers.Count > 0 && _scopeFactory != null)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notifier = scope.ServiceProvider.GetService<IUserRegistrationNotifier>();
                if (notifier != null)                
                    await notifier.NotifyApproversAsync(newUsers, cancellationToken);                
            }
            catch
            {
                // swallow notification errors to not affect user creation
            }
        }

        return result;
    }

    #endregion


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
             
        // Application User (the AspUsers created by Microsoft Identity)
        //-- Added some soft delete functionality
        builder.Entity<ApplicationUser>(e =>
        {
            e.HasQueryFilter(u => u.IsDeleted != true);
        });

        // Customer 
        builder.Entity<Customer>(e =>
        {
            e.HasKey(c => c.CustomerId);
            e.HasQueryFilter(c => c.IsDeleted != true);
            e.HasIndex(c => c.Email).IsUnique();
            e.HasIndex(c => c.IdNumber)
                .IsUnique()
                .HasFilter("[IdNumber] IS NOT NULL");
       
            e.HasMany<AuditEntry>(c => c.AuditEntries)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);            
        });

        // AuditActor (intersection entity) 
        builder.Entity<AuditActor>(e =>
        {
            e.HasKey(a => a.Id);

            e.HasOne(a => a.User)
                .WithMany(u => u.AuditActors)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(a => a.UserId);
        });

        // AuditEntry
        builder.Entity<AuditEntry>(e =>
        {
            e.HasKey(a => a.Id);

            e.HasOne(a => a.SubjectActor)
                .WithMany(aa => aa.SubjectEntries)
                .HasForeignKey(a => a.SubjectActorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.PerformerActor)
                .WithMany(aa => aa.PerformerEntries)
                .HasForeignKey(a => a.PerformerActorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(a => a.SubjectActorId);
            e.HasIndex(a => a.PerformerActorId);
            e.HasIndex(a => a.CustomerId);
            e.HasIndex(a => a.TimestampUtc);
        });

        // CustomerChangeLog
        builder.Entity<CustomerChangeLog>(e =>
        {
            e.HasKey(cl => cl.Id);

            e.HasOne(cl => cl.Customer)
                .WithMany(c => c.ChangeLogs)
                .HasForeignKey(cl => cl.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(cl => cl.PerformerActor)
                .WithMany()
                .HasForeignKey(cl => cl.PerformerActorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(cl => cl.CustomerId);
            e.HasIndex(cl => cl.TimestampUtc);
        });

        // Admin notifications
        builder.Entity<AdminNotification>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasIndex(n => new { n.RecipientUserId, n.IsRead });

            e.HasOne(n => n.Recipient)
                .WithMany(u => u.ReceivedNotifications)
                .HasForeignKey(n => n.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(n => n.SubjectUser)
                .WithMany()
                .HasForeignKey(n => n.SubjectUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Email queue
        builder.Entity<EmailQueueEntry>(e =>
        {
            e.HasKey(q => q.Id);
            e.HasIndex(q => new { q.Status, q.NextRetryAt });
        });

        // Country dial codes
        builder.Entity<CountryDialCode>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.CountryName).HasMaxLength(150).IsRequired();
            e.Property(c => c.DialCode).HasMaxLength(10).IsRequired();
            e.HasIndex(c => c.CountryName).IsUnique();
        });

        // Employee email templates (private per employee)
        builder.Entity<EmployeeEmailTemplate>(e =>
        {
            e.HasKey(t => t.Id);

            e.Property(t => t.OwnerUserId).HasMaxLength(450).IsRequired();
            e.Property(t => t.Name).HasMaxLength(120).IsRequired();
            e.Property(t => t.Subject).HasMaxLength(200).IsRequired();
            e.Property(t => t.BodyHtml).HasMaxLength(20000).IsRequired();

            e.HasIndex(t => new { t.OwnerUserId, t.Name }).IsUnique();

            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(t => t.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
