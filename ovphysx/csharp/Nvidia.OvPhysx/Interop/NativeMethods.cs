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

    /// <summary>Creates an instance from <paramref name="args"/>; writes the new handle to <paramref name="out_handle"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_create_instance(ovphysx_create_args* args, ulong* out_handle);

    /// <summary>Destroys an instance and releases its native resources.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_destroy_instance(ulong handle);

    // --------------------------------------------------------------------
    // USD / stage
    // --------------------------------------------------------------------

    /// <summary>Adds a USD layer (async); writes the USD handle to <paramref name="out_usd_handle"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_enqueue_result_t ovphysx_add_usd(ulong handle, ovphysx_string_t usd_path, ovphysx_string_t path_prefix, ulong* out_usd_handle);

    /// <summary>Removes a previously-added USD layer (async).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_enqueue_result_t ovphysx_remove_usd(ulong handle, ulong usd_handle);

    /// <summary>Resets the stage to empty (async).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_enqueue_result_t ovphysx_reset(ulong handle);

    /// <summary>Clones a prim hierarchy to <paramref name="target_paths"/> with optional per-target transforms (async).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_enqueue_result_t ovphysx_clone(ulong handle, ovphysx_string_t source_path, ovphysx_string_t* target_paths, uint target_count, float* parent_transforms);

    /// <summary>Writes the attached USD stage id to <paramref name="out_stage_id"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_stage_id(ulong handle, long* out_stage_id);

    /// <summary>Attaches an external stage handle as the orchestration surface.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_attach_stage(ulong handle, nint stage);

    /// <summary>Detaches the currently-attached stage.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_detach_stage(ulong handle);

    // --------------------------------------------------------------------
    // Stepping
    // --------------------------------------------------------------------

    /// <summary>Enqueues a single physics step (async).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_enqueue_result_t ovphysx_step(ulong handle, float dt, float sim_time);

    /// <summary>Steps once and blocks until completion.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_step_sync(ulong handle, float dt, float sim_time);

    /// <summary>Runs <paramref name="n"/> steps in one call and blocks until completion.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_step_n_sync(ulong handle, int n, float dt, float current_time);

    /// <summary>Updates articulation link poses from joint positions without stepping.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_update_articulations_kinematic(ulong handle);

    /// <summary>Eagerly initializes GPU buffers to avoid first-read latency.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_warmup_gpu(ulong handle);

    // --------------------------------------------------------------------
    // Async operation tracking
    // --------------------------------------------------------------------

    /// <summary>Waits up to <paramref name="timeout_ns"/> for an operation; fills <paramref name="out_result"/> (free via destroy).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_wait_op(ulong handle, ulong op_index, ulong timeout_ns, ovphysx_op_wait_result_t* out_result);

    /// <summary>Frees the buffers owned by an <c>ovphysx_op_wait_result_t</c>.</summary>
    [LibraryImport(Lib)]
    internal static partial void ovphysx_destroy_wait_result(ovphysx_op_wait_result_t* result);

    /// <summary>Returns the thread-local last-error message for synchronous calls.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_string_t ovphysx_get_last_error();

    /// <summary>Returns the error message for a specific failed async operation index.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_string_t ovphysx_get_last_op_error(ulong op_index);

    // --------------------------------------------------------------------
    // Version
    // --------------------------------------------------------------------

    /// <summary>Returns the native library version as a null-terminated UTF-8 string pointer.</summary>
    [LibraryImport(Lib)]
    internal static partial nint ovphysx_get_version_string();

    // --------------------------------------------------------------------
    // Typed config (process-global)
    // --------------------------------------------------------------------

    /// <summary>Applies a single typed config entry to process-global state.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_set_global_config(ovphysx_config_entry_t entry);

    /// <summary>Reads a boolean config value into <paramref name="out_value"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_global_config_bool(int key, byte* out_value);

    /// <summary>Reads an int32 config value into <paramref name="out_value"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_global_config_int32(int key, int* out_value);

    /// <summary>Reads a float config value into <paramref name="out_value"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_global_config_float(int key, float* out_value);

    /// <summary>Reads a string config value into <paramref name="out_value"/> and its size into <paramref name="out_size"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_global_config_string(int key, ovphysx_string_t* out_value, nuint* out_size);

    // --------------------------------------------------------------------
    // Logging
    // --------------------------------------------------------------------

    /// <summary>Sets the global log verbosity threshold.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_set_log_level(uint level);

    /// <summary>Returns the current global log verbosity threshold.</summary>
    [LibraryImport(Lib)]
    internal static partial uint ovphysx_get_log_level();

    /// <summary>Enables or disables Carbonite's built-in console log output.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_enable_default_log_output([MarshalAs(UnmanagedType.U1)] bool enable);

    /// <summary>Registers a native log callback function pointer.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_register_log_callback(nint callback, nint user_data);

    /// <summary>Unregisters a previously-registered native log callback.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_unregister_log_callback(nint callback, nint user_data);

    /// <summary>Emits a set of test log messages (native diagnostic aid).</summary>
    [LibraryImport(Lib)]
    internal static partial void ovphysx_log_emit_test_messages();

    // --------------------------------------------------------------------
    // Remote storage credentials
    // --------------------------------------------------------------------

    /// <summary>Configures S3 credentials for loading USD assets from S3-backed URLs.</summary>
    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial ovphysx_result_t ovphysx_configure_s3(string host, string bucket, string region, string access_key_id, string secret_access_key, string? session_token);

    /// <summary>Configures an Azure SAS token for loading USD assets from Azure-backed URLs.</summary>
    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial ovphysx_result_t ovphysx_configure_azure_sas(string host, string container, string sas_token);

    // --------------------------------------------------------------------
    // Tensor bindings
    // --------------------------------------------------------------------

    /// <summary>Creates a tensor binding from <paramref name="desc"/>; writes the handle to <paramref name="out_binding"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_create_tensor_binding(ulong handle, ovphysx_tensor_binding_desc_t* desc, ulong* out_binding);

    /// <summary>Destroys a tensor binding.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_destroy_tensor_binding(ulong handle, ulong binding);

    /// <summary>Writes the binding's dtype/rank/shape into <paramref name="out_spec"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_tensor_binding_spec(ulong handle, ulong binding, ovphysx_tensor_spec_t* out_spec);

    /// <summary>Reads simulation state into <paramref name="dst_tensor"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_read_tensor_binding(ulong handle, ulong binding, DLTensor* dst_tensor);

    /// <summary>Writes <paramref name="src_tensor"/> to the simulation; <paramref name="index_tensor"/> (optional) selects rows.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_write_tensor_binding(ulong handle, ulong binding, DLTensor* src_tensor, DLTensor* index_tensor);

    /// <summary>Writes <paramref name="src_tensor"/> for entities selected by <paramref name="mask_tensor"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_write_tensor_binding_masked(ulong handle, ulong binding, DLTensor* src_tensor, DLTensor* mask_tensor);

    // --------------------------------------------------------------------
    // Articulation / binding introspection (two-call size-then-fill)
    // --------------------------------------------------------------------

    /// <summary>Writes articulation topology metadata into <paramref name="out_meta"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_articulation_metadata(ulong handle, ulong binding, ovphysx_articulation_metadata_t* out_meta);

    /// <summary>Fills <paramref name="out_names"/> with DOF names (size-then-fill via <paramref name="capacity"/>/<paramref name="out_count"/>).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_articulation_get_dof_names(ulong handle, ulong binding, ovphysx_string_t* out_names, uint capacity, uint* out_count);

    /// <summary>Fills <paramref name="out_names"/> with link/body names (size-then-fill).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_articulation_get_body_names(ulong handle, ulong binding, ovphysx_string_t* out_names, uint capacity, uint* out_count);

    /// <summary>Fills <paramref name="out_names"/> with joint names (size-then-fill).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_articulation_get_joint_names(ulong handle, ulong binding, ovphysx_string_t* out_names, uint capacity, uint* out_count);

    /// <summary>Fills <paramref name="out_paths"/> with the binding's resolved prim paths (size-then-fill).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_tensor_binding_get_prim_paths(ulong handle, ulong binding, ovphysx_string_t* out_paths, uint capacity, uint* out_count);

    // --------------------------------------------------------------------
    // Contact bindings
    // --------------------------------------------------------------------

    /// <summary>Creates a contact binding over sensor/filter patterns; writes the handle to <paramref name="out_binding"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_create_contact_binding(ulong handle, ovphysx_string_t* sensors, uint sensor_count, ovphysx_string_t* filters, uint filter_count, uint max_contact_data, ulong* out_binding);

    /// <summary>Destroys a contact binding.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_destroy_contact_binding(ulong handle, ulong binding);

    /// <summary>Writes matched sensor and per-sensor filter counts into the out parameters.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_contact_binding_spec(ulong handle, ulong binding, int* out_sensor_count, int* out_filter_count);

    /// <summary>Fills <paramref name="out_paths"/> with resolved sensor prim paths (size-then-fill).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_contact_binding_get_sensor_paths(ulong handle, ulong binding, ovphysx_string_t* out_paths, uint capacity, uint* out_count);

    /// <summary>Fills <paramref name="out_paths"/> with resolved filter prim paths (size-then-fill).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_contact_binding_get_filter_paths(ulong handle, ulong binding, ovphysx_string_t* out_paths, uint capacity, uint* out_count);

    /// <summary>Writes the binding's actual detailed-read capacity into <paramref name="out_capacity"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_contact_binding_capacity(ulong handle, ulong binding, uint* out_capacity);

    /// <summary>Reads net contact forces into <paramref name="out_tensor"/> (shape [S, 3]).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_read_contact_net_forces(ulong handle, ulong binding, DLTensor* out_tensor);

    /// <summary>Reads the contact force matrix into <paramref name="out_tensor"/> (shape [S, F, 3]).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_read_contact_force_matrix(ulong handle, ulong binding, DLTensor* out_tensor);

    /// <summary>Reads detailed per-contact data into the supplied flat tensors.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_read_contact_data(ulong handle, ulong binding, DLTensor* contact_forces, DLTensor* positions, DLTensor* normals, DLTensor* separations, DLTensor* counts, DLTensor* start_indices);

    /// <summary>Reads detailed per-anchor friction data into the supplied flat tensors.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_read_friction_data(ulong handle, ulong binding, DLTensor* friction_forces, DLTensor* friction_points, DLTensor* counts, DLTensor* start_indices);

    /// <summary>Returns borrowed per-step contact event/point/anchor buffers (anchors optional, may be null).</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_get_contact_report(ulong handle, ovphysx_contact_event_header_t** out_headers, uint* out_num_headers, ovphysx_contact_point_t** out_points, uint* out_num_points, ovphysx_friction_anchor_t** out_anchors, uint* out_num_anchors);

    // --------------------------------------------------------------------
    // PhysX object interop
    // --------------------------------------------------------------------

    /// <summary>Writes the raw PhysX SDK pointer for a prim path/type into <paramref name="out_ptr"/>.</summary>
    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial ovphysx_result_t ovphysx_get_physx_ptr(ulong handle, string prim_path, int physx_type, nint* out_ptr);

    // --------------------------------------------------------------------
    // Scene queries
    // --------------------------------------------------------------------

    /// <summary>Casts a ray; returns borrowed hit buffer in <paramref name="out_hits"/>/<paramref name="out_count"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_raycast(ulong handle, float* origin, float* direction, float distance, [MarshalAs(UnmanagedType.U1)] bool both_sides, int mode, ovphysx_scene_query_hit_t** out_hits, uint* out_count);

    /// <summary>Sweeps a geometry; returns borrowed hit buffer in <paramref name="out_hits"/>/<paramref name="out_count"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_sweep(ulong handle, ovphysx_scene_query_geometry_desc_t* geometry, float* direction, float distance, [MarshalAs(UnmanagedType.U1)] bool both_sides, int mode, ovphysx_scene_query_hit_t** out_hits, uint* out_count);

    /// <summary>Tests geometry overlap; returns borrowed hit buffer in <paramref name="out_hits"/>/<paramref name="out_count"/>.</summary>
    [LibraryImport(Lib)]
    internal static partial ovphysx_result_t ovphysx_overlap(ulong handle, ovphysx_scene_query_geometry_desc_t* geometry, int mode, ovphysx_scene_query_hit_t** out_hits, uint* out_count);
}
