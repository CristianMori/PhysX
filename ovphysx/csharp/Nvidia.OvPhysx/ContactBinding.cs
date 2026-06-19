// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using System.Numerics;
using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

/// <summary>
/// A binding that aggregates contact forces between a set of sensor bodies and (optionally)
/// per-sensor filter bodies. Create via <see cref="PhysX.CreateContactBinding"/>; dispose to release.
/// </summary>
public sealed class ContactBinding : IDisposable
{
    private readonly PhysX _physx;
    private ulong _handle;

    private string[]? _sensorPaths;
    private string[][]? _filterPaths;

    internal ContactBinding(PhysX physx, ulong handle, int sensorCount, int filterCount, uint maxContactDataCount)
    {
        _physx = physx;
        _handle = handle;
        SensorCount = sensorCount;
        FilterCount = filterCount;
        MaxContactDataCount = maxContactDataCount;
    }

    /// <summary>The raw native binding handle.</summary>
    public ulong Handle => _handle;

    /// <summary>Number of sensor bodies matched.</summary>
    public int SensorCount { get; }

    /// <summary>Number of filter bodies per sensor (0 when unfiltered).</summary>
    public int FilterCount { get; }

    /// <summary>Flat-buffer capacity for detailed contact/friction reads (0 disables them).</summary>
    public uint MaxContactDataCount { get; }

    /// <summary>Resolved sensor USD prim paths in contact-row order.</summary>
    public IReadOnlyList<string> SensorPaths => _sensorPaths ??= GetSensorPaths();

    /// <summary>
    /// Resolved filter USD prim paths as <c>[sensorIndex][filterIndex]</c>. Inner lists are empty
    /// for unfiltered bindings.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> FilterPaths => _filterPaths ??= GetFilterPaths();

    // ----- Aggregate reads -----

