using Carpooling.Application.DTOs.Booking;
using Carpooling.Application.Interfaces;
using Carpooling.Application.Services;
using Carpooling.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

public class BookingServiceTests
{
    [Fact]
    public void BookRide_Should_Create_Booking()
    {
        var rides = new List<Ride>
        {
            new Ride { Id = Guid.NewGuid(), AvailableSeats = 1 }
        };

        var bookings = new List<Booking>();

        var db = new Mock<IAppDbContext>();
        var audit = new Mock<IAuditLogService>();

        db.Setup(x => x.Rides).Returns(rides.AsQueryable());
        db.Setup(x => x.Bookings).Returns(bookings.AsQueryable());
        db.Setup(x => x.Add(It.IsAny<Booking>()))
          .Callback<Booking>(b => bookings.Add(b));
        db.Setup(x => x.SaveChanges()).Returns(1);

        var service = new BookingService(db.Object, audit.Object);

        service.BookRide(Guid.NewGuid(), new CreateBookingDto { RideId = rides[0].Id });

        bookings.Should().HaveCount(1);
    }
}
