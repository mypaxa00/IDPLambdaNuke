using System;
using System.Diagnostics;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;

class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.Deploy);
    
    [Solution] readonly Solution Solution;
    
    [Parameter]
    public string Profile = "default";
    [Parameter]
    public string FunctionName = "IDPNuke";
    [Parameter]
    public string Region = "eu-central-1";
    [Parameter]
    public string Role = "AlarmRole";
    [Parameter]
    public string Description = "AlarmMap - Ukraine. Lambda function for monitoring the status of the air raid warnings in Ukraine.";
    [Parameter]
    public uint Memory = 128;
    
    
    Target Deploy => _ => _
        .Executes(() =>
        {
            AbsolutePath projectPath = Solution.GetProject("IDPNuke")!.Directory;
            
            Memory = Math.Max(128, Memory);
            
            const string fileName = "dotnet";
            string arguments = $"lambda deploy-function {FunctionName} --profile {Profile} -pl {projectPath} --region {Region} -frole {Role} -fd \"{Description}\" -fms {Memory}";
            Log.Information("Deploying function {FileName} {Arguments}", fileName, arguments);
            ProcessStartInfo dotnetLambdaDeployFunction = new()
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            Process dotnetLambdaDeployFunctionProcess = Process.Start(dotnetLambdaDeployFunction);
            if(dotnetLambdaDeployFunctionProcess == null)
            {
                throw new Exception("Process is null");
            }
            
            string output = dotnetLambdaDeployFunctionProcess.StandardOutput.ReadToEnd();
            string error = dotnetLambdaDeployFunctionProcess.StandardError.ReadToEnd();

            
            dotnetLambdaDeployFunctionProcess.WaitForExit();
            int exitCode = dotnetLambdaDeployFunctionProcess.ExitCode;
            if (exitCode != 0)
            {
                Log.Information("Deploy:\n{Output}\n{Error}", output, error);
                throw new Exception($"Error: {error}");
            }
            Log.Information("Deployed");
        });

}
