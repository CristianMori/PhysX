// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

/// <summary>
/// Exception thrown when a native ovphysx call returns a non-success status.
/// Carries the <see cref="ApiStatus"/> code and, when available, the thread-local
/// error message retrieved from <c>ovphysx_get_last_error()</c>.
/// </summary>
public class OvPhysxException : Exception
{
    /// <summary>The native status code that triggered this exception.</summary>
    public ApiStatus Status { get; }

    /// <summary>The operation context (caller-supplied label), if any.</summary>
    public string? Context { get; }

    /// <summary>Creates an exception from a status code, an operation context, and the native error message.</summary>
    public OvPhysxException(ApiStatus status, string? context, string? nativeMessage)
        : base(BuildMessage(status, context, nativeMessage))
    {
        Status = status;
        Context = context;
    }

    /// <summary>Composes the exception message from the status, context, and native message.</summary>
    private static string BuildMessage(ApiStatus status, string? context, string? nativeMessage)
    {
        string prefix = string.IsNullOrEmpty(context) ? "ovphysx" : $"ovphysx {context}";
        string detail = string.IsNullOrEmpty(nativeMessage) ? $"status={status}" : nativeMessage;
        return $"{prefix}: {detail} ({status})";
    }

    /// <summary>Reads the thread-local last-error string from the native library (empty if none).</summary>
    internal static string GetLastError()
    {
        ovphysx_string_t s = NativeMethods.ovphysx_get_last_error();
        return s.ToManaged();
    }

    /// <summary>Throws an <see cref="OvPhysxException"/> if <paramref name="status"/> is not success.</summary>
    internal static void Check(ovphysx_api_status_t status, string? context = null)
    {
        if (status != ovphysx_api_status_t.OVPHYSX_API_SUCCESS)
            throw new OvPhysxException((ApiStatus)status, context, GetLastError());
    }

    /// <summary>Throws if a synchronous <c>ovphysx_result_t</c> indicates failure.</summary>
    internal static void Check(ovphysx_result_t result, string? context = null)
        => Check(result.status, context);
}

/// <summary>
/// Raised when an asynchronous operation times out (wraps <see cref="ApiStatus.Timeout"/>).
/// </summary>
public sealed class OvPhysxTimeoutException : OvPhysxException
{
    /// <summary>Creates a timeout exception for the given operation context and native message.</summary>
    public OvPhysxTimeoutException(string? context, string? nativeMessage)
        : base(ApiStatus.Timeout, context, nativeMessage)
    {
    }
}
