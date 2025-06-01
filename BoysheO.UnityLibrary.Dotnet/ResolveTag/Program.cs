// Console.WriteLine("Hello, World!");
// Console.WriteLine(args.JoinAsOneString(","));
try
{
    var tagName = args[0];
    var sp = tagName.Split("_");
    var packName = sp[0];
    var version = sp[1];

    Console.WriteLine($"PACKAGE_NAME={packName}");
    Console.WriteLine($"PACKAGE_VER={version}");
    Console.WriteLine($"PROJECT_DIR=BoysheO.UnityLibrary.Dotnet/{packName}");
}
catch (Exception exception)
{
    Console.WriteLine($"Exception={exception}");
}