using System.Data;

namespace PsilmtyApi.Interfaces.Repositories;

public interface IDatabaseRepository
{
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null);
    Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null);
    Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null);
    Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null);
}
