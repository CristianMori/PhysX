# Skill: Tensors & DLPack

How bulk physics data crosses the boundary.

## Public model: `DlTensor`

Dependency-free, float32, row-major. Two backings:

- `DlTensor.Cpu(float[] data, params long[] shape)` — managed array; length must equal the shape product.
- `DlTensor.Cuda(nint devicePtr, int deviceId, params long[] shape)` — raw CUDA device pointer (caller owns the memory; keep it valid across the call).

`TensorBinding` also offers `Span<float>`/`float[]` convenience overloads that build the CPU view internally from `binding.Shape`.

## Marshalling (`Interop/TensorMarshal.cs`)

- `Make(data, dtype, gpu, deviceId, shape*, ndim)` builds a non-owning `DLTensor`.
- `ReadInto(handle, binding, DlTensor, fn, ctx)` pins a `DlTensor` (CPU or GPU) and invokes any `(handle, binding, DLTensor*) -> result` native function. Reused by tensor read/write-no-index and contact net-force/matrix reads.
- The pinning trick: `fixed (float* cpu = t.CpuData)` yields `null` for GPU tensors, so one block handles both — `void* data = t.IsGpu ? (void*)t.DevicePtr : cpu`.

## dtypes

- State tensors: **float32** (`DLDataType.Float32`).
- Partial-update **indices**: int32 (`DLDataType.Int32`).
- Masked-update **mask**: uint8 (`DLDataType.UInt8`), non-zero = write.
- Contact/friction **counts** & **start_indices**: int32 (the C API accepts int32 or uint32).

## Shapes

Get a binding's shape from `ovphysx_get_tensor_binding_spec` (done once at create time, stored in
`TensorBinding`). Contact detailed-read shapes are derived from `MaxContactDataCount` (C),
`SensorCount` (S), `FilterCount` (F): forces `[C,1]`, positions/normals `[C,3]`, counts/start `[S,F]`.

## Gotchas

- Validate element counts before the call (`ValidateElementCount`) — a too-small buffer is a native crash, not an exception.
- Writes use `ovphysx_write_tensor_binding(src, indexOrNull)`; masked writes use the separate `_masked` entry point.
- Don't store `Span<T>` in fields; only pin within the synchronous call scope.
