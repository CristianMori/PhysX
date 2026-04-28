// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

namespace Nvidia.OvPhysx;

/// <summary>Return status from ovPhysX API calls.</summary>
public enum ApiStatus
{
    /// <summary>The call completed successfully.</summary>
    Success = 0,
    /// <summary>The call failed. Use GetLastError() for details.</summary>
    Error = 1,
    /// <summary>The operation timed out.</summary>
    Timeout = 2,
    /// <summary>The requested feature is not implemented.</summary>
    NotImplemented = 3,
    /// <summary>An argument was invalid.</summary>
    InvalidArgument = 4,
    /// <summary>The requested object was not found.</summary>
    NotFound = 5,
    /// <summary>The output buffer is too small.</summary>
    BufferTooSmall = 6,
    /// <summary>A device mismatch occurred (e.g., CPU tensor on GPU instance).</summary>
    DeviceMismatch = 7,
    /// <summary>GPU is not available on this system.</summary>
    GpuNotAvailable = 8,
}

/// <summary>Simulation device mode. Locked process-wide on first instance creation.</summary>
public enum DeviceType
{
    /// <summary>GPU preferred, CPU fallback.</summary>
    Auto = 0,
    /// <summary>GPU required (fails if unavailable).</summary>
    Gpu = 1,
    /// <summary>CPU only.</summary>
    Cpu = 2,
}

/// <summary>Log severity levels.</summary>
public enum LogLevel
{
    Verbose = 0, Info = 1, Warning = 2, Error = 3, None = 4,
}

/// <summary>Scene query hit mode.</summary>
public enum SceneQueryMode
{
    /// <summary>Return only the closest hit (0 or 1 result).</summary>
    Closest = 0,
    /// <summary>Return any single hit (0 or 1 result).</summary>
    Any = 1,
    /// <summary>Return all hits.</summary>
    All = 2,
}

/// <summary>Scene query geometry shape type.</summary>
public enum SceneQueryGeometryType { Sphere = 0, Box = 1, Shape = 2 }

/// <summary>PhysX C++ object types for GetPhysXPtr().</summary>
public enum PhysXObjectType
{
    /// <summary>physx::PxScene*</summary>
    Scene = 1,
    /// <summary>physx::PxMaterial*</summary>
    Material = 2,
    /// <summary>physx::PxShape*</summary>
    Shape = 3,
    CompoundShape = 4,
    /// <summary>physx::PxRigidDynamic* or PxRigidStatic*</summary>
    Actor = 5,
    /// <summary>physx::PxJoint*</summary>
    Joint = 6,
    CustomJoint = 7,
    /// <summary>physx::PxArticulationReducedCoordinate*</summary>
    Articulation = 8,
    /// <summary>physx::PxArticulationLink*</summary>
    Link = 9,
    /// <summary>physx::PxArticulationJointReducedCoordinate*</summary>
    LinkJoint = 10,
    ParticleSystem = 11,
    ParticleSet = 12,
    /// <summary>physx::PxPhysics*</summary>
    Physics = 31,
}

/// <summary>Boolean configuration keys (process-global).</summary>
public enum ConfigBoolKey
{
    DisableContactProcessing = 0,
    CollisionConeCustomGeometry = 1,
    CollisionCylinderCustomGeometry = 2,
}

/// <summary>Integer configuration keys (process-global).</summary>
public enum ConfigInt32Key
{
    /// <summary>Number of simulation threads.</summary>
    NumThreads = 0,
    /// <summary>Multi-GPU mode: 0=disabled, 1=all, 2=skip-first.</summary>
    SceneMultiGpuMode = 1,
}

