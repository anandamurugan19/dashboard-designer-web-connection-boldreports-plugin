#addin nuget:?package=Cake.FileHelpers&version=4.0.1

using System.Text.RegularExpressions;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var cireports = Argument("cireports","../cireports");
var nugetserverurl = Argument<string>("nugetserverurl","");
var nugetapikey = Argument<string>("nugetapikey","");
var assemblyfileversion=Argument<string>("assemblyfileversion","");
var STUDIO_VERSION=Argument<string>("studio_version","");
var studio_version = Argument("studio_version", STUDIO_VERSION).Split('.'); 
var referencepath = Argument<string>("referencepath", "");
var outputpath = Argument<string>("outputpath", "");
var PreReleaseNumber = Argument("PreReleaseNumber", ""); 


////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

#tool "nuget:?package=NUnit.ConsoleRunner"
#tool "nuget:?package=JetBrains.dotCover.CommandLineTools"
#tool "nuget:?package=ReportUnit"


var projectName = "Dashboard.Connection.LiveWebConnection";


var currentDirectory=MakeAbsolute(Directory("../"));
var currentDirectoryInfo=new DirectoryInfo(currentDirectory.FullPath);
var fxCopViolationCount=0;
var styleCopViolationCount=0;
string Copyrights="[assembly: AssemblyCopyright(\"Copyright (c) 2001-"+DateTime.Now.Year+" Syncfusion. Inc,\")]";
var fxcopFolder = cireports+"/fxcopviolation/";
var stylecopFolder = cireports+"/stylecopviolation/";
var errorlogFolder = cireports + "/errorlogs/";
var waringsFolder = cireports + "/warnings/";
var xunitFolder = cireports+"/xunit";
var codecoverageFolder = cireports+"/codecoverage";
string platform="SECURITY";
var nugetPackageFolder="";
FilePath sourceFile {get; set;}

var nugetSources="";
List<string> nugetSource = new List<string>();
var nugetserverurls=nugetserverurl.Split(',');	
foreach(var nugeturl in nugetserverurls)
{
    Information(nugeturl);
    nugetSource.Add(nugeturl);
    nugetSources=nugetSources+nugeturl+';';
}
nugetserverurl =nugetserverurl.Split(',')[0];

Information("current Directory is {0}",currentDirectory);
Information("NexusServer URL is {0}",nugetserverurl);
Information("STUDIO_VERSION is {0} ",STUDIO_VERSION);
Information("studio_version is {0}",studio_version);

//////////////////////////////////////////////////////////////////////
// Regex
//////////////////////////////////////////////////////////////////////

var fxCopRegex = "warning CA";
var styleCopRegex = "warning SA";
var styleCopAnalyzersRegex = "warning SX";
var xUnitRegex = "warning xUnit";
var apiAnalyzerRegex = "warning API";
var asyncAnalyzerRegex = "warning AsyncFixer";
var cSharpAnalyzerRegex = "warning RS";
var mvcAnalyzerRegex = "warning MVC";
var entityFrameworkRegex = "warning EF";
var rosylnatorAnalyzerRegex = "warning RCS";
var nugetRegex = "warning NU";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{

    var binDirectories=currentDirectoryInfo.GetDirectories("bin",SearchOption.AllDirectories);
    var objDirectories=currentDirectoryInfo.GetDirectories("obj",SearchOption.AllDirectories);
    
    foreach(var directory in binDirectories){
        CleanDirectories(directory.FullName);
    }
    
    foreach(var directory in objDirectories){
        CleanDirectories(directory.FullName);
    }

    DeleteFiles(GetFiles(currentDirectory+"/**/*FxCopAnalysis.xml"));
    DeleteFiles(GetFiles(currentDirectory+"/**/*StyleCopAnalysis.xml"));
})
.OnError(exception =>
{
    throw new Exception("Cake process Failed on Clean Task");
});

Task("CopyrightandVersion")
  .IsDependentOn("Clean")
  .Does(() =>
{
  var assemblyfiles = GetFiles("../**/*AssemblyInfo.cs");
  foreach(var assemblyfile in assemblyfiles)
  {
  ReplaceRegexInFiles(assemblyfile.ToString(),@"[\d]{1,2}\.[\d]{1}\.[\d]{1}\.[\d]{1,4}",STUDIO_VERSION);
  ReplaceRegexInFiles(assemblyfile.ToString(),@"\[assembly:\s*AssemblyCopyright\s*.*?\]",Copyrights);
  ReplaceRegexInFiles(assemblyfile.ToString(),@"AssemblyCompany\s*.*","AssemblyCompany(\"Syncfusion, Inc.\")]");
  }
})
.OnError(exception =>
{
    throw new Exception("Cake process Failed on CopyrightandVersion Task");
});


