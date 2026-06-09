// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using System.Numerics;
using System.Runtime.InteropServices;
using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

public sealed partial class PhysX
{
    /// <summary>Casts a ray and returns the matching hits.</summary>
    public unsafe SceneQueryHit[] Raycast(
        Vector3 origin, Vector3 direction, float distance,
        SceneQueryMode mode = SceneQueryMode.Closest, bool bothSides = false)
    {
        EnsureValid();
        float* o = stackalloc float[3] { origin.X, origin.Y, origin.Z };
        float* d = stackalloc float[3] { direction.X, direction.Y, direction.Z };

        ovphysx_scene_query_hit_t* hits;
        uint count;
        OvPhysxException.Check(
            NativeMethods.ovphysx_raycast(_handle, o, d, distance, bothSides, (int)mode, &hits, &count),
            "raycast");
        return CopyHits(hits, count);
    }

    /// <summary>Sweeps geometry along a direction and returns the matching hits.</summary>
    public unsafe SceneQueryHit[] Sweep(
        SceneQueryGeometry geometry, Vector3 direction, float distance,
        SceneQueryMode mode = SceneQueryMode.Closest, bool bothSides = false)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(geometry);

        nint primPathPtr = nint.Zero;
        try
        {
            ovphysx_scene_query_geometry_desc_t desc = BuildGeometry(geometry, ref primPathPtr);
            float* d = stackalloc float[3] { direction.X, direction.Y, direction.Z };

            ovphysx_scene_query_hit_t* hits;
            uint count;
            OvPhysxException.Check(
                NativeMethods.ovphysx_sweep(_handle, &desc, d, distance, bothSides, (int)mode, &hits, &count),
                "sweep");
            return CopyHits(hits, count);
        }
        finally
        {
            if (primPathPtr != nint.Zero)
                Marshal.FreeCoTaskMem(primPathPtr);
        }
    }

    /// <summary>Returns objects whose geometry overlaps the query geometry (location fields are zeroed).</summary>
    public unsafe SceneQueryHit[] Overlap(SceneQueryGeometry geometry, SceneQueryMode mode = SceneQueryMode.All)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(geometry);

        nint primPathPtr = nint.Zero;
        try
        {
            ovphysx_scene_query_geometry_desc_t desc = BuildGeometry(geometry, ref primPathPtr);

            ovphysx_scene_query_hit_t* hits;
            uint count;
            OvPhysxException.Check(
                NativeMethods.ovphysx_overlap(_handle, &desc, (int)mode, &hits, &count),
                "overlap");
            return CopyHits(hits, count);
        }
        finally
        {
            if (primPathPtr != nint.Zero)
                Marshal.FreeCoTaskMem(primPathPtr);
        }
    }

    private static unsafe ovphysx_scene_query_geometry_desc_t BuildGeometry(SceneQueryGeometry g, ref nint primPathPtr)
    {
        var desc = new ovphysx_scene_query_geometry_desc_t { type = (int)g.Kind };
        switch (g.Kind)
        {
            case SceneQueryGeometryType.Sphere:
                desc.sphere_radius = g.Radius;
                desc.sphere_position[0] = g.Position.X;
                desc.sphere_position[1] = g.Position.Y;
                desc.sphere_position[2] = g.Position.Z;
                break;

            case SceneQueryGeometryType.Box:
                desc.box_half_extent[0] = g.HalfExtent.X;
                desc.box_half_extent[1] = g.HalfExtent.Y;
                desc.box_half_extent[2] = g.HalfExtent.Z;
                desc.box_position[0] = g.Position.X;
                desc.box_position[1] = g.Position.Y;
                desc.box_position[2] = g.Position.Z;
                desc.box_rotation[0] = g.Rotation.X;
                desc.box_rotation[1] = g.Rotation.Y;
                desc.box_rotation[2] = g.Rotation.Z;
                desc.box_rotation[3] = g.Rotation.W;
                break;

            case SceneQueryGeometryType.Shape:
                primPathPtr = Marshal.StringToCoTaskMemUTF8(g.PrimPath);
                desc.shape_prim_path = primPathPtr;
                break;
        }
        return desc;
    }

    private static unsafe SceneQueryHit[] CopyHits(ovphysx_scene_query_hit_t* hits, uint count)
    {
        if (hits == null || count == 0)
            return [];

        var result = new SceneQueryHit[count];
        for (uint i = 0; i < count; i++)
        {
            ovphysx_scene_query_hit_t* h = &hits[i];
            result[i] = new SceneQueryHit(
                h->collision,
                h->rigid_body,
                h->proto_index,
                new Vector3(h->normal[0], h->normal[1], h->normal[2]),
                new Vector3(h->position[0], h->position[1], h->position[2]),
                h->distance,
                h->face_index,
                h->material);
        }
        return result;
    }
}
