// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using System.Numerics;

namespace Nvidia.OvPhysx;

/// <summary>
/// Geometry used for <see cref="PhysX.Sweep"/> and <see cref="PhysX.Overlap"/> queries.
/// Construct via <see cref="Sphere"/>, <see cref="Box"/>, or <see cref="Shape"/>.
/// </summary>
public sealed class SceneQueryGeometry
{
    internal SceneQueryGeometryType Kind { get; private init; }
    internal float Radius { get; private init; }
    internal Vector3 Position { get; private init; }
    internal Vector3 HalfExtent { get; private init; }
    internal Quaternion Rotation { get; private init; }
    internal string? PrimPath { get; private init; }

    private SceneQueryGeometry() { }

    /// <summary>A sphere defined by radius and world-space center.</summary>
    public static SceneQueryGeometry Sphere(float radius, Vector3 position) => new()
    {
        Kind = SceneQueryGeometryType.Sphere,
        Radius = radius,
        Position = position,
    };

    /// <summary>An oriented box defined by half-extents, world-space center, and orientation.</summary>
    public static SceneQueryGeometry Box(Vector3 halfExtent, Vector3 position, Quaternion rotation) => new()
    {
        Kind = SceneQueryGeometryType.Box,
        HalfExtent = halfExtent,
        Position = position,
        Rotation = rotation,
    };

    /// <summary>Any <c>UsdGeomGPrim</c> identified by its USD prim path (meshes use a convex approximation).</summary>
    public static SceneQueryGeometry Shape(string primPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(primPath);
        return new SceneQueryGeometry { Kind = SceneQueryGeometryType.Shape, PrimPath = primPath };
    }
}