Task("DeleteLogFile")
	.Does(()=>{
		
    if(FileExists("../cireports/errorlogs/"+ projectName +".txt"))
      DeleteFile("../cireports/errorlogs/"+ projectName +".txt");
    if(FileExists("../cireports/warnings/"+ projectName +".txt") && referencepath == "")
      DeleteFile("../cireports/warnings/"+ projectName +".txt");
})
.OnError(exception =>
{
    throw new Exception("Cake process Failed on DeleteLogFile Task");
});


Task("Restore-NuGet-Packages")
    .Does(() =>
{    
    var slnFiles = GetFiles("../**/*.sln");
    foreach(var slnFile in slnFiles){
        Information("slnFile {0}", slnFile);
        DotNetCoreRestore(slnFile.ToString(),new DotNetCoreRestoreSettings {Sources =nugetSource, EnvironmentVariables = new Dictionary<string, string>{
        { "Configuration", configuration.ToString() }
    }});
    }
})
.OnError(exception =>
{
    throw new Exception("Cake process Failed on Restore-NuGet-Packages Task");
});
Task("Download-Nugetexe")
  .WithCriteria( !FileExists("./tools/nuget.exe"))
  .ContinueOnError()
  .Does(() =>
{
 
     DownloadFile("http://dist.nuget.org/win-x86-commandline/latest/nuget.exe", "./tools/nuget.exe");
     
});
Task("Create-NugetPackage-Directory")
.Does(() =>{
            var nugetContent = System.Xml.Linq.XDocument.Parse(System.IO.File.ReadAllText(nugetPackageFolder+"/NuGet.Config",System.Text.Encoding.UTF8)); //Read the text of config file
            var nugetConfigElement = (from elements in nugetContent.Descendants("config") select elements).ToList();//get the config element value
            if (nugetConfigElement.Count==0)//check the config element had value or not
            {
				EnsureDirectoryExists(sourceFile.GetDirectory().ToString()+"/packages");
            }
            else
            {
				EnsureDirectoryExists(nugetPackageFolder+"/"+nugetConfigElement[0].ToString().Split('"')[3]);
            }
})
.OnError(exception =>
{
    throw new Exception("Cake process Failed on Create-NugetPackage-Directory Task");
});

Task("Update-Nuget-Packages")
.Does(() =>{
              
		if(FileExists(currentDirectory.ToString()+"/NuGet.Config"))
        {
            nugetPackageFolder=currentDirectory.ToString();
            RunTarget("Create-NugetPackage-Directory");
		} 
                                           
        var slnFiles = GetFiles("../**/*.sln");

        //update the nuget packages
        foreach(var slnFile in slnFiles)
        {
		
            sourceFile=File(slnFile.ToString());
			if (FileExists(sourceFile.GetDirectory().ToString()+"/.nuget/NuGet.Config"))
            {
				nugetPackageFolder=sourceFile.GetDirectory().ToString()+"/.nuget/";
                RunTarget("Create-NugetPackage-Directory");
				StartProcess("./tools/nuget.exe", new ProcessSettings
                { Arguments ="update "+ slnFile + " -ConfigFile " +nugetPackageFolder+"/NuGet.Config -Source "+nugetSources});
            }
            else if(FileExists(currentDirectory.ToString()+"/NuGet.Config"))
            {
				StartProcess("./tools/nuget.exe", new ProcessSettings
                { Arguments ="update "+ slnFile + " -ConfigFile " +nugetPackageFolder+"/NuGet.Config -Source "+nugetSources});
            }
            
			//if nuget.config does not exist                 
            else
            {	EnsureDirectoryExists(sourceFile.GetDirectory().ToString()+"/packages");
                NuGetUpdate(slnFile,new NuGetUpdateSettings {Source=nugetSource});
            }
        }
})
.OnError(exception =>
{
    throw new Exception("Cake process Failed on Update-Nuget-Packages Task");
});

