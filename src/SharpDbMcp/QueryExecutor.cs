using System.Data.Common;
using System.Globalization;

namespace Beginor.SharpDbMcp;

public sealed class QueryExecutor(
    IDbConnectionFactory connectionFactory,
    DatabaseOptions options
) {

    public async Task<string> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default) {
        if (string.IsNullOrWhiteSpace(sql)) {
            throw new ArgumentException("SQL must not be empty.", nameof(sql));
        }

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = options.CommandTimeoutSeconds;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (reader.FieldCount == 0) {
            return FormatRowsAffected(reader.RecordsAffected);
        }

        return await MarkdownTableFormatter.FormatAsync(reader, cancellationToken);
    }

    private static string FormatRowsAffected(int recordsAffected) {
        var value = recordsAffected < 0
            ? "unknown"
            : recordsAffected.ToString(CultureInfo.InvariantCulture);

        return $"Rows affected: {value}";
    }

}
