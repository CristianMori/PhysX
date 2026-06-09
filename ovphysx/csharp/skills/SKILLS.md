# Nvidia.OvPhysx — Agent Skills Index

Guidance for contributors working in the `Nvidia.OvPhysx` C# wrapper (.NET 8 P/Invoke over the
native ovphysx C API). Read the relevant skill before editing.

| Skill | When to use |
|-------|-------------|
| [interop-abi](skills/interop-abi.md) | Adding/changing P/Invoke signatures, structs, or enums; anything touching the C ABI. |
| [facade-conventions](skills/facade-conventions.md) | Adding or changing public API (PhysX, bindings, queries); naming and lifecycle rules. |
| [tensors-dlpack](skills/tensors-dlpack.md) | Working with `DlTensor`, tensor/contact reads/writes, DLPack marshalling. |
| [testing](skills/testing.md) | Adding tests; ABI layout checks vs. DLL-gated runtime tests. |

## Authoritative ABI sources (read these, not memory)

- `ovphysx/python/ovphysx/_bindings.py` — near-1:1 ctypes mirror of every exported function. **The source of truth** for signatures.
- `ovphysx/include/ovphysx/ovphysx_types.h` — enum values, struct layouts, handle model.
- `ovphysx/include/ovphysx/dlpack/dlpack.h` — DLPack structs.
- `ovphysx/tests/c_samples/*/main.c` — usage parity targets (hello_world, clone, tensor_bindings, contact_binding).

## Hard ABI facts (do not regress)

- P/Invoke library id is `ovphysx` (not `ovphysx-dynamic`). Dev override env: `OVPHYSX_LIB`.
- Handles are `uint64` **values**, not pointers; `0` is invalid. No `SafeHandle`.
- `ovphysx_string_t = { const char* ptr; size_t length }` — **not null-terminated**. Always honour `length`.
- Sync calls return `ovphysx_result_t`; async return `ovphysx_enqueue_result_t { status, op_index }`.
- Wait results from `ovphysx_wait_op` **must** be freed with `ovphysx_destroy_wait_result`.
- Tensor read/write pass `DLTensor*` directly; all tensors are float32 row-major.
- One device mode per process; all instances must agree.
