// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx;

/// <summary>
/// Opaque handle to a USD layer added via <see cref="PhysX.AddUsd"/>. The underlying
/// ovphysx handle model uses <see cref="ulong"/> values (not pointers); <c>0</c> is invalid.
/// </summary>
public readonly record struct UsdHandle(ulong Value)
{
    /// <summary>The invalid/sentinel handle (<c>OVPHYSX_INVALID_HANDLE</c>).</summary>
    public static UsdHandle Invalid => new(0);

    /// <summary>True when this is a usable (non-zero) handle.</summary>
    public bool IsValid => Value != 0;
}
