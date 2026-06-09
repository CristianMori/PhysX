// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

/// <summary>
/// A binding that connects USD prims to a <see cref="TensorType"/>, enabling bulk float32
/// read/write of physics state for all matching prims. Create via
/// <see cref="PhysX.CreateTensorBinding(TensorType, string, bool)"/>; dispose to release.
/// </summary>
public sealed class TensorBinding : IDisposable
{
    private readonly PhysX _physx;
    private ulong _handle;
    private readonly long[] _shape;

    // Lazily-populated introspection caches.
    private ArticulationMetadata? _metadata;
    private string[]? _primPaths;
    private string[]? _dofNames;
    private string[]? _bodyNames;
    private string[]? _jointNames;

    internal TensorBinding(PhysX physx, ulong handle, TensorType tensorType, long[] shape)
    {
        _physx = physx;
        _handle = handle;
        TensorType = tensorType;
        _shape = shape;

        long count = 1;
        foreach (long d in shape)
            count *= d;
        ElementCount = count;
    }

    /// <summary>The raw native binding handle.</summary>
    public ulong Handle => _handle;

    /// <summary>The tensor type this binding reads/writes.</summary>
    public TensorType TensorType { get; }

    /// <summary>Number of tensor dimensions (2 or 3).</summary>
    public int Ndim => _shape.Length;

    /// <summary>The tensor shape (e.g. <c>[N, 7]</c> or <c>[N, L, 6]</c>).</summary>
    public IReadOnlyList<long> Shape => _shape;

    /// <summary>Number of bound entities (the first/batch dimension).</summary>
    public long Count => _shape.Length > 0 ? _shape[0] : 0;

    /// <summary>Total element count (product of <see cref="Shape"/>) — the required flat buffer size.</summary>
    public long ElementCount { get; }

    /// <summary>Resolved USD prim paths in tensor row order.</summary>
    public IReadOnlyList<string> PrimPaths => _primPaths ??= GetPrimPaths();

    // ----- Articulation-only introspection -----

    /// <summary>Full articulation topology metadata (articulation bindings only).</summary>
    public ArticulationMetadata Metadata => _metadata ??= FetchMetadata();

    /// <summary>Degrees of freedom (articulation bindings only).</summary>
    public int DofCount => Metadata.DofCount;

    /// <summary>Number of links (articulation bindings only).</summary>
    public int BodyCount => Metadata.BodyCount;

    /// <summary>Number of joints (articulation bindings only).</summary>
    public int JointCount => Metadata.JointCount;

    /// <summary>Max fixed tendons (0 if none).</summary>
    public int FixedTendonCount => Metadata.FixedTendonCount;

    /// <summary>Max spatial tendons (0 if none).</summary>
    public int SpatialTendonCount => Metadata.SpatialTendonCount;

    /// <summary>Whether the articulation base link is fixed in world.</summary>
    public bool IsFixedBase => Metadata.IsFixedBase;

    /// <summary>DOF names in DOF order (articulation bindings only).</summary>
    public IReadOnlyList<string> DofNames => _dofNames ??= GetDofNames();

    /// <summary>Link/body names (articulation bindings only).</summary>
    public IReadOnlyList<string> BodyNames => _bodyNames ??= GetBodyNames();

    /// <summary>Joint names (articulation bindings only).</summary>
    public IReadOnlyList<string> JointNames => _jointNames ??= GetJointNames();

    // ----- Data access -----

