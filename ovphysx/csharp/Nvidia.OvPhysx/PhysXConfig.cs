// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx;

/// <summary>
/// Typed configuration applied at <see cref="PhysX"/> construction. Mirrors the Python
/// <c>PhysXConfig</c> dataclass: every field defaults to <see langword="null"/> (= keep the
/// Carbonite/PhysX default); only non-null fields are sent to the native API.
/// </summary>
/// <remarks>
/// OmniPVD-related fields must be set before instance creation to take effect.
/// </remarks>
public sealed class PhysXConfig
{
    /// <summary><c>/physics/disableContactProcessing</c></summary>
    public bool? DisableContactProcessing { get; set; }

    /// <summary><c>/physics/collisionConeCustomGeometry</c></summary>
    public bool? CollisionConeCustomGeometry { get; set; }

    /// <summary><c>/physics/collisionCylinderCustomGeometry</c></summary>
    public bool? CollisionCylinderCustomGeometry { get; set; }

    /// <summary><c>/physics/numThreads</c></summary>
    public int? NumThreads { get; set; }

    /// <summary><c>/physics/sceneMultiGPUMode</c> (0=disabled, 1=all GPUs, 2=skip first GPU).</summary>
    public int? SceneMultiGpuMode { get; set; }

    /// <summary><c>/physics/omniPvdOutputEnabled</c> (must be set before instance creation).</summary>
    public bool? OmniPvdOutputEnabled { get; set; }

    /// <summary><c>/persistent/physics/omniPvdOvdRecordingDirectory</c> (must be set before instance creation).</summary>
    public string? OmniPvdOvdRecordingDirectory { get; set; }

    /// <summary>
    /// Escape hatch: arbitrary Carbonite settings paths to override. Values may be
    /// <see cref="bool"/>, <see cref="int"/>, <see cref="float"/>/<see cref="double"/>, or
    /// <see cref="string"/>. Keys that collide with a typed field above are rejected.
    /// </summary>
    public IDictionary<string, object>? CarboniteOverrides { get; set; }
}
