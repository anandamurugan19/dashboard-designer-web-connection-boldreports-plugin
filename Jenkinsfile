node('BoldBI')
{
   timestamps 
   { 
   timeout(time: 7200000, unit: 'MILLISECONDS') 
   {
   if(githubSourceBranch == 'master')
	{
		env.nugetserverurl = 'http://nexus.syncfusion.com/repository/nuget-master/,https://api.nuget.org/v3/index.json';
	}
        else if(githubTargetBranch.toLowerCase() == 'development')
	{
	    env.nugetserverurls = 'http://nexus.syncfusion.com/repository/boldbi-development/,https://api.nuget.org/v3/index.json';
	}

    stage 'Checkout' 
    try
    {     
	   checkout scm
	}
 
    catch(Exception e)
    {
		echo "Exception in checkout stage \r\n"+e
		currentBuild.result = 'FAILURE'	
    } 

if(currentBuild.result != 'FAILURE')
{ 
	stage 'Build Source'
	try
	{	    
	    gitlabCommitStatus("Build")
		{
			bat 'powershell.exe -ExecutionPolicy ByPass -File build/build.ps1 -Script '+env.WORKSPACE+"/build/build.cake -Target build -nugetserverurl "+env.nugetserverurls +" -StudioVersion "+env.studio_version
	 	}
		
		def files = findFiles(glob: '**/cireports/errorlogs/*.txt')               
        if(files.size() > 0)           
        {                 
   	       currentBuild.result = 'FAILURE'                              
        }
    } 
	 catch(Exception e) 
    {
		echo "Exception in Build stage \r\n"+e
		currentBuild.result = 'FAILURE'
    }
}	

// Product team request to comment this task

 //if(currentBuild.result != 'FAILURE')
// { 
//	 stage 'Test'
	// try
	// {
	  //   gitlabCommitStatus("Test")
	// {
		//	 bat 'powershell.exe -ExecutionPolicy ByPass -File build/build.ps1 -Script '+env.WORKSPACE+"/build/build.cake -Target test"
	 	 //}
    //}
	 // catch(Exception e) 
    // {
		// echo "Exception in Test stage \r\n"+e
		// currentBuild.result = 'FAILURE'
     //}
 //}
 
if(currentBuild.result != 'FAILURE')
{
   stage 'Code violation'	
   try
   {
	    gitlabCommitStatus("Code violation")
	    {
			 bat 'powershell.exe -ExecutionPolicy ByPass -File build/build.ps1 -Script '+env.WORKSPACE+"/build/build.cake -Target codeviolation"
	    }	
   }
	catch(Exception e) 
	{
		echo "Exception in codeviolation stage \r\n"+e
		currentBuild.result = 'FAILURE'
	}
} 

 if(currentBuild.result != 'FAILURE' && env.publishBranch.contains(githubSourceBranch))
 { 
	 stage 'Publish'
	 try
	 {	    
	     gitlabCommitStatus("Publish")
		 {			
			  bat 'powershell.exe -ExecutionPolicy ByPass -File build/build.ps1 -Script '+env.WORKSPACE+"/build/build.cake -Target publish -nugetapikey "+env.nugetapikey+' -revisionNumber '+env.revisionNumber+' -nugetserverurl '+env.nugetserverurls+" -StudioVersion "+env.studio_version
	 	 }
     } 
	  catch(Exception e) 
     {
		 echo "Exception in Publish stage \r\n"+e
		 currentBuild.result = 'FAILURE'
     }
  }	
  
	stage 'Delete Workspace'
	
	// Archiving artifacts when the folder was not empty
	
    def files = findFiles(glob: '**/cireports/**/*.*')      
    
    if(files.size() > 0) 		
    { 		
        archiveArtifacts artifacts: 'cireports/', excludes: null 		
    }
	
	   ([$class: 'WsCleanup'])  
	   }
    }	   
}