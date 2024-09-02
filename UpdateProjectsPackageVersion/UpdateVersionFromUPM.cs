using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using BoysheO.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UpdateProjectsPackageVersion;

public class UpdateVersionFromUPM
{
    private readonly Regex _regex = new Regex(@"(?<=<Version>)\d.\d.\d(?=</Version>)", RegexOptions.Compiled);
    private readonly ILogger<UpdateVersionFromUPM> _logger;
    private readonly IConfiguration _configuration;

    public UpdateVersionFromUPM(ILogger<UpdateVersionFromUPM> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string GetVersion(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            throw new Exception($"The file '{jsonFilePath}' does not exist.");
        }

        var json = File.ReadAllText(jsonFilePath);
        var doc = JsonSerializer.Deserialize<JsonNode>(json) ??
                  throw new NullReferenceException($"missing {jsonFilePath}");
        var version = doc["version"]?.GetValue<string>() ??
                      throw new NullReferenceException($"missing version entry(file={jsonFilePath})");
        return version;
    }

    public void WriteVersion(string propsFile, string version)
    {
        var text = File.ReadAllText(propsFile);
        text = _regex.Replace(text, version, 1);
        File.WriteAllText(propsFile, text);
    }
    
    public void Go()
    {
        var templ = File.ReadAllText("upm.props.templ");
        var slnFile = _configuration.GetValue<string>("SolutionPath");
        var dir = slnFile.AsPath().GetDirectoryName().Value;
        var ver = GetVersion(@"D:\Repository\BoysheO.UnityLibrary\UPMVerDetecterSolution\CustomVersionTest\src.json");
    }
}