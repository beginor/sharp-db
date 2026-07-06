using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Beginor.SharpDbMcp;

[McpServerToolType]
public sealed class DatabaseTools(
    QueryExecutor queryExecutor,
    MetadataQueryService metadataQueryService
) {

    [McpServerTool(Name = "ExecuteQuery")]
    [Description("Execute a SQL statement and return the result as a markdown table.")]
    public Task<string> ExecuteQuery(
        [Description("Database type. Supported values: postgres, mysql, sqlite.")] string dbType,
        [Description("ADO.NET database connection string.")] string connectionString,
        [Description("SQL statement to execute.")] string sql,
        CancellationToken cancellationToken = default
    ) {
        return queryExecutor.ExecuteQueryAsync(dbType, connectionString, sql, cancellationToken);
    }

    [McpServerTool(Name = "QueryTables")]
    [Description("Return tables and views in the current database. Schema can be supplied for databases that support schemas.")]
    public Task<string> QueryTables(
        [Description("Database type. Supported values: postgres, mysql, sqlite.")] string dbType,
        [Description("ADO.NET database connection string.")] string connectionString,
        [Description("Optional schema name. Use for databases that support schemas.")] string? schema = null,
        CancellationToken cancellationToken = default
    ) {
        return metadataQueryService.QueryTablesAsync(dbType, connectionString, schema, cancellationToken);
    }

    [McpServerTool(Name = "QueryColumns")]
    [Description("Return columns for a table or view. Schema can be supplied for databases that support schemas.")]
    public Task<string> QueryColumns(
        [Description("Database type. Supported values: postgres, mysql, sqlite.")] string dbType,
        [Description("ADO.NET database connection string.")] string connectionString,
        [Description("Table or view name.")] string tableName,
        [Description("Optional schema name. Use for databases that support schemas.")] string? schema = null,
        CancellationToken cancellationToken = default
    ) {
        return metadataQueryService.QueryColumnsAsync(dbType, connectionString, tableName, schema, cancellationToken);
    }

}
