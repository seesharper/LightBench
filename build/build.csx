
#load "common.csx"

string pathToBuildDirectory = @"tmp/";
private string version = "1.1.1";

WriteLine("LightBench version {0}" , version);

Execute(() => InitializBuildDirectories(), "Preparing build directories");
Execute(() => BuildAllFrameworks(), "Building all frameworks");
Execute(() => CreateNugetPackages(), "Creating NuGet packages");

private void CreateNugetPackages()
{		
	string pathToProjectFile = Path.Combine(pathToBuildDirectory, @"netstandard16/Binary/project.json");
	DotNet.Pack(pathToProjectFile, pathToBuildDirectory);					
    string myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    RoboCopy(pathToBuildDirectory, myDocumentsFolder, "*.nupkg");		
}

private void BuildAllFrameworks()
{		
	BuildDotNet();
}

private void BuildDotNet()
{		
	string pathToProjectFile = Path.Combine(pathToBuildDirectory, @"netstandard16/Binary/project.json");
	DotNet.Build(pathToProjectFile);
}

private void InitializBuildDirectories()
{
	DirectoryUtils.Delete(pathToBuildDirectory);			
	Execute(() => InitializeNugetBuildDirectory("NETSTANDARD16"), "Preparing NetStandard1.6");	    						
}

private void InitializeNugetBuildDirectory(string frameworkMoniker)
{
	var pathToBinary = Path.Combine(pathToBuildDirectory, frameworkMoniker +  "/Binary");		
    CreateDirectory(pathToBinary);
	RoboCopy("../src", pathToBinary, "/e /XD bin obj .vs NuGet TestResults packages /XF project.lock.json");											  
}
 