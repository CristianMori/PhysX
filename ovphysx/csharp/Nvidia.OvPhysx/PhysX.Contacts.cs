// SPDX-FileCopyrightText: Copyright (c) 2026 Cristian Mori
// SPDX-License-Identifier: BSD-3-Clause

using System.Numerics;
using Nvidia.OvPhysx.Interop;

namespace Nvidia.OvPhysx;

public sealed partial class PhysX
{
    /// <summary>
    /// Creates a contact binding. Each sensor pattern aggregates contacts; supplying
    /// <paramref name="filterPatterns"/> (a flat list of <c>sensorPatterns.Count * filtersPerSensor</c>
    /// entries) restricts aggregation to specific counterpart bodies per sensor.
    /// </summary>
    /// <param name="filtersPerSensor">Number of filter bodies per sensor (0 = unfiltered: contact with all bodies).</param>
    /// <param name="maxContactDataCount">Capacity for detailed contact/friction reads (0 disables them).</param>
    public unsafe ContactBinding CreateContactBinding(
        IReadOnlyList<string> sensorPatterns,
        IReadOnlyList<string>? filterPatterns = null,
        uint filtersPerSensor = 0,
        uint maxContactDataCount = 0)
    {
        EnsureValid();
        ArgumentNullException.ThrowIfNull(sensorPatterns);
        if (sensorPatterns.Count == 0)
            throw new ArgumentException("At least one sensor pattern is required.", nameof(sensorPatterns));

        if (filtersPerSensor > 0)
        {
            long expected = (long)sensorPatterns.Count * filtersPerSensor;
            if (filterPatterns is null || filterPatterns.Count != expected)
                throw new ArgumentException(
                    $"filterPatterns must contain sensorPatterns.Count * filtersPerSensor = {expected} entries when filtersPerSensor > 0.",
                    nameof(filterPatterns));
        }

        using var sensors = new NativeStringArray(sensorPatterns);
        using var filters = new NativeStringArray(filterPatterns ?? []);

        ulong binding;
        OvPhysxException.Check(
            NativeMethods.ovphysx_create_contact_binding(
                _handle, sensors.Ptr, sensors.Count, filters.Ptr, filtersPerSensor, maxContactDataCount, &binding),
            "create_contact_binding");

        int sensorCount, filterCount;
        OvPhysxException.Check(
            NativeMethods.ovphysx_get_contact_binding_spec(_handle, binding, &sensorCount, &filterCount),
            "get_contact_binding_spec");

        // The actual allocated capacity can differ from the requested value; use the native value
        // (as the Python API does) so detailed contact/friction reads size their buffers correctly.
        uint actualCapacity;
        OvPhysxException.Check(
            NativeMethods.ovphysx_get_contact_binding_capacity(_handle, binding, &actualCapacity),
            "get_contact_binding_capacity");

        return new ContactBinding(this, binding, sensorCount, filterCount, actualCapacity);
    }

    /// <summary>
    /// Returns per-contact-point events for the current step. The returned data is copied out of the
    /// library's per-step buffers, so it remains valid after the next step.
    /// </summary>
    /// <param name="includeFrictionAnchors">When true, also collects friction anchor data.</param>
    public unsafe ContactReport GetContactReport(bool includeFrictionAnchors = false)
    {
        EnsureValid();

        ovphysx_contact_event_header_t* headers;
        uint numHeaders;
        ovphysx_contact_point_t* points;
        uint numPoints;
        ovphysx_friction_anchor_t* anchors = null;
        uint numAnchors = 0;

        ovphysx_result_t r = includeFrictionAnchors
            ? NativeMethods.ovphysx_get_contact_report(_handle, &headers, &numHeaders, &points, &numPoints, &anchors, &numAnchors)
            : NativeMethods.ovphysx_get_contact_report(_handle, &headers, &numHeaders, &points, &numPoints, null, null);
        OvPhysxException.Check(r, "get_contact_report");

        var headerList = new ContactEventHeader[numHeaders];
        for (uint i = 0; i < numHeaders; i++)
        {
            ovphysx_contact_event_header_t* h = &headers[i];
            headerList[i] = new ContactEventHeader(
                h->type, h->stageId, h->actor0, h->actor1, h->collider0, h->collider1,
                h->contactDataOffset, h->numContactData, h->frictionAnchorsDataOffset,
                h->numfrictionAnchorsData, h->protoIndex0, h->protoIndex1);
        }

        var pointList = new ContactPoint[numPoints];
        for (uint i = 0; i < numPoints; i++)
        {
            ovphysx_contact_point_t* p = &points[i];
            pointList[i] = new ContactPoint(
                new Vector3(p->position[0], p->position[1], p->position[2]),
                new Vector3(p->normal[0], p->normal[1], p->normal[2]),
                new Vector3(p->impulse[0], p->impulse[1], p->impulse[2]),
                p->separation, p->faceIndex0, p->faceIndex1, p->material0, p->material1);
        }

        FrictionAnchor[] anchorList;
        if (includeFrictionAnchors && anchors != null && numAnchors > 0)
        {
            anchorList = new FrictionAnchor[numAnchors];
            for (uint i = 0; i < numAnchors; i++)
            {
                ovphysx_friction_anchor_t* a = &anchors[i];
                anchorList[i] = new FrictionAnchor(
                    new Vector3(a->position[0], a->position[1], a->position[2]),
                    new Vector3(a->impulse[0], a->impulse[1], a->impulse[2]));
            }
        }
        else
        {
            anchorList = [];
        }

        return new ContactReport(headerList, pointList, anchorList);
    }
}
