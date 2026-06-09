# Skill: Facade Conventions

Rules for the public API so it stays consistent and mirrors the Python `ovphysx` package.

## Naming

- Python `snake_case` → C# `PascalCase` methods/properties. `add_usd` → `AddUsd`, `read_net_forces` → `ReadNetForces`.
- Keep Python semantics: methods that enqueue work return an `Operation`; getters that hit C stay methods (`GetConfigInt32`, `GetStageId`); cached introspection is exposed as properties (`PrimPaths`, `DofNames`, `Metadata`).
- Use `System.Numerics.Vector3`/`Quaternion` for geometric data (no custom vector type, no extra dependency).

## Lifecycle

- Every native-resource owner implements `IDisposable` with an explicit `Destroy()`/`Release()` that is **safe to call repeatedly**, plus a finalizer.
- `PhysX.Release()` zeroes the handle; bindings null their handle on `Destroy()`.
- Guard every method with `EnsureValid()` → `ObjectDisposedException` once released.

## Async model

- `Step`/`AddUsd`/`RemoveUsd`/`Reset`/`Clone` return `Operation` (and `AddUsd` also a `UsdHandle`).
- `Operation.Wait(timeout?)`: `null` = infinite, `TimeSpan.Zero` = poll. Timeout → `OvPhysxTimeoutException`; op failure → `OvPhysxException`.
- Provide `*Sync` variants where the C API does (`StepSync`, `StepNSync`).
- `WaitAll()` waits on `Operation.AllIndex` (`0xFFFF...FF`).

## Errors

- Convert every non-success status via `OvPhysxException.Check(result, "context")`. The context string is the C function name (sans `ovphysx_` prefix).
- Never let a managed exception cross into native code (see the log callback's try/catch).

## Two-call (size-then-fill) lists

Use `Introspection.GetStrings(...)`: probe with capacity 0 to get the count, allocate a
`NativeStringBuffer`, fill, decode. Used for prim paths and DOF/body/joint names.

## Partial class layout

`PhysX` is split across `PhysX.cs` (core/lifecycle/config), `PhysX.Tensors.cs`,
`PhysX.Contacts.cs`, `PhysX.SceneQuery.cs`, `PhysX.Interop.cs`. Put new method groups in the
matching partial.
