using System.Diagnostics;

namespace OAMMP.Common;

public class Cmder
{
    public static async Task<string> RunAndWaitForExit(string command, string? args=null)
    {
        Process process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = args;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return output;
    }
    public static void Run(string command, string? args=null)
    {
        Process process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = args;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
    }
}