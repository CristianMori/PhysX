// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Runtime.InteropServices;
using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

/// <summary>Configuration for creating a PhysX instance.</summary>
public sealed record PhysXConfig
{
    public DeviceType Device { get; init; } = DeviceType.Auto;
    public int GpuIndex { get; init; } = 0;
    public bool? DisableContactProcessing { get; init; }
    public int? NumThreads { get; init; }
    public int? SceneMultiGpuMode { get; init; }
}

/// <summary>
/// Main ovPhysX simulation instance. Create, load USD scenes, step the simulation,
/// and exchange data via tensor bindings.
/// </summary>
public sealed class PhysX : IDisposable
{
    private ulong _handle;
    private bool _disposed;

    internal ulong Handle
    {
        get { ObjectDisposedException.ThrowIf(_disposed, this); return _handle; }
    }

    public PhysX(PhysXConfig? config = null)
    {
        config ??= new PhysXConfig();
        var args = new NativeCreateArgs
        {
            Device = (int)config.Device,
            GpuIndex = config.GpuIndex,
        };

        var result = NativeMethods.ovphysx_create_instance(in args, out _handle);
        OvPhysxException.ThrowIfFailed(result, "create PhysX instance");

        // Apply config entries
        if (config.DisableContactProcessing.HasValue)
            SetConfigBool(ConfigBoolKey.DisableContactProcessing, config.DisableContactProcessing.Value);
        if (config.NumThreads.HasValue)
            SetConfigInt32(ConfigInt32Key.NumThreads, config.NumThreads.Value);
        if (config.SceneMultiGpuMode.HasValue)
            SetConfigInt32(ConfigInt32Key.SceneMultiGpuMode, config.SceneMultiGpuMode.Value);
    }

    // ========================================================================
    // Version
    // ========================================================================

    public static (uint Major, uint Minor, uint Patch) Version
    {
        get { NativeMethods.ovphysx_get_version(out var ma, out var mi, out var pa); return (ma, mi, pa); }
    }

    public static string VersionString => Marshal.PtrToStringUTF8(NativeMethods.ovphysx_get_version_string()) ?? "";

    // ========================================================================
    // USD Stage
    // ========================================================================

    /// <summary>Load a USD file into the simulation (async). Returns (usdHandle, opIndex).</summary>
    public (ulong UsdHandle, ulong OpIndex) AddUsd(string usdPath, string pathPrefix = "")
    {
        using var pathCtx = new NativeStringContext(usdPath);
        using var prefixCtx = new NativeStringContext(pathPrefix);
        var result = NativeMethods.ovphysx_add_usd(Handle, pathCtx.Value, prefixCtx.Value, out var usdHandle);
        OvPhysxException.ThrowIfFailed(result, "add USD");
        return (usdHandle, result.OpIndex);
    }

    /// <summary>Remove a previously loaded USD file (async).</summary>
    public ulong RemoveUsd(ulong usdHandle)
    {
        var result = NativeMethods.ovphysx_remove_usd(Handle, usdHandle);
        OvPhysxException.ThrowIfFailed(result, "remove USD");
        return result.OpIndex;
    }

    /// <summary>Clear the stage (async).</summary>
    public ulong Reset()
    {
        var result = NativeMethods.ovphysx_reset(Handle);
        OvPhysxException.ThrowIfFailed(result, "reset");
        return result.OpIndex;
    }

    /// <summary>Clone a USD subtree to multiple target paths (async).</summary>
    public ulong Clone(string sourcePath, string[] targetPaths, float[]? parentTransforms = null)
    {
        using var sourceCtx = new NativeStringContext(sourcePath);
        using var targetsCtx = new NativeStringArrayContext(targetPaths);

        IntPtr transformsPtr = IntPtr.Zero;
        GCHandle transformsHandle = default;
        if (parentTransforms != null)
        {
            transformsHandle = GCHandle.Alloc(parentTransforms, GCHandleType.Pinned);
            transformsPtr = transformsHandle.AddrOfPinnedObject();
        }

        try
        {
            var result = NativeMethods.ovphysx_clone(Handle, sourceCtx.Value, targetsCtx.Pointer, targetsCtx.Count, transformsPtr);
            OvPhysxException.ThrowIfFailed(result, "clone");
            return result.OpIndex;
        }
        finally
        {
            if (transformsHandle.IsAllocated) transformsHandle.Free();
        }
    }