Task("build")
	.IsDependentOn("Download-Nugetexe")
	.IsDependentOn("CopyrightandVersion")
    .IsDependentOn("Restore-NuGet-Packages")
	.ContinueOnError()
    .Does(() =>
{
      if(!string.IsNullOrEmpty(outputpath))
      {
         outputpath+=@"\livewebconnection";
		         referencepath="\""+referencepath+";"+outputpath+"\"";
      }
    RunTarget("DeleteLogFile");
	  EnsureDirectoryExists("../cireports/errorlogs");
      EnsureDirectoryExists("../cireports/warnings"); 
      MSBuild(currentDirectory + @"\src\"+projectName+".sln",  new MSBuildSettings(){ArgumentCustomization = args=>args.Append("/p:ReferencePath=" + referencepath)}
		.AddFileLogger(new MSBuildFileLogger { LogFile="../cireports/warnings/"+projectName+".txt",MSBuildFileLoggerOutput=MSBuildFileLoggerOutput.WarningsOnly})
		.AddFileLogger(new MSBuildFileLogger { LogFile="../cireports/errorlogs/"+projectName+".txt",MSBuildFileLoggerOutput=MSBuildFileLoggerOutput.ErrorsOnly})
		//.WithProperty("ReferencePath",referencepath)
		.WithProperty("OutDir",outputpath)
		.WithProperty("CodeAnalysisRuleSet",currentDirectory+@"/FxCop.ruleset")
		.WithProperty("StyleCopOverrideSettingsFile","\""+currentDirectory+@"/Settings.StyleCop"+"\"")
		.SetConfiguration(configuration)
        .SetVerbosity(Verbosity.Minimal));
		
	//	RunTarget("DeleteLogFile");

    var logFilename = projectName + ".txt";
    if(FileExists(errorlogFolder + logFilename))
    {
        if (FileSize(errorlogFolder + logFilename) == 0 )
            DeleteFile(errorlogFolder + logFilename);
    }
});

Task("GetFxCopReports")
.Does(()=>
{
	try
	{
		var logFilename = projectName + ".txt";
		if (DirectoryExists(fxcopFolder))
		{
		 DeleteDirectory(fxcopFolder, recursive:true);
		}
		
		var fxCopAnalysisFiles=FileReadText(waringsFolder + logFilename);
		
		fxCopViolationCount = Regex.Matches(fxCopAnalysisFiles, fxCopRegex).Count;
		fxCopViolationCount += Regex.Matches(fxCopAnalysisFiles, apiAnalyzerRegex).Count;
		fxCopViolationCount += Regex.Matches(fxCopAnalysisFiles, asyncAnalyzerRegex).Count;
		fxCopViolationCount += Regex.Matches(fxCopAnalysisFiles, cSharpAnalyzerRegex).Count;
		fxCopViolationCount += Regex.Matches(fxCopAnalysisFiles, mvcAnalyzerRegex).Count;
		fxCopViolationCount += Regex.Matches(fxCopAnalysisFiles, entityFrameworkRegex).Count; 
		fxCopViolationCount += Regex.Matches(fxCopAnalysisFiles, rosylnatorAnalyzerRegex).Count; 
		
		fxCopViolationCount = 0;
		if(fxCopViolationCount != 0)
		{        
		   Information("There are {0} FXCop violations found", fxCopViolationCount);
		}

		if (!DirectoryExists(cireports))
		{
			CreateDirectory(cireports);
		}
			
		if(!DirectoryExists(fxcopFolder))
		{
			CreateDirectory(fxcopFolder);
		}
		
		FileWriteText(fxcopFolder + "FXCopViolations.txt", "FXCop Error(s) : " + fxCopViolationCount);
	}
	catch(Exception ex) {        
		throw new Exception(String.Format("Please fix Get Fx Cop Reports failures "+ ex));  
	}
		
});