/// <summary>
/// Tensor data types for creating tensor bindings. Each type defines the
/// shape and semantics of the data exchanged with the simulation.
/// All tensors are float32 with row-major (C-order) layout.
/// </summary>
public enum TensorType
{
    Invalid = 0,
    /// <summary>[N, 7]: position (3) + quaternion xyzw (4)</summary>
    RigidBodyPose = 1,
    /// <summary>[N, 6]: linear velocity (3) + angular velocity (3)</summary>
    RigidBodyVelocity = 2,
    /// <summary>[N]: scalar mass</summary>
    RigidBodyMass = 3,
    /// <summary>[N, 9]: row-major 3x3 inertia tensor</summary>
    RigidBodyInertia = 4,
    /// <summary>[N, 7]: center-of-mass pose in local frame</summary>
    RigidBodyComPose = 5,
    /// <summary>[N, 7]: articulation root pose</summary>
    ArticulationRootPose = 10,
    /// <summary>[N, 6]: articulation root velocity</summary>
    ArticulationRootVelocity = 11,
    /// <summary>[N, L, 7]: per-link poses (L = max links per articulation)</summary>
    ArticulationLinkPose = 20,
    /// <summary>[N, L, 6]: per-link velocities</summary>
    ArticulationLinkVelocity = 21,
    /// <summary>[N, L, 6]: per-link accelerations (read-only)</summary>
    ArticulationLinkAcceleration = 22,
    /// <summary>[N, D]: joint positions in radians or meters</summary>
    ArticulationDofPosition = 30,
    /// <summary>[N, D]: joint velocities</summary>
    ArticulationDofVelocity = 31,
    /// <summary>[N, D]: PD position targets</summary>
    ArticulationDofPositionTarget = 32,
    /// <summary>[N, D]: PD velocity targets</summary>
    ArticulationDofVelocityTarget = 33,
    /// <summary>[N, D]: joint actuation forces (read/write staging buffer)</summary>
    ArticulationDofActuationForce = 34,
    /// <summary>[N, D]: joint stiffness</summary>
    ArticulationDofStiffness = 35,
    /// <summary>[N, D]: joint damping</summary>
    ArticulationDofDamping = 36,
    /// <summary>[N, D]: joint limits</summary>
    ArticulationDofLimit = 37,
    ArticulationDofMaxVelocity = 38,
    ArticulationDofMaxForce = 39,
    ArticulationDofArmature = 40,
    ArticulationDofFrictionProperties = 41,
    /// <summary>[N, 3]: center-of-mass force (write-only)</summary>
    RigidBodyForce = 50,
    /// <summary>[N, 9]: force + torque + application point (write-only)</summary>
    RigidBodyWrench = 51,
    /// <summary>[N, L, 9]: per-link wrench (write-only)</summary>
    ArticulationLinkWrench = 52,
    /// <summary>[N, L]: per-link mass</summary>
    ArticulationBodyMass = 60,
    ArticulationBodyComPose = 61,
    ArticulationBodyInertia = 62,
    /// <summary>[N, L]: inverse mass (read-only)</summary>
    ArticulationBodyInvMass = 63,
    ArticulationBodyInvInertia = 64,
    /// <summary>[N, R, C]: geometric Jacobian (read-only)</summary>
    ArticulationJacobian = 70,
    /// <summary>[N, M, M]: generalized mass matrix (read-only)</summary>
    ArticulationMassMatrix = 71,
    ArticulationCoriolisAndCentrifugalForce = 72,
    ArticulationGravityForce = 73,
    /// <summary>[N, L, 6]: joint reaction forces (read-only)</summary>
    ArticulationLinkIncomingJointForce = 74,
    ArticulationDofProjectedJointForce = 75,
    ArticulationFixedTendonStiffness = 80,
    ArticulationFixedTendonDamping = 81,
    ArticulationFixedTendonLimitStiffness = 82,
    ArticulationFixedTendonLimit = 83,
    ArticulationFixedTendonRestLength = 84,
    ArticulationFixedTendonOffset = 85,
    ArticulationSpatialTendonStiffness = 90,
    ArticulationSpatialTendonDamping = 91,
    ArticulationSpatialTendonLimitStiffness = 92,
    ArticulationSpatialTendonOffset = 93,
    /// <summary>[N, S, 3]: per-shape friction and restitution</summary>
    RigidBodyShapeFrictionAndRestitution = 100,
    RigidBodyContactOffset = 101,
    RigidBodyRestOffset = 102,
    ArticulationShapeFrictionAndRestitution = 110,
    ArticulationContactOffset = 111,
    ArticulationRestOffset = 112,
}
