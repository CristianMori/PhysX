// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx;
using Xunit;

namespace Nvidia.OvPhysx.Tests;

/// <summary>Verifies public enum values match the integer constants in <c>ovphysx_types.h</c>.</summary>
public class EnumTests
{
    /// <summary>Verifies <see cref="ApiStatus"/> integer values match the C header.</summary>
    [Fact]
    public void ApiStatusValues()
    {
        Assert.Equal(0, (int)ApiStatus.Success);
        Assert.Equal(4, (int)ApiStatus.InvalidArgument);
        Assert.Equal(5, (int)ApiStatus.NotFound);
        Assert.Equal(8, (int)ApiStatus.GpuNotAvailable);
    }

    /// <summary>Verifies <see cref="DeviceType"/> integer values match the C header.</summary>
    [Fact]
    public void DeviceTypeValues()
    {
        Assert.Equal(0, (int)DeviceType.Auto);
        Assert.Equal(1, (int)DeviceType.Gpu);
        Assert.Equal(2, (int)DeviceType.Cpu);
    }

    /// <summary>Verifies <see cref="LogLevel"/> integer values match the C header.</summary>
    [Fact]
    public void LogLevelValues()
    {
        Assert.Equal(0u, (uint)LogLevel.Verbose);
        Assert.Equal(2u, (uint)LogLevel.Warning);
        Assert.Equal(4u, (uint)LogLevel.None);
    }

    /// <summary>Verifies <see cref="PhysXType"/> integer values match the C header (note the jump to 31).</summary>
    [Fact]
    public void PhysXTypeValues()
    {
        Assert.Equal(1, (int)PhysXType.Scene);
        Assert.Equal(8, (int)PhysXType.Articulation);
        Assert.Equal(31, (int)PhysXType.Physics); // non-contiguous jump in the C enum
    }

    /// <summary>Verifies representative <see cref="TensorType"/> values match the C header.</summary>
    [Fact]
    public void TensorTypeValues()
    {
        Assert.Equal(0, (int)TensorType.Invalid);
        Assert.Equal(1, (int)TensorType.RigidBodyPose);
        Assert.Equal(30, (int)TensorType.ArticulationDofPosition);
        Assert.Equal(70, (int)TensorType.ArticulationJacobian);
        Assert.Equal(112, (int)TensorType.ArticulationRestOffset);
    }

    /// <summary>Verifies scene-query mode/geometry enum values match the C header.</summary>
    [Fact]
    public void SceneQueryEnumValues()
    {
        Assert.Equal(0, (int)SceneQueryMode.Closest);
        Assert.Equal(2, (int)SceneQueryMode.All);
        Assert.Equal(0, (int)SceneQueryGeometryType.Sphere);
        Assert.Equal(2, (int)SceneQueryGeometryType.Shape);
    }

    /// <summary>Verifies typed-config key enum values match the C header.</summary>
    [Fact]
    public void ConfigKeyValues()
    {
        Assert.Equal(0, (int)ConfigBool.DisableContactProcessing);
        Assert.Equal(3, (int)ConfigBool.OmniPvdOutputEnabled);
        Assert.Equal(0, (int)ConfigInt32.NumThreads);
        Assert.Equal(1, (int)ConfigInt32.SceneMultiGpuMode);
        Assert.Equal(0, (int)ConfigString.OmniPvdOvdRecordingDirectory);
    }
}
