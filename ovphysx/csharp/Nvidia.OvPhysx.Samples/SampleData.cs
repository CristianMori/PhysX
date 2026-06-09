// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx.Samples;

/// <summary>Locates USD assets in <c>ovphysx/tests/data</c> regardless of run location.</summary>
internal static class SampleData
{
    private static readonly Lazy<string?> Dir = new(Locate);

    public static string Path(string fileName)
    {
        string? dir = Dir.Value
            ?? throw new DirectoryNotFoundException(
                "Could not locate ovphysx/tests/data. Run the sample from within the repository.");
        return System.IO.Path.Combine(dir, fileName);
    }

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
