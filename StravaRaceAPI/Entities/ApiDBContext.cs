using Microsoft.EntityFrameworkCore;

namespace StravaRaceAPI.Entities;

public class ApiDBContext : DbContext
{
    public ApiDBContext(DbContextOptions<ApiDBContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Result> Results { get; set; }
    public DbSet<Segment> Segments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Token> Tokens { get; set; }
    public DbSet<UserWithEvent> UsersWithEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Description).IsRequired();
            e.Property(x => x.CreationDate).IsRequired();
            e.Property(x => x.CreationDate).HasPrecision(0);
            e.Property(x => x.CreationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.StartDate).HasPrecision(0);
            e.Property(x => x.EndDate).HasPrecision(0);

            e.HasOne<User>(x => x.Creator).WithMany(x => x.CreatedEvents).HasForeignKey(x => x.CreatorId);

            e.HasMany<User>(x => x.Competitors).WithMany(x => x.Events)
                .UsingEntity<UserWithEvent>(
                    l => l.HasOne(x => x.User).WithMany().HasForeignKey(y => y.UserId),
                    r => r.HasOne(y => y.Event).WithMany().HasForeignKey(y => y.EventId),
                    x =>
                    {
                        x.HasAlternateKey(y => new { y.UserId, y.EventId });
                        x.Property(y => y.UserId).IsRequired();
                        x.Property(y => y.EventId).IsRequired();
                    }
                );

            e.HasMany<Result>(x => x.Results).WithOne(x => x.Event).HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Segments).WithMany(x => x.Events)
                .UsingEntity<RaceSegment>(
                    l => l.HasOne(x => x.Segment).WithMany().HasForeignKey(y => y.SegmentId),
                    r => r.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId),
                    x =>
                    {
                        x.HasAlternateKey(y => new { y.EventId, y.SegmentId });
                        x.Property(y => y.EventId).IsRequired();
                        x.Property(y => y.SegmentId).IsRequired();
                    }
                );
        });

        modelBuilder.Entity<Result>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EventId).IsRequired();
            e.Property(x => x.SegmentId).IsRequired();
            e.Property(x => x.UserId).IsRequired();
            e.Property(x => x.Time).IsRequired();

            e.HasOne<Segment>(x => x.Segment).WithMany(x => x.Results).HasForeignKey(x => x.SegmentId);
            e.HasOne<User>(x => x.User).WithMany(x => x.Results).HasForeignKey(x => x.UserId);
            e.HasOne<Event>(x => x.Event).WithMany(x => x.Results).HasForeignKey(x => x.EventId);
        });

        modelBuilder.Entity<Segment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Distance).IsRequired();
            e.Property(x => x.Elevation).IsRequired();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).IsRequired();
            e.Property(x => x.Gender).IsRequired();
            e.Property(x => x.Email).IsRequired();
            e.HasOne(x => x.Token).WithOne(x => x.User).HasForeignKey<Token>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Token>(e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.ExpirationOfToken).HasPrecision(0);
        });
    }
}