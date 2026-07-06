using System.Data.Common;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Beginor.SharpDbMcp;

public interface IDbConnectionFactory {

    DbConnection CreateConnection();

}

public sealed class DbConnectionFactory(DatabaseOptions options) : IDbConnectionFactory {

    public DbConnection CreateConnection() {
        return options.Type switch {
            "postgres" or "postgresql" => new NpgsqlConnection(options.ConnectionString),
            "mysql" => new MySqlConnection(options.ConnectionString),
            "sqlite" => new SqliteConnection(options.ConnectionString),
            _ => throw new NotSupportedException(
                $"Unsupported DB_TYPE '{options.Type}'. Supported values are postgres, mysql, sqlite."
            )
        };
    }

}
