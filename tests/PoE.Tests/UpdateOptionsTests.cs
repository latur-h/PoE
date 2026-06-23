using PoE.Updater;
using Xunit;

namespace PoE.Tests;

public class UpdateOptionsTests
{
    [Fact]
    public void Parse_reads_required_arguments()
    {
        var options = UpdateOptions.Parse(
        [
            "--wait-pid", "1234",
            "--zip", @"C:\temp\PoE-1.0.16-win-x64.zip",
            "--install", @"C:\apps\PoE",
            "--exe", @"C:\apps\PoE\PoE.exe",
        ]);

        Assert.Equal(1234, options.WaitProcessId);
        Assert.Equal(@"C:\temp\PoE-1.0.16-win-x64.zip", options.ZipPath);
        Assert.Equal(@"C:\apps\PoE", options.InstallDirectory);
        Assert.Equal(@"C:\apps\PoE\PoE.exe", options.ExecutablePath);
    }

    [Fact]
    public void Parse_throws_when_wait_pid_missing()
    {
        Assert.Throws<ArgumentException>(() => UpdateOptions.Parse(
        [
            "--zip", "a.zip",
            "--install", "C:\\apps\\PoE",
            "--exe", "C:\\apps\\PoE\\PoE.exe",
        ]));
    }
}
