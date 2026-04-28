// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Runtime.InteropServices;

namespace Nvidia.OvPhysx.Interop;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void NativeLogCallback(uint level, IntPtr message, IntPtr userData);

internal static class NativeMethods
{
    private const string Lib = "ovphysx";

    // Instance management
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_create_instance(in NativeCreateArgs args, out ulong handle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_destroy_instance(ulong handle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_set_shutting_down();

    // Version
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ovphysx_get_version_string(); // returns const char*

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ovphysx_get_version(out uint major, out uint minor, out uint patch);

    // Error
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeString ovphysx_get_last_error();

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeString ovphysx_get_last_op_error(ulong opIndex);

    // USD stage
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovphysx_add_usd(ulong handle, NativeString path, NativeString prefix, out ulong usdHandle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovphysx_remove_usd(ulong handle, ulong usdHandle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovphysx_reset(ulong handle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovphysx_clone(ulong handle, NativeString sourcePath, IntPtr targetPaths, uint numTargets, IntPtr parentTransforms);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_get_stage_id(ulong handle, out long stageId);

    // Simulation
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeEnqueueResult ovphysx_step(ulong handle, float dt, float currentTime);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_step_sync(ulong handle, float dt, float currentTime);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_step_n_sync(ulong handle, int nSteps, float dt, float currentTime);

    // Wait
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_wait_op(ulong handle, ulong opIndex, ulong timeoutNs, out NativeOpWaitResult waitResult);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ovphysx_destroy_wait_result(ref NativeOpWaitResult result);

    // Tensor binding
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_create_tensor_binding(ulong handle, in NativeTensorBindingDesc desc, out ulong bindingHandle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_destroy_tensor_binding(ulong handle, ulong bindingHandle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_get_tensor_binding_spec(ulong handle, ulong bindingHandle, out NativeTensorSpec spec);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_read_tensor_binding(ulong handle, ulong bindingHandle, ref DLTensor dst);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_write_tensor_binding(ulong handle, ulong bindingHandle, ref DLTensor src, IntPtr indexTensor);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_write_tensor_binding_masked(ulong handle, ulong bindingHandle, ref DLTensor src, ref DLTensor mask);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_warmup_gpu(ulong handle);

    // Articulation metadata
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_get_articulation_metadata(ulong handle, ulong bindingHandle, out NativeArticulationMetadata metadata);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_articulation_get_dof_names(ulong handle, ulong bindingHandle, IntPtr outNames, uint maxNames, out uint outCount);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_articulation_get_body_names(ulong handle, ulong bindingHandle, IntPtr outNames, uint maxNames, out uint outCount);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_articulation_get_joint_names(ulong handle, ulong bindingHandle, IntPtr outNames, uint maxNames, out uint outCount);

    // Contact binding
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_create_contact_binding(ulong handle, IntPtr sensorPatterns, uint sensorCount, IntPtr filterPatterns, uint filtersPerSensor, uint maxContactDataCount, out ulong contactHandle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_destroy_contact_binding(ulong handle, ulong contactHandle);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_get_contact_binding_spec(ulong handle, ulong contactHandle, out int sensorCount, out int filterCount);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_read_contact_net_forces(ulong handle, ulong contactHandle, ref DLTensor dst);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_read_contact_force_matrix(ulong handle, ulong contactHandle, ref DLTensor dst);

    // Contact report (raw)
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_get_contact_report(ulong handle, out IntPtr eventHeaders, out uint numEventHeaders, out IntPtr contactData, out uint numContactData, out IntPtr frictionAnchors, out uint numFrictionAnchors);

    // Scene queries
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_raycast(ulong handle, float[] origin, float[] direction, float distance, byte bothSides, int mode, out IntPtr hits, out uint hitCount);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_overlap(ulong handle, IntPtr geometry, int mode, out IntPtr hits, out uint hitCount);

    // PhysX object interop
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_get_physx_ptr(ulong handle, IntPtr primPath, int physxType, out IntPtr outPtr);

    // Config
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_set_global_config(NativeConfigEntry entry);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_get_global_config_bool(int key, out byte value);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_get_global_config_int32(int key, out int value);

    // Logging
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_set_log_level(uint level);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ovphysx_get_log_level();

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_enable_default_log_output(byte enable);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_register_log_callback(NativeLogCallback fn, IntPtr userData);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_unregister_log_callback(NativeLogCallback fn, IntPtr userData);

    // S3/Azure
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_configure_s3(IntPtr host, IntPtr bucket, IntPtr region, IntPtr accessKeyId, IntPtr secretAccessKey, IntPtr sessionToken);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
    internal static extern NativeResult ovphysx_configure_azure_sas(IntPtr host, IntPtr container, IntPtr sasToken);
}
