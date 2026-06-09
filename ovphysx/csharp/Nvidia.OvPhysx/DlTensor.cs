// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx;

/// <summary>
/// A dependency-free, row-major float32 tensor view passed to/from tensor and contact bindings.
/// Backed either by a managed CPU array or by a raw CUDA device pointer (for GPU/DirectGPU
/// workflows). The view does not own GPU memory — the caller manages the device allocation's
/// lifetime. All ovphysx tensors are currently float32.
/// </summary>
public sealed class DlTensor
{
    /// <summary>Row-major shape (e.g. <c>[N, 7]</c> or <c>[N, L, 6]</c>).</summary>
    public long[] Shape { get; }

    /// <summary>True when this tensor is backed by CUDA device memory.</summary>
    public bool IsGpu { get; }

    /// <summary>CUDA device ordinal (0 for CPU tensors).</summary>
    public int DeviceId { get; }

    /// <summary>Managed CPU backing array, or <see langword="null"/> for GPU tensors.</summary>
    public float[]? CpuData { get; }

    /// <summary>Raw CUDA device pointer, or <see cref="nint.Zero"/> for CPU tensors.</summary>
    public nint DevicePtr { get; }

    /// <summary>Total element count (product of <see cref="Shape"/>).</summary>
    public long ElementCount { get; }

    private DlTensor(long[] shape, bool isGpu, int deviceId, float[]? cpuData, nint devicePtr)
    {
        Shape = shape;
        IsGpu = isGpu;
        DeviceId = deviceId;
        CpuData = cpuData;
        DevicePtr = devicePtr;

        long count = 1;
        foreach (long dim in shape)
        {
            if (dim < 0)
                throw new ArgumentException("Shape dimensions must be non-negative.", nameof(shape));
            count *= dim;
        }
        ElementCount = count;
    }

    /// <summary>Creates a CPU tensor view over a managed float array.</summary>
    public static DlTensor Cpu(float[] data, params long[] shape)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(shape);
        if (shape.Length is < 1 or > 4)
            throw new ArgumentException("Shape must have 1 to 4 dimensions.", nameof(shape));
        var t = new DlTensor(shape, isGpu: false, deviceId: 0, cpuData: data, devicePtr: nint.Zero);
        if (data.Length != t.ElementCount)
            throw new ArgumentException($"data length ({data.Length}) does not match shape product ({t.ElementCount}).", nameof(data));
        return t;
    }

    /// <summary>
    /// Creates a GPU tensor view over a raw CUDA device pointer. The caller owns the device memory
    /// and must keep it valid for the duration of any read/write call.
    /// </summary>
    public static DlTensor Cuda(nint devicePtr, int deviceId, params long[] shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        if (devicePtr == nint.Zero)
            throw new ArgumentException("Device pointer must be non-null.", nameof(devicePtr));
        if (shape.Length is < 1 or > 4)
            throw new ArgumentException("Shape must have 1 to 4 dimensions.", nameof(shape));
        if (deviceId < 0)
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        return new DlTensor(shape, isGpu: true, deviceId: deviceId, cpuData: null, devicePtr: devicePtr);
    }
}
