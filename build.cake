#load scripts/FixAutomationCoreIL.cake

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// Variables
///////////////////////////////////////////////////////////////////////////////

var visualStudioCommandPrompt = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\VsDevCmd.bat";
if (!FileExists(visualStudioCommandPrompt))
{
    // Fallback to community edition
    visualStudioCommandPrompt = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat";
}

//var windowsSdkVersion = new Version(10, 0, 16299, 0);
//var windowsSdkVersion = new Version(10, 0, 17763, 0);
//var windowsSdkVersion = new Version(10, 0, 18362, 0);
//var windowsSdkVersion = new Version(10, 0, 19041, 0);
//var windowsSdkVersion = new Version(10, 0, 20348, 0);
var windowsSdkVersion = new Version(10, 0, 22000, 194);

var windowsSdkVersionString = windowsSdkVersion.ToString();
var nugetPackageVersion = $"{windowsSdkVersion.Major}.{windowsSdkVersion.Build}.0";

var artifacts = Directory("./.artifacts");
var temp = Directory("./.temp");
var files = Directory("./files");
var nugetFolder = Directory("./.nuget");
var tlbimpExecutableMap = new Dictionary<string, string>() {
   { "3.5", @"files\Microsoft SDK\TlbImp\v7.0A\TlbImp.exe" },
   { "4.5", @"files\Microsoft SDK\TlbImp\v10.0A\NETFX 4.8 Tools\TlbImp.exe" }
};

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Nothing to do
});

