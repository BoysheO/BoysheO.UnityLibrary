using System;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.IO;
using Newtonsoft.Json.Linq;

public class ReadPackageJsonVersion : Microsoft.Build.Utilities.Task
{
    public bool IsConsoleDebug = false;

    [Required] public string PackageJsonPath { get; set; }

    [Output] public string Version { get; set; }

    public override bool Execute()
    {
        // TaskLoggingHelper.LogError("1");
        if (!File.Exists(PackageJsonPath))
        {
            PrintEr($"The file '{PackageJsonPath}' does not exist.");
            return false;
        }

        var jsonContent = File.ReadAllText(PackageJsonPath);
        var jsonObject = JObject.Parse(jsonContent);
        var version = jsonObject["version"]?.ToString();

        if (string.IsNullOrEmpty(version))
        {
            PrintEr("The 'version' property is not found in the package.json file.");
            return false;
        }

        Version = version;
        Print($"Version from package.json: {Version}");
        return !Log.HasLoggedErrors; //ms建议的
    }

    private void PrintEr(string msg)
    {
        if (IsConsoleDebug)
        {
            Console.Error.WriteLine(msg);
        }
        else
        {
            Log.LogError(msg);
        }
    }

    private void Print(string msg)
    {
        if (IsConsoleDebug)
        {
            Console.WriteLine(msg);
        }
        else
        {
            Log.LogMessage(msg);
        }
    }
}