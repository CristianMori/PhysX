// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx.Interop;

/// <summary>Shared size-then-fill string-list reads used by tensor/contact bindings.</summary>
internal static unsafe class Introspection
{
    /// <summary>
    /// Invokes a native <c>(handle, binding, ovphysx_string_t* buffer, capacity, out count)</c>
    /// function. The native API requires a non-null buffer, so this allocates one sized to
    /// <paramref name="capacityHint"/> (the known element count) and grows it if the call reports
    /// <see cref="ovphysx_api_status_t.OVPHYSX_API_BUFFER_TOO_SMALL"/>.
    /// </summary>
    public static string[] GetStrings(
        ulong handle, ulong binding,
        delegate*<ulong, ulong, ovphysx_string_t*, uint, uint*, ovphysx_result_t> fn,
        string ctx, uint capacityHint)
    {
        uint capacity = Math.Max(1u, capacityHint);
        while (true)
        {
            var buffer = new NativeStringBuffer(capacity);
            try
            {
                uint count;
                ovphysx_result_t r = fn(handle, binding, buffer.Ptr, capacity, &count);
                if (r.status == ovphysx_api_status_t.OVPHYSX_API_BUFFER_TOO_SMALL && count > capacity)
                {
                    capacity = count;
                    continue; // retry with the required capacity
                }
                OvPhysxException.Check(r, ctx);
                return buffer.Decode(Math.Min(count, capacity));
            }
            finally
            {
                buffer.Dispose();
            }
        }
    }
}
