using Microsoft.EntityFrameworkCore;
using FinSenseAPI.Models;

namespace FinSenseAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UploadSession> UploadSessions { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.Email).IsRequired();
        });

        modelBuilder.Entity<UploadSession>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasMany(s => s.Transactions).WithOne(t => t.Session).HasForeignKey(t => t.SessionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Transaction>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Amount).HasPrecision(18,2);
            b.HasOne(t => t.Session).WithMany(s => s.Transactions).HasForeignKey(t => t.SessionId);
        });

        modelBuilder.Entity<Goal>(b =>
        {
            b.HasKey(g => g.Id);
            b.Property(g => g.TargetAmount).HasPrecision(18,2);
            b.HasOne(g => g.User).WithMany(u => u.Goals).HasForeignKey(g => g.UserId);
        });

        modelBuilder.Entity<ChatMessage>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasIndex(c => c.SessionId).HasDatabaseName("IX_ChatMessages_SessionId");
        });

        modelBuilder.Entity<PasswordResetToken>(b =>
        {
            b.HasKey(p => p.Token);
            b.HasIndex(p => p.UserId);
            b.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId);
        });
    }
}
