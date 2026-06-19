// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using System.Runtime.InteropServices;

namespace Nvidia.OvPhysx.Interop;

/// <summary>
/// Source-generated P/Invoke declarations for the native <c>ovphysx</c> C API.
/// One-to-one with the prototypes in the Python <c>_bindings.py</c>. Pointer parameters
/// are used where the C API takes <c>POINTER(...)</c>, including optional (nullable) tensors.
/// </summary>
internal static unsafe partial class NativeMethods
{
    private const string Lib = NativeLibraryResolver.LibraryName;

    // --------------------------------------------------------------------
    // Lifecycle
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_create_instance(ovphysx_create_args* args, ulong* out_handle);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_destroy_instance(ulong handle);

    // --------------------------------------------------------------------
    // USD / stage
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial ovphysx_enqueue_result_t ovphysx_add_usd(ulong handle, ovphysx_string_t usd_path, ovphysx_string_t path_prefix, ulong* out_usd_handle);

    [LibraryImport(Lib)]
    internal static partial ovphysx_enqueue_result_t ovphysx_remove_usd(ulong handle, ulong usd_handle);

    [LibraryImport(Lib)]
    internal static partial ovphysx_enqueue_result_t ovphysx_reset(ulong handle);

    [LibraryImport(Lib)]
    internal static partial ovphysx_enqueue_result_t ovphysx_clone(ulong handle, ovphysx_string_t source_path, ovphysx_string_t* target_paths, uint target_count, float* parent_transforms);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_stage_id(ulong handle, long* out_stage_id);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_attach_stage(ulong handle, nint stage);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_detach_stage(ulong handle);

    // --------------------------------------------------------------------
    // Stepping
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial ovphysx_enqueue_result_t ovphysx_step(ulong handle, float dt, float sim_time);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_step_sync(ulong handle, float dt, float sim_time);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_step_n_sync(ulong handle, int n, float dt, float current_time);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_update_articulations_kinematic(ulong handle);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_warmup_gpu(ulong handle);

    // --------------------------------------------------------------------
    // Async operation tracking
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_wait_op(ulong handle, ulong op_index, ulong timeout_ns, ovphysx_op_wait_result_t* out_result);

    [LibraryImport(Lib)]
    internal static partial void ovphysx_destroy_wait_result(ovphysx_op_wait_result_t* result);

    [LibraryImport(Lib)]
    internal static partial ovphysx_string_t ovphysx_get_last_error();

    [LibraryImport(Lib)]
    internal static partial ovphysx_string_t ovphysx_get_last_op_error(ulong op_index);

    // --------------------------------------------------------------------
    // Version
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial nint ovphysx_get_version_string();

    // --------------------------------------------------------------------
    // Typed config (process-global)
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_set_global_config(ovphysx_config_entry_t entry);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_global_config_bool(int key, byte* out_value);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_global_config_int32(int key, int* out_value);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_global_config_float(int key, float* out_value);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_global_config_string(int key, ovphysx_string_t* out_value, nuint* out_size);

    // --------------------------------------------------------------------
    // Logging
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_set_log_level(uint level);

    [LibraryImport(Lib)]
    internal static partial uint ovphysx_get_log_level();

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_enable_default_log_output([MarshalAs(UnmanagedType.U1)] bool enable);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_register_log_callback(nint callback, nint user_data);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_unregister_log_callback(nint callback, nint user_data);

    [LibraryImport(Lib)]
    internal static partial void ovphysx_log_emit_test_messages();

    // --------------------------------------------------------------------
    // Remote storage credentials
    // --------------------------------------------------------------------

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial ovphysx_result_t ovphysx_configure_s3(string host, string bucket, string region, string access_key_id, string secret_access_key, string? session_token);

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial ovphysx_result_t ovphysx_configure_azure_sas(string host, string container, string sas_token);

