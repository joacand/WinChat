using System.Diagnostics;
using System.IO;
using System.Windows;
using WinChat.Infrastructure;

namespace WinChat.Services;

internal static class CommandLineCommandService
{
    public static string ProcessCommands(string text)
    {
        var commands = new Dictionary<string, Action<string>>
        {
            [Constants.Commands.CommandLine.Name] = result => MessageBox.Show(result),
        };

        var processedText = text;

        foreach (var (command, action) in commands)
        {
            var commandWithAffixes = $"{{{command}:";
            if (!processedText.Contains(commandWithAffixes)) continue;

            var parts = processedText.Split([commandWithAffixes], StringSplitOptions.None);
            if (parts.Length < 2) continue;

            var args = new string([.. parts[1].TakeWhile(x => x != '}')]);
            if (args.Length == 0) continue;

            var userResult = MessageBox.Show("The assistant wants to send the following command, do you want to proceed?" +
                $"{Environment.NewLine}{Environment.NewLine}{args}", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (userResult != MessageBoxResult.OK)
            {
                return $"⚠️ Command '{args}' aborted by user ⚠️";
            }

            var result = FormatProcessResult(SendCmd(args), args);

            processedText = result;
        }

        return processedText;
    }

    private static string FormatProcessResult(string output, string command)
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

    private static string SendCmd(string args)
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
