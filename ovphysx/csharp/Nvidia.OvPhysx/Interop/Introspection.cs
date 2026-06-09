// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx.Interop;

/// <summary>Shared two-call (size-then-fill) string-list reads used by tensor/contact bindings.</summary>
internal static unsafe class Introspection
{
    /// <summary>
    /// Invokes a native <c>(handle, binding, ovphysx_string_t* buffer, capacity, out count)</c>
    /// function using the two-call pattern: probe for the required count, then fill a sized buffer.
    /// </summary>
    public static string[] GetStrings(
        ulong handle, ulong binding,
        delegate*<ulong, ulong, ovphysx_string_t*, uint, uint*, ovphysx_result_t> fn, string ctx)
    {
        uint count;
        ovphysx_result_t probe = fn(handle, binding, null, 0, &count);
        if (probe.status is not (ovphysx_api_status_t.OVPHYSX_API_SUCCESS or ovphysx_api_status_t.OVPHYSX_API_BUFFER_TOO_SMALL))
            OvPhysxException.Check(probe, ctx);
        if (count == 0)
            return [];

        using var buffer = new NativeStringBuffer(count);
        uint filled;
        OvPhysxException.Check(fn(handle, binding, buffer.Ptr, count, &filled), ctx);
        return buffer.Decode(filled);
    }
}
