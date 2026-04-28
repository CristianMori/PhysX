// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Runtime.InteropServices;

namespace Nvidia.OvPhysx.Interop;

/// <summary>DLPack data type code (matches kDL* constants).</summary>
public enum DLDataTypeCode : byte
{
    Int = 0, UInt = 1, Float = 2, OpaqueHandle = 3, Bfloat = 4, Complex = 5, Bool = 6,
}

/// <summary>DLPack device type (matches kDL* constants).</summary>
public enum DLDeviceType : int
{
    Cpu = 1, Cuda = 2, CudaHost = 3,
}

/// <summary>Element data type descriptor (4 bytes). Matches DLDataType from dlpack.h.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct DLDataType
{
    /// <summary>Type code (Int, UInt, Float, etc.).</summary>
    public DLDataTypeCode Code;
    /// <summary>Bit width per element (e.g., 32 for float32).</summary>
    public byte Bits;
    /// <summary>Number of vector lanes (1 for scalar).</summary>
    public ushort Lanes;

    /// <summary>32-bit floating point.</summary>
    public static readonly DLDataType Float32 = new() { Code = DLDataTypeCode.Float, Bits = 32, Lanes = 1 };
    /// <summary>32-bit signed integer.</summary>
    public static readonly DLDataType Int32 = new() { Code = DLDataTypeCode.Int, Bits = 32, Lanes = 1 };
    /// <summary>Boolean (8-bit).</summary>
    public static readonly DLDataType Bool = new() { Code = DLDataTypeCode.Bool, Bits = 8, Lanes = 1 };
}

/// <summary>Device descriptor (8 bytes). Matches DLDevice from dlpack.h.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct DLDevice
{
    /// <summary>Device type (CPU, CUDA, etc.).</summary>
    public DLDeviceType DeviceType;
    /// <summary>Device ordinal (e.g., GPU index).</summary>
    public int DeviceId;

    /// <summary>CPU device with ID 0.</summary>
    public static readonly DLDevice Cpu = new() { DeviceType = DLDeviceType.Cpu, DeviceId = 0 };
    /// <summary>CUDA device with the given ID.</summary>
    public static DLDevice Cuda(int id = 0) => new() { DeviceType = DLDeviceType.Cuda, DeviceId = id };
}

/// <summary>
/// Plain tensor descriptor (48 bytes on x64). Matches DLTensor from dlpack.h.
/// Does not own the data — the caller manages memory lifetime.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct DLTensor
{
    /// <summary>Pointer to the tensor data.</summary>
    public IntPtr Data;
    /// <summary>Device where the data resides.</summary>
    public DLDevice Device;
    /// <summary>Number of dimensions.</summary>
    public int NDim;
    /// <summary>Element data type.</summary>
    public DLDataType DType;
    /// <summary>Pointer to int64 shape array (length = NDim).</summary>
    public IntPtr Shape;
    /// <summary>Pointer to int64 strides array, or IntPtr.Zero for C-contiguous.</summary>
    public IntPtr Strides;
    /// <summary>Byte offset from Data pointer to the start of the tensor.</summary>
    public ulong ByteOffset;

    /// <summary>Read the shape dimensions as a span.</summary>
    public unsafe ReadOnlySpan<long> GetShape()
    {
        if (NDim <= 0 || Shape == IntPtr.Zero) return ReadOnlySpan<long>.Empty;
        return new ReadOnlySpan<long>((void*)Shape, NDim);
    }
}
