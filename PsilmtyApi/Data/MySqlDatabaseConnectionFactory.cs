using Microsoft.Extensions.Options;
using MySqlConnector;
using PsilmtyApi.Interfaces.Data;
using PsilmtyApi.Models.Options;
using System.Data.Common;

namespace PsilmtyApi.Data;

public sealed class MySqlDatabaseConnectionFactory(IOptions<DatabaseOptions> options) : IDatabaseConnectionFactory
{
    private readonly DatabaseOptions _options = options.Value;

    public DbConnection CreateConnection()
    {
        var connectionString = new MySqlConnectionStringBuilder
        {
            Server = _options.Host,
            Port = _options.Port,
            Database = _options.Name,
            UserID = _options.User,
            Password = _options.Password,
            SslMode = MySqlSslMode.Preferred,
            ConnectionTimeout = 15
        }.ConnectionString;

        return new MySqlConnection(connectionString);
    }
}
