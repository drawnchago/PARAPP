using Dapper;
using PsilmtyApi.Interfaces.Data;
using PsilmtyApi.Interfaces.Repositories;
using System.Data;

namespace PsilmtyApi.Repositories;

public sealed class DatabaseRepository(IDatabaseConnectionFactory connectionFactory) : IDatabaseRepository
{
    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null)
    {
        if (transaction is not null)
            return (await transaction.Connection!.QueryAsync<T>(sql, parameters, transaction)).AsList();

        await using var connection = connectionFactory.CreateConnection();
        return (await connection.QueryAsync<T>(sql, parameters)).AsList();
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null)
    {
        if (transaction is not null)
            return await transaction.Connection!.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction);

        await using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null)
    {
        if (transaction is not null)
            return await transaction.Connection!.ExecuteAsync(sql, parameters, transaction);

        await using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null)
    {
        if (transaction is not null)
            return (await transaction.Connection!.ExecuteScalarAsync<T>(sql, parameters, transaction))!;

        await using var connection = connectionFactory.CreateConnection();
        return (await connection.ExecuteScalarAsync<T>(sql, parameters))!;
    }
}
