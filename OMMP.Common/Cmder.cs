using System.Diagnostics;

namespace OMMP.Common;

public class Cmder
{
    public static string Run(string command)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo("/bin/bash", "")
        };
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.StandardInput.WriteLine(command);
        process.StandardInput.Close();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Dispose();
        return output;
    }

    public static string Run(string command, string arg)
    {
        var process = new Process();
        process.StartInfo.FileName = command; // Linux shell
        process.StartInfo.Arguments = arg; // 列出当前目录下的文件
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        // 读取命令的输出
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        // 等待命令执行完成
        process.WaitForExit();
        // 关闭进程
        process.Close();
        return output;
    }
}