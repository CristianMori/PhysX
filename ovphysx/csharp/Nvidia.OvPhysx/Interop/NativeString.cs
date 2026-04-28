// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Runtime.InteropServices;
using System.Text;

namespace Nvidia.OvPhysx.Interop;

internal sealed class NativeStringContext : IDisposable
{
    private GCHandle _handle;
    public NativeString Value;

    public NativeStringContext(string? managed)
    {
        if (string.IsNullOrEmpty(managed)) { Value = default; return; }
        byte[] utf8 = Encoding.UTF8.GetBytes(managed);
        _handle = GCHandle.Alloc(utf8, GCHandleType.Pinned);
        Value = new NativeString { Ptr = _handle.AddrOfPinnedObject(), Length = (nuint)utf8.Length };
    }

    public void Dispose() { if (_handle.IsAllocated) _handle.Free(); }
}

internal sealed class NativeStringArrayContext : IDisposable
{
    private readonly NativeStringContext[] _contexts;
    private GCHandle _arrayHandle;
    public NativeString[] Values { get; }
    public IntPtr Pointer => _arrayHandle.IsAllocated ? _arrayHandle.AddrOfPinnedObject() : IntPtr.Zero;
    public uint Count => (uint)Values.Length;

    public NativeStringArrayContext(string[] managed)
    {
        _contexts = new NativeStringContext[managed.Length];
        Values = new NativeString[managed.Length];
        for (int i = 0; i < managed.Length; i++)
        {
            _contexts[i] = new NativeStringContext(managed[i]);
            Values[i] = _contexts[i].Value;
        }
        if (Values.Length > 0)
            _arrayHandle = GCHandle.Alloc(Values, GCHandleType.Pinned);
    }

    public void Dispose()
    {
        if (_arrayHandle.IsAllocated) _arrayHandle.Free();
        foreach (var ctx in _contexts) ctx.Dispose();
    }
}
