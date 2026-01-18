using Carpooling.Domain.Entities;

namespace Carpooling.Application.Interfaces;

public interface IAppDbContext
{
    IQueryable<User> Users { get; }
    IQueryable<Ride> Rides { get; }
    IQueryable<Booking> Bookings { get; }

    void Add<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;

    int SaveChanges();
}