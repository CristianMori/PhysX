// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//
// Hello World C# sample — C# port of tests/c_samples/hello_world_c/main.c
// Creates a PhysX instance, loads a USD scene, steps the simulation.

using Nvidia.OvPhysx;

Console.WriteLine($"Using ovPhysX version: {PhysX.VersionString}");

// Create PhysX instance
using var physx = new PhysX();

// Load USD scene
string usdPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data", "simple_physics_scene.usda");
if (!File.Exists(usdPath))
{
    // Try relative to script location
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
