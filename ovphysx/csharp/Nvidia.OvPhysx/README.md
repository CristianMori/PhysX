# Nvidia.OvPhysx

Idiomatic **.NET 8** wrapper over the native **ovphysx** (NVIDIA Omni PhysX) C API — USD-based
physics simulation, DLPack tensor bindings, contact reporting, and scene queries — via in-process
`[LibraryImport]` P/Invoke. It mirrors the official Python `ovphysx` package surface with
idiomatic C# ergonomics (typed enums, `IDisposable`, exceptions, `System.Numerics`).

> Native version targeted: **0.4.13**.

## Installation

```bash
dotnet add package Nvidia.OvPhysx
```

The NuGet package contains **only managed code**. You must supply the native `ovphysx` library
(and its transitive dependencies, e.g. `carb.dll`) yourself — from a pip wheel's `lib/` folder or
the GitHub Releases SDK.

### Pointing at the native library

Set the `OVPHYSX_LIB` environment variable to the absolute path of the native library
(`ovphysx.dll` on Windows, `libovphysx.so` on Linux):

```powershell
$env:OVPHYSX_LIB = "C:\path\to\sdk\lib\ovphysx.dll"
```

The resolver also searches a `lib/` subfolder and the application base directory. On Windows it
adds the `lib/`, `plugins/`, and kit-SDK directories to the DLL search path so transitive
dependencies load. Discovery order matches the Python loader:

1. `OVPHYSX_LIB` (absolute path) — development override.
2. `lib/<name>` or `<name>` next to the assembly.
3. Default OS loader search path.

## Quick start (hello world)

```csharp
using Nvidia.OvPhysx;

using var physx = new PhysX(device: DeviceType.Cpu);

// Load a USD scene (asynchronous; wait for the enqueued op).
var (usd, load) = physx.AddUsd("simple_physics_scene.usda");
load.Wait();

// Step the simulation.
physx.Step(dt: 1f / 60f, simTime: 0f).Wait();

// ...or step-and-wait in one call:
physx.StepSync(1f / 60f, 0f);
```

## Tensor bindings (bulk state read/write)

```csharp
using var binding = physx.CreateTensorBinding(
    TensorType.ArticulationDofVelocityTarget, "/World/articulation");

Console.WriteLine($"shape = [{string.Join(", ", binding.Shape)}], dofs = {binding.DofCount}");

// Write velocity targets (flat row-major float buffer matching binding.Shape).
var targets = new float[binding.ElementCount];
Array.Fill(targets, 5.0f);
binding.Write(targets);

physx.StepSync(1f / 60f, 0f);

// Read link poses back.
using var poses = physx.CreateTensorBinding(TensorType.ArticulationLinkPose, "/World/articulation");
float[] data = poses.Read();
```

GPU/DirectGPU workflows use `DlTensor.Cuda(devicePtr, deviceId, shape...)`; CPU buffers use
`DlTensor.Cpu(array, shape...)` or the `Span<float>` overloads.

## Contact bindings & reports

```csharp
using var contacts = physx.CreateContactBinding(
    sensorPatterns: ["/World/Cube1"],
    filterPatterns: ["/World/GroundPlane/CollisionMesh"],
    filtersPerSensor: 1);

physx.StepSync(1f / 60f, 0f);

System.Numerics.Vector3[] net = contacts.ReadNetForces();          // [sensor]
System.Numerics.Vector3[,] matrix = contacts.ReadForceMatrix();    // [sensor, filter]

ContactReport report = physx.GetContactReport(includeFrictionAnchors: true);
```

## Scene queries

```csharp
using System.Numerics;

SceneQueryHit[] hits = physx.Raycast(
    origin: new Vector3(0, 10, 0), direction: new Vector3(0, -1, 0), distance: 100f);

SceneQueryHit[] swept = physx.Sweep(
    SceneQueryGeometry.Sphere(0.5f, new Vector3(0, 5, 0)),
    direction: new Vector3(0, -1, 0), distance: 10f);

SceneQueryHit[] overlaps = physx.Overlap(
    SceneQueryGeometry.Box(new Vector3(1, 1, 1), Vector3.Zero, Quaternion.Identity));
```

## Configuration

```csharp
using var physx = new PhysX(new PhysXConfig
{
    NumThreads = 8,
    DisableContactProcessing = false,
    CarboniteOverrides = new Dictionary<string, object>
    {
        ["/physics/fabricUpdateVelocities"] = true,
    },
}, device: DeviceType.Cpu);

physx.SetConfig(ConfigInt32.NumThreads, 4);          // process-global
int n = physx.GetConfigInt32(ConfigInt32.NumThreads);
```

## Logging

```csharp
Logging.Level = LogLevel.Warning;
Logging.EnableDefaultOutput(true);
Logging.RegisterCallback((level, message) => Console.WriteLine($"[{level}] {message}"));
```

## Remote storage (USD over S3 / Azure)

```csharp
OvPhysxLibrary.ConfigureS3("s3.amazonaws.com", "my-bucket", "us-east-1", keyId, secret);
OvPhysxLibrary.ConfigureAzureSas("account.blob.core.windows.net", "container", sasToken);
```

## Error handling

Failed native calls throw `OvPhysxException` (carrying the `ApiStatus` and the thread-local
native error message). Operation timeouts throw `OvPhysxTimeoutException`.

## Notes & constraints

- **Not thread-safe.** Use one `PhysX` instance per thread, or synchronize externally.
- **One device per process.** All instances must share the same `DeviceType` (the first
  `new PhysX(...)` locks it process-wide).
- Handles are opaque `ulong` values, not pointers (`0` is invalid).
- All tensor data is currently **float32**, row-major contiguous.
- `Operation.Wait` is only needed to observe results outside the stream; subsequent ovphysx
  calls already see prior results in submission order.

## Building from source

```bash
cd ovphysx/csharp
dotnet build
dotnet test          # 40 managed/ABI tests run; 5 runtime tests skip without OVPHYSX_LIB
```

The ABI layout tests (`LayoutTests`) validate every interop struct against the C `sizeof`/offsets
without needing the native library — they catch ABI drift at build time.

## License

BSD-3-Clause (matching upstream ovphysx).
