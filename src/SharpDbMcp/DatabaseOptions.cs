namespace Beginor.SharpDbMcp;

public sealed record DatabaseOptions(
    string Type,
    string ConnectionString,
    int CommandTimeoutSeconds = 30
) {

    public static DatabaseOptions FromEnvironment() {
        var dbType = Environment.GetEnvironmentVariable("DB_TYPE");
        var connectionString = Environment.GetEnvironmentVariable("DB_CONN_STR");

        if (string.IsNullOrWhiteSpace(dbType)) {
            throw new InvalidOperationException("Environment variable DB_TYPE is required.");
        }

        if (string.IsNullOrWhiteSpace(connectionString)) {
            throw new InvalidOperationException("Environment variable DB_CONN_STR is required.");
        }

        return new DatabaseOptions(dbType.Trim().ToLowerInvariant(), connectionString);
    }

}
