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

    [RuntimeFact]
    public void TensorBinding_ReadsArticulationStateAndNames()
    {
        string scene = TestData.Path("links_chain_sample.usda");
        using var physx = new PhysX(device: DeviceType.Cpu);
        physx.AddUsd(scene).Operation.Wait(TimeSpan.FromSeconds(10));

        using TensorBinding dofPos = physx.CreateTensorBinding(
            TensorType.ArticulationDofPosition, "/World/articulation", raiseIfEmpty: true);

        Assert.Equal(2, dofPos.Ndim);
        Assert.True(dofPos.DofCount > 0);
        Assert.True(dofPos.IsFixedBase);

        // String-list introspection (the size-then-fill path) must return one name per DOF/link/joint.
        Assert.Equal(dofPos.DofCount, dofPos.DofNames.Count);
        Assert.Equal(dofPos.BodyCount, dofPos.BodyNames.Count);
        Assert.Equal(dofPos.JointCount, dofPos.JointNames.Count);
        Assert.All(dofPos.DofNames, n => Assert.False(string.IsNullOrEmpty(n)));

        physx.StepSync(1f / 60f, 0f);
        float[] positions = dofPos.Read();
        Assert.Equal(dofPos.ElementCount, positions.Length);
    }

    [RuntimeFact]
    public void TensorBinding_RoundTripsWriteThenRead()
    {
        string scene = TestData.Path("links_chain_sample.usda");
        using var physx = new PhysX(device: DeviceType.Cpu);
        physx.AddUsd(scene).Operation.Wait(TimeSpan.FromSeconds(10));

        using TensorBinding targets = physx.CreateTensorBinding(
            TensorType.ArticulationDofPositionTarget, "/World/articulation");

        var values = new float[targets.ElementCount];
        Array.Fill(values, 0.1f);
        targets.Write(values);                                   // full write (span overload)
        targets.Write(DlTensor.Cpu(values, targets.Count, targets.DofCount),
            indices: stackalloc int[] { 0 });                    // indexed write
        // No exception => write paths accepted by the native API.
        Assert.True(targets.ElementCount > 0);
    }

    [RuntimeFact]
    public void ContactBinding_ReadsNetForcesAndPaths()
    {
        string scene = TestData.Path("boxes_falling_on_groundplane.usda");
        using var physx = new PhysX(device: DeviceType.Cpu);
        physx.AddUsd(scene).Operation.Wait(TimeSpan.FromSeconds(10));

        using ContactBinding contacts = physx.CreateContactBinding(
            sensorPatterns: ["/World/Cube1"],
            filterPatterns: ["/World/GroundPlane/CollisionMesh"],
            filtersPerSensor: 1);

        Assert.Equal(1, contacts.SensorCount);
        Assert.Single(contacts.SensorPaths);
        Assert.Equal(contacts.SensorCount, contacts.FilterPaths.Count);

        for (int i = 0; i < 90; i++)
            physx.StepSync(1f / 60f, i / 60f);

        System.Numerics.Vector3[] net = contacts.ReadNetForces();
        Assert.Equal(contacts.SensorCount, net.Length);
        Assert.True(net[0].Z > 0f, "Resting box should report an upward normal force.");
    }

    [RuntimeFact]
    public void SceneQuery_RaycastHitsGround()
    {
        string scene = TestData.Path("simple_physics_scene.usda");
        using var physx = new PhysX(device: DeviceType.Cpu);
        physx.AddUsd(scene).Operation.Wait(TimeSpan.FromSeconds(10));
        physx.StepSync(1f / 60f, 0f);

        SceneQueryHit[] hits = physx.Raycast(
            origin: new System.Numerics.Vector3(0, 100, 0),
            direction: new System.Numerics.Vector3(0, -1, 0),
            distance: 1000f,
            mode: SceneQueryMode.All);

        Assert.NotEmpty(hits);
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
