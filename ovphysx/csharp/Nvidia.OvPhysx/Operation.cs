// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx;

/// <summary>
/// A handle to an asynchronous (enqueued) ovphysx operation such as a step, USD load, reset,
/// or clone. Operations execute in submission (stream) order; results are visible to subsequent
/// stream operations automatically. Call <see cref="Wait"/> only when you need to observe results
/// outside the stream (e.g. from the CPU before the next ovphysx call).
/// </summary>
public readonly struct Operation
{
    /// <summary>Sentinel op index meaning "all operations submitted so far" (<c>OVPHYSX_OP_INDEX_ALL</c>).</summary>
    internal const ulong AllIndex = 0xFFFFFFFFFFFFFFFF;

    private readonly PhysX? _physx;

    /// <summary>The native operation index used for waiting.</summary>
    public ulong Index { get; }

    internal Operation(PhysX physx, ulong index)
    {
        _physx = physx;
        Index = index;
    }

    /// <summary>
    /// Blocks until this operation completes.
    /// </summary>
    /// <param name="timeout">
    /// Maximum time to wait. <see langword="null"/> waits indefinitely; <see cref="TimeSpan.Zero"/>
    /// polls without blocking. Throws <see cref="OvPhysxTimeoutException"/> if the timeout elapses,
    /// or <see cref="OvPhysxException"/> if the operation failed.
    /// </param>
    public void Wait(TimeSpan? timeout = null)
    {
        if (_physx is null)
            throw new InvalidOperationException("Operation was not associated with a PhysX instance.");
        _physx.WaitOp(Index, timeout);
    }
}
