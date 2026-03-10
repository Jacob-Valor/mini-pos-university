using System;
using System.IO;

using mini_pos.Configuration;

using Xunit;

namespace mini_pos.Tests;

public class DotEnvLoaderTests
{
    [Fact]
    public void LoadFromSearchPaths_LoadsVariablesFromParentEnvFile()
    {
        var variableName = $"MINI_POS_TEST_{Guid.NewGuid():N}";
        var quotedVariableName = $"MINI_POS_TEST_{Guid.NewGuid():N}";
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"mini-pos-env-{Guid.NewGuid():N}");
        var nestedDirectory = Path.Combine(rootDirectory, "a", "b");

        Directory.CreateDirectory(nestedDirectory);
        File.WriteAllText(
            Path.Combine(rootDirectory, ".env"),
            $"# comment{Environment.NewLine}export {variableName}=value-from-env{Environment.NewLine}{quotedVariableName}=\"quoted value\"{Environment.NewLine}INVALID_LINE{Environment.NewLine}");

        try
        {
            Environment.SetEnvironmentVariable(variableName, null);
            Environment.SetEnvironmentVariable(quotedVariableName, null);

            DotEnvLoader.LoadFromSearchPaths(Path.Combine(nestedDirectory, "test.txt"));

            Assert.Equal("value-from-env", Environment.GetEnvironmentVariable(variableName));
            Assert.Equal("quoted value", Environment.GetEnvironmentVariable(quotedVariableName));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, null);
            Environment.SetEnvironmentVariable(quotedVariableName, null);

            if (Directory.Exists(rootDirectory))
                Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [Fact]
    public void LoadFromSearchPaths_DoesNotOverwriteExistingEnvironmentVariable()
    {
        var variableName = $"MINI_POS_TEST_{Guid.NewGuid():N}";
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"mini-pos-env-{Guid.NewGuid():N}");

        Directory.CreateDirectory(rootDirectory);
        File.WriteAllText(Path.Combine(rootDirectory, ".env"), $"{variableName}=from-file{Environment.NewLine}");

        try
        {
            Environment.SetEnvironmentVariable(variableName, "already-set");

            DotEnvLoader.LoadFromSearchPaths(rootDirectory);

            Assert.Equal("already-set", Environment.GetEnvironmentVariable(variableName));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, null);

            if (Directory.Exists(rootDirectory))
                Directory.Delete(rootDirectory, recursive: true);
        }
    }
}