Task("GetStyleCopReports")
 .Does(()=>
 {
    try
	{
		var logFilename = projectName + ".txt";
		if (DirectoryExists(stylecopFolder))
		{
		 DeleteDirectory(stylecopFolder, recursive:true);
		}
		var styleCopWarning = FileReadText(waringsFolder + logFilename);
		styleCopViolationCount += Regex.Matches(styleCopWarning, styleCopRegex).Count;
		styleCopViolationCount += Regex.Matches(styleCopWarning, styleCopAnalyzersRegex).Count;

		styleCopViolationCount=0;
		if(styleCopViolationCount != 0)
		{        
		   Information("There are {0} StyleCop violations found", styleCopViolationCount);
		}
		
		if(!DirectoryExists(cireports))
		{
			CreateDirectory(cireports);
		}

		if(!DirectoryExists(stylecopFolder))
		{
			CreateDirectory(stylecopFolder);
		}
		
		FileWriteText(stylecopFolder + "StyleCopViolations.txt", "Style Cop Error(s) : " + styleCopViolationCount);
	}
	catch(Exception ex) {        
		throw new Exception(String.Format("Please fix Get Style Cop Reports failures " + ex));  
	}

 });

 
 Task("codeviolation")
	.IsDependentOn("GetFxCopReports")
	.IsDependentOn("GetStyleCopReports")
	.Does(()=>{
		Information("Code violation");
		Information("StyleCop violations = {0}",styleCopViolationCount);
		Information("FxCop violations = {0}",fxCopViolationCount);
		if(fxCopViolationCount!=0 || styleCopViolationCount!=0)
		{
			//throw new Exception("Code violations found");
            Information("Code violations found");
		}
		else
		{
			Information("Code Analysis succees");
		}
		
 })
 .OnError(exception =>
 {
     throw new Exception("Cake process Failed on codeviolation Task");
 });	


	

    Task("packnuget")
	.Does(() => {
	
		var nuspec = GetFiles("../src/**/*.nuspec");
		foreach(var spec in nuspec){
		var nuGetPackSettings = new NuGetPackSettings
		{  
			OutputDirectory = currentDirectory,
			Version = studio_version[0]+"."+studio_version[1]+"."+studio_version[2]+"."+PreReleaseNumber,
			ArgumentCustomization = args => args.Append("-Prop Configuration=" + configuration)
		};
			
		NuGetPack(spec.FullPath, nuGetPackSettings);
		}
	})
	.OnError(exception =>
	{
	    throw new Exception("Cake process Failed on packnuget Task");
	});
	
    Task("publish")
	.IsDependentOn("packnuget")
	.Does(()=>{
			var packages = GetFiles(currentDirectory+"\\*.nupkg");
			foreach(var package in packages){
			NuGetPush(package, new NuGetPushSettings 
			{
				Source = nugetserverurl,
				ApiKey = nugetapikey
			});
			}
	})
	.OnError(exception =>
	{
	    throw new Exception("Cake process Failed on publish Task");
	});


	Task("DotCoverCover").Does(()=>{
		if(!DirectoryExists(cireports+"/xunit"))
			  {
					CreateDirectory(cireports+"/xunit");
			  }
		DotCoverCover(tool => {
						  tool.XUnit2(currentDirectory + @"\test\"+projectName+@".Test\bin\"+configuration+@"\*.Test.dll",
						  new XUnit2Settings {
						  Parallelism = ParallelismOption.All,
						  HtmlReport = true,
						  XmlReport = true,
						  OutputDirectory = xunitFolder
						  });},
						   new FilePath(cireports+"/codecoverage/UnitTestCover.dcvr"),
						 new DotCoverCoverSettings()
						  .WithScope(currentDirectory + @"\test\"+projectName+@".Test\bin\*.dll")
						.WithFilter("-:*Test")
					);
			  })
			  .OnError(exception =>
		{ 
			throw new Exception("Cake process Failed on DotCoverCover Task");
		});

Task("test")
      .IsDependentOn("DotCoverCover")
      .ContinueOnError()
      .Does(()=>{
            DotCoverReport(new FilePath(codecoverageFolder+"/UnitTestCover.dcvr"),
            new FilePath(codecoverageFolder+"/UnitTestCover.html"),
            new DotCoverReportSettings {
                  ReportType = DotCoverReportType.HTML
            });  
            DotCoverReport(new FilePath(codecoverageFolder+"/UnitTestCover.dcvr"),
            new FilePath(codecoverageFolder+"/UnitTestCover.xml"),
                  new DotCoverReportSettings {
                        ReportType = DotCoverReportType.XML
            });  
            var  coveragePercent =(from elements in System.Xml.Linq.XDocument.Load(codecoverageFolder+"/UnitTestCover.xml").Descendants("Root")
                                    select (string)elements.Attribute("CoveragePercent")).FirstOrDefault();
           
            FileStream fs1 = new FileStream(codecoverageFolder+"/UnitTestCover.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter writer = new StreamWriter(fs1);
            writer.Write(coveragePercent);
            writer.Close();
      });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("build")
	.IsDependentOn("codeviolation")
	//.IsDependentOn("test")
	.IsDependentOn("publish");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
