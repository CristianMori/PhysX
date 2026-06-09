// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using System.Runtime.InteropServices;

namespace Nvidia.OvPhysx.Interop;

/// <summary>Result returned by synchronous API functions (<c>ovphysx_result_t</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ovphysx_result_t
{
    public ovphysx_api_status_t status;
}

/// <summary>Result returned by asynchronous API functions (<c>ovphysx_enqueue_result_t</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ovphysx_enqueue_result_t
{
    public ovphysx_api_status_t status;
    public ulong op_index;
}

/// <summary>Result from <c>ovphysx_wait_op</c>; must be released via <c>ovphysx_destroy_wait_result</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ovphysx_op_wait_result_t
{
    public ulong* error_op_indices;
    public nuint num_errors;
    public ulong lowest_pending_op_index;
}

/// <summary>CUDA synchronization descriptor (<c>ovphysx_cuda_sync_t</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ovphysx_cuda_sync_t
{
    public nuint stream;
    public nuint wait_event;
    public nuint signal_event;
}

/// <summary>Configuration for creating an instance (<c>ovphysx_create_args</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ovphysx_create_args
{
    public ovphysx_string_t bundled_deps_path;
    public ovphysx_config_entry_t* config_entries;
    public uint config_entry_count;
    public int device; // ovphysx_device_t
    public ovphysx_string_t active_cuda_gpus;
}

/// <summary>
/// Typed config entry tagged union (<c>ovphysx_config_entry_t</c>).
/// Explicit layout mirrors the C union: key starts at offset 8 (8-byte aligned
/// because the largest key member is an <see cref="ovphysx_string_t"/>), value at offset 24.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal struct ovphysx_config_entry_t
{
    [FieldOffset(0)] public ovphysx_config_key_type_t key_type;

    // key union (offset 8)
    [FieldOffset(8)] public int bool_key;
    [FieldOffset(8)] public int int32_key;
    [FieldOffset(8)] public int float_key;
    [FieldOffset(8)] public int string_key;
    [FieldOffset(8)] public ovphysx_string_t carbonite_key;

    // value union (offset 24)
    [FieldOffset(24)] public byte bool_value;
    [FieldOffset(24)] public int int32_value;
    [FieldOffset(24)] public float float_value;
    [FieldOffset(24)] public ovphysx_string_t string_value;
}

/// <summary>Descriptor for creating a tensor binding (<c>ovphysx_tensor_binding_desc_t</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ovphysx_tensor_binding_desc_t
{
    public ovphysx_string_t pattern;
    public ovphysx_string_t* prim_paths;
    public uint prim_paths_count;
    public int tensor_type; // ovphysx_tensor_type_t
}

/// <summary>Tensor specification returned by <c>ovphysx_get_tensor_binding_spec</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ovphysx_tensor_spec_t
{
    public DLDataType dtype;
    public int ndim;
    public fixed long shape[4];
}

/// <summary>Articulation topology metadata (<c>ovphysx_articulation_metadata_t</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ovphysx_articulation_metadata_t
{
    public int dof_count;
    public int body_count;
    public int joint_count;
    public int fixed_tendon_count;
    public int spatial_tendon_count;
    public byte is_fixed_base;
}

/// <summary>Scene-query hit result (<c>ovphysx_scene_query_hit_t</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ovphysx_scene_query_hit_t
{
    public ulong collision;
    public ulong rigid_body;
    public uint proto_index;
    public fixed float normal[3];
    public fixed float position[3];
    public float distance;
    public uint face_index;
    public ulong material;
}

/// <summary>Geometry descriptor for sweep/overlap queries (<c>ovphysx_scene_query_geometry_desc_t</c>).</summary>
[StructLayout(LayoutKind.Explicit)]
internal unsafe struct ovphysx_scene_query_geometry_desc_t
{
    [FieldOffset(0)] public int type;

    // sphere { float radius; float position[3]; }
    [FieldOffset(8)] public float sphere_radius;
    [FieldOffset(12)] public fixed float sphere_position[3];

    // box { float half_extent[3]; float position[3]; float rotation[4]; }
    [FieldOffset(8)] public fixed float box_half_extent[3];
    [FieldOffset(20)] public fixed float box_position[3];
    [FieldOffset(32)] public fixed float box_rotation[4];

    // shape { const char* prim_path; }
    [FieldOffset(8)] public nint shape_prim_path;
}

/// <summary>Contact event header (<c>ovphysx_contact_event_header_t</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ovphysx_contact_event_header_t
{
    public int type;
    public long stageId;
    public ulong actor0;
    public ulong actor1;
    public ulong collider0;
    public ulong collider1;
    public uint contactDataOffset;
    public uint numContactData;
    public uint frictionAnchorsDataOffset;
    public uint numfrictionAnchorsData;
    public uint protoIndex0;
    public uint protoIndex1;
}

/// <summary>Per-contact-point data (<c>ovphysx_contact_point_t</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ovphysx_contact_point_t
{
    public fixed float position[3];
    public fixed float normal[3];
    public fixed float impulse[3];
    public float separation;
    public uint faceIndex0;
    public uint faceIndex1;
    public ulong material0;
    public ulong material1;
}

/// <summary>Friction anchor data (<c>ovphysx_friction_anchor_t</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ovphysx_friction_anchor_t
{
    public fixed float position[3];
    public fixed float impulse[3];
}
