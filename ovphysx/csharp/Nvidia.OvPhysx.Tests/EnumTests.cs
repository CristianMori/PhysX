// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx;
using Xunit;

namespace Nvidia.OvPhysx.Tests;

/// <summary>Verifies public enum values match the integer constants in <c>ovphysx_types.h</c>.</summary>
public class EnumTests
{
    [Fact]
    public void ApiStatusValues()
    {
        Assert.Equal(0, (int)ApiStatus.Success);
        Assert.Equal(4, (int)ApiStatus.InvalidArgument);
        Assert.Equal(5, (int)ApiStatus.NotFound);
        Assert.Equal(8, (int)ApiStatus.GpuNotAvailable);
    }

    [Fact]
    public void DeviceTypeValues()
    {
        Assert.Equal(0, (int)DeviceType.Auto);
        Assert.Equal(1, (int)DeviceType.Gpu);
        Assert.Equal(2, (int)DeviceType.Cpu);
    }

    [Fact]
    public void LogLevelValues()
    {
        Assert.Equal(0u, (uint)LogLevel.Verbose);
        Assert.Equal(2u, (uint)LogLevel.Warning);
        Assert.Equal(4u, (uint)LogLevel.None);
    }

    [Fact]
    public void PhysXTypeValues()
    {
        Assert.Equal(1, (int)PhysXType.Scene);
        Assert.Equal(8, (int)PhysXType.Articulation);
        Assert.Equal(31, (int)PhysXType.Physics); // non-contiguous jump in the C enum
    }

    [Fact]
    public void TensorTypeValues()
    {
        Assert.Equal(0, (int)TensorType.Invalid);
        Assert.Equal(1, (int)TensorType.RigidBodyPose);
        Assert.Equal(30, (int)TensorType.ArticulationDofPosition);
        Assert.Equal(70, (int)TensorType.ArticulationJacobian);
        Assert.Equal(112, (int)TensorType.ArticulationRestOffset);
    }

    [Fact]
    public void SceneQueryEnumValues()
    {
        Assert.Equal(0, (int)SceneQueryMode.Closest);
        Assert.Equal(2, (int)SceneQueryMode.All);
        Assert.Equal(0, (int)SceneQueryGeometryType.Sphere);
        Assert.Equal(2, (int)SceneQueryGeometryType.Shape);
    }

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
