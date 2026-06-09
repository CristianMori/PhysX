// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx;

/// <summary>Operation status codes returned by the native API (mirrors <c>ovphysx_api_status_t</c>).</summary>
public enum ApiStatus
{
    Success = 0,
    Error = 1,
    Timeout = 2,
    NotImplemented = 3,
    InvalidArgument = 4,
    NotFound = 5,
    BufferTooSmall = 6,
    DeviceMismatch = 7,
    GpuNotAvailable = 8,
}

/// <summary>Simulation device / backend selection (mirrors <c>ovphysx_device_t</c>).</summary>
public enum DeviceType
{
    /// <summary>GPU preferred; falls back to CPU when CUDA is unavailable. Zero-init default.</summary>
    Auto = 0,
    /// <summary>GPU required; instance creation fails with <see cref="ApiStatus.GpuNotAvailable"/> if CUDA is unavailable.</summary>
    Gpu = 1,
    /// <summary>CPU-only simulation; never touches CUDA.</summary>
    Cpu = 2,
}

/// <summary>Global log verbosity threshold (mirrors <c>ovphysx_log_level_t</c>).</summary>
public enum LogLevel : uint
{
    Verbose = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    None = 4,
}

/// <summary>Scene-query hit selection mode (mirrors <c>ovphysx_scene_query_mode_t</c>).</summary>
public enum SceneQueryMode
{
    Closest = 0,
    Any = 1,
    All = 2,
}

/// <summary>Geometry kind for sweep/overlap queries (mirrors <c>ovphysx_scene_query_geometry_type_t</c>).</summary>
public enum SceneQueryGeometryType
{
    Sphere = 0,
    Box = 1,
    Shape = 2,
}

/// <summary>PhysX SDK object type for <see cref="PhysX.GetPhysxPtr"/> (mirrors <c>ovphysx_physx_type_t</c>).</summary>
public enum PhysXType
{
    Scene = 1,
    Material = 2,
    Shape = 3,
    CompoundShape = 4,
    Actor = 5,
    Joint = 6,
    CustomJoint = 7,
    Articulation = 8,
    Link = 9,
    LinkJoint = 10,
    ParticleSystem = 11,
    ParticleSet = 12,
    Physics = 31,
}

/// <summary>Boolean typed-config keys (mirrors <c>ovphysx_config_bool_t</c>).</summary>
public enum ConfigBool
{
    DisableContactProcessing = 0,
    CollisionConeCustomGeometry = 1,
    CollisionCylinderCustomGeometry = 2,
    OmniPvdOutputEnabled = 3,
}

/// <summary>Int32 typed-config keys (mirrors <c>ovphysx_config_int32_t</c>).</summary>
public enum ConfigInt32
{
    NumThreads = 0,
    /// <summary>0=disabled, 1=all GPUs, 2=skip-first.</summary>
    SceneMultiGpuMode = 1,
}

/// <summary>String typed-config keys (mirrors <c>ovphysx_config_string_t</c>).</summary>
public enum ConfigString
{
    OmniPvdOvdRecordingDirectory = 0,
}

/// <summary>
/// Tensor type identifiers for bulk read/write via tensor bindings
/// (mirrors <c>ovphysx_tensor_type_t</c>). All tensors are float32.
/// Shapes are documented per member; N = matched prims, L = max links,
/// D = max DOFs, T = max tendons, S = max shapes per body/link.
/// </summary>
public enum TensorType
{
    Invalid = 0,

    // Rigid body
    RigidBodyPose = 1,            // [N, 7] (px,py,pz,qx,qy,qz,qw), world
    RigidBodyVelocity = 2,        // [N, 6] (lin xyz, ang xyz), world
    RigidBodyMass = 3,            // [N]
    RigidBodyInertia = 4,         // [N, 9] row-major 3x3
    RigidBodyComPose = 5,         // [N, 7] COM local pose
    RigidBodyAcceleration = 6,    // [N, 6] world (read-only)
    RigidBodyInvMass = 7,         // [N] (read-only)
    RigidBodyInvInertia = 8,      // [N, 9] (read-only)

    // Articulation root
    ArticulationRootPose = 10,    // [N, 7]
    ArticulationRootVelocity = 11,// [N, 6]

    // Articulation links (3D)
    ArticulationLinkPose = 20,        // [N, L, 7]
    ArticulationLinkVelocity = 21,    // [N, L, 6]
    ArticulationLinkAcceleration = 22,// [N, L, 6] (read-only)

    // Articulation DOF
    ArticulationDofPosition = 30,       // [N, D]
    ArticulationDofVelocity = 31,       // [N, D]
    ArticulationDofPositionTarget = 32, // [N, D]
    ArticulationDofVelocityTarget = 33, // [N, D]
    ArticulationDofActuationForce = 34, // [N, D]

    // DOF properties
    ArticulationDofStiffness = 35,           // [N, D]
    ArticulationDofDamping = 36,             // [N, D]
    ArticulationDofLimit = 37,               // [N, D, 2] (lower, upper)
    ArticulationDofMaxVelocity = 38,         // [N, D]
    ArticulationDofMaxForce = 39,            // [N, D]
    ArticulationDofArmature = 40,            // [N, D]
    ArticulationDofFrictionProperties = 41,  // [N, D, 3] (static, dynamic, viscous)

    // External forces (write-only control inputs)
    RigidBodyForce = 50,          // [N, 3] forces at COM
    RigidBodyWrench = 51,         // [N, 9] (fx,fy,fz,tx,ty,tz,px,py,pz)
    ArticulationLinkWrench = 52,  // [N, L, 9]

    // Articulation body properties
    ArticulationBodyMass = 60,        // [N, L]
    ArticulationBodyComPose = 61,     // [N, L, 7]
    ArticulationBodyInertia = 62,     // [N, L, 9]
    ArticulationBodyInvMass = 63,     // [N, L] (read-only)
    ArticulationBodyInvInertia = 64,  // [N, L, 9] (read-only)

    // Dynamics queries (read-only)
    ArticulationJacobian = 70,                     // [N, R, C]
    ArticulationMassMatrix = 71,                   // [N, M, M]
    ArticulationCoriolisAndCentrifugalForce = 72,  // [N, M]
    ArticulationGravityForce = 73,                 // [N, M]
    ArticulationLinkIncomingJointForce = 74,       // [N, L, 6]
    ArticulationDofProjectedJointForce = 75,       // [N, D] (read-only)

    // Fixed tendon properties
    ArticulationFixedTendonStiffness = 80,       // [N, T]
    ArticulationFixedTendonDamping = 81,         // [N, T]
    ArticulationFixedTendonLimitStiffness = 82,  // [N, T]
    ArticulationFixedTendonLimit = 83,           // [N, T, 2]
    ArticulationFixedTendonRestLength = 84,      // [N, T]
    ArticulationFixedTendonOffset = 85,          // [N, T]

    // Spatial tendon properties
    ArticulationSpatialTendonStiffness = 90,       // [N, T]
    ArticulationSpatialTendonDamping = 91,         // [N, T]
    ArticulationSpatialTendonLimitStiffness = 92,  // [N, T]
    ArticulationSpatialTendonOffset = 93,          // [N, T]

    // Shape-level properties (per collision shape)
    RigidBodyShapeFrictionAndRestitution = 100,      // [N, S, 3]
    RigidBodyContactOffset = 101,                    // [N, S]
    RigidBodyRestOffset = 102,                       // [N, S]
    ArticulationShapeFrictionAndRestitution = 110,   // [N, S, 3]
    ArticulationContactOffset = 111,                 // [N, S]
    ArticulationRestOffset = 112,                    // [N, S]
}
