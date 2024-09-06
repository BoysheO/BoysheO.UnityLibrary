// See https://aka.ms/new-console-template for more information
//请在工程目录运行dotnet run执行，勿要在IDE中执行。工作目录会不同
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UpdateProjectsPackageVersion;

Console.WriteLine("Hello, World!");
Console.WriteLine(AppContext.BaseDirectory);
Console.WriteLine(Directory.GetCurrentDirectory());
Console.WriteLine(Assembly.GetExecutingAssembly().Location);

var builder  = Host.CreateDefaultBuilder();
builder.ConfigureAppConfiguration(v =>
{
   v.AddYamlFile("appsetting.yaml");
});
builder.ConfigureServices(v =>
{
   v.AddSingleton<UpdateVersionFromUPM>();
});

var host = builder.Build();
var handle = host.Services.GetRequiredService<UpdateVersionFromUPM>();
handle.Go();
Console.WriteLine("done");
