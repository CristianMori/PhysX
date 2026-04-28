// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

/// <summary>
/// A tensor binding for reading/writing simulation data via DLPack tensors.
/// </summary>
public sealed class TensorBinding : IDisposable
{
    private readonly ulong _instanceHandle;
    private readonly ulong _bindingHandle;
    private bool _disposed;

    internal TensorBinding(ulong instanceHandle, ulong bindingHandle, NativeTensorSpec spec)
    {
        _instanceHandle = instanceHandle;
        _bindingHandle = bindingHandle;
        Shape = spec.GetShape();
        NDim = spec.NDim;
        Count = Shape.Length > 0 ? (int)Shape[0] : 0;
    }

    public ulong Handle => _bindingHandle;
    public long[] Shape { get; }
    public int NDim { get; }
    public int Count { get; }

    /// <summary>Read simulation data into the provided DLTensor (must match spec shape).</summary>
    public void Read(ref DLTensor dst)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var result = NativeMethods.ovphysx_read_tensor_binding(_instanceHandle, _bindingHandle, ref dst);
        OvPhysxException.ThrowIfFailed(result, "read tensor binding");
    }

    /// <summary>Write data from the provided DLTensor into the simulation.</summary>
    public void Write(ref DLTensor src, IntPtr indexTensor = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var result = NativeMethods.ovphysx_write_tensor_binding(_instanceHandle, _bindingHandle, ref src, indexTensor);
        OvPhysxException.ThrowIfFailed(result, "write tensor binding");
    }

    /// <summary>Write with a binary mask tensor.</summary>
    public void WriteMasked(ref DLTensor src, ref DLTensor mask)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var result = NativeMethods.ovphysx_write_tensor_binding_masked(_instanceHandle, _bindingHandle, ref src, ref mask);
        OvPhysxException.ThrowIfFailed(result, "write tensor binding masked");
    }

    /// <summary>Get articulation metadata for this binding.</summary>
    public ArticulationMetadata GetArticulationMetadata()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var result = NativeMethods.ovphysx_get_articulation_metadata(_instanceHandle, _bindingHandle, out var meta);
        OvPhysxException.ThrowIfFailed(result, "get articulation metadata");
        return new ArticulationMetadata(meta.DofCount, meta.BodyCount, meta.JointCount,
            meta.FixedTendonCount, meta.SpatialTendonCount, meta.IsFixedBase != 0);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        NativeMethods.ovphysx_destroy_tensor_binding(_instanceHandle, _bindingHandle);
    }
}

public record ArticulationMetadata(
    int DofCount, int BodyCount, int JointCount,
    int FixedTendonCount, int SpatialTendonCount, bool IsFixedBase);
