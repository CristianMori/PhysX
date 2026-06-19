// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx.Interop;

/// <summary>Builds non-owning native <see cref="DLTensor"/> views over already-pinned memory.</summary>
internal static unsafe class TensorMarshal
{
    public static DLTensor Make(void* data, DLDataType dtype, bool gpu, int deviceId, long* shape, int ndim) => new()
    {
        data = data,
        device = new DLDevice
        {
            device_type = gpu ? DLDeviceType.kDLCUDA : DLDeviceType.kDLCPU,
            device_id = deviceId,
        },
        ndim = ndim,
        dtype = dtype,
        shape = shape,
        strides = null, // row-major contiguous
        byte_offset = 0,
    };

    public static DLTensor MakeFloat(void* data, bool gpu, int deviceId, long* shape, int ndim)
        => Make(data, DLDataType.Float32, gpu, deviceId, shape, ndim);

    /// <summary>
    /// Pins a <see cref="DlTensor"/> (CPU or GPU) and invokes a native function with signature
    /// <c>(handle, binding, DLTensor*) -&gt; result</c>, checking the result. Used by tensor reads
    /// and writes, and contact net-force/matrix reads.
    /// </summary>
    public static void ReadInto(
        ulong handle, ulong binding, DlTensor t,
        delegate*<ulong, ulong, DLTensor*, ovphysx_result_t> fn, string ctx)
    {
        fixed (long* s = t.Shape)
        fixed (float* cpu = t.CpuData) // null pointer for GPU tensors
        {
            void* data = t.IsGpu ? (void*)t.DevicePtr : cpu;
            DLTensor nt = MakeFloat(data, t.IsGpu, t.DeviceId, s, t.Shape.Length);
            OvPhysxException.Check(fn(handle, binding, &nt), ctx);
        }
    }
}
