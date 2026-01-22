using Carpooling.Application.DTOs.Ride;
using Carpooling.Application.Exceptions;
using Carpooling.Application.Interfaces;
using Carpooling.Application.Services;
using Carpooling.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Carpooling.Tests.Services;

public class RideServiceTests
{
    [Fact]
    public void CreateRide_Should_Add_Ride()
    {
        // Arrange
        var rides = new List<Ride>();

        var db = new Mock<IAppDbContext>();
        var logger = new Mock<ILogger<RideService>>();

        db.Setup(x => x.Rides).Returns(rides.AsQueryable());
        db.Setup(x => x.Add(It.IsAny<Ride>()))
          .Callback<Ride>(r => rides.Add(r));
        db.Setup(x => x.SaveChanges()).Returns(1);

        var service = new RideService(
            db.Object,
            logger.Object
        );

        var dto = new CreateRideDto
        {
            FromCity = "A",
            ToCity = "B",
            RideDate = DateTime.UtcNow.AddDays(1),
            AvailableSeats = 3,
            PricePerSeat = 100
        };

        // Act
        service.CreateRide(Guid.NewGuid(), dto);

        // Assert
        rides.Should().HaveCount(1);
        db.Verify(x => x.SaveChanges(), Times.Once);
    }

    [Fact]
    public void CreateRide_Should_Throw_When_Seats_Invalid()
    {
        // Arrange
        var db = new Mock<IAppDbContext>();
        var logger = new Mock<ILogger<RideService>>();

        var service = new RideService(
            db.Object,
            logger.Object
        );

        var dto = new CreateRideDto
        {
            AvailableSeats = 0
        };

        // Act
        Action act = () => service.CreateRide(Guid.NewGuid(), dto);

        // Assert
        act.Should().Throw<AppException>();
    }
}
