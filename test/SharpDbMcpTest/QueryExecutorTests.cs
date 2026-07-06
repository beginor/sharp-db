using Beginor.SharpDbMcp;
using Microsoft.Data.Sqlite;
using System.Diagnostics.CodeAnalysis;

namespace Beginor.SharpDbMcpTest;

public sealed class QueryExecutorTests {

    [Test]
    public async Task ExecuteQueryAsync_ReturnsMarkdownTable() {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var executor = database.CreateExecutor();

        var markdown = await executor.ExecuteQueryAsync(
            "select id, name, note from people order by id"
        );

        Assert.That(markdown, Is.EqualTo(
            """
            | id | name | note |
            | --- | --- | --- |
            | 1 | Ada | first |
            | 2 | Grace \| Hopper | NULL |
            """
        ));
    }

    [Test]
    public async Task ExecuteQueryAsync_ReturnsEmptyTableMessage() {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var executor = database.CreateExecutor();

        var markdown = await executor.ExecuteQueryAsync(
            "select id, name from people where id < 0"
        );

        Assert.That(markdown, Is.EqualTo(
            """
            | id | name |
            | --- | --- |

            _No rows returned._
            """
        ));
    }

    [Test]
    public async Task ExecuteQueryAsync_ReturnsRowsAffectedForNonQuery() {
        await using var database = await InMemorySqliteDatabase.CreateAsync();
        var executor = database.CreateExecutor();

        var result = await executor.ExecuteQueryAsync(
            "update people set note = 'changed' where id = 1"
        );

        Assert.That(result, Is.EqualTo("Rows affected: 1"));
    }

    private sealed class InMemorySqliteDatabase : IAsyncDisposable {

        private readonly SqliteConnection connection;

        private InMemorySqliteDatabase(SqliteConnection connection) {
            this.connection = connection;
        }

        public static async Task<InMemorySqliteDatabase> CreateAsync() {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = """
                create table people (
                    id integer not null primary key,
                    name text not null,
                    note text null
                );

                insert into people (id, name, note) values
                    (1, 'Ada', 'first'),
                    (2, 'Grace | Hopper', null);
                """;
            await command.ExecuteNonQueryAsync();

            return new InMemorySqliteDatabase(connection);
        }

        public QueryExecutor CreateExecutor() {
            var options = new DatabaseOptions("sqlite", connection.ConnectionString);
            return new QueryExecutor(new SharedConnectionFactory(connection), options);
        }

        public async ValueTask DisposeAsync() {
            await connection.DisposeAsync();
        }

    }

    private sealed class SharedConnectionFactory(SqliteConnection sharedConnection) : IDbConnectionFactory {

        public System.Data.Common.DbConnection CreateConnection() {
            return new NonDisposingSqliteConnection(sharedConnection);
        }

    }

    private sealed class NonDisposingSqliteConnection(SqliteConnection inner) : System.Data.Common.DbConnection {

        [AllowNull]
        public override string ConnectionString {
            get => inner.ConnectionString;
            set => inner.ConnectionString = value ?? string.Empty;
        }

        public override string Database => inner.Database;

        public override string DataSource => inner.DataSource;

        public override string ServerVersion => inner.ServerVersion;

        public override System.Data.ConnectionState State => inner.State;

        public override void ChangeDatabase(string databaseName) {
            inner.ChangeDatabase(databaseName);
        }

        public override void Close() {
        }

        public override void Open() {
        }

        public override Task OpenAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        protected override System.Data.Common.DbTransaction BeginDbTransaction(
            System.Data.IsolationLevel isolationLevel
        ) {
            return inner.BeginTransaction(isolationLevel);
        }

        protected override System.Data.Common.DbCommand CreateDbCommand() {
            return inner.CreateCommand();
        }

        protected override void Dispose(bool disposing) {
        }

        public override ValueTask DisposeAsync() {
            return ValueTask.CompletedTask;
        }

    }

}
