// SPDX-FileCopyrightText: Copyright (c) 2025-2026 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// SPDX-License-Identifier: BSD-3-Clause

using System.Globalization;
using System.Runtime.InteropServices;

namespace Nvidia.OvPhysx.Interop;

/// <summary>
/// Builds a pinned, native <c>ovphysx_config_entry_t[]</c> from a <see cref="PhysXConfig"/> and
/// keeps the backing UTF-8 string buffers alive for the duration of a native call. Dispose after
/// the call returns.
/// </summary>
internal sealed unsafe class NativeConfigEntries : IDisposable
{
    // field name -> (key type, enum key value, carbonite path) — mirrors config.py _FIELD_TO_ENTRY.
    private static readonly (string Path, string Field)[] KnownPaths =
    [
        ("/physics/disableContactProcessing", nameof(PhysXConfig.DisableContactProcessing)),
        ("/physics/collisionConeCustomGeometry", nameof(PhysXConfig.CollisionConeCustomGeometry)),
        ("/physics/collisionCylinderCustomGeometry", nameof(PhysXConfig.CollisionCylinderCustomGeometry)),
        ("/physics/numThreads", nameof(PhysXConfig.NumThreads)),
        ("/physics/sceneMultiGPUMode", nameof(PhysXConfig.SceneMultiGpuMode)),
        ("/physics/omniPvdOutputEnabled", nameof(PhysXConfig.OmniPvdOutputEnabled)),
        ("/persistent/physics/omniPvdOvdRecordingDirectory", nameof(PhysXConfig.OmniPvdOvdRecordingDirectory)),
    ];

    private readonly List<NativeStringArg> _strings = [];
    private readonly ovphysx_config_entry_t[] _entries;
    private GCHandle _arrayHandle;

    public ovphysx_config_entry_t* Entries { get; }
    public uint Count { get; }

    public NativeConfigEntries(PhysXConfig? config)
    {
        List<ovphysx_config_entry_t> list = [];

        if (config is not null)
        {
            if (config.DisableContactProcessing is { } dcp)
                list.Add(Bool(ConfigBool.DisableContactProcessing, dcp));
            if (config.CollisionConeCustomGeometry is { } cone)
                list.Add(Bool(ConfigBool.CollisionConeCustomGeometry, cone));
            if (config.CollisionCylinderCustomGeometry is { } cyl)
                list.Add(Bool(ConfigBool.CollisionCylinderCustomGeometry, cyl));
            if (config.OmniPvdOutputEnabled is { } pvd)
                list.Add(Bool(ConfigBool.OmniPvdOutputEnabled, pvd));
            if (config.NumThreads is { } nt)
                list.Add(Int32(ConfigInt32.NumThreads, nt));
            if (config.SceneMultiGpuMode is { } gpu)
                list.Add(Int32(ConfigInt32.SceneMultiGpuMode, gpu));
            if (config.OmniPvdOvdRecordingDirectory is { } dir)
                list.Add(StringEntry(ConfigString.OmniPvdOvdRecordingDirectory, dir));

            if (config.CarboniteOverrides is { Count: > 0 } overrides)
            {
                foreach (KeyValuePair<string, object> kv in overrides)
                {
                    foreach ((string path, string field) in KnownPaths)
                    {
                        if (path == kv.Key)
                            throw new ArgumentException(
                                $"CarboniteOverrides key '{kv.Key}' conflicts with typed field '{field}'. Use the typed property instead.",
                                nameof(config));
                    }
                    list.Add(Carbonite(kv.Key, kv.Value));
                }
            }
        }

        _entries = list.ToArray();
        Count = (uint)_entries.Length;
        if (_entries.Length > 0)
        {
            _arrayHandle = GCHandle.Alloc(_entries, GCHandleType.Pinned);
            Entries = (ovphysx_config_entry_t*)_arrayHandle.AddrOfPinnedObject();
        }
        else
        {
            Entries = null;
        }
    }

    private static ovphysx_config_entry_t Bool(ConfigBool key, bool value) => new()
    {
        key_type = ovphysx_config_key_type_t.OVPHYSX_CONFIG_KEY_TYPE_BOOL,
        bool_key = (int)key,
        bool_value = (byte)(value ? 1 : 0),
    };

    private static ovphysx_config_entry_t Int32(ConfigInt32 key, int value) => new()
    {
        key_type = ovphysx_config_key_type_t.OVPHYSX_CONFIG_KEY_TYPE_INT32,
        int32_key = (int)key,
        int32_value = value,
    };

    private ovphysx_config_entry_t StringEntry(ConfigString key, string value)
    {
        var arg = new NativeStringArg(value);
        _strings.Add(arg);
        return new ovphysx_config_entry_t
        {
            key_type = ovphysx_config_key_type_t.OVPHYSX_CONFIG_KEY_TYPE_STRING,
            string_key = (int)key,
            string_value = arg.Value,
        };
    }

    private ovphysx_config_entry_t Carbonite(string key, object value)
    {
        var keyArg = new NativeStringArg(key);
        var valueArg = new NativeStringArg(ToCarboniteString(value));
        _strings.Add(keyArg);
        _strings.Add(valueArg);
        return new ovphysx_config_entry_t
        {
            key_type = ovphysx_config_key_type_t.OVPHYSX_CONFIG_KEY_TYPE_CARBONITE,
            carbonite_key = keyArg.Value,
            string_value = valueArg.Value,
        };
    }

    private static string ToCarboniteString(object value) => value switch
    {
        bool b => b ? "true" : "false",
        float f => f.ToString(CultureInfo.InvariantCulture),
        double d => d.ToString(CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    public void Dispose()
    {
        if (_arrayHandle.IsAllocated)
            _arrayHandle.Free();
        foreach (NativeStringArg s in _strings)
            s.Dispose();
        _strings.Clear();
    }
}