    // --------------------------------------------------------------------
    // Tensor bindings
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_create_tensor_binding(ulong handle, ovphysx_tensor_binding_desc_t* desc, ulong* out_binding);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_destroy_tensor_binding(ulong handle, ulong binding);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_tensor_binding_spec(ulong handle, ulong binding, ovphysx_tensor_spec_t* out_spec);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_read_tensor_binding(ulong handle, ulong binding, DLTensor* dst_tensor);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_write_tensor_binding(ulong handle, ulong binding, DLTensor* src_tensor, DLTensor* index_tensor);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_write_tensor_binding_masked(ulong handle, ulong binding, DLTensor* src_tensor, DLTensor* mask_tensor);

    // --------------------------------------------------------------------
    // Articulation / binding introspection (two-call size-then-fill)
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_articulation_metadata(ulong handle, ulong binding, ovphysx_articulation_metadata_t* out_meta);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_articulation_get_dof_names(ulong handle, ulong binding, ovphysx_string_t* out_names, uint capacity, uint* out_count);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_articulation_get_body_names(ulong handle, ulong binding, ovphysx_string_t* out_names, uint capacity, uint* out_count);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_articulation_get_joint_names(ulong handle, ulong binding, ovphysx_string_t* out_names, uint capacity, uint* out_count);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_tensor_binding_get_prim_paths(ulong handle, ulong binding, ovphysx_string_t* out_paths, uint capacity, uint* out_count);

    // --------------------------------------------------------------------
    // Contact bindings
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_create_contact_binding(ulong handle, ovphysx_string_t* sensors, uint sensor_count, ovphysx_string_t* filters, uint filter_count, uint max_contact_data, ulong* out_binding);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_destroy_contact_binding(ulong handle, ulong binding);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_contact_binding_spec(ulong handle, ulong binding, int* out_sensor_count, int* out_filter_count);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_contact_binding_get_sensor_paths(ulong handle, ulong binding, ovphysx_string_t* out_paths, uint capacity, uint* out_count);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_contact_binding_get_filter_paths(ulong handle, ulong binding, ovphysx_string_t* out_paths, uint capacity, uint* out_count);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_contact_binding_capacity(ulong handle, ulong binding, uint* out_capacity);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_read_contact_net_forces(ulong handle, ulong binding, DLTensor* out_tensor);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_read_contact_force_matrix(ulong handle, ulong binding, DLTensor* out_tensor);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_read_contact_data(ulong handle, ulong binding, DLTensor* contact_forces, DLTensor* positions, DLTensor* normals, DLTensor* separations, DLTensor* counts, DLTensor* start_indices);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_read_friction_data(ulong handle, ulong binding, DLTensor* friction_forces, DLTensor* friction_points, DLTensor* counts, DLTensor* start_indices);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_contact_report(ulong handle, ovphysx_contact_event_header_t** out_headers, uint* out_num_headers, ovphysx_contact_point_t** out_points, uint* out_num_points, ovphysx_friction_anchor_t** out_anchors, uint* out_num_anchors);

    // --------------------------------------------------------------------
    // PhysX object interop
    // --------------------------------------------------------------------

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial ovphysx_result_t ovphysx_get_physx_ptr(ulong handle, string prim_path, int physx_type, nint* out_ptr);

    // --------------------------------------------------------------------
    // Scene queries
    // --------------------------------------------------------------------

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_raycast(ulong handle, float* origin, float* direction, float distance, [MarshalAs(UnmanagedType.U1)] bool both_sides, int mode, ovphysx_scene_query_hit_t** out_hits, uint* out_count);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_sweep(ulong handle, ovphysx_scene_query_geometry_desc_t* geometry, float* direction, float distance, [MarshalAs(UnmanagedType.U1)] bool both_sides, int mode, ovphysx_scene_query_hit_t** out_hits, uint* out_count);

    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_overlap(ulong handle, ovphysx_scene_query_geometry_desc_t* geometry, int mode, ovphysx_scene_query_hit_t** out_hits, uint* out_count);
}
