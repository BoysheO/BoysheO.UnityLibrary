// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

var path = @"D:/Repository/BoysheO.UnityLibrary/CustomVersionTest/src.json";
var ins = new ReadPackageJsonVersion();
ins.IsConsoleDebug = true;
ins.PackageJsonPath = path; 
var r = ins.Execute();
Console.WriteLine($"isSuccess={r}");
var version = ins.Version;
Console.WriteLine($"version = {version}");

