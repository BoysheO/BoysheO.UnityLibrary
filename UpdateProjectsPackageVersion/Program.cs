// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UpdateProjectsPackageVersion;

Console.WriteLine("Hello, World!");

var builder  = Host.CreateDefaultBuilder();
builder.ConfigureServices(v =>
{
   v.AddSingleton<UpdateVersionFromUPM>();
});

var host = builder.Build();
var handle = host.Services.GetRequiredService<UpdateVersionFromUPM>();
handle.Go();
