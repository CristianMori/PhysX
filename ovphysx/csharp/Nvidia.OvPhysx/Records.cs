// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx;

/// <summary>
/// Articulation topology metadata, constant for the lifetime of a tensor binding
/// (mirrors <c>ovphysx_articulation_metadata_t</c>).
/// </summary>
public readonly record struct ArticulationMetadata(
    int DofCount,
    int BodyCount,
    int JointCount,
    int FixedTendonCount,
    int SpatialTendonCount,
    bool IsFixedBase);

/// <summary>
/// A single scene-query hit (raycast/sweep/overlap). For overlap queries the location fields
/// (<see cref="Normal"/>, <see cref="Position"/>, <see cref="Distance"/>, <see cref="FaceIndex"/>,
/// <see cref="Material"/>) are zeroed. Path fields are uint64-encoded <c>SdfPath</c>s.
/// </summary>
public readonly record struct SceneQueryHit(
    ulong Collision,
    ulong RigidBody,
    uint ProtoIndex,
    System.Numerics.Vector3 Normal,
    System.Numerics.Vector3 Position,
    float Distance,
    uint FaceIndex,
    ulong Material);

/// <summary>Contact event header describing one contact pair (mirrors <c>ovphysx_contact_event_header_t</c>).</summary>
public readonly record struct ContactEventHeader(
    int Type,
    long StageId,
    ulong Actor0,
    ulong Actor1,
    ulong Collider0,
    ulong Collider1,
    uint ContactDataOffset,
    uint NumContactData,
    uint FrictionAnchorsDataOffset,
    uint NumFrictionAnchorsData,
    uint ProtoIndex0,
    uint ProtoIndex1);

/// <summary>Per-contact-point data (mirrors <c>ovphysx_contact_point_t</c>). Divide impulse by dt for force.</summary>
public readonly record struct ContactPoint(
    System.Numerics.Vector3 Position,
    System.Numerics.Vector3 Normal,
    System.Numerics.Vector3 Impulse,
    float Separation,
    uint FaceIndex0,
    uint FaceIndex1,
    ulong Material0,
    ulong Material1);

/// <summary>Friction anchor data (mirrors <c>ovphysx_friction_anchor_t</c>). Divide impulse by dt for force.</summary>
public readonly record struct FrictionAnchor(
    System.Numerics.Vector3 Position,
    System.Numerics.Vector3 Impulse);

/// <summary>
/// Per-step contact report. Headers index slices of <see cref="Points"/> (and optionally
/// <see cref="Anchors"/>) via their offset/count fields.
/// </summary>
public sealed record ContactReport(
    IReadOnlyList<ContactEventHeader> Headers,
    IReadOnlyList<ContactPoint> Points,
    IReadOnlyList<FrictionAnchor> Anchors);
