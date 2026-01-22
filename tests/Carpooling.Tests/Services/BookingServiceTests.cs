using Carpooling.Application.DTOs.Booking;
using Carpooling.Application.Interfaces;
using Carpooling.Application.Services;
using Carpooling.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Carpooling.Tests.Services;

public class BookingServiceTests
{
    [Fact]
    public void BookRide_Should_Create_Booking_And_Outbox_Event()
    {
        // Arrange
        var rideId = Guid.NewGuid();

        var rides = new List<Ride>
        {
            new Ride
            {
                Id = rideId,
                AvailableSeats = 1,
                IsDeleted = false
            }
        };

        var bookings = new List<Booking>();
        var outboxEvents = new List<object>();

        var db = new Mock<IAppDbContext>();
        var audit = new Mock<IAuditLogService>();

        db.Setup(x => x.Rides).Returns(rides.AsQueryable());
        db.Setup(x => x.Bookings).Returns(bookings.AsQueryable());

        db.Setup(x => x.Add(It.IsAny<Booking>()))
            .Callback<Booking>(b => bookings.Add(b));

        db.Setup(x => x.SaveChanges()).Returns(1);

        var service = new BookingService(
            db.Object,
            audit.Object,
            null,
            Mock.Of<ILogger<BookingService>>()
            );

        // Act
        service.BookRide(
            Guid.NewGuid(),
            new CreateBookingDto { RideId = rideId }
        );

        // Assert
        bookings.Should().HaveCount(1);
        outboxEvents.Should().HaveCount(1);

        rides[0].AvailableSeats.Should().Be(0);

        audit.Verify(
            x => x.Log("BOOK_RIDE", It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Once
        );

        db.Verify(x => x.SaveChanges(), Times.Once);
    }
}
