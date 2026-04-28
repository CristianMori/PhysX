<!-- Copyright (c) 2026 Cristian Mori. Licensed under the MIT License. -->
# Tensor Bindings C# Sample

Demonstrates simulation data exchange via DLPack tensor bindings:

1. Loads a USD scene with an articulated chain
2. Creates tensor bindings for DOF velocity targets (control inputs) and link poses (observations)
3. Writes alternating velocity targets to drive the joints
4. Runs 100 simulation steps, reading link poses periodically

This is the C# port of `tests/python_samples/tensor_bindings.py`.

## Build and Run

```bash
cd ovphysx/tests/csharp_samples/tensor_bindings
dotnet run
```

## Requirements

- .NET 8 SDK
- ovphysx native library in the system library path
