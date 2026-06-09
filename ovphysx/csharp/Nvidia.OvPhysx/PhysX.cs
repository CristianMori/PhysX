// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

/// <summary>
/// Managed wrapper around an ovphysx instance: USD scene management, simulation stepping,
/// stage attachment, typed configuration, tensor/contact bindings, and scene queries.
/// </summary>
/// <remarks>
/// Not thread-safe. Dispose (or call <see cref="Release"/>) to free native resources;
/// the instance also supports <c>using</c> via <see cref="IDisposable"/>.
/// All instances in a process must use the same <see cref="DeviceType"/>.
/// </remarks>
public sealed partial class PhysX : IDisposable
{
    private ulong _handle;

    /// <summary>The raw native <c>ovphysx_handle_t</c>, for C/C++ interop. Zero once released.</summary>
    public ulong Handle => _handle;

    /// <summary>True if the underlying native instance is still alive.</summary>
    public bool IsValid => _handle != 0;

    /// <summary>
    /// Creates a new ovphysx instance.
    /// </summary>
    /// <param name="config">Typed configuration; <see langword="null"/> keeps all defaults.</param>
    /// <param name="device">Simulation device. Must match any other instance in the process.</param>
    /// <param name="activeCudaGpus">
    /// Comma-separated CUDA device ordinals used when <paramref name="device"/> selects GPU.
    /// <see langword="null"/> or empty selects GPU 0.
    /// </param>
    public unsafe PhysX(PhysXConfig? config = null, DeviceType device = DeviceType.Auto, string? activeCudaGpus = null)
    {
        NativeLibraryResolver.Register();

        using var cfg = new NativeConfigEntries(config);
        using var deps = new NativeStringArg(string.Empty);
        using var gpus = new NativeStringArg(activeCudaGpus ?? string.Empty);

        var args = new ovphysx_create_args
        {
            bundled_deps_path = deps.Value,
            config_entries = cfg.Entries,
            config_entry_count = cfg.Count,
            device = (int)device,
            active_cuda_gpus = gpus.Value,
        };

        ulong handle;
        ovphysx_result_t result = NativeMethods.ovphysx_create_instance(&args, &handle);
        OvPhysxException.Check(result, "create_instance");
        _handle = handle;
    }

    // --------------------------------------------------------------------
    // USD scene management (async)
    // --------------------------------------------------------------------

    /// <summary>
    /// Adds a USD file to the simulation stage (asynchronous).
    /// </summary>
    /// <returns>The new <see cref="UsdHandle"/> and the <see cref="Operation"/> tracking the load.</returns>
    public unsafe (UsdHandle Handle, Operation Operation) AddUsd(string usdPath, string pathPrefix = "")
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(usdPath);

        using var path = new NativeStringArg(usdPath);
        using var prefix = new NativeStringArg(pathPrefix);

