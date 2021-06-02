using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class Math
{
    public static float Float3DotProd(float3 _a, float3 _b)
    {
        float result = _a.x * _b.x + _a.y * _b.y + _a.z * _b.z;
        return result;
    }

    public static float3 Float3Projection(float3 _a, float3 _b)
    {
        float aMag = Float3Mag(_a);
        float scale = Float3DotProd(_a, _b) / aMag;

        float3 result = scale * (aMag / aMag);
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static float Float3Mag(float3 _v)
    {
        float result = Mathf.Sqrt(_v.x * _v.x + _v.y * _v.y + _v.z * _v.z);
        return result;
    }


    [System.Diagnostics.Contracts.Pure]
    public static float3 Float3Normalize(float3 _v)
    {
        float3 result = _v;
        float mag = Float3Mag(_v);
        result.x /= mag;
        result.y /= mag;
        result.z /= mag;
        return result;
    }

    [System.Diagnostics.Contracts.Pure]
    public static float3 Float3FromDirAndMag(float3 _start, float3 _dir, float _mag)
    {
        float3 result = _dir;
        result = Float3Normalize(result);
        result.x *= _mag;
        result.y *= _mag;
        result.z *= _mag;
        result = _start + result;
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static bool Float3Equality(float3 _a, float3 _b)
    {
        return _a.x == _b.x && _a.y == _b.y && _a.z == _b.z;
    }

}
