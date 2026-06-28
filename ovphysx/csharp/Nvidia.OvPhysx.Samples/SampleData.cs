// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx.Samples;

/// <summary>Locates USD assets in <c>ovphysx/tests/data</c> regardless of run location.</summary>
internal static class SampleData
{
    private static readonly Lazy<string?> Dir = new(Locate);

    /// <summary>Resolves a file name to its absolute path inside the test-data directory.</summary>
    public static string Path(string fileName)
    {
        string? dir = Dir.Value
            ?? throw new DirectoryNotFoundException(
                "Could not locate ovphysx/tests/data. Run the sample from within the repository.");
        return System.IO.Path.Combine(dir, fileName);
    }

    /// <summary>Walks up from the app base directory to find the ovphysx test-data directory.</summary>
    private static string? Locate()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            foreach (string candidate in new[]
                     {
                         System.IO.Path.Combine(dir.FullName, "tests", "data"),
                         System.IO.Path.Combine(dir.FullName, "ovphysx", "tests", "data"),
                     })
            {
                if (File.Exists(System.IO.Path.Combine(candidate, "simple_physics_scene.usda")))
                    return candidate;
            }
            dir = dir.Parent;
        }
        return null;
    }
}
