using Microsoft.EntityFrameworkCore;
using MyPortal.Core.Entities;

namespace MyPortal.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    // DbSets for all entities
    public DbSet<Status> Statuses { get; set; }
    public DbSet<UserType> UserTypes { get; set; }
    public DbSet<Network> Networks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Token> Tokens { get; set; }
    public DbSet<CampaignDetails> CampaignDetails { get; set; }
    public DbSet<CampaignReport> CampaignReports { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<ContactGroup> ContactGroups { get; set; }
    public DbSet<Email> Emails { get; set; }
    public DbSet<SmtpSetup> SmtpSetups { get; set; }
    public DbSet<SecuritySetup> SecuritySetups { get; set; }
    public DbSet<Guarantor> Guarantors { get; set; }
    public DbSet<Leave> Leaves { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure entity relationships and constraints
        
        // Status
        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });
        
        // UserType
        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });
        
        // Network
        modelBuilder.Entity<Network>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Company).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NetworkName).IsRequired().HasMaxLength(200);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.HasOne(e => e.UserType)
                .WithMany(ut => ut.Users)
                .HasForeignKey(e => e.UserTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Network)
                .WithMany(n => n.Users)
                .HasForeignKey(e => e.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // UserProfile
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Token
        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Purpose).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // CampaignDetails
        modelBuilder.Entity<CampaignDetails>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            
            entity.HasOne(e => e.Network)
                .WithMany(n => n.Campaigns)
                .HasForeignKey(e => e.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // CampaignReport
        modelBuilder.Entity<CampaignReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Network)
                .WithMany()
                .HasForeignKey(e => e.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Contact
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Network)
                .WithMany(n => n.Contacts)
                .HasForeignKey(e => e.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Group
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GroupName).IsRequired().HasMaxLength(200);
            
            entity.HasOne(e => e.Network)
                .WithMany()
                .HasForeignKey(e => e.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ContactGroup
        modelBuilder.Entity<ContactGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Contact)
                .WithMany(c => c.ContactGroups)
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Group)
                .WithMany(g => g.ContactGroups)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Network)
                .WithMany()
                .HasForeignKey(e => e.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Email
        modelBuilder.Entity<Email>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Network)
                .WithMany()
                .HasForeignKey(e => e.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // SmtpSetup
        modelBuilder.Entity<SmtpSetup>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Network)
                .WithMany()
                .HasForeignKey(e => e.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // SecuritySetup
        modelBuilder.Entity<SecuritySetup>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Network)
                .WithMany()
                .HasForeignKey(e => e.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Guarantor
        modelBuilder.Entity<Guarantor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        // Leave
        modelBuilder.Entity<Leave>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // EmailTemplate
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.HtmlContent).IsRequired();
            
            entity.HasOne(e => e.Network)
                .WithMany()
                .HasForeignKey(e => e.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Set default timestamps
        // Set default timestamps
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Core.Entities.BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("CreatedAt")
                    .HasDefaultValueSql("GETUTCDATE()");
                
                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("UpdatedAt")
                    .HasDefaultValueSql("GETUTCDATE()");
            }
        }
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Core.Entities.BaseEntity && 
                       (e.State == EntityState.Modified));
        
        foreach (var entry in entries)
        {
            ((Core.Entities.BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}
