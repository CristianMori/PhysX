// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx;
using Xunit;

namespace Nvidia.OvPhysx.Tests;

/// <summary>
/// Runtime tests that exercise the native library. They auto-skip via <see cref="RuntimeFactAttribute"/>
/// unless a native ovphysx library is loadable (e.g. <c>OVPHYSX_LIB</c> points at a built binary).
/// </summary>
public class RuntimeTests
{
    [RuntimeFact]
    public void GetVersion_ReturnsString()
    {
        string version = OvPhysxLibrary.GetVersion();
        Assert.NotNull(version);
    }

    [RuntimeFact]
    public void CreateAndRelease_Cpu()
    {
        using var physx = new PhysX(device: DeviceType.Cpu);
        Assert.True(physx.IsValid);
        Assert.NotEqual(0ul, physx.Handle);
        physx.Release();
        Assert.False(physx.IsValid);
    }

    [RuntimeFact]
    public void HelloWorld_AddUsdStepWait()
    {
        string scene = TestData.Path("simple_physics_scene.usda");
        using var physx = new PhysX(device: DeviceType.Cpu);

        (UsdHandle usd, Operation load) = physx.AddUsd(scene);
        load.Wait(TimeSpan.FromSeconds(10));
        Assert.True(usd.IsValid);

        Operation step = physx.Step(1.0f / 60.0f, 0.0f);
        step.Wait(TimeSpan.FromSeconds(10));
    }

    [RuntimeFact]
    public void StepNSync_RunsMultipleSteps()
    {
        string scene = TestData.Path("simple_physics_scene.usda");
        using var physx = new PhysX(device: DeviceType.Cpu);

        physx.AddUsd(scene).Operation.Wait(TimeSpan.FromSeconds(10));
        physx.StepNSync(10, 1.0f / 60.0f, 0.0f);
    }

    [RuntimeFact]
    public void GlobalConfig_RoundTripsInt32()
    {
        using var physx = new PhysX(device: DeviceType.Cpu);
        physx.SetConfig(ConfigInt32.NumThreads, 4);
        Assert.Equal(4, physx.GetConfigInt32(ConfigInt32.NumThreads));
    }
}

/// <summary>Resolves paths into the <c>ovphysx/tests/data</c> directory regardless of run location.</summary>
internal static class TestData
{
    private static readonly Lazy<string?> _dir = new(Locate);

    public static string Path(string fileName)
    {
        string? dir = _dir.Value
            ?? throw new DirectoryNotFoundException("Could not locate ovphysx/tests/data. Run from within the repository.");
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