    /// <summary>Reads simulation state into <paramref name="tensor"/> (CPU or GPU, matching this binding's shape).</summary>
    public unsafe void Read(DlTensor tensor)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(tensor);
        ValidateElementCount(tensor.ElementCount, "read");
        TensorMarshal.ReadInto(_physx.Handle, _handle, tensor, &NativeMethods.ovphysx_read_tensor_binding, "read_tensor_binding");
    }

    /// <summary>Reads simulation state into a CPU span (length must equal <see cref="ElementCount"/>).</summary>
    public unsafe void Read(Span<float> destination)
    {
        EnsureValid();
        ValidateElementCount(destination.Length, "read");
        fixed (long* s = _shape)
        fixed (float* d = destination)
        {
            DLTensor nt = TensorMarshal.MakeFloat(d, false, 0, s, _shape.Length);
            OvPhysxException.Check(NativeMethods.ovphysx_read_tensor_binding(_physx.Handle, _handle, &nt), "read_tensor_binding");
        }
    }

    /// <summary>Allocates a flat CPU buffer, reads into it, and returns it.</summary>
    public float[] Read()
    {
        var buffer = new float[ElementCount];
        Read(buffer.AsSpan());
        return buffer;
    }

    /// <summary>Writes all bound entities from <paramref name="source"/> (CPU or GPU).</summary>
    public unsafe void Write(DlTensor source)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(source);
        ValidateElementCount(source.ElementCount, "write");
        TensorMarshal.ReadInto(_physx.Handle, _handle, source, &WriteNoIndex, "write_tensor_binding");
    }

    /// <summary>Writes all bound entities from a CPU span (length must equal <see cref="ElementCount"/>).</summary>
    public unsafe void Write(ReadOnlySpan<float> source)
    {
        EnsureValid();
        ValidateElementCount(source.Length, "write");
        fixed (long* s = _shape)
        fixed (float* d = source)
        {
            DLTensor src = TensorMarshal.MakeFloat(d, false, 0, s, _shape.Length);
            OvPhysxException.Check(NativeMethods.ovphysx_write_tensor_binding(_physx.Handle, _handle, &src, null), "write_tensor_binding");
        }
    }

    /// <summary>
    /// Writes a subset of entities selected by <paramref name="indices"/> (int32 row indices).
    /// <paramref name="source"/>'s first dimension must equal <c>indices.Length</c>.
    /// </summary>
    public unsafe void Write(DlTensor source, ReadOnlySpan<int> indices)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(source);
        fixed (long* s = source.Shape)
        fixed (float* cpu = source.CpuData)
        fixed (int* ip = indices)
        {
            void* data = source.IsGpu ? (void*)source.DevicePtr : cpu;
            DLTensor src = TensorMarshal.MakeFloat(data, source.IsGpu, source.DeviceId, s, source.Shape.Length);

            long idxLen = indices.Length;
            DLTensor idx = TensorMarshal.Make(ip, DLDataType.Int32, false, 0, &idxLen, 1);
            OvPhysxException.Check(NativeMethods.ovphysx_write_tensor_binding(_physx.Handle, _handle, &src, &idx), "write_tensor_binding");
        }
    }

    /// <summary>
    /// Writes entities selected by a boolean <paramref name="mask"/> (uint8; non-zero = write).
    /// The mask length must equal <see cref="Count"/>.
    /// </summary>
    public unsafe void WriteMasked(DlTensor source, ReadOnlySpan<byte> mask)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(source);
        if (mask.Length != Count)
            throw new ArgumentException($"mask length ({mask.Length}) must equal entity count ({Count}).", nameof(mask));

        fixed (long* s = source.Shape)
        fixed (float* cpu = source.CpuData)
        fixed (byte* mp = mask)
        {
            void* data = source.IsGpu ? (void*)source.DevicePtr : cpu;
            DLTensor src = TensorMarshal.MakeFloat(data, source.IsGpu, source.DeviceId, s, source.Shape.Length);

            long maskLen = mask.Length;
            DLTensor maskTensor = TensorMarshal.Make(mp, DLDataType.UInt8, false, 0, &maskLen, 1);
            OvPhysxException.Check(NativeMethods.ovphysx_write_tensor_binding_masked(_physx.Handle, _handle, &src, &maskTensor), "write_tensor_binding_masked");
        }
    }

    // Adapter so write (no index) matches the (handle, binding, DLTensor*) signature used by ReadInto.
    private static unsafe ovphysx_result_t WriteNoIndex(ulong handle, ulong binding, DLTensor* src)
        => NativeMethods.ovphysx_write_tensor_binding(handle, binding, src, null);

    // ----- Introspection helpers -----

    private unsafe ArticulationMetadata FetchMetadata()
    {
        EnsureValid();
        ovphysx_articulation_metadata_t m;
        OvPhysxException.Check(NativeMethods.ovphysx_get_articulation_metadata(_physx.Handle, _handle, &m), "get_articulation_metadata");
        return new ArticulationMetadata(m.dof_count, m.body_count, m.joint_count, m.fixed_tendon_count, m.spatial_tendon_count, m.is_fixed_base != 0);
    }

    private unsafe string[] GetPrimPaths()
        => GetStrings(&NativeMethods.ovphysx_tensor_binding_get_prim_paths, "tensor_binding_get_prim_paths");

    private unsafe string[] GetDofNames()
        => GetStrings(&NativeMethods.ovphysx_articulation_get_dof_names, "articulation_get_dof_names");

    private unsafe string[] GetBodyNames()
        => GetStrings(&NativeMethods.ovphysx_articulation_get_body_names, "articulation_get_body_names");

    private unsafe string[] GetJointNames()
        => GetStrings(&NativeMethods.ovphysx_articulation_get_joint_names, "articulation_get_joint_names");

    private unsafe string[] GetStrings(
        delegate*<ulong, ulong, ovphysx_string_t*, uint, uint*, ovphysx_result_t> fn, string ctx)
    {
        EnsureValid();
        return Introspection.GetStrings(_physx.Handle, _handle, fn, ctx);
    }

    private void ValidateElementCount(long provided, string op)
    {
        if (provided != ElementCount)
            throw new ArgumentException(
                $"Tensor element count for {op} ({provided}) does not match binding element count ({ElementCount}) for shape [{string.Join(", ", _shape)}].");
    }

    // ----- Lifecycle -----

    /// <summary>Releases the binding. Safe to call multiple times.</summary>
    public void Destroy()
    {
        if (_handle != 0 && _physx.IsValid)
            _ = NativeMethods.ovphysx_destroy_tensor_binding(_physx.Handle, _handle);
        _handle = 0;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Destroy();
        GC.SuppressFinalize(this);
    }

    ~TensorBinding() => Destroy();

    private void EnsureValid()
    {
        if (_handle == 0)
            throw new ObjectDisposedException(nameof(TensorBinding), "The tensor binding has been destroyed.");
    }
}