        ulong usd;
        ovphysx_enqueue_result_t r = NativeMethods.ovphysx_add_usd(_handle, path.Value, prefix.Value, &usd);
        OvPhysxException.Check(r.status, "add_usd");
        return (new UsdHandle(usd), new Operation(this, r.op_index));
    }

    /// <summary>Removes a previously-added USD layer (asynchronous).</summary>
    public Operation RemoveUsd(UsdHandle usdHandle)
    {
        EnsureValid();
        ovphysx_enqueue_result_t r = NativeMethods.ovphysx_remove_usd(_handle, usdHandle.Value);
        OvPhysxException.Check(r.status, "remove_usd");
        return new Operation(this, r.op_index);
    }

    /// <summary>Resets the stage to empty (asynchronous).</summary>
    public Operation Reset()
    {
        EnsureValid();
        ovphysx_enqueue_result_t r = NativeMethods.ovphysx_reset(_handle);
        OvPhysxException.Check(r.status, "reset");
        return new Operation(this, r.op_index);
    }

    /// <summary>
    /// Clones a USD prim hierarchy to one or more target paths (asynchronous).
    /// </summary>
    /// <param name="parentTransforms">
    /// Optional flat array of 7 floats per target — <c>(px, py, pz, qx, qy, qz, qw)</c> world poses.
    /// When provided, its length must equal <c>targetPaths.Count * 7</c>.
    /// </param>
    public unsafe Operation Clone(string sourcePath, IReadOnlyList<string> targetPaths, float[]? parentTransforms = null)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(sourcePath);
        ArgumentNullException.ThrowIfNull(targetPaths);
        if (targetPaths.Count == 0)
            throw new ArgumentException("At least one target path is required.", nameof(targetPaths));
        if (parentTransforms is not null && parentTransforms.Length != targetPaths.Count * 7)
            throw new ArgumentException(
                $"parentTransforms must contain 7 floats per target ({targetPaths.Count * 7} total), got {parentTransforms.Length}.",
                nameof(parentTransforms));

        using var source = new NativeStringArg(sourcePath);
        using var targets = new NativeStringArray(targetPaths);

        fixed (float* xf = parentTransforms)
        {
            ovphysx_enqueue_result_t r = NativeMethods.ovphysx_clone(
                _handle, source.Value, targets.Ptr, targets.Count, xf);
            OvPhysxException.Check(r.status, "clone");
            return new Operation(this, r.op_index);
        }
    }

    // --------------------------------------------------------------------
    // Stepping
    // --------------------------------------------------------------------

    /// <summary>Enqueues a single physics step (asynchronous).</summary>
    public Operation Step(float dt, float simTime)
    {
        EnsureValid();
        ovphysx_enqueue_result_t r = NativeMethods.ovphysx_step(_handle, dt, simTime);
        OvPhysxException.Check(r.status, "step");
        return new Operation(this, r.op_index);
    }

    /// <summary>Steps once and blocks until the step completes.</summary>
    public void StepSync(float dt, float simTime)
    {
        EnsureValid();
        OvPhysxException.Check(NativeMethods.ovphysx_step_sync(_handle, dt, simTime), "step_sync");
    }

    /// <summary>Runs <paramref name="n"/> steps in a single native call and blocks until done.</summary>
    public void StepNSync(int n, float dt, float currentTime)
    {
        EnsureValid();
        OvPhysxException.Check(NativeMethods.ovphysx_step_n_sync(_handle, n, dt, currentTime), "step_n_sync");
    }

    /// <summary>Updates articulation link poses from joint positions without stepping (synchronous).</summary>
    public void UpdateArticulationsKinematic()
    {
        EnsureValid();
        OvPhysxException.Check(NativeMethods.ovphysx_update_articulations_kinematic(_handle), "update_articulations_kinematic");
    }

    /// <summary>Eagerly initializes GPU buffers to avoid first-read latency in GPU mode (idempotent).</summary>
    public void WarmupGpu()
    {
        EnsureValid();
        OvPhysxException.Check(NativeMethods.ovphysx_warmup_gpu(_handle), "warmup_gpu");
    }

    // --------------------------------------------------------------------
    // Operation waiting
    // --------------------------------------------------------------------

    /// <summary>Waits for all operations submitted so far to complete.</summary>
    public void WaitAll(TimeSpan? timeout = null) => WaitOp(Operation.AllIndex, timeout);

    /// <summary>Waits for a single operation index (used by <see cref="Operation.Wait"/>).</summary>
    internal unsafe void WaitOp(ulong opIndex, TimeSpan? timeout)
    {
        EnsureValid();
        ulong timeoutNs = ToNanos(timeout);

        ovphysx_op_wait_result_t wait = default;
        ovphysx_result_t status = NativeMethods.ovphysx_wait_op(_handle, opIndex, timeoutNs, &wait);

        bool hasErrors = wait.num_errors != 0;
        ulong firstErrorOp = hasErrors && wait.error_op_indices != null ? wait.error_op_indices[0] : 0;
        NativeMethods.ovphysx_destroy_wait_result(&wait);

        if (status.status == ovphysx_api_status_t.OVPHYSX_API_TIMEOUT)
            throw new OvPhysxTimeoutException("wait_op", OvPhysxException.GetLastError());
        OvPhysxException.Check(status, "wait_op");

        if (hasErrors)
        {
            string msg = NativeMethods.ovphysx_get_last_op_error(firstErrorOp).ToManaged();
            throw new OvPhysxException(ApiStatus.Error, "wait_op", string.IsNullOrEmpty(msg) ? "asynchronous operation failed" : msg);
        }
    }

    private static ulong ToNanos(TimeSpan? timeout)
    {
        if (timeout is null)
            return ulong.MaxValue; // infinite
        TimeSpan t = timeout.Value;
        if (t <= TimeSpan.Zero)
            return 0; // poll
        double ns = t.Ticks * 100.0;
        return ns >= ulong.MaxValue ? ulong.MaxValue : (ulong)ns;
    }

    // --------------------------------------------------------------------
    // Stage attachment / introspection
    // --------------------------------------------------------------------

    /// <summary>Returns the attached USD stage id. Throws if no stage is attached.</summary>
    public unsafe long GetStageId()
    {
        EnsureValid();
        long id;
        OvPhysxException.Check(NativeMethods.ovphysx_get_stage_id(_handle, &id), "get_stage_id");
        return id;
    }

    /// <summary>Attaches an external stage (raw ovstage handle) as the orchestration surface.</summary>
    public void AttachStage(nint stage)
    {
        EnsureValid();
        OvPhysxException.Check(NativeMethods.ovphysx_attach_stage(_handle, stage), "attach_stage");
    }

    /// <summary>Detaches the currently-attached stage (idempotent on the native side).</summary>
    public void DetachStage()
    {
        EnsureValid();
        OvPhysxException.Check(NativeMethods.ovphysx_detach_stage(_handle), "detach_stage");
    }

    // --------------------------------------------------------------------
    // Typed config (process-global)
    // --------------------------------------------------------------------

    /// <summary>Sets a boolean process-global config value.</summary>
    public void SetConfig(ConfigBool key, bool value)
    {
        var entry = new ovphysx_config_entry_t
        {
            key_type = ovphysx_config_key_type_t.OVPHYSX_CONFIG_KEY_TYPE_BOOL,
            bool_key = (int)key,
            bool_value = (byte)(value ? 1 : 0),
        };
        OvPhysxException.Check(NativeMethods.ovphysx_set_global_config(entry), "set_global_config");
    }

    /// <summary>Sets an int32 process-global config value.</summary>
    public void SetConfig(ConfigInt32 key, int value)
    {
        var entry = new ovphysx_config_entry_t
        {
            key_type = ovphysx_config_key_type_t.OVPHYSX_CONFIG_KEY_TYPE_INT32,
            int32_key = (int)key,
            int32_value = value,
        };
        OvPhysxException.Check(NativeMethods.ovphysx_set_global_config(entry), "set_global_config");
    }

    /// <summary>Sets a string process-global config value.</summary>
    public void SetConfig(ConfigString key, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var arg = new NativeStringArg(value);
        var entry = new ovphysx_config_entry_t
        {
            key_type = ovphysx_config_key_type_t.OVPHYSX_CONFIG_KEY_TYPE_STRING,
            string_key = (int)key,
            string_value = arg.Value,
        };
        OvPhysxException.Check(NativeMethods.ovphysx_set_global_config(entry), "set_global_config");
    }

    /// <summary>Reads a boolean process-global config value.</summary>
    public unsafe bool GetConfigBool(ConfigBool key)
    {
        byte value;
        OvPhysxException.Check(NativeMethods.ovphysx_get_global_config_bool((int)key, &value), "get_global_config_bool");
        return value != 0;
    }

    /// <summary>Reads an int32 process-global config value.</summary>
    public unsafe int GetConfigInt32(ConfigInt32 key)
    {
        int value;
        OvPhysxException.Check(NativeMethods.ovphysx_get_global_config_int32((int)key, &value), "get_global_config_int32");
        return value;
    }

    /// <summary>Reads a float process-global config value by raw key index.</summary>
    public unsafe float GetConfigFloat(int key)
    {
        float value;
        OvPhysxException.Check(NativeMethods.ovphysx_get_global_config_float(key, &value), "get_global_config_float");
        return value;
    }

    /// <summary>Reads a string process-global config value; returns <see langword="null"/> if unset.</summary>
    public unsafe string? GetConfigString(ConfigString key)
    {
        ovphysx_string_t s = default;
        nuint size;
        OvPhysxException.Check(NativeMethods.ovphysx_get_global_config_string((int)key, &s, &size), "get_global_config_string");
        string result = s.ToManaged();
        return result.Length == 0 ? null : result;
    }

    // --------------------------------------------------------------------
    // Lifecycle
    // --------------------------------------------------------------------

    /// <summary>Releases the native instance. Safe to call multiple times.</summary>
    public void Release()
    {
        if (_handle != 0)
        {
            _ = NativeMethods.ovphysx_destroy_instance(_handle);
            _handle = 0;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~PhysX() => Release();

    private void EnsureValid()
    {
        if (_handle == 0)
            throw new ObjectDisposedException(nameof(PhysX), "The PhysX instance has been released.");
    }
}
