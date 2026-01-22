using Carpooling.Application.Interfaces;
using Carpooling.Domain.Common;
using Carpooling.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Carpooling.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ride> Rides => Set<Ride>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    IQueryable<User> IAppDbContext.Users => Users;
    IQueryable<Ride> IAppDbContext.Rides => Rides;
    IQueryable<Booking> IAppDbContext.Bookings => Bookings;
    IQueryable<AuditLog> IAppDbContext.AuditLogs => AuditLogs;
    IQueryable<RefreshToken> IAppDbContext.RefreshTokens => RefreshTokens;
    IQueryable<OutboxEvent> IAppDbContext.OutboxEvents => OutboxEvents;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Ride>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Booking>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(x => !x.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChanges();
    }

    public void Add<T>(T entity) where T : class => base.Add(entity);
    public void Update<T>(T entity) where T : class => base.Update(entity);
    public void Remove<T>(T entity) where T : class => base.Remove(entity);
}
