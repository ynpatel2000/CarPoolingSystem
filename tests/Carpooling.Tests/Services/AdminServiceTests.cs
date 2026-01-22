using Carpooling.Application.Interfaces;
using Carpooling.Application.Services;
using Carpooling.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Carpooling.Tests.Services;

public class AdminServiceTests
{
    [Fact]
    public void BlockUser_Should_Set_IsBlocked()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), IsBlocked = false }
        };

        var db = new Mock<IAppDbContext>();
        var audit = new Mock<IAuditLogService>();
        var logger = new Mock<ILogger<AdminService>>();

        db.Setup(x => x.Users).Returns(users.AsQueryable());
        db.Setup(x => x.Update(It.IsAny<User>()));
        db.Setup(x => x.SaveChanges()).Returns(1);

        var service = new AdminService(
            db.Object,
            audit.Object,
            logger.Object
        );

        // Act
        service.BlockUser(users[0].Id);

        // Assert
        users[0].IsBlocked.Should().BeTrue();
        db.Verify(x => x.SaveChanges(), Times.Once);
        audit.Verify(
            x => x.Log("BLOCK_USER", users[0].Id, It.IsAny<string>()),
            Times.Once
        );
    }
}
