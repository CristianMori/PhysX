// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.OvPhysx.Interop;
using Xunit;

namespace Nvidia.OvPhysx.Tests;

public class NativeStringTests
{
    [Fact] public void NullString_ProducesEmpty() { using var c = new NativeStringContext(null); Assert.Equal(IntPtr.Zero, c.Value.Ptr); }
    [Fact] public void EmptyString_ProducesEmpty() { using var c = new NativeStringContext(""); Assert.Equal(IntPtr.Zero, c.Value.Ptr); }

    [Fact]
    public void AsciiString_RoundTrips()
    {
        using var c = new NativeStringContext("hello");
        Assert.Equal((nuint)5, c.Value.Length);
        Assert.Equal("hello", c.Value.ToManaged());
    }

    [Fact]
    public void UnicodeString_RoundTrips()
    {
        using var c = new NativeStringContext("héllo");
        Assert.Equal("héllo", c.Value.ToManaged());
    }

    [Fact]
    public void StringArray_Works()
    {
        using var c = new NativeStringArrayContext(["alpha", "beta"]);
        Assert.Equal((uint)2, c.Count);
        Assert.Equal("alpha", c.Values[0].ToManaged());
        Assert.Equal("beta", c.Values[1].ToManaged());
    }

    [Fact]
    public void EmptyArray_Works()
    {
        using var c = new NativeStringArrayContext([]);
        Assert.Equal((uint)0, c.Count);
    }

    [Fact]
    public void DisposeTwice_Safe()
    {
        var c = new NativeStringContext("test");
        c.Dispose(); c.Dispose();
    }
}
