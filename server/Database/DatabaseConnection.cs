using DotNetEnv;
using Npgsql;

namespace app.Database;

public class DatabaseConnection
{
    private NpgsqlDataSource _connection;

    public NpgsqlDataSource Connection()
    {
        return _connection;
    }

    public DatabaseConnection()
    {
        _connection = NpgsqlDataSource.Create(Env.GetString("DBConnectString"));
        using var conn = _connection.OpenConnection();
    }
}