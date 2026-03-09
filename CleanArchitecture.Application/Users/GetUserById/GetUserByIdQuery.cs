using CleanArchitecture.Application.Abstractions.Messaging;

namespace CleanArchitecture.Application.Users.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponse>;
