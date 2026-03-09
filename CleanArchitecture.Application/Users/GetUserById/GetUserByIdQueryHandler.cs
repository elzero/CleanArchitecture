using CleanArchitecture.Application.Abstractions.Data;
using CleanArchitecture.Application.Abstractions.Messaging;
using CleanArchitecture.Domain.Abstractions;
using CleanArchitecture.Domain.Users;
using Dapper;

namespace CleanArchitecture.Application.Users.GetUserById;

internal sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetUserByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<UserResponse>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                Id,
                FirstName,
                LastName,
                Email
            FROM Users
            WHERE Id = @UserId
            """;

        var user = await connection.QueryFirstOrDefaultAsync<UserResponse>(
            sql,
            new { request.UserId });

        if (user is null)
        {
            // 在更完善的例子中，此处会抛出 Error.NotFound
            return Result.Failure<UserResponse>(Error.NullValue);
        }

        return Result.Success(user);
    }
}
