namespace Hitch.Tests;

public sealed class BootstrapSmokeTests
{
    [Fact]
    public void TestHarnessIsOperational()
    {
        Assert.True(true);
    }

    [Fact]
    public void PinnedTargetFrameworkIsNet8()
    {
        Assert.StartsWith("8.", Environment.Version.ToString());
    }
}
