// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx;
using Nvidia.OvPhysx.Interop;
using Xunit;

namespace Nvidia.OvPhysx.Tests;

/// <summary>Pure managed-side tests that do not touch the native library.</summary>
public class DlTensorTests
{
    /// <summary>A CPU tensor exposes the right element count, shape, and device flag.</summary>
    [Fact]
    public void Cpu_ComputesElementCount()
    {
        var t = DlTensor.Cpu(new float[12], 4, 3);
        Assert.False(t.IsGpu);
        Assert.Equal(12, t.ElementCount);
        Assert.Equal(new long[] { 4, 3 }, t.Shape);
    }

    /// <summary>A CPU tensor whose data length disagrees with its shape is rejected.</summary>
    [Fact]
    public void Cpu_RejectsShapeMismatch()
    {
        Assert.Throws<ArgumentException>(() => DlTensor.Cpu(new float[10], 4, 3));
    }

    /// <summary>A GPU tensor with a null device pointer is rejected.</summary>
    [Fact]
    public void Cuda_RequiresNonNullPointer()
    {
        Assert.Throws<ArgumentException>(() => DlTensor.Cuda(nint.Zero, 0, 4, 3));
    }

    /// <summary>A GPU tensor reports its device id, element count, and null CPU data.</summary>
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
    /// <summary>A native string arg round-trips UTF-8 content and byte length.</summary>
    [Fact]
    public void NativeStringArg_RoundTripsUtf8()
    {
        using var arg = new NativeStringArg("/World/robot_é");
        Assert.Equal("/World/robot_é", arg.Value.ToManaged());
        Assert.Equal((nuint)System.Text.Encoding.UTF8.GetByteCount("/World/robot_é"), arg.Value.length);
    }

    /// <summary>An empty native string arg marshals as a null pointer.</summary>
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
    /// <summary>Only non-null config fields produce native entries.</summary>
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

    /// <summary>An all-default config produces zero native entries.</summary>
    [Fact]
    public void EmptyConfigProducesNoEntries()
    {
        using var entries = new NativeConfigEntries(new PhysXConfig());
        Assert.Equal(0u, entries.Count);
    }

    /// <summary>A null config produces zero native entries.</summary>
    [Fact]
    public void NullConfigProducesNoEntries()
    {
        using var entries = new NativeConfigEntries(null);
        Assert.Equal(0u, entries.Count);
    }

    /// <summary>A Carbonite override that collides with a typed field is rejected.</summary>
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

    /// <summary>Distinct Carbonite overrides each produce a native entry.</summary>
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
    /// <summary>The exception message includes the context, native message, and status.</summary>
    [Fact]
    public void MessageIncludesContextAndStatus()
    {
        var ex = new OvPhysxException(ApiStatus.NotFound, "add_usd", "file missing");
        Assert.Equal(ApiStatus.NotFound, ex.Status);
        Assert.Contains("add_usd", ex.Message);
        Assert.Contains("file missing", ex.Message);
        Assert.Contains("NotFound", ex.Message);
    }

    /// <summary>The exception message falls back to the status when no native message is present.</summary>
    [Fact]
    public void FallsBackToStatusWhenNoMessage()
    {
        var ex = new OvPhysxException(ApiStatus.Error, null, null);
        Assert.Contains("status=Error", ex.Message);
    }
}
