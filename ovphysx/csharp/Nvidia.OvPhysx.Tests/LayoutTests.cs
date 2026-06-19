// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using System.Runtime.InteropServices;
using Nvidia.OvPhysx.Interop;
using Xunit;

namespace Nvidia.OvPhysx.Tests;

/// <summary>
/// Verifies the managed interop struct mirrors are binary-compatible (x64) with the C ABI in
/// <c>ovphysx_types.h</c> / <c>dlpack.h</c>. These run without the native library, catching ABI
/// drift before any P/Invoke is ever made.
/// </summary>
public class LayoutTests
{
    [Theory]
    [InlineData(typeof(ovphysx_string_t), 16)]
    [InlineData(typeof(ovphysx_result_t), 4)]
    [InlineData(typeof(ovphysx_enqueue_result_t), 16)]
    [InlineData(typeof(ovphysx_op_wait_result_t), 24)]
    [InlineData(typeof(ovphysx_cuda_sync_t), 24)]
    [InlineData(typeof(ovphysx_create_args), 48)]
    [InlineData(typeof(ovphysx_config_entry_t), 40)]
    [InlineData(typeof(ovphysx_tensor_binding_desc_t), 32)]
    [InlineData(typeof(ovphysx_tensor_spec_t), 40)]
    [InlineData(typeof(ovphysx_articulation_metadata_t), 24)]
    [InlineData(typeof(ovphysx_scene_query_hit_t), 64)]
    [InlineData(typeof(ovphysx_scene_query_geometry_desc_t), 48)]
    [InlineData(typeof(ovphysx_contact_event_header_t), 72)]
    [InlineData(typeof(ovphysx_contact_point_t), 64)]
    [InlineData(typeof(ovphysx_friction_anchor_t), 24)]
    [InlineData(typeof(DLDevice), 8)]
    [InlineData(typeof(DLDataType), 4)]
    [InlineData(typeof(DLTensor), 48)]
    public void StructSizeMatchesC(Type type, int expectedSize)
    {
        Assert.Equal(expectedSize, Marshal.SizeOf(type));
    }

    [Fact]
    public void ConfigEntryUnionOffsetsMatchC()
    {
        // key_type @0; key union @8 (8-byte aligned due to ovphysx_string_t); value union @24.
        Assert.Equal(0, (int)Marshal.OffsetOf<ovphysx_config_entry_t>(nameof(ovphysx_config_entry_t.key_type)));
        Assert.Equal(8, (int)Marshal.OffsetOf<ovphysx_config_entry_t>(nameof(ovphysx_config_entry_t.carbonite_key)));
        Assert.Equal(8, (int)Marshal.OffsetOf<ovphysx_config_entry_t>(nameof(ovphysx_config_entry_t.int32_key)));
        Assert.Equal(24, (int)Marshal.OffsetOf<ovphysx_config_entry_t>(nameof(ovphysx_config_entry_t.string_value)));
        Assert.Equal(24, (int)Marshal.OffsetOf<ovphysx_config_entry_t>(nameof(ovphysx_config_entry_t.int32_value)));
    }

    [Fact]
    public void GeometryDescUnionOverlapsCorrectly()
    {
        Assert.Equal(0, (int)Marshal.OffsetOf<ovphysx_scene_query_geometry_desc_t>(nameof(ovphysx_scene_query_geometry_desc_t.type)));
        // All union members start at offset 8.
        Assert.Equal(8, (int)Marshal.OffsetOf<ovphysx_scene_query_geometry_desc_t>(nameof(ovphysx_scene_query_geometry_desc_t.sphere_radius)));
        Assert.Equal(8, (int)Marshal.OffsetOf<ovphysx_scene_query_geometry_desc_t>(nameof(ovphysx_scene_query_geometry_desc_t.shape_prim_path)));
    }
}
