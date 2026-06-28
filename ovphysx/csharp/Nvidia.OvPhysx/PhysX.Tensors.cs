// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

public sealed partial class PhysX
{
    /// <summary>
    /// Creates a tensor binding for all prims matching a USD glob <paramref name="pattern"/>
    /// (e.g. <c>"/World/robot*"</c>).
    /// </summary>
    /// <param name="raiseIfEmpty">When true, throws if the pattern matches no prims.</param>
    public unsafe TensorBinding CreateTensorBinding(TensorType tensorType, string pattern, bool raiseIfEmpty = false)
    {
        EnsureValid();
        ArgumentException.ThrowIfNullOrEmpty(pattern);

        using var pat = new NativeStringArg(pattern);
        var desc = new ovphysx_tensor_binding_desc_t
        {
            pattern = pat.Value,
            prim_paths = null,
            prim_paths_count = 0,
            tensor_type = (int)tensorType,
        };
        return CreateBinding(&desc, tensorType, raiseIfEmpty);
    }

    /// <summary>
    /// Creates a tensor binding for an explicit list of exact prim paths (in row order).
    /// </summary>
    /// <param name="raiseIfEmpty">When true, throws if none of the paths resolve.</param>
    public unsafe TensorBinding CreateTensorBinding(TensorType tensorType, IReadOnlyList<string> primPaths, bool raiseIfEmpty = false)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(primPaths);
        if (primPaths.Count == 0)
            throw new ArgumentException("At least one prim path is required.", nameof(primPaths));

        using var paths = new NativeStringArray(primPaths);
        var desc = new ovphysx_tensor_binding_desc_t
        {
            pattern = default,
            prim_paths = paths.Ptr,
            prim_paths_count = paths.Count,
            tensor_type = (int)tensorType,
        };
        return CreateBinding(&desc, tensorType, raiseIfEmpty);
    }

    /// <summary>Creates the native binding, queries its spec/shape, and wraps it in a <see cref="TensorBinding"/>.</summary>
    private unsafe TensorBinding CreateBinding(ovphysx_tensor_binding_desc_t* desc, TensorType type, bool raiseIfEmpty)
    {
        ulong binding;
        OvPhysxException.Check(NativeMethods.ovphysx_create_tensor_binding(_handle, desc, &binding), "create_tensor_binding");

        ovphysx_tensor_spec_t spec;
        OvPhysxException.Check(NativeMethods.ovphysx_get_tensor_binding_spec(_handle, binding, &spec), "get_tensor_binding_spec");

        int ndim = spec.ndim < 0 ? 0 : Math.Min(spec.ndim, 4);
        var shape = new long[ndim];
        for (int i = 0; i < ndim; i++)
            shape[i] = spec.shape[i];

        var tb = new TensorBinding(this, binding, type, shape);
        if (raiseIfEmpty && tb.Count == 0)
        {
            tb.Destroy();
            throw new OvPhysxException(ApiStatus.NotFound, "create_tensor_binding", $"No prims matched for tensor type {type}.");
        }
        return tb;
    }
}
