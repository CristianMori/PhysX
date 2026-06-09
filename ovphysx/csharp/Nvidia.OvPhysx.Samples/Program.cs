// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using System.Numerics;
using Nvidia.OvPhysx;
using Nvidia.OvPhysx.Samples;

// Runnable ports of ovphysx/tests/c_samples. Requires the native library:
//   set OVPHYSX_LIB to a built ovphysx.dll / libovphysx.so, then:
//   dotnet run --project Nvidia.OvPhysx.Samples -- <sample>
// Samples: hello (default), clone, tensors, contacts, query

string sample = args.Length > 0 ? args[0].ToLowerInvariant() : "hello";
var timeout = TimeSpan.FromSeconds(10);

try
{
    Console.WriteLine($"ovphysx native version: {OvPhysxLibrary.GetVersion()}");
    switch (sample)
    {
        case "hello": HelloWorld(); break;
        case "clone": Clone(); break;
        case "tensors": Tensors(); break;
        case "contacts": Contacts(); break;
        case "query": Query(); break;
        default:
            Console.Error.WriteLine($"Unknown sample '{sample}'. Use: hello | clone | tensors | contacts | query");
            return 2;
    }
    return 0;
}
catch (DllNotFoundException)
{
    Console.Error.WriteLine("Native ovphysx library not found. Set OVPHYSX_LIB to a built ovphysx.dll / libovphysx.so.");
    return 1;
}
catch (OvPhysxException ex)
{
    Console.Error.WriteLine($"ovphysx error: {ex.Message}");
    return 1;
}

void HelloWorld()
{
    Console.WriteLine("=== hello world ===");
    using var physx = new PhysX(device: DeviceType.Cpu);

    var (usd, load) = physx.AddUsd(SampleData.Path("simple_physics_scene.usda"));
    load.Wait(timeout);
    Console.WriteLine($"  USD loaded (handle {usd.Value}).");

    physx.Step(1f / 60f, 0f).Wait(timeout);
    Console.WriteLine("  Step completed. Stage id: " + physx.GetStageId());
}

void Clone()
{
    Console.WriteLine("=== clone ===");
    using var physx = new PhysX(device: DeviceType.Cpu);

    physx.AddUsd(SampleData.Path("basic_simulation.usda")).Operation.Wait(timeout);

    string[] targets = ["/World/envs/env1", "/World/envs/env2", "/World/envs/env3"];
    physx.Clone("/World/envs/env0", targets).Wait(timeout);
    Console.WriteLine($"  Created {targets.Length} clones.");

    for (int i = 0; i < 10; i++)
        physx.Step(1f / 60f, i / 60f).Wait(timeout);
    Console.WriteLine("  10 steps completed with clones.");
}

void Tensors()
{
    Console.WriteLine("=== tensor bindings ===");
    using var physx = new PhysX(device: DeviceType.Cpu);

    physx.AddUsd(SampleData.Path("links_chain_sample.usda")).Operation.Wait(timeout);

    using var dofTargets = physx.CreateTensorBinding(TensorType.ArticulationDofVelocityTarget, "/World/articulation");
    using var linkPoses = physx.CreateTensorBinding(TensorType.ArticulationLinkPose, "/World/articulation");

    Console.WriteLine($"  DOF targets shape [{string.Join(", ", dofTargets.Shape)}], dofs={dofTargets.DofCount}");
    Console.WriteLine($"  Link poses  shape [{string.Join(", ", linkPoses.Shape)}], links={linkPoses.BodyCount}");

    var targets = new float[dofTargets.ElementCount];
    dofTargets.Write(targets); // start at rest

    var poses = new float[linkPoses.ElementCount];
    long linkCount = linkPoses.Shape.Count > 1 ? linkPoses.Shape[1] : 0;
    long components = linkPoses.Shape.Count > 2 ? linkPoses.Shape[2] : 7;
    long lastLink = linkCount > 0 ? linkCount - 1 : 0;

    for (int step = 0; step < 120; step++)
    {
        if (step % 50 == 0)
        {
            float v = (step / 50) % 2 == 0 ? 50f : -50f;
            for (int i = 0; i < targets.Length; i++)
                targets[i] = i % 2 == 0 ? v : -v;
            dofTargets.Write(targets);
        }

        physx.StepSync(1f / 60f, step / 60f);

        if (step % 30 == 0)
        {
            linkPoses.Read(poses);
            long off = lastLink * components;
            Console.WriteLine($"  step {step,3} | link {lastLink} pos=({poses[off]:F3}, {poses[off + 1]:F3}, {poses[off + 2]:F3})");
        }
    }
}

void Contacts()
{
    Console.WriteLine("=== contact binding ===");
    using var physx = new PhysX(device: DeviceType.Cpu);

    physx.AddUsd(SampleData.Path("boxes_falling_on_groundplane.usda")).Operation.Wait(timeout);

    using var contacts = physx.CreateContactBinding(
        sensorPatterns: ["/World/Cube1"],
        filterPatterns: ["/World/GroundPlane/CollisionMesh"],
        filtersPerSensor: 1,
        maxContactDataCount: 256);

    Console.WriteLine($"  Sensors: {contacts.SensorCount}  Filters/sensor: {contacts.FilterCount}");

    for (int i = 0; i < 120; i++)
        physx.StepSync(1f / 60f, i / 60f);

    Vector3[] net = contacts.ReadNetForces();
    for (int s = 0; s < net.Length; s++)
        Console.WriteLine($"  sensor {s}: net force = ({net[s].X:F3}, {net[s].Y:F3}, {net[s].Z:F3})");

    Vector3[,] matrix = contacts.ReadForceMatrix();
    for (int s = 0; s < matrix.GetLength(0); s++)
        for (int f = 0; f < matrix.GetLength(1); f++)
            Console.WriteLine($"  [{s}][{f}] = ({matrix[s, f].X:F3}, {matrix[s, f].Y:F3}, {matrix[s, f].Z:F3})");
}

void Query()
{
    Console.WriteLine("=== scene query ===");
    using var physx = new PhysX(device: DeviceType.Cpu);

    physx.AddUsd(SampleData.Path("simple_physics_scene.usda")).Operation.Wait(timeout);
    physx.StepSync(1f / 60f, 0f);

    SceneQueryHit[] hits = physx.Raycast(
        origin: new Vector3(0, 100, 0),
        direction: new Vector3(0, -1, 0),
        distance: 1000f,
        mode: SceneQueryMode.All);

    Console.WriteLine($"  Raycast hits: {hits.Length}");
    foreach (SceneQueryHit h in hits)
        Console.WriteLine($"    dist={h.Distance:F3} pos=({h.Position.X:F2}, {h.Position.Y:F2}, {h.Position.Z:F2}) normal=({h.Normal.X:F2}, {h.Normal.Y:F2}, {h.Normal.Z:F2})");
}
