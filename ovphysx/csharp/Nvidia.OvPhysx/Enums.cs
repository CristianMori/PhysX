// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

namespace Nvidia.OvPhysx;

public enum ApiStatus
{
    Success = 0, Error = 1, Timeout = 2, NotImplemented = 3,
    InvalidArgument = 4, NotFound = 5, BufferTooSmall = 6,
    DeviceMismatch = 7, GpuNotAvailable = 8,
}

public enum DeviceType { Auto = 0, Gpu = 1, Cpu = 2 }

public enum LogLevel { Verbose = 0, Info = 1, Warning = 2, Error = 3, None = 4 }

public enum SceneQueryMode { Closest = 0, Any = 1, All = 2 }

public enum SceneQueryGeometryType { Sphere = 0, Box = 1, Shape = 2 }

public enum PhysXObjectType
{
    Scene = 1, Material = 2, Shape = 3, CompoundShape = 4, Actor = 5,
    Joint = 6, CustomJoint = 7, Articulation = 8, Link = 9, LinkJoint = 10,
    ParticleSystem = 11, ParticleSet = 12, Physics = 31,
}

public enum ConfigBoolKey { DisableContactProcessing = 0, CollisionConeCustomGeometry = 1, CollisionCylinderCustomGeometry = 2 }
public enum ConfigInt32Key { NumThreads = 0, SceneMultiGpuMode = 1 }

public enum TensorType
{
    Invalid = 0,
    RigidBodyPose = 1, RigidBodyVelocity = 2, RigidBodyMass = 3,
    RigidBodyInertia = 4, RigidBodyComPose = 5,
    ArticulationRootPose = 10, ArticulationRootVelocity = 11,
    ArticulationLinkPose = 20, ArticulationLinkVelocity = 21, ArticulationLinkAcceleration = 22,
    ArticulationDofPosition = 30, ArticulationDofVelocity = 31,
    ArticulationDofPositionTarget = 32, ArticulationDofVelocityTarget = 33,
    ArticulationDofActuationForce = 34, ArticulationDofStiffness = 35,
    ArticulationDofDamping = 36, ArticulationDofLimit = 37,
    ArticulationDofMaxVelocity = 38, ArticulationDofMaxForce = 39,
    ArticulationDofArmature = 40, ArticulationDofFrictionProperties = 41,
    RigidBodyForce = 50, RigidBodyWrench = 51, ArticulationLinkWrench = 52,
    ArticulationBodyMass = 60, ArticulationBodyComPose = 61,
    ArticulationBodyInertia = 62, ArticulationBodyInvMass = 63, ArticulationBodyInvInertia = 64,
    ArticulationJacobian = 70, ArticulationMassMatrix = 71,
    ArticulationCoriolisAndCentrifugalForce = 72, ArticulationGravityForce = 73,
    ArticulationLinkIncomingJointForce = 74, ArticulationDofProjectedJointForce = 75,
    ArticulationFixedTendonStiffness = 80, ArticulationFixedTendonDamping = 81,
    ArticulationFixedTendonLimitStiffness = 82, ArticulationFixedTendonLimit = 83,
    ArticulationFixedTendonRestLength = 84, ArticulationFixedTendonOffset = 85,
    ArticulationSpatialTendonStiffness = 90, ArticulationSpatialTendonDamping = 91,
    ArticulationSpatialTendonLimitStiffness = 92, ArticulationSpatialTendonOffset = 93,
    RigidBodyShapeFrictionAndRestitution = 100, RigidBodyContactOffset = 101, RigidBodyRestOffset = 102,
    ArticulationShapeFrictionAndRestitution = 110, ArticulationContactOffset = 111, ArticulationRestOffset = 112,
}
