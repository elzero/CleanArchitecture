using CleanArchitecture.Domain.Abstractions;

namespace CleanArchitecture.Domain.Users;

public sealed record UserCreatedDomainEvent(Guid UserId) : IDomainEvent;
