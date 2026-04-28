# ovphysx

[![PyPI](https://img.shields.io/pypi/v/ovphysx)](https://pypi.org/project/ovphysx/)
[![Python 3.10+](https://img.shields.io/badge/python-3.10%2B-blue)](https://pypi.org/project/ovphysx/)
[![Linux | Windows](https://img.shields.io/badge/platform-linux%20%7C%20windows-lightgrey)](https://pypi.org/project/ovphysx/)

ovphysx is a self-contained library for USD-based physics simulation, offering
a C API with Python and C# bindings. It wraps NVIDIA PhysX and Omni PhysX,
loads USD scenes, runs simulation, and exchanges data via DLPack tensors —
no Omniverse installation required.

> **Note:** Pre-release notice: ovphysx is pre-release software and not yet mature. Current limitations include a strict in-process USD requirement: ovphysx can only coexist with OpenUSD v25.11 that is non-monolithic, Python-enabled, and linked against oneTBB (in particular, ovphysx cannot be used together with usd-core in the same process today), and parts of the API are still being completed and may change before 1.0.

## Quick Start

```bash
pip install ovphysx
```

```python
from ovphysx import PhysX

physx = PhysX()
physx.add_usd("scene.usda")
physx.step(1.0 / 60.0, 0.0)
physx.release()
```

## Environment Cloning

Clone environments for batched reinforcement-learning workloads:

```python
from ovphysx import PhysX
from ovphysx.types import TensorType
import numpy as np

physx = PhysX(device="cpu")
usd_handle, _ = physx.add_usd("scene.usda")
physx.wait_all()

# Clone env0 to create 3 additional environments
physx.clone("/World/envs/env0", ["/World/envs/env1", "/World/envs/env2", "/World/envs/env3"])
physx.wait_all()

# Read rigid body poses across all environments via DLPack-compatible tensors
pose_binding = physx.create_tensor_binding(
    pattern="/World/envs/env*/table",
    tensor_type=TensorType.RIGID_BODY_POSE,
)
poses = np.zeros(pose_binding.shape, dtype=np.float32)
pose_binding.read(poses)

pose_binding.destroy()
physx.remove_usd(usd_handle)
physx.release()
```

## C/C++ SDK

A standalone C SDK is available for integration into non-Python applications.
Download pre-built packages from the [GitHub Releases](https://github.com/NVIDIA-Omniverse/PhysX/releases) page.

After extracting the SDK, you can build and run a bundled sample directly:

```bash
# /path/to/ovphysx-sdk is the extracted SDK package (pre-built binaries from GitHub Releases)
cmake -B build -S /path/to/ovphysx-sdk/samples/c_samples/hello_world_c -DCMAKE_PREFIX_PATH=/path/to/ovphysx-sdk
cmake --build build
./build/hello_world_c
```

The SDK includes ready-to-build samples in `samples/c_samples/` covering core
workflows (hello world, tensor bindings, cloning, contacts, OmniPVD recording).
Each sample has its own `CMakeLists.txt` that uses `find_package(ovphysx)`.

## C# / .NET

A .NET 8 wrapper is available for integrating ovphysx into C# applications:

```csharp
using Nvidia.OvPhysx;

using var physx = new PhysX();
physx.AddUsd("scene.usda");
physx.WaitAll();
physx.StepSync(1.0f / 60.0f, 0.0f);
```

The C# wrapper supports the full API surface: instance management, USD loading, simulation stepping, DLPack tensor bindings, contact bindings, scene queries, and articulation metadata.

See the [C# wrapper README](csharp/Nvidia.OvPhysx/README.md) for setup instructions and API reference.

### Running C# tests

```bash
cd ovphysx/csharp/Nvidia.OvPhysx.Tests
dotnet test
```

67 unit tests validate struct layouts, enum values, string marshaling, and the public API surface — all without the native library.

### C# samples

- [`tests/csharp_samples/hello_world/`](tests/csharp_samples/hello_world/) — Create instance, load USD, step simulation
- [`tests/csharp_samples/tensor_bindings/`](tests/csharp_samples/tensor_bindings/) — DLPack tensor exchange: write DOF targets, read link poses

## Documentation

- [User Guide & Tutorials](https://nvidia-omniverse.github.io/PhysX/ovphysx/latest/index.html)
- [API Reference](https://nvidia-omniverse.github.io/PhysX/ovphysx/latest/api/index.html)
- [Samples](https://nvidia-omniverse.github.io/PhysX/ovphysx/latest/samples.html)

## Requirements

- Python 3.10+
- Linux (x86_64, aarch64) or Windows (x86_64)
- NVIDIA GPU + driver recommended (CPU-only simulation also supported)

## License

The ovphysx source code is licensed under the BSD-3-Clause License — see [LICENSE.txt](LICENSE.txt).

Pre-built binary distributions (SDK packages and Python wheels) are licensed under the [NVIDIA Omniverse License](licenses/LICENSE-binary.txt).

## Links

- [PyPI](https://pypi.org/project/ovphysx/)
- [Documentation](https://nvidia-omniverse.github.io/PhysX/ovphysx/latest/index.html)
- [Issues](https://github.com/NVIDIA-Omniverse/PhysX/issues)
- [Discord](https://discord.com/invite/XWQNJDNuaC)

