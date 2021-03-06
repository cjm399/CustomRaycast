using Unity.Mathematics;
using UnityEngine;

public static class Collision
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static bool IsInside(bounds _b, float3 _pos)
    {
        return (_b.maxPoints.x > _pos.x && _b.maxPoints.y > _pos.y && _b.maxPoints.z > _pos.z)
            && (_b.minPoints.x < _pos.x && _b.minPoints.y < _pos.y && _b.minPoints.z < _pos.z);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static bool AABBOverlap(bounds _a, bounds _b)
    {
        return (_a.minPoints.x <= _b.maxPoints.x && _a.maxPoints.x >= _b.minPoints.x)
            && (_a.minPoints.y <= _b.maxPoints.y && _a.maxPoints.y >= _b.minPoints.y)
            && (_a.minPoints.z <= _b.maxPoints.z && _a.maxPoints.z >= _b.minPoints.z);
    }

    public static bool AABBSphereOverlap(bounds _a, sphereBounds _s)
    {
        float x = Mathf.Max(_a.minPoints.x, Mathf.Min(_s.center.x, _a.maxPoints.x));
        float y = Mathf.Max(_a.minPoints.y, Mathf.Min(_s.center.y, _a.maxPoints.y));
        float z = Mathf.Max(_a.minPoints.z, Mathf.Min(_s.center.z, _a.maxPoints.z));

        float distance = Mathf.Sqrt(
            (x - _s.center.x) * (x - _s.center.x) +
            (y - _s.center.y) * (y - _s.center.y) +
            (z - _s.center.z) * (z - _s.center.z));

        return distance < _s.radius;
    }

    public static bounds MakeBoundsFromVector(float3 _start, float3 _end)
    {
        bounds result = new bounds();
        result.maxPoints.x = _end.x;
        result.minPoints.x = _start.x;
        result.maxPoints.y = _end.y;
        result.minPoints.y = _start.y;
        result.maxPoints.z = _end.z;
        result.minPoints.z = _start.z;

        if (_start.x > _end.x)
        {
            result.maxPoints.x = _start.x;
            result.minPoints.x = _end.x;
        }
        if (_start.y > _end.y)
        {
            result.maxPoints.y = _start.y;
            result.minPoints.y = _end.y;
        }
        if (_start.z > _end.z)
        {
            result.maxPoints.z = _start.z;
            result.minPoints.z = _end.z;
        }
        return result;
    }
}
