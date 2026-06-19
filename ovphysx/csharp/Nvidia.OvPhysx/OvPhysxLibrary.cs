// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using System.Runtime.InteropServices;
using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

/// <summary>
/// Process-global entry points for the native ovphysx library: version query, explicit
/// native-load bootstrap, and remote-storage credential configuration.
/// </summary>
public static class OvPhysxLibrary
{
    /// <summary>
    /// Forces the native library to load now (resolving <c>OVPHYSX_LIB</c> / bundled paths)
    /// and returns its version string. Throws if the native library cannot be located.
    /// </summary>
    public static void Bootstrap() => _ = GetVersion();

    /// <summary>
    /// Returns the native library version string (e.g. <c>"0.4.13"</c>), or an empty string
    /// if the library exposes no version symbol.
    /// </summary>
    public static string GetVersion()
    {
        NativeLibraryResolver.Register();
        nint ptr = NativeMethods.ovphysx_get_version_string();
        return ptr == nint.Zero ? string.Empty : Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
    }

    /// <summary>
    /// Configures S3 credentials for loading USD assets from S3-backed URLs (process-global).
    /// </summary>
    /// <param name="sessionToken">Optional temporary-credential session token (may be null).</param>
    public static void ConfigureS3(
        string host,
        string bucket,
        string region,
        string accessKeyId,
        string secretAccessKey,
        string? sessionToken = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(host);
        ArgumentException.ThrowIfNullOrEmpty(bucket);
        ArgumentException.ThrowIfNullOrEmpty(region);
        ArgumentException.ThrowIfNullOrEmpty(accessKeyId);
        ArgumentException.ThrowIfNullOrEmpty(secretAccessKey);
        OvPhysxException.Check(
            NativeMethods.ovphysx_configure_s3(host, bucket, region, accessKeyId, secretAccessKey, sessionToken),
            "configure_s3");
    }

    /// <summary>
    /// Configures an Azure SAS token for loading USD assets from Azure-backed URLs (process-global).
    /// </summary>
    public static void ConfigureAzureSas(string host, string container, string sasToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(host);
        ArgumentException.ThrowIfNullOrEmpty(container);
        ArgumentException.ThrowIfNullOrEmpty(sasToken);
        OvPhysxException.Check(
            NativeMethods.ovphysx_configure_azure_sas(host, container, sasToken),
            "configure_azure_sas");
    }
}
