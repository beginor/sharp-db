using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Beginor.SharpDbMcp;

[McpServerToolType]
public sealed class DatabaseTools(QueryExecutor queryExecutor) {

    [McpServerTool(Name = "ExecuteQuery")]
    [Description("Execute a SQL statement and return the result as a markdown table.")]
    public Task<string> ExecuteQuery(
        [Description("SQL statement to execute.")] string sql,
        CancellationToken cancellationToken = default
    ) {
        return queryExecutor.ExecuteQueryAsync(sql, cancellationToken);
    }

}
