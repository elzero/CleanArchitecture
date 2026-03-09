using System.Data;
using CleanArchitecture.Application.Abstractions.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace CleanArchitecture.Infrastructure.Database;

internal sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Database") 
                            ?? throw new ArgumentNullException(nameof(configuration));
    }

    public IDbConnection CreateConnection()
    {
        var connection = new MySqlConnection(_connectionString);
        connection.Open();

        return connection;
    }
}
