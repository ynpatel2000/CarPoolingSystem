using Carpooling.Application.DTOs.Ride;
using Carpooling.Application.Exceptions;
using Carpooling.Application.Interfaces;
using Carpooling.Application.Services;
using Carpooling.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

public class RideServiceTests
{
    [Fact]
    public void CreateRide_Should_Add_Ride()
    {
        // Arrange
        var rides = new List<Ride>();

        var dbMock = new Mock<IAppDbContext>();
        dbMock.Setup(x => x.Rides).Returns(rides.AsQueryable());
        dbMock.Setup(x => x.Add(It.IsAny<Ride>()))
              .Callback<Ride>(r => rides.Add(r));
        dbMock.Setup(x => x.SaveChanges()).Returns(1);

        var service = new RideService(dbMock.Object);

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
        rides.Count.Should().Be(1);
    }

    [Fact]
    public void CreateRide_Should_Throw_When_Seats_Invalid()
    {
        var dbMock = new Mock<IAppDbContext>();
        var service = new RideService(dbMock.Object);

        var dto = new CreateRideDto
        {
            AvailableSeats = 0
        };

        Action act = () => service.CreateRide(Guid.NewGuid(), dto);

        act.Should().Throw<AppException>();
    }
}
