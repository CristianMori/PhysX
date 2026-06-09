# Skill: Testing

Two tiers, in `Nvidia.OvPhysx.Tests`.

## Tier 1 — managed/ABI tests (always run, no native library)

- `LayoutTests` — `Marshal.SizeOf`/`OffsetOf` for every interop struct vs. the C `sizeof`/offsets. **The highest-value tests in the repo while no DLL is bundled.** Add an entry whenever you add or change an interop struct.
- `EnumTests` — public enum integer values vs. `ovphysx_types.h`.
- `UnitTests` — `DlTensor` validation, `NativeStringArg` UTF-8 round-trip, `NativeConfigEntries` counting + carbonite conflict, exception message shape.

`InternalsVisibleTo("Nvidia.OvPhysx.Tests")` (in the main csproj) lets tests see `Interop` types.

## Tier 2 — runtime tests (gated)

- Use `[RuntimeFact]` (not `[Fact]`). It auto-skips with a clear reason when the native library
  can't be loaded (`NativeAvailability` probes `OvPhysxLibrary.GetVersion()` once).
- To actually run them: set `OVPHYSX_LIB` to a built `ovphysx.dll`/`libovphysx.so` and
  `dotnet test`. Use `DeviceType.Cpu` to avoid GPU requirements.
- `TestData.Path("...")` walks up from the test bin dir to find `ovphysx/tests/data`.
- Runtime tests mirror the `c_samples`: hello-world (add_usd → step → wait), step-n-sync, config round-trip.

## Running

```bash
cd ovphysx/csharp
dotnet test                       # 40 pass, 5 skip without a DLL
$env:OVPHYSX_LIB="...ovphysx.dll"; dotnet test   # runs the 5 runtime tests too
```
