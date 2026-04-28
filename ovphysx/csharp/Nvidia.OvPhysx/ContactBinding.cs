// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

/// <summary>
/// Contact binding for reading aggregate contact forces between sensor and filter bodies.
/// Must be created before the first simulation step.
/// </summary>
public sealed class ContactBinding : IDisposable
{
    private readonly ulong _instanceHandle;
    private readonly ulong _contactHandle;
    private bool _disposed;

    internal ContactBinding(ulong instanceHandle, ulong contactHandle, int sensorCount, int filterCount)
    {
        _instanceHandle = instanceHandle;
        _contactHandle = contactHandle;
        SensorCount = sensorCount;
        FilterCount = filterCount;
    }

    public int SensorCount { get; }
    public int FilterCount { get; }

    /// <summary>Read net contact forces: shape [SensorCount, 3].</summary>
    public void ReadNetForces(ref DLTensor dst)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var result = NativeMethods.ovphysx_read_contact_net_forces(_instanceHandle, _contactHandle, ref dst);
        OvPhysxException.ThrowIfFailed(result, "read contact net forces");
    }

    /// <summary>Read contact force matrix: shape [SensorCount, FilterCount, 3].</summary>
    public void ReadForceMatrix(ref DLTensor dst)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var result = NativeMethods.ovphysx_read_contact_force_matrix(_instanceHandle, _contactHandle, ref dst);
        OvPhysxException.ThrowIfFailed(result, "read contact force matrix");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMethods.ovphysx_destroy_contact_binding(_instanceHandle, _contactHandle);
    }
}
