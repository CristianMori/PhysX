// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

namespace Nvidia.OvPhysx.Interop;

/// <summary>Status codes returned in <c>ovphysx_result_t</c> / <c>ovphysx_enqueue_result_t</c>.</summary>
internal enum ovphysx_api_status_t : int
{
    OVPHYSX_API_SUCCESS = 0,
    OVPHYSX_API_ERROR = 1,
    OVPHYSX_API_TIMEOUT = 2,
    OVPHYSX_API_NOT_IMPLEMENTED = 3,
    OVPHYSX_API_INVALID_ARGUMENT = 4,
    OVPHYSX_API_NOT_FOUND = 5,
    OVPHYSX_API_BUFFER_TOO_SMALL = 6,
    OVPHYSX_API_DEVICE_MISMATCH = 7,
    OVPHYSX_API_GPU_NOT_AVAILABLE = 8,
}

/// <summary>Config key-type discriminator (mirrors <c>ovphysx_config_key_type_t</c>).</summary>
internal enum ovphysx_config_key_type_t : int
{
    OVPHYSX_CONFIG_KEY_TYPE_BOOL = 0,
    OVPHYSX_CONFIG_KEY_TYPE_INT32 = 1,
    OVPHYSX_CONFIG_KEY_TYPE_FLOAT = 2,
    OVPHYSX_CONFIG_KEY_TYPE_STRING = 3,
    OVPHYSX_CONFIG_KEY_TYPE_CARBONITE = 4,
}
