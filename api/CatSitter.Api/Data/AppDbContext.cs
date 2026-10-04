using CatSitter.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<SitterProfile> SitterProfiles => Set<SitterProfile>();
    public DbSet<Cat> Cats => Set<Cat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingCat> BookingCats => Set<BookingCat>();
    public DbSet<Checklist> Checklists => Set<Checklist>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ServiceFee> ServiceFees => Set<ServiceFee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.WalletBalance).HasColumnType("numeric(12,2)");
        });

        modelBuilder.Entity<SitterProfile>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.PricePerDay).HasColumnType("numeric(10,2)");
            e.Property(s => s.RatingAvg).HasColumnType("numeric(3,2)");
            e.HasOne(s => s.User)
                .WithOne(u => u.SitterProfile)
                .HasForeignKey<SitterProfile>(s => s.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cat>(e =>
        {
            e.Property(c => c.Weight).HasColumnType("numeric(5,2)");
            e.HasOne(c => c.Owner)
                .WithMany(u => u.Cats)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasIndex(b => b.BookingCode).IsUnique();
            e.Property(b => b.BasePrice).HasColumnType("numeric(10,2)");
            e.Property(b => b.ServiceFee).HasColumnType("numeric(10,2)");
            e.Property(b => b.TotalPrice).HasColumnType("numeric(10,2)");
            e.HasOne(b => b.Owner)
                .WithMany()
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Sitter)
                .WithMany()
                .HasForeignKey(b => b.SitterId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(b => b.Status);
        });

        modelBuilder.Entity<BookingCat>(e =>
        {
            e.HasKey(bc => new { bc.BookingId, bc.CatId });
            e.HasOne(bc => bc.Booking)
                .WithMany(b => b.BookingCats)
                .HasForeignKey(bc => bc.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(bc => bc.Cat)
                .WithMany()
                .HasForeignKey(bc => bc.CatId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Checklist>(e =>
        {
            e.HasOne<Booking>()
                .WithMany()
                .HasForeignKey(c => c.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChecklistItem>(e =>
        {
            e.HasOne(i => i.Checklist)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.ChecklistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AttendanceRecord>(e =>
        {
            e.HasOne<Booking>()
                .WithMany()
                .HasForeignKey(a => a.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasIndex(r => r.BookingId).IsUnique();
            e.HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.SitterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatRoom>(e =>
        {
            e.HasIndex(c => new { c.OwnerId, c.SitterId }).IsUnique();
        });

        modelBuilder.Entity<Message>(e =>
        {
            e.HasOne<ChatRoom>()
                .WithMany(r => r.Messages)
                .HasForeignKey(m => m.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
