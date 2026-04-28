// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

public class OvPhysxException : Exception
{
    public ApiStatus Status { get; }

    public OvPhysxException(string message, ApiStatus status = ApiStatus.Error) : base(message)
    {
        Status = status;
    }

    internal static OvPhysxException FromLastError(string operation)
    {
        var err = NativeMethods.ovphysx_get_last_error();
        return new OvPhysxException($"Failed to {operation}: {err.ToManaged() ?? "Unknown error"}");
    }

    internal static void ThrowIfFailed(NativeResult result, string operation)
    {
        if (result.IsError)
            throw FromLastError(operation);
    }

    internal static void ThrowIfFailed(NativeEnqueueResult result, string operation)
    {
        if (!result.IsSuccess)
            throw FromLastError(operation);
    }
}