    public long GetStageId()
    {
        var result = NativeMethods.ovphysx_get_stage_id(Handle, out var stageId);
        OvPhysxException.ThrowIfFailed(result, "get stage ID");
        return stageId;
    }

    // ========================================================================
    // Simulation
    // ========================================================================

    /// <summary>Enqueue an async simulation step. Returns the operation index.</summary>
    public ulong Step(float dt, float currentTime)
    {
        var result = NativeMethods.ovphysx_step(Handle, dt, currentTime);
        OvPhysxException.ThrowIfFailed(result, "step");
        return result.OpIndex;
    }

    /// <summary>Run a single synchronous simulation step (blocks until complete).</summary>
    public void StepSync(float dt, float currentTime)
    {
        var result = NativeMethods.ovphysx_step_sync(Handle, dt, currentTime);
        OvPhysxException.ThrowIfFailed(result, "step sync");
    }

    /// <summary>Run N consecutive synchronous simulation steps.</summary>
    public void StepNSync(int nSteps, float dt, float currentTime)
    {
        var result = NativeMethods.ovphysx_step_n_sync(Handle, nSteps, dt, currentTime);
        OvPhysxException.ThrowIfFailed(result, "step N sync");
    }

    // ========================================================================
    // Wait
    // ========================================================================

    /// <summary>Wait for a specific operation to complete.</summary>
    public void WaitOp(ulong opIndex, ulong timeoutNs = ulong.MaxValue)
    {
        var result = NativeMethods.ovphysx_wait_op(Handle, opIndex, timeoutNs, out var waitResult);
        try
        {
            if (result.Status == (int)ApiStatus.Timeout)
                throw new OvPhysxException("Operation timed out", ApiStatus.Timeout);
            OvPhysxException.ThrowIfFailed(result, "wait op");
            if (waitResult.NumErrors > 0)
            {
                unsafe
                {
                    var opId = *(ulong*)waitResult.ErrorOpIndices;
                    var err = NativeMethods.ovphysx_get_last_op_error(opId);
                    throw new OvPhysxException($"Operation {opId} failed: {err.ToManaged() ?? "Unknown"}");
                }
            }
        }
        finally
        {
            NativeMethods.ovphysx_destroy_wait_result(ref waitResult);
        }
    }

    /// <summary>Wait for all pending operations.</summary>
    public void WaitAll(ulong timeoutNs = ulong.MaxValue) => WaitOp(0xFFFFFFFFFFFFFFFF, timeoutNs);

    // ========================================================================
    // Tensor Bindings
    // ========================================================================

    /// <summary>Create a tensor binding by pattern or explicit prim paths.</summary>
    public TensorBinding CreateTensorBinding(TensorType tensorType, string? pattern = null, string[]? primPaths = null)
    {
        using var patternCtx = new NativeStringContext(pattern);
        using var primsCtx = primPaths != null ? new NativeStringArrayContext(primPaths) : null;

        var desc = new NativeTensorBindingDesc
        {
            Pattern = patternCtx.Value,
            PrimPaths = primsCtx?.Pointer ?? IntPtr.Zero,
            PrimPathsCount = primsCtx?.Count ?? 0,
            TensorType = (int)tensorType,
        };

        var result = NativeMethods.ovphysx_create_tensor_binding(Handle, in desc, out var bindingHandle);
        OvPhysxException.ThrowIfFailed(result, "create tensor binding");

        var specResult = NativeMethods.ovphysx_get_tensor_binding_spec(Handle, bindingHandle, out var spec);
        OvPhysxException.ThrowIfFailed(specResult, "get tensor binding spec");

        return new TensorBinding(Handle, bindingHandle, spec);
    }

    /// <summary>Warm up GPU tensor pipeline (optional, auto-triggered on first read).</summary>
    public void WarmupGpu()
    {
        var result = NativeMethods.ovphysx_warmup_gpu(Handle);
        OvPhysxException.ThrowIfFailed(result, "warmup GPU");
    }

    // ========================================================================
    // Contact Bindings
    // ========================================================================

