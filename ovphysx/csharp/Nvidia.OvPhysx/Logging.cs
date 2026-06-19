// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using System.Runtime.InteropServices;
using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

/// <summary>
/// Process-global logging configuration for the native ovphysx library: verbosity threshold,
/// Carbonite's built-in console output, and a managed log callback.
/// </summary>
public static class Logging
{
    /// <summary>Managed log callback signature.</summary>
    public delegate void LogHandler(LogLevel level, string message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void NativeLogDelegate(uint level, nint message, nint userData);

    private static readonly object Gate = new();
    private static NativeLogDelegate? _nativeDelegate; // GC-rooted while registered
    private static LogHandler? _handler;
    private static nint _fnPtr;

    /// <summary>Gets or sets the global log verbosity threshold.</summary>
    public static LogLevel Level
    {
        get
        {
            NativeLibraryResolver.Register();
            return (LogLevel)NativeMethods.ovphysx_get_log_level();
        }
        set
        {
            NativeLibraryResolver.Register();
            OvPhysxException.Check(NativeMethods.ovphysx_set_log_level((uint)value), "set_log_level");
        }
    }

    /// <summary>Enables or disables Carbonite's built-in console log output (independent of callbacks).</summary>
    public static void EnableDefaultOutput(bool enable = true)
    {
        NativeLibraryResolver.Register();
        OvPhysxException.Check(NativeMethods.ovphysx_enable_default_log_output(enable), "enable_default_log_output");
    }

    /// <summary>
    /// Registers a managed log callback, replacing any previously registered one. The delegate is
    /// GC-rooted for the duration of the registration. Pass <see langword="null"/> or call
    /// <see cref="UnregisterCallback"/> to stop receiving messages.
    /// </summary>
    public static void RegisterCallback(LogHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        NativeLibraryResolver.Register();

        lock (Gate)
        {
            UnregisterCallbackCore();

            _handler = handler;
            _nativeDelegate = OnNativeLog;
            _fnPtr = Marshal.GetFunctionPointerForDelegate(_nativeDelegate);
            OvPhysxException.Check(NativeMethods.ovphysx_register_log_callback(_fnPtr, nint.Zero), "register_log_callback");
        }
    }

    /// <summary>Unregisters the current managed log callback (idempotent).</summary>
    public static void UnregisterCallback()
    {
        lock (Gate)
            UnregisterCallbackCore();
    }

    /// <summary>Emits a set of test log messages at each level (native diagnostic aid).</summary>
    public static void EmitTestMessages()
    {
        NativeLibraryResolver.Register();
        NativeMethods.ovphysx_log_emit_test_messages();
    }

    private static void UnregisterCallbackCore()
    {
        if (_nativeDelegate is null)
            return;
        try
        {
            _ = NativeMethods.ovphysx_unregister_log_callback(_fnPtr, nint.Zero);
        }
        finally
        {
            _nativeDelegate = null;
            _handler = null;
            _fnPtr = nint.Zero;
        }
    }

    private static void OnNativeLog(uint level, nint message, nint userData)
    {
        LogHandler? handler = _handler;
        if (handler is null)
            return;
        string text = message == nint.Zero ? string.Empty : Marshal.PtrToStringUTF8(message) ?? string.Empty;
        try
        {
            handler((LogLevel)level, text);
        }
        catch
        {
            // Never let a managed exception propagate across the native boundary.
        }
    }
}
