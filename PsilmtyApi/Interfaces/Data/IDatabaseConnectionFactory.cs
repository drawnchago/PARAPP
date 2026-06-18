using System.Data.Common;

namespace PsilmtyApi.Interfaces.Data;

public interface IDatabaseConnectionFactory
{
    DbConnection CreateConnection();
}
