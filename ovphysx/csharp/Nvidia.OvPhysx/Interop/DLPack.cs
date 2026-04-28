// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Runtime.InteropServices;

namespace Nvidia.OvPhysx.Interop;

public enum DLDataTypeCode : byte
{
    Int = 0, UInt = 1, Float = 2, OpaqueHandle = 3, Bfloat = 4, Complex = 5, Bool = 6,
}

public enum DLDeviceType : int
{
    Cpu = 1, Cuda = 2, CudaHost = 3,
}

[StructLayout(LayoutKind.Sequential)]
public struct DLDataType
{
    public DLDataTypeCode Code;
    public byte Bits;
    public ushort Lanes;

    public static readonly DLDataType Float32 = new() { Code = DLDataTypeCode.Float, Bits = 32, Lanes = 1 };
    public static readonly DLDataType Int32 = new() { Code = DLDataTypeCode.Int, Bits = 32, Lanes = 1 };
    public static readonly DLDataType Bool = new() { Code = DLDataTypeCode.Bool, Bits = 8, Lanes = 1 };
}

[StructLayout(LayoutKind.Sequential)]
public struct DLDevice
{
    public DLDeviceType DeviceType;
    public int DeviceId;

    public static readonly DLDevice Cpu = new() { DeviceType = DLDeviceType.Cpu, DeviceId = 0 };
    public static DLDevice Cuda(int id = 0) => new() { DeviceType = DLDeviceType.Cuda, DeviceId = id };
}

[StructLayout(LayoutKind.Sequential)]
public struct DLTensor
{
    public IntPtr Data;
    public DLDevice Device;
    public int NDim;
    public DLDataType DType;
    public IntPtr Shape;    // int64_t*
    public IntPtr Strides;  // int64_t* (null = C-contiguous)
    public ulong ByteOffset;

    public unsafe ReadOnlySpan<long> GetShape()
    {
        if (NDim <= 0 || Shape == IntPtr.Zero) return ReadOnlySpan<long>.Empty;
        return new ReadOnlySpan<long>((void*)Shape, NDim);
    }
}
