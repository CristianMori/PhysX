// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using System.Runtime.InteropServices;

namespace Nvidia.OvPhysx.Interop;

/// <summary>DLPack device type codes (subset; mirrors <c>DLDeviceType</c>).</summary>
internal enum DLDeviceType : int
{
    kDLCPU = 1,
    kDLCUDA = 2,
    kDLCUDAHost = 3,
    kDLCUDAManaged = 13,
}

/// <summary>DLPack data type category codes (mirrors <c>DLDataTypeCode</c>).</summary>
internal enum DLDataTypeCode : byte
{
    kDLInt = 0,
    kDLUInt = 1,
    kDLFloat = 2,
    kDLOpaqueHandle = 3,
    kDLBfloat = 4,
    kDLComplex = 5,
    kDLBool = 6,
}

/// <summary>Mirrors the C <c>DLDevice</c> struct.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct DLDevice
{
    public DLDeviceType device_type;
    public int device_id;
}

/// <summary>Mirrors the C <c>DLDataType</c> struct.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct DLDataType
{
    public DLDataTypeCode code;
    public byte bits;
    public ushort lanes;

    /// <summary>All ovphysx tensors are currently float32 (kDLFloat, 32 bits, 1 lane).</summary>
    public static DLDataType Float32 => new() { code = DLDataTypeCode.kDLFloat, bits = 32, lanes = 1 };

    /// <summary>Signed 32-bit integer (used for partial-update index tensors).</summary>
    public static DLDataType Int32 => new() { code = DLDataTypeCode.kDLInt, bits = 32, lanes = 1 };

    /// <summary>Unsigned 8-bit (used for masked-update mask tensors).</summary>
    public static DLDataType UInt8 => new() { code = DLDataTypeCode.kDLUInt, bits = 8, lanes = 1 };
}

/// <summary>
/// Mirrors the C <c>DLTensor</c> struct — a plain, non-owning tensor view passed by
/// pointer to the ovphysx tensor/contact read/write functions.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct DLTensor
{
    public void* data;
    public DLDevice device;
    public int ndim;
    public DLDataType dtype;
    public long* shape;
    public long* strides;   // null => row-major contiguous
    public ulong byte_offset;
}
