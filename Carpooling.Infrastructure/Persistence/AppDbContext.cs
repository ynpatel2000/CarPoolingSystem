using Carpooling.Application.Interfaces;
using Carpooling.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext, IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Ride> Rides => Set<Ride>();
    public DbSet<Booking> Bookings => Set<Booking>();

    IQueryable<User> IAppDbContext.Users => Users;
    IQueryable<Ride> IAppDbContext.Rides => Rides;
    IQueryable<Booking> IAppDbContext.Bookings => Bookings;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public void Add<T>(T entity) where T : class
        => base.Add(entity);

    public void Remove<T>(T entity) where T : class
        => base.Remove(entity);

    public void Update<T>(T entity) where T : class
        => base.Update(entity);
}
