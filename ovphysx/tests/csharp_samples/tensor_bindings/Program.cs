// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.
//
// Tensor Bindings C# sample — C# port of tests/python_samples/tensor_bindings.py
// Creates tensor bindings, writes DOF velocity targets, reads link poses.

using System.Runtime.InteropServices;
using Nvidia.OvPhysx;
using Nvidia.OvPhysx.Interop;

Console.WriteLine("Tensor Bindings C# Sample");

// Initialize
using var physx = new PhysX(new PhysXConfig { Device = DeviceType.Cpu });

// Load USD scene
string usdPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data", "links_chain_sample.usda");
if (!File.Exists(usdPath))
    usdPath = Path.GetFullPath(Path.Combine("..", "..", "data", "links_chain_sample.usda"));

Console.WriteLine($"Loading USD: {usdPath}");
physx.AddUsd(usdPath);
physx.WaitAll();
Console.WriteLine("USD loaded.");

// Create tensor binding for DOF velocity targets
Console.WriteLine("Creating DOF velocity target binding...");
using var velTargetBinding = physx.CreateTensorBinding(
    TensorType.ArticulationDofVelocityTarget,
    pattern: "/World/articulation/articulationLink*");
Console.WriteLine($"  Shape: [{string.Join(", ", velTargetBinding.Shape)}]");

// Create tensor binding for link poses
Console.WriteLine("Creating link pose binding...");
using var linkPoseBinding = physx.CreateTensorBinding(
    TensorType.ArticulationLinkPose,
    pattern: "/World/articulation/articulationLink*");
Console.WriteLine($"  Shape: [{string.Join(", ", linkPoseBinding.Shape)}]");

// Allocate float arrays for tensor data
int velElements = 1;
foreach (var s in velTargetBinding.Shape) velElements *= (int)s;
float[] velTargets = new float[velElements];

int poseElements = 1;
foreach (var s in linkPoseBinding.Shape) poseElements *= (int)s;
float[] linkPoses = new float[poseElements];

// Set alternating velocity targets
int numDofs = (int)velTargetBinding.Shape[1];
for (int i = 0; i < numDofs; i++)
    velTargets[i] = (i % 2 == 0) ? 25.0f : -25.0f;

Console.WriteLine($"Writing DOF velocity targets (alternating +/-25 rad/s, {numDofs} DOFs)...");

unsafe
{
    // Write velocity targets
    fixed (float* velPtr = velTargets)
    {
        long[] velShape = velTargetBinding.Shape;
        fixed (long* shapePtr = velShape)
        {
            var velTensor = new DLTensor
            {
                Data = (IntPtr)velPtr,
                Device = DLDevice.Cpu,
                NDim = velTargetBinding.NDim,
                DType = DLDataType.Float32,
                Shape = (IntPtr)shapePtr,
                Strides = IntPtr.Zero,
            };
            velTargetBinding.Write(ref velTensor);
        }
    }

    // Run 100 simulation steps
    Console.WriteLine("Running 100 steps...");
    float dt = 0.01f;
    for (int i = 0; i < 100; i++)
    {
        physx.StepSync(dt, i * dt);

        if (i % 25 == 0 || i == 99)
        {
            // Read link poses
            fixed (float* posePtr = linkPoses)
            {
                long[] poseShape = linkPoseBinding.Shape;
                fixed (long* shapePtr = poseShape)
                {
                    var poseTensor = new DLTensor
                    {
                        Data = (IntPtr)posePtr,
                        Device = DLDevice.Cpu,
                        NDim = linkPoseBinding.NDim,
                        DType = DLDataType.Float32,
                        Shape = (IntPtr)shapePtr,
                        Strides = IntPtr.Zero,
                    };
                    linkPoseBinding.Read(ref poseTensor);
                }
            }

            // Print first link position
            float px = linkPoses[0], py = linkPoses[1], pz = linkPoses[2];
            Console.WriteLine($"  Step {i,3}: link0 pos=({px:F4}, {py:F4}, {pz:F4})");
        }
    }
}

Console.WriteLine("[SUCCESS]");
return 0;
