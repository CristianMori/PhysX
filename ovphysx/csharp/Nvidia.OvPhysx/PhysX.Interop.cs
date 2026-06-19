// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

public sealed partial class PhysX
{
    /// <summary>
    /// Returns the raw PhysX SDK object pointer for a prim path and object type, for advanced
    /// C++ interop. Returns <see cref="nint.Zero"/> when no object of that type exists at the path.
    /// </summary>
    public unsafe nint GetPhysxPtr(string primPath, PhysXType physxType)
    {
        EnsureValid();
        ArgumentException.ThrowIfNullOrEmpty(primPath);
        nint ptr;
        OvPhysxException.Check(NativeMethods.ovphysx_get_physx_ptr(_handle, primPath, (int)physxType, &ptr), "get_physx_ptr");
        return ptr;
    }
}
