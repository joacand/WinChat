using System.Diagnostics;

namespace WinChat.Infrastructure.Services;

public sealed class CommandSenderService
{
    public static string FormatProcessResult(string output, string command)
    {
        var lines = output.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        var capture = false;
        List<string> resultLines = [];

        foreach (var line in lines)
        {
            if (capture && !line.Contains("exit"))
            {
                resultLines.Add(line);
            }
            else if (line.Trim().Contains(command, StringComparison.OrdinalIgnoreCase))
            {
                capture = true;
            }
        }

        return string.Join(Environment.NewLine, resultLines).Trim();
    }

    public static string SendCmd(string args)
    {
        using Process process = new();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
        process.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");

        process.Start();

        process.StandardInput.WriteLine(args);
        process.StandardInput.WriteLine("exit");
        process.StandardInput.Flush();
        process.StandardInput.Close();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return output + Environment.NewLine + error;
    }
}
