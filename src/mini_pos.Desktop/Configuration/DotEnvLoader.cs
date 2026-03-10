using System;
using System.Collections.Generic;
using System.IO;

namespace mini_pos.Configuration;

public static class DotEnvLoader
{
    public static void LoadFromSearchPaths(params string?[] startPaths)
    {
        var envFilePath = FindEnvFile(startPaths);
        if (envFilePath is null)
        {
            return;
        }

        foreach (var rawLine in File.ReadLines(envFilePath))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.Ordinal))
            {
                line = line[7..].TrimStart();
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            if (key.Length == 0)
            {
                continue;
            }

            var value = line[(separatorIndex + 1)..].Trim();
            if (value.Length >= 2)
            {
                var first = value[0];
                var last = value[^1];
                if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                {
                    value = value[1..^1];
                }
            }

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string? FindEnvFile(IEnumerable<string?> startPaths)
    {
        foreach (var startPath in startPaths)
        {
            if (string.IsNullOrWhiteSpace(startPath))
            {
                continue;
            }

            var directoryPath = Directory.Exists(startPath)
                ? startPath
                : Path.GetDirectoryName(startPath);

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                continue;
            }

            for (var directory = new DirectoryInfo(directoryPath); directory is not null; directory = directory.Parent)
            {
                var candidate = Path.Combine(directory.FullName, ".env");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }
}