    /// <summary>Create a contact binding. Must be called before the first step.</summary>
    public ContactBinding CreateContactBinding(
        string[] sensorPatterns,
        string[]? filterPatterns = null,
        uint filtersPerSensor = 0,
        uint maxContactDataCount = 256)
    {
        using var sensorsCtx = new NativeStringArrayContext(sensorPatterns);
        using var filtersCtx = filterPatterns != null ? new NativeStringArrayContext(filterPatterns) : null;

        var result = NativeMethods.ovphysx_create_contact_binding(
            Handle, sensorsCtx.Pointer, sensorsCtx.Count,
            filtersCtx?.Pointer ?? IntPtr.Zero, filtersPerSensor,
            maxContactDataCount, out var contactHandle);
        OvPhysxException.ThrowIfFailed(result, "create contact binding");

        var specResult = NativeMethods.ovphysx_get_contact_binding_spec(
            Handle, contactHandle, out var sensorCount, out var filterCount);
        OvPhysxException.ThrowIfFailed(specResult, "get contact binding spec");

        return new ContactBinding(Handle, contactHandle, sensorCount, filterCount);
    }

    // ========================================================================
    // Scene Queries
    // ========================================================================

    /// <summary>Cast a ray and return hit results.</summary>
    public unsafe SceneQueryHit[] Raycast(float[] origin, float[] direction, float distance,
        bool bothSides = false, SceneQueryMode mode = SceneQueryMode.Closest)
    {
        var result = NativeMethods.ovphysx_raycast(Handle, origin, direction, distance,
            bothSides ? (byte)1 : (byte)0, (int)mode, out var hitsPtr, out var hitCount);
        OvPhysxException.ThrowIfFailed(result, "raycast");

        var hits = new SceneQueryHit[hitCount];
        for (uint i = 0; i < hitCount; i++)
        {
            var native = Marshal.PtrToStructure<NativeSceneQueryHit>(hitsPtr + (int)(i * (uint)Marshal.SizeOf<NativeSceneQueryHit>()));
            hits[i] = new SceneQueryHit(native.Collision, native.RigidBody, native.ProtoIndex,
                native.Distance, native.FaceIndex, native.Material);
        }
        return hits;
    }

    // ========================================================================
    // PhysX Object Interop
    // ========================================================================

    /// <summary>Get a raw PhysX C++ pointer for a prim path.</summary>
    public IntPtr GetPhysXPtr(string primPath, PhysXObjectType type)
    {
        using var pathCtx = new NativeStringContext(primPath);
        var result = NativeMethods.ovphysx_get_physx_ptr(Handle, pathCtx.Value.Ptr, (int)type, out var ptr);
        OvPhysxException.ThrowIfFailed(result, "get PhysX ptr");
        return ptr;
    }

    // ========================================================================
    // Config
    // ========================================================================

    public void SetConfigBool(ConfigBoolKey key, bool value)
    {
        var entry = NativeConfigEntry.Bool(0, (int)key, value);
        OvPhysxException.ThrowIfFailed(NativeMethods.ovphysx_set_global_config(entry), "set config bool");
    }

    public void SetConfigInt32(ConfigInt32Key key, int value)
    {
        var entry = NativeConfigEntry.Int32(1, (int)key, value);
        OvPhysxException.ThrowIfFailed(NativeMethods.ovphysx_set_global_config(entry), "set config int32");
    }

    public bool GetConfigBool(ConfigBoolKey key)
    {
        OvPhysxException.ThrowIfFailed(NativeMethods.ovphysx_get_global_config_bool((int)key, out var val), "get config bool");
        return val != 0;
    }

    public int GetConfigInt32(ConfigInt32Key key)
    {
        OvPhysxException.ThrowIfFailed(NativeMethods.ovphysx_get_global_config_int32((int)key, out var val), "get config int32");
        return val;
    }

    // ========================================================================
    // Logging
    // ========================================================================

    public static void SetLogLevel(LogLevel level) =>
        OvPhysxException.ThrowIfFailed(NativeMethods.ovphysx_set_log_level((uint)level), "set log level");

    public static LogLevel GetLogLevel() => (LogLevel)NativeMethods.ovphysx_get_log_level();

    public static void EnableDefaultLogOutput(bool enable) =>
        OvPhysxException.ThrowIfFailed(NativeMethods.ovphysx_enable_default_log_output(enable ? (byte)1 : (byte)0), "enable default log output");

    // ========================================================================
    // Error
    // ========================================================================

    public static string GetLastError() => NativeMethods.ovphysx_get_last_error().ToManaged() ?? "";

    // ========================================================================
    // Dispose
    // ========================================================================

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_handle != 0)
        {
            NativeMethods.ovphysx_destroy_instance(_handle);
            _handle = 0;
        }
    }

    ~PhysX() { Dispose(); }
}

public record SceneQueryHit(ulong Collision, ulong RigidBody, uint ProtoIndex, float Distance, uint FaceIndex, ulong Material);
