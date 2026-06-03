using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// MCP server for controlling display brightness via DDC/CI (VCP code 0x10),
// reusing pdc's DisplayApiService. Communicates over stdio.
//
// IMPORTANT: with the stdio transport, stdout is the JSON-RPC channel. All
// diagnostic logging must go to stderr, and tools must never write to stdout.
var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
