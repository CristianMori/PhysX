// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.OvPhysx.Interop;
using Xunit;

namespace Nvidia.OvPhysx.Tests;

public class EnumTests
{
    [Theory]
    [InlineData(ApiStatus.Success, 0)]
    [InlineData(ApiStatus.Error, 1)]
    [InlineData(ApiStatus.Timeout, 2)]
    [InlineData(ApiStatus.NotImplemented, 3)]
    [InlineData(ApiStatus.InvalidArgument, 4)]
    [InlineData(ApiStatus.NotFound, 5)]
    [InlineData(ApiStatus.BufferTooSmall, 6)]
    [InlineData(ApiStatus.DeviceMismatch, 7)]
    [InlineData(ApiStatus.GpuNotAvailable, 8)]
    public void ApiStatus_Values(ApiStatus v, int expected) => Assert.Equal(expected, (int)v);

    [Theory]
    [InlineData(DeviceType.Auto, 0)]
    [InlineData(DeviceType.Gpu, 1)]
    [InlineData(DeviceType.Cpu, 2)]
    public void DeviceType_Values(DeviceType v, int expected) => Assert.Equal(expected, (int)v);

    [Theory]
    [InlineData(LogLevel.Verbose, 0)]
    [InlineData(LogLevel.Info, 1)]
    [InlineData(LogLevel.Warning, 2)]
    [InlineData(LogLevel.Error, 3)]
    [InlineData(LogLevel.None, 4)]
    public void LogLevel_Values(LogLevel v, int expected) => Assert.Equal(expected, (int)v);

    [Fact]
    public void TensorType_RigidBodyPose_Is1() => Assert.Equal(1, (int)TensorType.RigidBodyPose);
    [Fact]
    public void TensorType_ArticulationRootPose_Is10() => Assert.Equal(10, (int)TensorType.ArticulationRootPose);
    [Fact]
    public void TensorType_ArticulationLinkPose_Is20() => Assert.Equal(20, (int)TensorType.ArticulationLinkPose);
    [Fact]
    public void TensorType_ArticulationDofPosition_Is30() => Assert.Equal(30, (int)TensorType.ArticulationDofPosition);
    [Fact]
    public void TensorType_RigidBodyForce_Is50() => Assert.Equal(50, (int)TensorType.RigidBodyForce);
    [Fact]
    public void TensorType_ArticulationBodyMass_Is60() => Assert.Equal(60, (int)TensorType.ArticulationBodyMass);
    [Fact]
    public void TensorType_ArticulationJacobian_Is70() => Assert.Equal(70, (int)TensorType.ArticulationJacobian);

    [Theory]
    [InlineData(SceneQueryMode.Closest, 0)]
    [InlineData(SceneQueryMode.Any, 1)]
    [InlineData(SceneQueryMode.All, 2)]
    public void SceneQueryMode_Values(SceneQueryMode v, int expected) => Assert.Equal(expected, (int)v);

    [Theory]
    [InlineData(PhysXObjectType.Scene, 1)]
    [InlineData(PhysXObjectType.Actor, 5)]
    [InlineData(PhysXObjectType.Articulation, 8)]
    [InlineData(PhysXObjectType.Physics, 31)]
    public void PhysXObjectType_Values(PhysXObjectType v, int expected) => Assert.Equal(expected, (int)v);

    [Fact] public void ConfigBoolKey_DisableContactProcessing_Is0() => Assert.Equal(0, (int)ConfigBoolKey.DisableContactProcessing);
    [Fact] public void ConfigInt32Key_NumThreads_Is0() => Assert.Equal(0, (int)ConfigInt32Key.NumThreads);
    [Fact] public void ConfigInt32Key_SceneMultiGpuMode_Is1() => Assert.Equal(1, (int)ConfigInt32Key.SceneMultiGpuMode);

    [Fact] public void DLDataTypeCode_Float_Is2() => Assert.Equal(2, (int)DLDataTypeCode.Float);
    [Fact] public void DLDeviceType_Cpu_Is1() => Assert.Equal(1, (int)DLDeviceType.Cpu);
    [Fact] public void DLDeviceType_Cuda_Is2() => Assert.Equal(2, (int)DLDeviceType.Cuda);
}
