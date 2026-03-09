using CleanArchitecture.Application.Abstractions.Data;
using CleanArchitecture.Application.Users.CreateUser;
using CleanArchitecture.Domain.Users;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CleanArchitecture.Application.UnitTests.Users.CreateUser;

public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldPersistUser_AndReturnCreatedId()
    {
        var userRepository = Substitute.For<IUserRepository>();
        var dbContext = Substitute.For<IApplicationDbContext>();
        var command = new CreateUserCommand("Ada", "Lovelace", "ada@example.com");
        var handler = new CreateUserCommandHandler(userRepository, dbContext);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await dbContext.Received(1).SaveChangesAsync(CancellationToken.None);
        userRepository.Received(1).Add(Arg.Is<User>(user =>
            user.FirstName == command.FirstName &&
            user.LastName == command.LastName &&
            user.Email == command.Email &&
            user.Id == result.Value));
    }
}
