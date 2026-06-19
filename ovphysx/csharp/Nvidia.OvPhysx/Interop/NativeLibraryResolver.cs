// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Nvidia.OvPhysx.Interop;

/// <summary>
/// Resolves the native <c>ovphysx</c> shared library at runtime, mirroring the
/// discovery order of the Python <c>_bindings.py</c> loader:
/// <list type="number">
///   <item><c>OVPHYSX_LIB</c> environment variable (absolute path) — takes precedence for development.</item>
///   <item>A library next to the managed assembly (<c>lib/</c> subdir or the assembly directory).</item>
///   <item>The default OS loader search path (<c>ovphysx.dll</c> / <c>libovphysx.so</c>).</item>
/// </list>
/// On Windows it also adds the <c>lib/</c> and <c>plugins/</c> directories (and the kit SDK
/// directory derived from <c>OVPHYSX_LIB</c>) to the DLL search path so transitive
/// dependencies (carb.dll, etc.) are discoverable.
/// </summary>
public static class NativeLibraryResolver
{
    /// <summary>The P/Invoke library identifier used by <see cref="NativeMethods"/>.</summary>
    public const string LibraryName = "ovphysx";

    /// <summary>Environment variable holding an absolute path to the native library.</summary>
    public const string LibPathEnvVar = "OVPHYSX_LIB";

    private static int _registered;

    /// <summary>
    /// Registers the <see cref="DllImportResolver"/> for this assembly. Invoked automatically
    /// before any managed code in this assembly runs via <see cref="ModuleInitializerAttribute"/>,
    /// and safe to call again (idempotent).
    /// </summary>
    [ModuleInitializer]
    public static void Register()
    {
        if (Interlocked.Exchange(ref _registered, 1) != 0)
            return;

        AddDependencyDirectories();
        NativeLibrary.SetDllImportResolver(typeof(NativeLibraryResolver).Assembly, Resolve);
    }

    private static nint Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibraryName)
            return nint.Zero;

        // 1. OVPHYSX_LIB absolute path (development override).
        string? envPath = Environment.GetEnvironmentVariable(LibPathEnvVar);
        if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath) &&
            NativeLibrary.TryLoad(envPath, out nint envHandle))
        {
            return envHandle;
        }

        // 2. Bundled candidates next to the assembly.
        foreach (string candidate in BundledCandidates())
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out nint handle))
                return handle;
        }

        // 3. Default OS loader search.
        foreach (string name in PlatformLibNames())
        {
            if (NativeLibrary.TryLoad(name, assembly, searchPath, out nint handle))
                return handle;
        }

        return nint.Zero;
    }

    private static IEnumerable<string> BundledCandidates()
    {
        string baseDir = AppContext.BaseDirectory;
        foreach (string name in PlatformLibNames())
        {
            yield return Path.Combine(baseDir, "lib", name);
            yield return Path.Combine(baseDir, name);
        }
    }

    private static string[] PlatformLibNames() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ["ovphysx.dll"]
            : ["libovphysx.so"];

    private static void AddDependencyDirectories()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return; // On Linux, rpath=$ORIGIN handles transitive deps at the ELF level.

        string baseDir = AppContext.BaseDirectory;
        foreach (string sub in (string[])["lib", "plugins"])
            TryAddDllDirectory(Path.Combine(baseDir, sub));

        string? libEnv = Environment.GetEnvironmentVariable(LibPathEnvVar);
        if (!string.IsNullOrEmpty(libEnv))
        {
            // Both the pip wheel and the GitHub Releases SDK lay the native library out as
            // <root>/lib/ovphysx.dll with transitive deps in sibling folders (plugins/, bin/)
            // and a kit SDK under target-deps/. Add the lib dir and the likely sibling dirs.
            string? libDir = Path.GetDirectoryName(libEnv);
            if (libDir is not null)
                TryAddDllDirectory(libDir);

            string? root = Path.GetDirectoryName(libDir);
            if (root is not null)
            {
                TryAddDllDirectory(root);
                TryAddDllDirectory(Path.Combine(root, "plugins"));
                TryAddDllDirectory(Path.Combine(root, "bin"));
                foreach (string cfg in (string[])["debug", "release", "checked"])
                    TryAddDllDirectory(Path.Combine(root, "target-deps", $"kit_sdk_{cfg}"));
            }
        }
    }

    private static void TryAddDllDirectory(string dir)
    {
        try
        {
            if (!Directory.Exists(dir))
                return;

            // AddDllDirectory covers loads that opt into the user-directory search; prepending to
            // PATH is honoured universally by the OS loader when resolving transitive dependencies
            // (carb.dll, USD runtime, etc. in plugins/). Use both for robustness.
            AddDllDirectory(dir);

            string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string[] entries = path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            if (!entries.Any(e => string.Equals(e.TrimEnd(Path.DirectorySeparatorChar), dir.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase)))
                Environment.SetEnvironmentVariable("PATH", dir + Path.PathSeparator + path);
        }
        catch
        {
            // Best effort — ignore failures (e.g. older OS without AddDllDirectory).
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint AddDllDirectory(string newDirectory);
}
