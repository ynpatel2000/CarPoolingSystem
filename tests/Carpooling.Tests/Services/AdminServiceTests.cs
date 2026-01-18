using Carpooling.Application.Interfaces;
using Carpooling.Application.Services;
using Carpooling.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

public class AdminServiceTests
{
    [Fact]
    public void BlockUser_Should_Set_IsBlocked()
    {
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid() }
        };

        var db = new Mock<IAppDbContext>();
        var audit = new Mock<IAuditLogService>();

        db.Setup(x => x.Users).Returns(users.AsQueryable());
        db.Setup(x => x.SaveChanges()).Returns(1);

        var service = new AdminService(db.Object, audit.Object);

        service.BlockUser(users[0].Id);

        users[0].IsBlocked.Should().BeTrue();
    }
}
