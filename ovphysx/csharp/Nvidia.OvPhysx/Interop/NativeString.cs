// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using System.Runtime.InteropServices;
using System.Text;

namespace Nvidia.OvPhysx.Interop;

/// <summary>
/// Mirrors the C <c>ovphysx_string_t</c> — a (pointer, byte-length) pair that is
/// <b>not</b> guaranteed to be null-terminated. Always honour <see cref="length"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ovphysx_string_t
{
    public nint ptr;
    public nuint length;

    /// <summary>Decodes the UTF-8 bytes referenced by this struct into a managed string.</summary>
    public readonly unsafe string ToManaged()
    {
        if (ptr == nint.Zero || length == 0)
            return string.Empty;
        return Encoding.UTF8.GetString((byte*)ptr, checked((int)length));
    }
}

/// <summary>
/// Owns a pinned UTF-8 buffer backing an <see cref="ovphysx_string_t"/> passed into the
/// native API. Dispose after the native call returns to release the buffer.
/// </summary>
internal readonly struct NativeStringArg : IDisposable
{
    private readonly GCHandle _handle;

    /// <summary>The native struct to pass by value to the C API.</summary>
    public readonly ovphysx_string_t Value;

    public NativeStringArg(string? value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        _handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        Value = new ovphysx_string_t
        {
            ptr = bytes.Length == 0 ? nint.Zero : _handle.AddrOfPinnedObject(),
            length = (nuint)bytes.Length,
        };
    }

    public void Dispose()
    {
        if (_handle.IsAllocated)
            _handle.Free();
    }
}

/// <summary>
/// Pins a list of strings as a contiguous native <c>ovphysx_string_t[]</c> for the duration of a
/// native call. Each element's UTF-8 bytes and the array itself stay pinned until disposed.
/// </summary>
internal sealed unsafe class NativeStringArray : IDisposable
{
    private readonly NativeStringArg[] _args;
    private readonly ovphysx_string_t[] _structs;
    private GCHandle _arrayHandle;

    public ovphysx_string_t* Ptr { get; }
    public uint Count { get; }

    public NativeStringArray(IReadOnlyList<string> values)
    {
        Count = (uint)values.Count;
        _args = new NativeStringArg[values.Count];
        _structs = new ovphysx_string_t[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            _args[i] = new NativeStringArg(values[i]);
            _structs[i] = _args[i].Value;
        }

        if (_structs.Length > 0)
        {
            _arrayHandle = GCHandle.Alloc(_structs, GCHandleType.Pinned);
            Ptr = (ovphysx_string_t*)_arrayHandle.AddrOfPinnedObject();
        }
        else
        {
            Ptr = null;
        }
    }

    public void Dispose()
    {
        if (_arrayHandle.IsAllocated)
            _arrayHandle.Free();
        foreach (NativeStringArg a in _args)
            a.Dispose();
    }
}

/// <summary>
/// Allocates a writable native <c>ovphysx_string_t[]</c> buffer for two-call size-then-fill
/// reads (the library fills in pointers/lengths). Decode each element via
/// <see cref="ovphysx_string_t.ToManaged"/> after the call.
/// </summary>
internal sealed unsafe class NativeStringBuffer : IDisposable
{
    private readonly ovphysx_string_t[] _buffer;
    private GCHandle _handle;

    public ovphysx_string_t* Ptr { get; }
    public uint Capacity { get; }

    public NativeStringBuffer(uint capacity)
    {
        Capacity = capacity;
        _buffer = new ovphysx_string_t[capacity == 0 ? 1 : capacity];
        _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        Ptr = (ovphysx_string_t*)_handle.AddrOfPinnedObject();
    }

    public string[] Decode(uint count)
    {
        var result = new string[count];
        for (uint i = 0; i < count; i++)
            result[i] = _buffer[i].ToManaged();
        return result;
    }

    public void Dispose()
    {
        if (_handle.IsAllocated)
            _handle.Free();
    }
}
