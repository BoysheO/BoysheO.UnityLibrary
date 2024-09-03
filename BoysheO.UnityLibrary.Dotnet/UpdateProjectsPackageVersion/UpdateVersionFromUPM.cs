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
        // var slnFile = _configuration.GetValue<string>("SolutionPath") ??
        //               throw new Exception("missing SolutionPath entry in configuration");
        var dir = Directory.GetCurrentDirectory();///mnt/d/Repository/BoysheO.UnityLibrary/BoysheO.UnityLibrary.Dotnet/UpdateProjectsPackageVersion
        dir = dir.AsPath().GetDirectoryName().Value.GetDirectoryName().Value.Value;//mnt/d/Repository/BoysheO.UnityLibrary
        // var dir = slnFile.AsPath().GetDirectoryName().Value.Value.Replace(@"\","/");
        var projects = _configuration.GetSection("Projects").Get<string[]>() ??
                       throw new Exception("missing Projects entry in configuration");
        foreach (var project in projects)
        {
            var upmProps = dir + $"/BoysheO.UnityLibrary.Dotnet/{project}/upm.props";
            var pckJson = dir + $"/BoysheO.UnityLibrary.Unity/Assets/Scripts/{project}/package.json";
            var version = GetVersion(pckJson);
            var upmPropsContent = templ.Format(version);
            File.WriteAllText(upmProps, upmPropsContent);
            _logger.LogInformation("write success:{upmProps}", upmProps);
        }

        _logger.LogInformation("done");
    }
}