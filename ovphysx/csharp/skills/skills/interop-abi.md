# Skill: Interop & ABI

How to add or change anything that crosses the native boundary.

## Where things live (`Nvidia.OvPhysx/Interop/`)

- `NativeMethods.cs` — `[LibraryImport]` declarations, 1:1 with `_bindings.py`.
- `NativeTypes.cs` — blittable struct mirrors of `ovphysx_*_t`.
- `NativeEnums.cs` — internal status/key-type enums (public enums live in `Enums.cs`).
- `NativeString.cs` — `ovphysx_string_t` + pinning helpers (`NativeStringArg`, `NativeStringArray`, `NativeStringBuffer`).
- `NativeConfig.cs` — builds the tagged-union config entry array.
- `DLPack.cs` — `DLTensor`/`DLDevice`/`DLDataType`.
- `TensorMarshal.cs`, `Introspection.cs` — shared marshalling helpers.
- `NativeLibraryResolver.cs` — `DllImportResolver` (OVPHYSX_LIB → bundled → OS search).

## Rules

1. **Match `_bindings.py` exactly.** Argument order, pointer-ness, and return type. When in doubt, open `_bindings.py` and read the `argtypes`/`restype`.
2. **Blittable only.** Structs passed by value must contain only value types. Use `byte` for C `bool` fields (not `bool`), `nint`/`nuint` for pointers/`size_t`.
3. **Bools as parameters** marshal as 4-byte BOOL by default — annotate `[MarshalAs(UnmanagedType.U1)]` for C `bool` params.
4. **Strings:** runtime strings use `ovphysx_string_t` via `NativeStringArg` (length-carrying, **not** null-terminated). Only `c_char_p` parameters (S3/Azure creds, `get_physx_ptr` prim path, scene-query shape prim path) use null-terminated UTF-8 (`StringMarshalling.Utf8` or `Marshal.StringToCoTaskMemUTF8`).
5. **Unions** use `[StructLayout(LayoutKind.Explicit)]` with `[FieldOffset]`. Config entry: key @8, value @24, size 40. Geometry desc: union @8, size 48. Verify with a `LayoutTests` entry.
6. **Out buffers the library allocates** (scene hits, contact report) are treated as borrowed views — copy out into managed records; do not free.
7. **Wait results** from `ovphysx_wait_op` must always be passed to `ovphysx_destroy_wait_result`, even on error (see `PhysX.WaitOp`).

## After any ABI change

Add/adjust a `LayoutTests` size/offset assertion and run `dotnet test`. These pass **without** the
native library and are your first line of defence against ABI drift.
