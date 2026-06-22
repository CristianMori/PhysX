// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
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

    // Quiet the native USD/Carbonite diagnostic logging so sample output is readable.
    Logging.EnableDefaultOutput(false);
    Logging.Level = LogLevel.Error;
    switch (sample)
    {
        case "hello": HelloWorld(); break;
        case "clone": Clone(); break;
        case "tensors": Tensors(); break;
        case "contacts": Contacts(); break;
        case "query": Query(); break;
        case "sweep": Sweep(); break;
        case "overlap": Overlap(); break;
        case "dofcontrol": DofControl(); break;
        case "report": Report(); break;
        case "lifecycle": Lifecycle(); break;
        case "batched": Batched(); break;
        default:
            Console.Error.WriteLine(
                $"Unknown sample '{sample}'. Use: hello | clone | tensors | contacts | query | " +
                "sweep | overlap | dofcontrol | report | lifecycle | batched");
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
    Console.WriteLine("=== scene query: raycast ===");
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

void Sweep()
{
    Console.WriteLine("=== scene query: sweep (sphere) ===");
    using var physx = new PhysX(device: DeviceType.Cpu);

    physx.AddUsd(SampleData.Path("simple_physics_scene.usda")).Operation.Wait(timeout);
    physx.StepSync(1f / 60f, 0f);

    SceneQueryHit[] hits = physx.Sweep(
        SceneQueryGeometry.Sphere(radius: 0.5f, position: new Vector3(0, 100, 0)),
        direction: new Vector3(0, -1, 0),
        distance: 1000f,
        mode: SceneQueryMode.All);

    Console.WriteLine($"  Sweep hits: {hits.Length}");
    foreach (SceneQueryHit h in hits)
        Console.WriteLine($"    dist={h.Distance:F3} pos=({h.Position.X:F2}, {h.Position.Y:F2}, {h.Position.Z:F2})");
}

void Overlap()
{
    Console.WriteLine("=== scene query: overlap (box) ===");
    using var physx = new PhysX(device: DeviceType.Cpu);

    physx.AddUsd(SampleData.Path("simple_physics_scene.usda")).Operation.Wait(timeout);
    physx.StepSync(1f / 60f, 0f);

    // Overlap only reports object identity; location fields are zeroed.
    SceneQueryHit[] hits = physx.Overlap(
        SceneQueryGeometry.Box(
            halfExtent: new Vector3(100, 100, 100),
            position: Vector3.Zero,
            rotation: Quaternion.Identity),
        mode: SceneQueryMode.All);

    Console.WriteLine($"  Overlapping objects: {hits.Length}");
    foreach (SceneQueryHit h in hits)
        Console.WriteLine($"    collision={h.Collision} rigidBody={h.RigidBody}");
}

void DofControl()
{
    Console.WriteLine("=== articulation DOF control (metadata, names, indexed/masked writes) ===");
    using var physx = new PhysX(device: DeviceType.Cpu);

    physx.AddUsd(SampleData.Path("links_chain_sample.usda")).Operation.Wait(timeout);

    using var targets = physx.CreateTensorBinding(TensorType.ArticulationDofVelocityTarget, "/World/articulation");

    Console.WriteLine($"  articulations={targets.Count}, fixedBase={targets.IsFixedBase}");
    Console.WriteLine($"  metadata: dofs={targets.DofCount}, links={targets.BodyCount}, joints={targets.JointCount}");
    Console.WriteLine($"  DOF names:   {string.Join(", ", targets.DofNames)}");
    Console.WriteLine($"  body names:  {string.Join(", ", targets.BodyNames)}");
    Console.WriteLine($"  joint names: {string.Join(", ", targets.JointNames)}");

    // Full write: set every DOF velocity target to 10 rad/s.
    var values = new float[targets.ElementCount];
    Array.Fill(values, 10f);
    targets.Write(values);
    physx.StepSync(1f / 60f, 0f);
    Console.WriteLine("  wrote full DOF velocity targets.");

    // Indexed write: update only articulation row 0 (the chain holds a single articulation).
    targets.Write(DlTensor.Cpu(values, targets.Count, targets.DofCount), indices: stackalloc int[] { 0 });
    Console.WriteLine("  wrote DOF targets for indices [0].");

    // Masked write: same selection via a per-articulation mask.
    Span<byte> mask = stackalloc byte[(int)targets.Count];
    mask[0] = 1;
    targets.WriteMasked(DlTensor.Cpu(values, targets.Count, targets.DofCount), mask);
    Console.WriteLine("  wrote DOF targets via mask [1].");
}

void Report()
{
    Console.WriteLine("=== contact report ===");
    using var physx = new PhysX(device: DeviceType.Cpu);

    physx.AddUsd(SampleData.Path("boxes_falling_on_groundplane.usda")).Operation.Wait(timeout);

    for (int i = 0; i < 120; i++)
        physx.StepSync(1f / 60f, i / 60f);

    ContactReport report = physx.GetContactReport(includeFrictionAnchors: true);
    Console.WriteLine($"  contact pairs (headers): {report.Headers.Count}");
    Console.WriteLine($"  contact points:          {report.Points.Count}");
    Console.WriteLine($"  friction anchors:        {report.Anchors.Count}");

    foreach (ContactPoint p in report.Points.Take(3))
        Console.WriteLine($"    point: pos=({p.Position.X:F2}, {p.Position.Y:F2}, {p.Position.Z:F2}) sep={p.Separation:F4} impulse=({p.Impulse.X:F2}, {p.Impulse.Y:F2}, {p.Impulse.Z:F2})");
}

void Batched()
{
    // The reinforcement-learning pattern: clone one environment into many, then read/write
    // ALL of them in a single batched tensor call. This runs on CPU; for the GPU pipeline,
    // construct the instance with DeviceType.Gpu, call physx.WarmupGpu(), and pass a
    // DlTensor.Cuda(devicePtr, deviceId, shape...) to Read/Write instead of a CPU buffer.
    Console.WriteLine("=== batched environments (RL-style) ===");
    using var physx = new PhysX(device: DeviceType.Cpu);

    physx.AddUsd(SampleData.Path("basic_simulation.usda")).Operation.Wait(timeout);

    string[] envs = ["/World/envs/env1", "/World/envs/env2", "/World/envs/env3"];
    physx.Clone("/World/envs/env0", envs).Wait(timeout);
    Console.WriteLine($"  cloned env0 -> {envs.Length} more (4 environments total).");

    // One binding over every env's rigid-body table: shape [N, 7] = (px,py,pz, qx,qy,qz,qw).
    using var poses = physx.CreateTensorBinding(
        TensorType.RigidBodyPose, "/World/envs/*/table", raiseIfEmpty: true);
    Console.WriteLine($"  batched pose tensor: count={poses.Count}, shape=[{string.Join(", ", poses.Shape)}]");

    var buffer = new float[poses.ElementCount];
    for (int step = 0; step < 60; step++)
    {
        physx.StepSync(1f / 60f, step / 60f);
        if (step % 20 == 0)
        {
            poses.Read(buffer);
            int stride = (int)poses.Shape[1];
            Console.Write($"  step {step,2} | table Y per env:");
            for (int e = 0; e < poses.Count; e++)
                Console.Write($"  env{e}={buffer[e * stride + 1]:F3}");
            Console.WriteLine();
        }
    }
}

void Lifecycle()
{
    Console.WriteLine("=== lifecycle: logging callback, reset, remove USD ===");

    // Route native log messages into managed code. Real physics operations below
    // generate the traffic (the test-message emitter bypasses registered callbacks).
    int logCount = 0;
    Logging.EnableDefaultOutput(false);
    Logging.Level = LogLevel.Info;
    Logging.RegisterCallback((level, message) =>
    {
        if (logCount < 3)
            Console.WriteLine($"    [log {level}] {message.Trim()}");
        logCount++;
    });

    using (var physx = new PhysX(device: DeviceType.Cpu))
    {
        var (usd, load) = physx.AddUsd(SampleData.Path("simple_physics_scene.usda"));
        load.Wait(timeout);
        physx.StepSync(1f / 60f, 0f);
        Console.WriteLine($"  loaded USD (handle {usd.Value}), stage id {physx.GetStageId()}");

        physx.RemoveUsd(usd).Wait(timeout);
        Console.WriteLine("  removed USD layer.");

        physx.Reset().Wait(timeout);
        Console.WriteLine("  reset stage to empty.");
    }

    Logging.UnregisterCallback();
    Console.WriteLine($"  received {logCount} native log message(s) via callback.");
}
