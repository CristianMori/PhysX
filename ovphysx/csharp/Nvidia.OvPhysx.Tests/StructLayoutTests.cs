// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Runtime.InteropServices;
using Nvidia.OvPhysx.Interop;
using Xunit;

namespace Nvidia.OvPhysx.Tests;

public class StructLayoutTests
{
    [Fact] public void DLDataType_Is4Bytes() => Assert.Equal(4, Marshal.SizeOf<DLDataType>());
    [Fact] public void DLDevice_Is8Bytes() => Assert.Equal(8, Marshal.SizeOf<DLDevice>());
    [Fact] public void DLTensor_Is48Bytes() => Assert.Equal(48, Marshal.SizeOf<DLTensor>());
    [Fact] public void NativeString_Is16Bytes() => Assert.Equal(16, Marshal.SizeOf<NativeString>());
    [Fact] public void NativeResult_Is4Bytes() => Assert.Equal(4, Marshal.SizeOf<NativeResult>());
    [Fact] public void NativeOpWaitResult_Is24Bytes() => Assert.Equal(24, Marshal.SizeOf<NativeOpWaitResult>());
    [Fact] public void NativeTensorSpec_Is24Bytes() => Assert.Equal(40, Marshal.SizeOf<NativeTensorSpec>());
    [Fact] public void NativeArticulationMetadata_Is24Bytes() => Assert.Equal(24, Marshal.SizeOf<NativeArticulationMetadata>());

    [Fact]
    public void DLTensor_FieldOffsets()
    {
        Assert.Equal(0, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.Data)).ToInt32());
        Assert.Equal(8, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.Device)).ToInt32());
        Assert.Equal(16, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.NDim)).ToInt32());
        Assert.Equal(20, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.DType)).ToInt32());
        Assert.Equal(24, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.Shape)).ToInt32());
        Assert.Equal(32, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.Strides)).ToInt32());
        Assert.Equal(40, Marshal.OffsetOf<DLTensor>(nameof(DLTensor.ByteOffset)).ToInt32());
    }
}
