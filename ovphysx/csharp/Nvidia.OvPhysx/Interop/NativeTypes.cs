// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Runtime.InteropServices;

namespace Nvidia.OvPhysx.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeString
{
    public IntPtr Ptr;   // const char*
    public nuint Length;  // size_t

    public readonly string? ToManaged() =>
        Ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(Ptr, (int)Length);

    public readonly override string ToString() => ToManaged() ?? "";
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeResult
{
    public int Status;
    public readonly bool IsSuccess => Status == 0;
    public readonly bool IsError => Status != 0;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeEnqueueResult
{
    public int Status;
    public ulong OpIndex;
    public readonly bool IsSuccess => Status == 0;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeOpWaitResult
{
    public IntPtr ErrorOpIndices;   // uint64_t*
    public nuint NumErrors;
    public ulong LowestPendingOpIndex;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeTensorSpec
{
    public DLDataType DType;
    public int NDim;
    public long Shape0, Shape1, Shape2, Shape3; // shape[4]

    public readonly long[] GetShape()
    {
        var s = new long[NDim];
        if (NDim > 0) s[0] = Shape0;
        if (NDim > 1) s[1] = Shape1;
        if (NDim > 2) s[2] = Shape2;
        if (NDim > 3) s[3] = Shape3;
        return s;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeTensorBindingDesc
{
    public NativeString Pattern;
    public IntPtr PrimPaths;       // ovphysx_string_t*
    public uint PrimPathsCount;
    public int TensorType;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeArticulationMetadata
{
    public int DofCount;
    public int BodyCount;
    public int JointCount;
    public int FixedTendonCount;
    public int SpatialTendonCount;
    public byte IsFixedBase; // bool
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeCreateArgs
{
    public NativeString BundledDepsPath;
    public IntPtr ConfigEntries;    // ovphysx_config_entry_t*
    public uint ConfigEntryCount;
    public int Device;              // ovphysx_device_t
    public int GpuIndex;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeContactEventHeader
{
    public int Type;
    public long StageId;
    public ulong Actor0, Actor1;
    public ulong Collider0, Collider1;
    public uint ContactDataOffset, NumContactData;
    public uint FrictionAnchorsDataOffset, NumFrictionAnchorsData;
    public uint ProtoIndex0, ProtoIndex1;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativeContactPoint
{
    public fixed float Position[3];
    public fixed float Normal[3];
    public fixed float Impulse[3];
    public float Separation;
    public uint FaceIndex0, FaceIndex1;
    public ulong Material0, Material1;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativeSceneQueryHit
{
    public ulong Collision;
    public ulong RigidBody;
    public uint ProtoIndex;
    public fixed float Normal[3];
    public fixed float Position[3];
    public float Distance;
    public uint FaceIndex;
    public ulong Material;
}

// Config entry uses unions — use Explicit layout
[StructLayout(LayoutKind.Explicit, Size = 32)]
internal struct NativeConfigEntry
{
    [FieldOffset(0)] public int KeyType;
    // Key union at offset 4
    [FieldOffset(4)] public int IntKey;
    [FieldOffset(4)] public IntPtr StringKeyPtr;
    // Value union at offset 8 (after alignment on some platforms) — actually at 8 for the union
    // The Python ctypes shows: key_type(4) + key_union(8 for string) + value_union(16)
    // Let's use offset 16 for value to be safe with the string key
    [FieldOffset(16)] public byte BoolValue;
    [FieldOffset(16)] public int Int32Value;
    [FieldOffset(16)] public float FloatValue;
    [FieldOffset(16)] public IntPtr StringValuePtr;
    [FieldOffset(24)] public nuint StringValueLength;

    public static NativeConfigEntry Bool(int keyType, int key, bool value) => new()
    {
        KeyType = keyType,
        IntKey = key,
        BoolValue = value ? (byte)1 : (byte)0,
    };

    public static NativeConfigEntry Int32(int keyType, int key, int value) => new()
    {
        KeyType = keyType,
        IntKey = key,
        Int32Value = value,
    };
}
