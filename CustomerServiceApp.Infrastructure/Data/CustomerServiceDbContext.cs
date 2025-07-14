using Microsoft.EntityFrameworkCore;
using CustomerServiceApp.Domain.Users;
using CustomerServiceApp.Domain.Tickets;

namespace CustomerServiceApp.Infrastructure.Data;

/// <summary>
/// Entity Framework database context for the customer service application
/// </summary>
public class CustomerServiceDbContext : DbContext
{
    public CustomerServiceDbContext(DbContextOptions<CustomerServiceDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Agent> Agents { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<Reply> Replies { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User hierarchy with Table Per Hierarchy (TPH)
        modelBuilder.Entity<User>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Player>("Player")
            .HasValue<Agent>("Agent");

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Avatar).HasMaxLength(500);
            entity.Property(u => u.CreatedDate).IsRequired();
            
            // Create unique index on email
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Configure Player entity
        modelBuilder.Entity<Player>(entity =>
        {
            entity.Property(p => p.PlayerNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.PlayerNumber).IsUnique();
        });

        // Configure Ticket entity
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(2000);
            entity.Property(t => t.Status).IsRequired()
                .HasConversion<string>(); // Store enum as string
            entity.Property(t => t.CreatedDate).IsRequired();
            entity.Property(t => t.LastUpdateDate).IsRequired();

            // Configure relationship with Creator (Player)
            entity.HasOne(t => t.Creator)
                .WithMany()
                .HasForeignKey("CreatorId")
                .OnDelete(DeleteBehavior.Restrict);

            // Configure one-to-many relationship with Replies
            entity.HasMany(t => t.Messages)
                .WithOne()
                .HasForeignKey("TicketId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Reply entity
        modelBuilder.Entity<Reply>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Content).IsRequired().HasMaxLength(2000);
            entity.Property(r => r.CreatedDate).IsRequired();

            // Configure relationship with Author (User)
            entity.HasOne(r => r.Author)
                .WithMany()
                .HasForeignKey("AuthorId")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
