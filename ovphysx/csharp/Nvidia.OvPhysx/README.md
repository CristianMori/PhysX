<!-- Copyright (c) 2026 Cristian Mori. Licensed under the MIT License. See csharp/LICENSE for details. -->
# Nvidia.OvPhysx — C# Wrapper for NVIDIA ovPhysX

A .NET 8 wrapper for the ovPhysX C API, providing P/Invoke bindings and a high-level managed API for USD-based physics simulation with DLPack tensor interop.

## Quick Start

```csharp
using Nvidia.OvPhysx;

using var physx = new PhysX();
physx.AddUsd("scene.usda");
physx.WaitAll();

physx.StepSync(1.0f / 60.0f, 0.0f);
Console.WriteLine("Simulation step complete!");
```

## Tensor Bindings

Exchange simulation data via DLPack tensors:

```csharp
// Create binding for rigid body poses
using var binding = physx.CreateTensorBinding(
    TensorType.RigidBodyPose,
    pattern: "/World/objects/*");

// Allocate and read into a float array
float[] poses = new float[binding.Count * 7]; // [N, 7] = pos(3) + quat(4)
unsafe
{
    fixed (float* ptr = poses)
    fixed (long* shape = binding.Shape)
    {
        var tensor = new DLTensor
        {
            Data = (IntPtr)ptr,
            Device = DLDevice.Cpu,
            NDim = binding.NDim,
            DType = DLDataType.Float32,
            Shape = (IntPtr)shape,
        };
        binding.Read(ref tensor);
    }
}
```

## API Surface

| Method | Description |
|--------|-------------|
| `AddUsd` | Load a USD file (async) |
| `RemoveUsd` | Unload a USD file (async) |
| `Reset` | Clear the stage (async) |
| `Clone` | Clone a subtree for batched environments |
| `Step` / `StepSync` / `StepNSync` | Run simulation steps |
| `WaitOp` / `WaitAll` | Wait for async operations |
| `CreateTensorBinding` | Create tensor data binding by pattern or paths |
| `CreateContactBinding` | Create contact force binding |
| `Raycast` | Cast a ray against the scene |
| `GetPhysXPtr` | Access raw PhysX C++ objects |
| `WarmupGpu` | Pre-warm GPU tensor pipeline |

## Testing

```bash
cd csharp/Nvidia.OvPhysx.Tests
dotnet test
```

## Requirements

- .NET 8.0 SDK
- ovPhysX native library (`ovphysx.dll` / `libovphysx.so`)
- NVIDIA GPU (for GPU simulation mode)

## Author

Cristian Mori (cristian.mori@gmail.com)
