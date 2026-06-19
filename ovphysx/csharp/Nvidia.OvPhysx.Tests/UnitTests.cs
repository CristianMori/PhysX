// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx;
using Nvidia.OvPhysx.Interop;
using Xunit;

namespace Nvidia.OvPhysx.Tests;

/// <summary>Pure managed-side tests that do not touch the native library.</summary>
public class DlTensorTests
{
    [Fact]
    public void Cpu_ComputesElementCount()
    {
        var t = DlTensor.Cpu(new float[12], 4, 3);
        Assert.False(t.IsGpu);
        Assert.Equal(12, t.ElementCount);
        Assert.Equal(new long[] { 4, 3 }, t.Shape);
    }

    [Fact]
    public void Cpu_RejectsShapeMismatch()
    {
        Assert.Throws<ArgumentException>(() => DlTensor.Cpu(new float[10], 4, 3));
    }

    [Fact]
    public void Cuda_RequiresNonNullPointer()
    {
        Assert.Throws<ArgumentException>(() => DlTensor.Cuda(nint.Zero, 0, 4, 3));
    }

    [Fact]
    public void Cuda_BuildsGpuView()
    {
        var t = DlTensor.Cuda(0x1000, 1, 8, 6);
        Assert.True(t.IsGpu);
        Assert.Equal(1, t.DeviceId);
        Assert.Equal(48, t.ElementCount);
        Assert.Null(t.CpuData);
    }
}

public class NativeStringTests
{
    [Fact]
    public void NativeStringArg_RoundTripsUtf8()
    {
        using var arg = new NativeStringArg("/World/robot_é");
        Assert.Equal("/World/robot_é", arg.Value.ToManaged());
        Assert.Equal((nuint)System.Text.Encoding.UTF8.GetByteCount("/World/robot_é"), arg.Value.length);
    }

    [Fact]
    public void NativeStringArg_EmptyIsNullPointer()
    {
        using var arg = new NativeStringArg("");
        Assert.Equal(nint.Zero, arg.Value.ptr);
        Assert.Equal(string.Empty, arg.Value.ToManaged());
    }
}

public class ConfigTests
{
    [Fact]
    public void BuildsOnlyNonNullEntries()
    {
        var config = new PhysXConfig
        {
            DisableContactProcessing = true,
            NumThreads = 4,
        };
        using var entries = new NativeConfigEntries(config);
        Assert.Equal(2u, entries.Count);
    }

    [Fact]
    public void EmptyConfigProducesNoEntries()
    {
        using var entries = new NativeConfigEntries(new PhysXConfig());
        Assert.Equal(0u, entries.Count);
    }

    [Fact]
    public void NullConfigProducesNoEntries()
    {
        using var entries = new NativeConfigEntries(null);
        Assert.Equal(0u, entries.Count);
    }

    [Fact]
    public void CarboniteOverrideConflictThrows()
    {
        var config = new PhysXConfig
        {
            NumThreads = 4,
            CarboniteOverrides = new Dictionary<string, object> { ["/physics/numThreads"] = 8 },
        };
        Assert.Throws<ArgumentException>(() => new NativeConfigEntries(config));
    }

    [Fact]
    public void CarboniteOverridesAreCounted()
    {
        var config = new PhysXConfig
        {
            CarboniteOverrides = new Dictionary<string, object>
            {
                ["/physics/fabricUpdateVelocities"] = true,
                ["/physics/customFloat"] = 1.5f,
            },
        };
        using var entries = new NativeConfigEntries(config);
        Assert.Equal(2u, entries.Count);
    }
}

public class ExceptionTests
{
    [Fact]
    public void MessageIncludesContextAndStatus()
    {
        var ex = new OvPhysxException(ApiStatus.NotFound, "add_usd", "file missing");
        Assert.Equal(ApiStatus.NotFound, ex.Status);
        Assert.Contains("add_usd", ex.Message);
        Assert.Contains("file missing", ex.Message);
        Assert.Contains("NotFound", ex.Message);
    }

    [Fact]
    public void FallsBackToStatusWhenNoMessage()
    {
        var ex = new OvPhysxException(ApiStatus.Error, null, null);
        Assert.Contains("status=Error", ex.Message);
    }
}
