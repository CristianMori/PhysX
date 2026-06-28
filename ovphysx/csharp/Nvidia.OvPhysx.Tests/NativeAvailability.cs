// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx;
using Xunit;

namespace Nvidia.OvPhysx.Tests;

/// <summary>Detects whether the native ovphysx library can be loaded in this environment.</summary>
internal static class NativeAvailability
{
    private static readonly Lazy<bool> _available = new(Probe);

    public static bool Available => _available.Value;

    public static string SkipReason =>
        "Native ovphysx library not available. Set OVPHYSX_LIB to a built ovphysx.dll/libovphysx.so to run runtime tests.";

    /// <summary>Attempts to load the native library once; returns false if it cannot be loaded.</summary>
    private static bool Probe()
    {
        try
        {
            // GetVersion forces the native resolver to load the library.
            _ = OvPhysxLibrary.GetVersion();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>A <see cref="FactAttribute"/> that auto-skips when the native library is unavailable.</summary>
public sealed class RuntimeFactAttribute : FactAttribute
{
    /// <summary>Sets <see cref="FactAttribute.Skip"/> when the native library cannot be loaded.</summary>
    public RuntimeFactAttribute()
    {
        if (!NativeAvailability.Available)
            Skip = NativeAvailability.SkipReason;
    }
}
