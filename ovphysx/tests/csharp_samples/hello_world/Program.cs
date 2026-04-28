// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//
// Hello World C# sample — C# port of tests/c_samples/hello_world_c/main.c
// Creates a PhysX instance, loads a USD scene, steps the simulation.

using Nvidia.OvPhysx;

// Auto-discover the native library from the pip-installed ovphysx package,
// or set OVPHYSX_LIB_PATH environment variable to the lib/ directory.
string? libPath = Environment.GetEnvironmentVariable("OVPHYSX_LIB_PATH");
if (string.IsNullOrEmpty(libPath))
{
    // Try to find from pip install location
    string pipLib = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Python");
    if (Directory.Exists(pipLib))
    {
        foreach (var pyDir in Directory.GetDirectories(pipLib, "pythoncore-*"))
        {
            var candidate = Path.Combine(pyDir, "Lib", "site-packages", "ovphysx", "lib");
            if (File.Exists(Path.Combine(candidate, "ovphysx.dll")) ||
                File.Exists(Path.Combine(candidate, "libovphysx.so")))
            {
                libPath = candidate;
                break;
            }
        }
    }
}

if (!string.IsNullOrEmpty(libPath))
{
    Console.WriteLine($"Using native library from: {libPath}");
    OvPhysxLibrary.SetLibraryPath(libPath);
}

Console.WriteLine($"Using ovPhysX version: {PhysX.VersionString}");

// Create PhysX instance
using var physx = new PhysX();

// Load USD scene
string dataDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data"));
string usdPath = Path.Combine(dataDir, "simple_physics_scene.usda");
if (!File.Exists(usdPath))
{
    // Try relative from working directory
    usdPath = Path.GetFullPath(Path.Combine("..", "..", "data", "simple_physics_scene.usda"));
}

Console.WriteLine($"Loading USD: {usdPath}");
var (usdHandle, opIndex) = physx.AddUsd(usdPath);
physx.WaitAll();
Console.WriteLine("USD loaded.");

// Step simulation
float dt = 1.0f / 60.0f;
physx.Step(dt, 0.0f);
physx.WaitAll();
Console.WriteLine("Simulation step completed.");

Console.WriteLine("[SUCCESS]");
return 0;
