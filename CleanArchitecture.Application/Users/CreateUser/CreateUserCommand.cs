using CleanArchitecture.Application.Abstractions.Messaging;

namespace CleanArchitecture.Application.Users.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email) : ICommand<Guid>;
