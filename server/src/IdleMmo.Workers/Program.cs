// Slice 0 placeholder host. Real Quartz schedules land in Slice 4 (Battle Pass rollover, daily reset).
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var host = builder.Build();
await host.RunAsync().ConfigureAwait(false);
