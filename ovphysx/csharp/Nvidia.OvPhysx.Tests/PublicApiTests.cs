// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Xunit;

namespace Nvidia.OvPhysx.Tests;

public class PublicApiTests
{
    [Fact] public void PhysX_ImplementsIDisposable() => Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(PhysX)));
    [Fact] public void PhysX_IsSealed() => Assert.True(typeof(PhysX).IsSealed);
    [Fact] public void TensorBinding_ImplementsIDisposable() => Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(TensorBinding)));
    [Fact] public void ContactBinding_ImplementsIDisposable() => Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(ContactBinding)));

    [Fact]
    public void PhysX_HasExpectedMethods()
    {
        var t = typeof(PhysX);
        Assert.NotNull(t.GetMethod("AddUsd"));
        Assert.NotNull(t.GetMethod("RemoveUsd"));
        Assert.NotNull(t.GetMethod("Reset"));
        Assert.NotNull(t.GetMethod("Clone"));
        Assert.NotNull(t.GetMethod("Step"));
        Assert.NotNull(t.GetMethod("StepSync"));
        Assert.NotNull(t.GetMethod("StepNSync"));
        Assert.NotNull(t.GetMethod("WaitOp"));
        Assert.NotNull(t.GetMethod("WaitAll"));
        Assert.NotNull(t.GetMethod("CreateTensorBinding"));
        Assert.NotNull(t.GetMethod("CreateContactBinding"));
        Assert.NotNull(t.GetMethod("WarmupGpu"));
        Assert.NotNull(t.GetMethod("Raycast"));
        Assert.NotNull(t.GetMethod("GetPhysXPtr"));
        Assert.NotNull(t.GetMethod("GetStageId"));
        Assert.NotNull(t.GetMethod("SetConfigBool"));
        Assert.NotNull(t.GetMethod("SetConfigInt32"));
        Assert.NotNull(t.GetMethod("GetConfigBool"));
        Assert.NotNull(t.GetMethod("GetConfigInt32"));
        Assert.NotNull(t.GetMethod("SetLogLevel"));
        Assert.NotNull(t.GetMethod("GetLogLevel"));
        Assert.NotNull(t.GetMethod("GetLastError"));
    }

    [Fact]
    public void PhysX_HasVersionProperty()
    {
        Assert.NotNull(typeof(PhysX).GetProperty("Version"));
        Assert.NotNull(typeof(PhysX).GetProperty("VersionString"));
    }

    [Fact]
    public void TensorBinding_HasExpectedMembers()
    {
        var t = typeof(TensorBinding);
        Assert.NotNull(t.GetMethod("Read"));
        Assert.NotNull(t.GetMethod("Write"));
        Assert.NotNull(t.GetMethod("WriteMasked"));
        Assert.NotNull(t.GetMethod("GetArticulationMetadata"));
        Assert.NotNull(t.GetProperty("Shape"));
        Assert.NotNull(t.GetProperty("NDim"));
        Assert.NotNull(t.GetProperty("Count"));
    }

    [Fact]
    public void ContactBinding_HasExpectedMembers()
    {
        var t = typeof(ContactBinding);
        Assert.NotNull(t.GetMethod("ReadNetForces"));
        Assert.NotNull(t.GetMethod("ReadForceMatrix"));
        Assert.NotNull(t.GetProperty("SensorCount"));
        Assert.NotNull(t.GetProperty("FilterCount"));
    }

    [Fact]
    public void PhysXConfig_HasExpectedProperties()
    {
        var t = typeof(PhysXConfig);
        Assert.NotNull(t.GetProperty("Device"));
        Assert.NotNull(t.GetProperty("GpuIndex"));
        Assert.NotNull(t.GetProperty("DisableContactProcessing"));
        Assert.NotNull(t.GetProperty("NumThreads"));
        Assert.NotNull(t.GetProperty("SceneMultiGpuMode"));
    }

    [Fact]
    public void NativeMethods_IsInternal()
    {
        var t = typeof(PhysX).Assembly.GetType("Nvidia.OvPhysx.Interop.NativeMethods");
        Assert.NotNull(t);
        Assert.False(t!.IsPublic);
    }

    [Fact]
    public void OvPhysxException_IsException()
    {
        var ex = new OvPhysxException("test");
        Assert.IsAssignableFrom<Exception>(ex);
        Assert.Equal(ApiStatus.Error, ex.Status);
    }

    [Fact]
    public void OvPhysxException_CustomStatus()
    {
        var ex = new OvPhysxException("timeout", ApiStatus.Timeout);
        Assert.Equal(ApiStatus.Timeout, ex.Status);
    }

    [Fact]
    public void SceneQueryHit_IsRecord()
    {
        var hit = new SceneQueryHit(1, 2, 3, 1.5f, 4, 5);
        Assert.Equal(1UL, hit.Collision);
        Assert.Equal(1.5f, hit.Distance);
    }

    [Fact]
    public void ArticulationMetadata_IsRecord()
    {
        var meta = new ArticulationMetadata(6, 7, 6, 0, 0, true);
        Assert.Equal(6, meta.DofCount);
        Assert.True(meta.IsFixedBase);
    }
}
