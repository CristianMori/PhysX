// Copyright (c) 2026 Cristian Mori. Licensed under the MIT License.
// See csharp/LICENSE for details.

using System.Runtime.InteropServices;
using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

/// <summary>
/// Configures native library loading for ovPhysX.
/// Call SetLibraryPath before creating any PhysX instance.
/// </summary>
public static class OvPhysxLibrary
{
    private static bool _resolverSet;

    /// <summary>
    /// Set the directory containing ovphysx.dll (or libovphysx.so) and its
    /// plugins/ subdirectory. This adds the directory and plugins/ to the
    /// DLL search path so all dependencies can be found.
    /// Must be called before any other ovPhysX API call.
    /// </summary>
    public static void SetLibraryPath(string directoryPath)
    {
        if (_resolverSet)
            throw new InvalidOperationException("Library path resolver has already been set.");

        // Add the lib directory and plugins directory to the DLL search path
        // so that transitive dependencies (carb, USD, PhysX plugins) can be found.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Mirror Python's os.add_dll_directory() calls.
            // The package directory is the parent of directoryPath (lib/).
            var pkgDir = Path.GetDirectoryName(directoryPath) ?? directoryPath;
            var libDir = Path.Combine(pkgDir, "lib");
            var pluginsDir = Path.Combine(pkgDir, "plugins");

            // AddDllDirectory is the exact Win32 API behind os.add_dll_directory()
            if (Directory.Exists(libDir)) AddDllDirectory(libDir);
            if (Directory.Exists(pluginsDir)) AddDllDirectory(pluginsDir);
            // Also add the directoryPath itself in case it's not lib/
            AddDllDirectory(directoryPath);
        }
        else
        {
            // On Linux, set LD_LIBRARY_PATH (must be done before first dlopen)
            var ldPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? "";
            var pluginsDir = Path.Combine(directoryPath, "plugins");
            Environment.SetEnvironmentVariable("LD_LIBRARY_PATH",
                $"{directoryPath}:{pluginsDir}:{ldPath}");
        }

        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, (name, assembly, searchPath) =>
        {
            if (name != "ovphysx")
                return IntPtr.Zero;

            var libName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "ovphysx.dll" : "libovphysx.so";
            var fullPath = Path.Combine(directoryPath, libName);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR (0x100): search the directory
                // containing the DLL for its transitive dependencies.
                // LOAD_LIBRARY_SEARCH_USER_DIRS (0x400): search dirs added via
                // AddDllDirectory (plugins/, lib/).
                // LOAD_LIBRARY_SEARCH_SYSTEM32 (0x800): also search system32.
                var handle = LoadLibraryExW(fullPath, IntPtr.Zero, 0x100 | 0x400 | 0x800);
                if (handle != IntPtr.Zero)
                    return handle;
                // Fallback: try plain LoadLibrary
                handle = LoadLibraryW(fullPath);
                if (handle != IntPtr.Zero)
                    return handle;
            }

            return NativeLibrary.Load(fullPath);
        });

        _resolverSet = true;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr AddDllDirectory(string newDirectory);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetDefaultDllDirectories(uint directoryFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryW(string lpLibFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryExW(string lpLibFileName, IntPtr hFile, uint dwFlags);
}
