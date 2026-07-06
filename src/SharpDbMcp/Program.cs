using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Beginor.SharpDbMcp;

public static class Program {

    public static async Task Main(string[] args) {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.AddConsole(options => {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        builder.Services.AddSingleton<QueryExecutor>();
        builder.Services.AddSingleton<MetadataQueryService>();

        builder.Services
            .AddMcpServer(options => {
                options.ServerInfo = new() {
                    Name = "sharp-db-mcp",
                    Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"
                };
            })
            .WithStdioServerTransport()
            .WithTools<DatabaseTools>();

        await builder.Build().RunAsync();
    }

}