Teardown(ctx =>
{
   // Nothing to do
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean-Output")
   .Does(() =>
{
   CleanDirectory(artifacts);
   CleanDirectory(temp);
});

Task("Build")
   .IsDependentOn("Clean-Output")
   .Does(() =>
{
   // Create the type libraries
   RunInVisualStudioCommandPrompt($@"midl.exe /nologo /out ""{temp}"" /char signed /tlb UIAutomationClient.tlb /h UIAutomationClient_h.h ""{files}/Windows SDK/{windowsSdkVersionString}/UIAutomationClient.idl""");
   RunInVisualStudioCommandPrompt($@"midl.exe /nologo /out ""{temp}"" /char signed /tlb UIAutomationCore.tlb /h UIAutomationCore.h ""{files}/Windows SDK/{windowsSdkVersionString}/UIAutomationCore.idl""");

   // Create the dlls from the tlbs
   foreach (var fwVersion in new [] {"3.5", "4.5"}) {
      // Prepare some folders
      var outFolder = temp + Directory(fwVersion);
      var outFolderSigned = temp + Directory("signed") + Directory(fwVersion);
      CleanDirectory(outFolder);
      CleanDirectory(outFolderSigned);
      var finalFolder = artifacts + Directory(fwVersion);
      var finalFolderSigned = artifacts + Directory("signed") + Directory(fwVersion);
      CleanDirectory(finalFolder);
      CleanDirectory(finalFolderSigned);
      
      // Generate the dlls for the client
      RunInVisualStudioCommandPrompt($@"""{tlbimpExecutableMap[fwVersion]}"" /machine:Agnostic /silent /asmversion:{windowsSdkVersionString} /out:""{finalFolder}\Interop.UIAutomationClient.dll"" ""{temp}\UIAutomationClient.tlb""");
      RunInVisualStudioCommandPrompt($@"""{tlbimpExecutableMap[fwVersion]}"" /machine:Agnostic /silent /asmversion:{windowsSdkVersionString} /out:""{finalFolderSigned}\Interop.UIAutomationClient.dll"" ""{temp}\UIAutomationClient.tlb"" /keyfile:""key.snk""");
      // Generate the dlls for the core
      RunInVisualStudioCommandPrompt($@"""{tlbimpExecutableMap[fwVersion]}"" /machine:Agnostic /silent /asmversion:{windowsSdkVersionString} /out:""{outFolder}\Interop.UIAutomationCore.dll"" ""{temp}\UIAutomationCore.tlb""");
      RunInVisualStudioCommandPrompt($@"""{tlbimpExecutableMap[fwVersion]}"" /machine:Agnostic /silent /asmversion:{windowsSdkVersionString} /out:""{outFolderSigned}\Interop.UIAutomationCore.dll"" ""{temp}\UIAutomationCore.tlb"" /keyfile:""key.snk""");

      // Apply some manual fixes to the UIAutomationCore dlls
      // Disassemble the dlls
      RunInVisualStudioCommandPrompt($@"ildasm.exe {outFolder}\Interop.UIAutomationCore.dll /out={outFolder}\UIAutomationCore.il /nobar");
      RunInVisualStudioCommandPrompt($@"ildasm.exe {outFolderSigned}\Interop.UIAutomationCore.dll /out={outFolderSigned}\UIAutomationCore.il /nobar");
      // Apply the fixes to the IL files
      FixIL($@"{outFolder}\UIAutomationCore.il");
      FixIL($@"{outFolderSigned}\UIAutomationCore.il");
      // Re-Assemble the dlls
      RunInVisualStudioCommandPrompt($@"ilasm.exe /dll /output={finalFolder}\Interop.UIAutomationCore.dll {outFolder}\UIAutomationCore.il /res:{outFolder}\UIAutomationCore.res");
      RunInVisualStudioCommandPrompt($@"ilasm.exe /dll /output={finalFolderSigned}\Interop.UIAutomationCore.dll {outFolderSigned}\UIAutomationCore.il /res:{outFolderSigned}\UIAutomationCore.res");
   }
});

Task("Pack")
   .Does(() =>
{
   CleanDirectory(nugetFolder);

   NuGetPack(CreateNuGetPackSettings("UIAutomationClient", false));
   NuGetPack(CreateNuGetPackSettings("UIAutomationClient", true));

   NuGetPack(CreateNuGetPackSettings("UIAutomationCore", false));
   NuGetPack(CreateNuGetPackSettings("UIAutomationCore", true));
});

Task("Push")
   .Does(() =>
{
    var apiKey = System.IO.File.ReadAllText(".nugetapikey");

    // Get the paths to the packages.
    var files = GetFiles($"{nugetFolder}/*.nupkg");
    foreach (var package in files) {
        Information($"Pushing {package}");
        NuGetPush(package, new NuGetPushSettings {
            Source = "https://nuget.org/api/v2/package",
            ApiKey = apiKey
        });
    }
});

Task("Default")
   .Does(() =>
{
   Information("Hello Cake!");
});

RunTarget(target);

///////////////////////////////////////////////////////////////////////////////
// Methods
///////////////////////////////////////////////////////////////////////////////

private NuGetPackSettings CreateNuGetPackSettings(string package, bool signed) {
   var packageId = $"Interop.{package}";
   var baseFolder = artifacts;
   if (signed) {
      packageId += ".Signed";
      baseFolder += Directory("signed");
   }

   return new NuGetPackSettings {
      Id                       = packageId,
      Version                  = nugetPackageVersion,
      Title                    = packageId,
      Authors                  = new[] { "Roemer" },
      Owners                   = new[] { "Roemer" },
      Description              = "The UI Automation COM-to-.NET adapter makes it possible to use the new Windows Automation API 3.0 COM interfaces, with their improved reliability and performance, while still using the same System.Windows.Automation classes as in earlier versions of UI Automation",
      ProjectUrl               = new Uri("https://github.com/Roemer/UIAutomation-Interop"),
      License                  = new NuSpecLicense { Type = "file", Value = "LICENSE.txt" },
      Copyright                = $"Copyright (c) {DateTime.Now.Year}",
      Tags                     = new [] { package, "UIAutomation", "Windows", "Automation", "Accessibility", "System.Windows.Automation" },
      RequireLicenseAcceptance = false,
      Files                    = new [] {
                                    new NuSpecContent { Source = $"{baseFolder}/3.5/Interop.{package}.dll", Target = "lib/net35" },
                                    new NuSpecContent { Source = $"{baseFolder}/4.5/Interop.{package}.dll", Target = "lib/net40" },
                                    new NuSpecContent { Source = $"{baseFolder}/4.5/Interop.{package}.dll", Target = "lib/net45" },
                                    new NuSpecContent { Source = $"{baseFolder}/4.5/Interop.{package}.dll", Target = "lib/netcoreapp3.0" },
                                    new NuSpecContent { Source = $"{baseFolder}/4.5/Interop.{package}.dll", Target = "lib/netstandard2.0" },
                                    new NuSpecContent { Source = $"LICENSE.txt", Target = "" },
                                    new NuSpecContent { Source = $"files/Interop.{package}.targets", Target = "build" },
                                    new NuSpecContent { Source = $"files/install_{package}.ps1", Target = "tools/install.ps1" },
                                 },
      BasePath                 = "./",
      OutputDirectory          = nugetFolder
   };
}

private void RunInVisualStudioCommandPrompt(string command) {
   IEnumerable<string> redirectedStandardOutput;
   var exitCode = StartProcess("cmd.exe", new ProcessSettings {
         Arguments = $@"/c """"{visualStudioCommandPrompt}"" & {command}""",
         RedirectStandardOutput = true
      },
      out redirectedStandardOutput
   );
   foreach (var line in redirectedStandardOutput) {
      Information(line);
   }
   if (exitCode != 0) {
      throw new Exception($"Failed with exit code: {exitCode}");
   }
}
