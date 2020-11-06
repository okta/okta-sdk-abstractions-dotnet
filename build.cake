#addin nuget:?package=Cake.Figlet&version=1.3.1

// Default MSBuild configuration arguments
var configuration = Argument("configuration", "Release");

Task("Clean")
.Does(() =>
{
    CleanDirectory("./artifacts/");

    GetDirectories("./src/**/bin")
        .ToList()
        .ForEach(d => CleanDirectory(d));

    GetDirectories("./src/**/obj")
        .ToList()
        .ForEach(d => CleanDirectory(d));
});

Task("Restore")
.Does(() => 
{
    DotNetCoreRestore("./src/Okta.Sdk.Abstractions.sln");
});

Task("Build")
.IsDependentOn("Restore")
.Does(() =>
{
    var projects = GetFiles("./src/**/*.csproj");
    Console.WriteLine("Building {0} projects", projects.Count());

    foreach (var project in projects)
    {
        Console.WriteLine("Building project ", project.GetFilenameWithoutExtension());
        DotNetCoreBuild(project.FullPath, new DotNetCoreBuildSettings
        {
            Configuration = configuration
        });
    }
});

Task("Test")
.IsDependentOn("Restore")
.IsDependentOn("Build")
.Does(() =>
{
    var testProjects = new[] { "Okta.Sdk.Abstractions.UnitTests" };
    // For now, we won't run integration tests in CI

    foreach (var name in testProjects)
    {
        DotNetCoreTest(string.Format("./src/{0}/{0}.csproj", name));
    }
});
Task("Pack")
.IsDependentOn("Test")
.Does(() =>
{
	var projects = new List<string>()
	{
		"Okta.Sdk.Abstractions",
	};
	
	projects
    .ForEach(name =>
    {
        Console.WriteLine($"\nCreating NuGet package for {name}");
        
		DotNetCorePack($"./src/{name}", new DotNetCorePackSettings
		{
			Configuration = configuration,
			OutputDirectory = "./artifacts",
		});
    });
	
});

Task("Info")
.Does(() => 
{
    Information(Figlet("Okta.Sdk.Abstractions"));

    var cakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();

    Information("Building using {0} version of Cake", cakeVersion);
});

// Define top-level tasks
Task("Default")
    .IsDependentOn("Info")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

// Default task
var target = Argument("target", "Default");
RunTarget(target);
