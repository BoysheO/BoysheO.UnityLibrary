using System;
using System.IO;
using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;

public class ReadPackageJsonVersion : Microsoft.Build.Utilities.Task
{
    [Required]
    public string PackageJsonPath { get; set; }

    [Output]
    public string Version { get; set; }

    public override bool Execute()
    {
        if (!File.Exists(PackageJsonPath))
        {
            Log.LogError($"The file '{PackageJsonPath}' does not exist.");
            return false;
        }

        var jsonContent = File.ReadAllText(PackageJsonPath);
        var jsonObject = JObject.Parse(jsonContent);
        Version = jsonObject["version"]?.ToString();

        if (string.IsNullOrEmpty(Version))
        {
            Log.LogError("The 'version' property is not found in the package.json file.");
            return false;
        }

        Log.LogMessage($"Version from package.json: {Version}");
        return true;
    }
}