    /// <summary>Reads net contact forces into <paramref name="output"/> (shape <c>[SensorCount, 3]</c>).</summary>
    public unsafe void ReadNetForces(DlTensor output)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(output);
        TensorMarshal.ReadInto(_physx.Handle, _handle, output, &NativeMethods.ovphysx_read_contact_net_forces, "read_contact_net_forces");
    }

    /// <summary>Reads net contact forces and returns one force vector per sensor.</summary>
    public Vector3[] ReadNetForces()
    {
        var buffer = new float[SensorCount * 3];
        ReadNetForces(DlTensor.Cpu(buffer, SensorCount, 3));
        var result = new Vector3[SensorCount];
        for (int i = 0; i < SensorCount; i++)
            result[i] = new Vector3(buffer[i * 3], buffer[i * 3 + 1], buffer[i * 3 + 2]);
        return result;
    }

    /// <summary>Reads the contact force matrix into <paramref name="output"/> (shape <c>[SensorCount, FilterCount, 3]</c>).</summary>
    public unsafe void ReadForceMatrix(DlTensor output)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(output);
        TensorMarshal.ReadInto(_physx.Handle, _handle, output, &NativeMethods.ovphysx_read_contact_force_matrix, "read_contact_force_matrix");
    }

    /// <summary>Reads the contact force matrix and returns a <c>[SensorCount, FilterCount]</c> grid of force vectors.</summary>
    public Vector3[,] ReadForceMatrix()
    {
        var buffer = new float[SensorCount * FilterCount * 3];
        ReadForceMatrix(DlTensor.Cpu(buffer, SensorCount, FilterCount, 3));
        var result = new Vector3[SensorCount, FilterCount];
        for (int s = 0; s < SensorCount; s++)
            for (int f = 0; f < FilterCount; f++)
            {
                int b = (s * FilterCount + f) * 3;
                result[s, f] = new Vector3(buffer[b], buffer[b + 1], buffer[b + 2]);
            }
        return result;
    }

    // ----- Detailed reads (require MaxContactDataCount > 0 and FilterCount > 0) -----

    /// <summary>
    /// Reads detailed per-contact data into caller-allocated flat buffers. Shapes:
    /// <paramref name="contactForces"/> <c>[C]</c>, <paramref name="positions"/>/<paramref name="normals"/> <c>[C,3]</c>,
    /// <paramref name="separations"/> <c>[C]</c>, <paramref name="counts"/>/<paramref name="startIndices"/> <c>[S,F]</c>,
    /// where C = <see cref="MaxContactDataCount"/>, S = <see cref="SensorCount"/>, F = <see cref="FilterCount"/>.
    /// </summary>
    public unsafe void ReadContactData(
        Span<float> contactForces, Span<float> positions, Span<float> normals,
        Span<float> separations, Span<int> counts, Span<int> startIndices)
    {
        EnsureValid();
        long c = MaxContactDataCount, s = SensorCount, f = FilterCount;
        Require(contactForces.Length, c, nameof(contactForces));
        Require(positions.Length, c * 3, nameof(positions));
        Require(normals.Length, c * 3, nameof(normals));
        Require(separations.Length, c, nameof(separations));
        Require(counts.Length, s * f, nameof(counts));
        Require(startIndices.Length, s * f, nameof(startIndices));

        fixed (float* cf = contactForces)
        fixed (float* po = positions)
        fixed (float* no = normals)
        fixed (float* se = separations)
        fixed (int* co = counts)
        fixed (int* si = startIndices)
        {
            long* shC1 = stackalloc long[2] { c, 1 };
            long* shC3 = stackalloc long[2] { c, 3 };
            long* shSF = stackalloc long[2] { s, f };

            DLTensor tCf = TensorMarshal.Make(cf, DLDataType.Float32, false, 0, shC1, 2);
            DLTensor tPo = TensorMarshal.Make(po, DLDataType.Float32, false, 0, shC3, 2);
            DLTensor tNo = TensorMarshal.Make(no, DLDataType.Float32, false, 0, shC3, 2);
            DLTensor tSe = TensorMarshal.Make(se, DLDataType.Float32, false, 0, shC1, 2);
            DLTensor tCo = TensorMarshal.Make(co, DLDataType.Int32, false, 0, shSF, 2);
            DLTensor tSi = TensorMarshal.Make(si, DLDataType.Int32, false, 0, shSF, 2);

            OvPhysxException.Check(
                NativeMethods.ovphysx_read_contact_data(_physx.Handle, _handle, &tCf, &tPo, &tNo, &tSe, &tCo, &tSi),
                "read_contact_data");
        }
    }

    /// <summary>
    /// Reads detailed per-anchor friction data into caller-allocated flat buffers. Shapes:
    /// <paramref name="frictionForces"/>/<paramref name="frictionPoints"/> <c>[C,3]</c>,
    /// <paramref name="counts"/>/<paramref name="startIndices"/> <c>[S,F]</c>.
    /// </summary>
    public unsafe void ReadFrictionData(
        Span<float> frictionForces, Span<float> frictionPoints, Span<int> counts, Span<int> startIndices)
    {
        EnsureValid();
        long c = MaxContactDataCount, s = SensorCount, f = FilterCount;
        Require(frictionForces.Length, c * 3, nameof(frictionForces));
        Require(frictionPoints.Length, c * 3, nameof(frictionPoints));
        Require(counts.Length, s * f, nameof(counts));
        Require(startIndices.Length, s * f, nameof(startIndices));

        fixed (float* ff = frictionForces)
        fixed (float* fp = frictionPoints)
        fixed (int* co = counts)
        fixed (int* si = startIndices)
        {
            long* shC3 = stackalloc long[2] { c, 3 };
            long* shSF = stackalloc long[2] { s, f };

            DLTensor tFf = TensorMarshal.Make(ff, DLDataType.Float32, false, 0, shC3, 2);
            DLTensor tFp = TensorMarshal.Make(fp, DLDataType.Float32, false, 0, shC3, 2);
            DLTensor tCo = TensorMarshal.Make(co, DLDataType.Int32, false, 0, shSF, 2);
            DLTensor tSi = TensorMarshal.Make(si, DLDataType.Int32, false, 0, shSF, 2);

            OvPhysxException.Check(
                NativeMethods.ovphysx_read_friction_data(_physx.Handle, _handle, &tFf, &tFp, &tCo, &tSi),
                "read_friction_data");
        }
    }

    private static void Require(long actual, long expected, string name)
    {
        if (actual != expected)
            throw new ArgumentException($"{name} length ({actual}) must be {expected}.", name);
    }

    // ----- Introspection -----

    private unsafe string[] GetSensorPaths()
    {
        EnsureValid();
        return Introspection.GetStrings(_physx.Handle, _handle, &NativeMethods.ovphysx_contact_binding_get_sensor_paths, "contact_binding_get_sensor_paths", (uint)Math.Max(0, SensorCount));
    }

    private unsafe string[][] GetFilterPaths()
    {
        EnsureValid();
        uint hint = (uint)Math.Max(0, SensorCount) * (uint)Math.Max(0, FilterCount);
        string[] flat = Introspection.GetStrings(_physx.Handle, _handle, &NativeMethods.ovphysx_contact_binding_get_filter_paths, "contact_binding_get_filter_paths", hint);

        var result = new string[SensorCount][];
        if (FilterCount == 0 || flat.Length == 0)
        {
            for (int i = 0; i < SensorCount; i++)
                result[i] = [];
            return result;
        }

        for (int s = 0; s < SensorCount; s++)
        {
            var row = new string[FilterCount];
            for (int f = 0; f < FilterCount; f++)
            {
                int idx = s * FilterCount + f;
                row[f] = idx < flat.Length ? flat[idx] : string.Empty;
            }
            result[s] = row;
        }
        return result;
    }

    // ----- Lifecycle -----

    /// <summary>Releases the binding. Safe to call multiple times.</summary>
    public void Destroy()
    {
        if (_handle != 0 && _physx.IsValid)
            _ = NativeMethods.ovphysx_destroy_contact_binding(_physx.Handle, _handle);
        _handle = 0;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Destroy();
        GC.SuppressFinalize(this);
    }

    ~ContactBinding() => Destroy();

    private void EnsureValid()
    {
        if (_handle == 0)
            throw new ObjectDisposedException(nameof(ContactBinding), "The contact binding has been destroyed.");
    }
}
