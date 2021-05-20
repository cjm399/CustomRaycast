using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;


public struct v3
{
    public float x, y, z;

    public v3(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }
}

public struct bounds
{
    public v3 minPoints;
    public v3 maxPoints;
}

public struct raycast_result
{
    public bool didHit;
    public entity hitEntity;
    public v3 hitPos;
}

public struct entity
{
    public v3 position;
    public v3 scale;
    public bounds bounds;
}

public struct world
{
    public NativeArray<entity> entities;
    public int entityCount, maxEntities;
    public static int VOXEL_SIZE = 50;

    public world(int _maxEntities)
    {
        maxEntities = _maxEntities;
        entityCount = 0;
        entities = new NativeArray<entity>(maxEntities, Allocator.Persistent, NativeArrayOptions.ClearMemory);//new entity[maxEntities];
    }
}

public class game : MonoBehaviour
{
    private world gameWorld;

    public static v3 up = new v3(0, 1, 0);
    public static v3 down = new v3(0, -1, 0);
    public static v3 left = new v3(-1, 0, 0);
    public static v3 right = new v3(1, 0, 0);
    public static v3 forward = new v3(0, 0, 1);
    public static v3 back = new v3(0, 0, -1);

    public static v3 ruf = v3Normalize(new v3(1, 1, 1));
    public static v3 rub = v3Normalize(new v3(1, 1, -1));
    public static v3 rdf = v3Normalize(new v3(1, -1, 1));
    public static v3 rdb = v3Normalize(new v3(1, -1, -1));
    public static v3 luf = v3Normalize(new v3(-1, 1, 1));
    public static v3 lub = v3Normalize(new v3(-1, 1, -1));
    public static v3 ldf = v3Normalize(new v3(-1, -1, 1));
    public static v3 ldb = v3Normalize(new v3(-1, -1, -1));

    [BurstCompile(CompileSynchronously = true, Debug = false, DisableSafetyChecks = true, FloatMode =FloatMode.Fast, FloatPrecision =FloatPrecision.Low)]
    private struct RaycastJob : IJob
    {
        public v3 start;
        public v3 dir;
        public float maxDist;
        public NativeArray<raycast_result> hitRes;
        public world gameWorld;

        public void Execute()
        {
            raycast_result hit = new raycast_result();
            SlowRayCast(gameWorld, start, dir, maxDist, out hit);
            hitRes[0] = hit;
        }
    }

    private void OnDestroy()
    {
        gameWorld.entities.Dispose();
    }

    private void Start()
    {
        gameWorld = new world(30_000);
        //SpawnLotsOfCubes(ref gameWorld, 29_999);

        entity curr = CreateCubePrimative(new v3(0, 0, 0));
        AddEntityToWorld(ref gameWorld, ref curr);

        entity curr2 = CreateCubePrimative(new v3(1, 0, 1));
        AddEntityToWorld(ref gameWorld, ref curr2);

        NativeArray<raycast_result> hitResults = new NativeArray<raycast_result>(1, Allocator.TempJob);

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        var runMyJob = new RaycastJob { gameWorld = gameWorld, start = new v3(0, 0, -50), dir = forward, maxDist = 10_000, hitRes = hitResults };
        runMyJob.Run();
        sw.Stop();
        raycast_result hitResult = runMyJob.hitRes[0];
        hitResults.Dispose();
        Debug.Log($"Raycast took {sw.ElapsedMilliseconds}ms and hit {hitResult.hitPos.x}, {hitResult.hitPos.y}, {hitResult.hitPos.z}");
        /*
        sw = System.Diagnostics.Stopwatch.StartNew();
        SlowRayCast(gameWorld, new v3(0, 20, 0), forward, 10_000, out hitResult);
        sw.Stop();
        Debug.Log($"Raycast 2 took {sw.ElapsedMilliseconds}ms");*/


#if true
        RayCastAlongSphere(ref gameWorld, new v3(0, 0, 0), 20f);
#endif
#if false
        BoundsTests(ref test);
#endif

#if false
        v3CreationTests();
#endif
    }

    private void SpawnLotsOfCubes(ref world _w, int _spawnCount)
    {
        int dimSizeX = (int)Mathf.Floor(Mathf.Sqrt(_spawnCount));
        int dimSizeY = dimSizeX;
        float paddingX = .5f;
        float paddingY = .5f;
        int count = 0;
        for (int y = 0; y < dimSizeY; ++y)
        {
            for (int x = 0; x < dimSizeX; ++x)
            {
                entity curr = CreateCubePrimative(new v3(x + paddingX, y + paddingY, 0));
                AddEntityToWorld(ref _w, ref curr);
                paddingX += .25f;
            }
            paddingY += .25f;
        }
    }

    private static void RayCastAlongSphere(ref world _w, v3 _origin, float _radius)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        float diameter = _radius * 2;
        float yStep = _radius/25;
        float cStep = 360 / 25;
        raycast_result hitResult;
        int count = 0;

        for (float y = -_radius;
            y <= _radius;
            y += yStep)
        {
            float absY = Mathf.Abs(y);
            float ratio = (_radius - absY);
            for (float c = 0;
                c < 360;
                c += cStep)
            {
                float x = Mathf.Sin(c) * ratio;
                float z = Mathf.Cos(c) * ratio;
                v3 start = new v3(x, y, z);
                start = v3Normalize(start);
                start = v3Mul(start, _radius);
                v3 dir = v3Normalize(v3Substractv3(new v3(0, 0, 0), start));
                if (FastRaycast(ref _w, start, dir, _radius, out hitResult))
                {
                    raycast_result result2;
                    v3 newStart = v3Addv3(start, v3FromDirAndMag(start, v3Mul(dir, 1f), _radius*2));
                    //newStart = v3Addv3(hitResult.hitPos, v3FromDirAndMag(hitResult.hitPos, dir, 1));
                    if (FastRaycast(ref _w, newStart, v3Mul(dir, 1f), _radius * 2, out result2))
                    {
                        Debug.DrawLine(v3ToVector3(hitResult.hitPos), v3ToVector3(result2.hitPos), Color.green, Mathf.Infinity);
                    }
                    count++;
                }
                count++;
            }
        }
        sw.Stop();
        Debug.Log($"ElapsedTime: {sw.ElapsedMilliseconds}ms for {count} raycasts");
    }

    private static void BoundsTests(ref entity _e)
    {
        Debug.Assert(IsInside(_e.bounds, new v3(0, 0, 0)));
        Debug.Assert(IsInside(_e.bounds, new v3(.5f, .5f, .5f)));
        Debug.Assert(IsInside(_e.bounds, new v3(-.5f, -.5f, -.5f)));
        Debug.Assert(!IsInside(_e.bounds, new v3(1, 1, 1)));
        Debug.Assert(!IsInside(_e.bounds, new v3(-1, -1, -1)));
        Debug.Assert(!IsInside(_e.bounds, new v3(2, 2, 2)));
        Debug.Assert(!IsInside(_e.bounds, new v3(-2, -2, -2)));
    }

    private static void v3CreationTests()
    {
#if false
        v3 a = v3FromDirAndMag(new v3(0,0,0), up, 1f);
        v3 b = v3FromDirAndMag(new v3(0,0,0), down, 1f);
        v3 c = v3FromDirAndMag(new v3(0,0,0), left, 1f);
        v3 d = v3FromDirAndMag(new v3(0,0,0), right, 1f);
        v3 e = v3FromDirAndMag(new v3(0,0,0), forward, 1f);
        v3 f = v3FromDirAndMag(new v3(0,0,0), back, 1f);

        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(a), Color.red, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(b), Color.green, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(c), Color.blue, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(d), Color.magenta, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(e), Color.yellow, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(f), Color.black, Mathf.Infinity);
#else
        v3 a = v3FromDirAndMag(new v3(0, 0, 0), ruf, 1f);
        v3 b = v3FromDirAndMag(new v3(0, 0, 0), rub, 1f);
        v3 c = v3FromDirAndMag(new v3(0, 0, 0), rdf, 1f);
        v3 d = v3FromDirAndMag(new v3(0, 0, 0), rdb, 1f);
        v3 e = v3FromDirAndMag(new v3(0, 0, 0), luf, 1f);
        v3 f = v3FromDirAndMag(new v3(0, 0, 0), lub, 1f);
        v3 g = v3FromDirAndMag(new v3(0, 0, 0), ldf, 1f);
        v3 h = v3FromDirAndMag(new v3(0, 0, 0), ldb, 1f);

        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(a), Color.red, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(b), Color.green, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(c), Color.blue, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(d), Color.magenta, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(e), Color.yellow, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(f), Color.black, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(g), Color.white, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), v3ToVector3(h), Color.gray, Mathf.Infinity);
#endif
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static Vector3 v3ToVector3(v3 _v)
    {
        Vector3 result = new Vector3(_v.x, _v.y, _v.z);
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static v3 Vector3Tov3(Vector3 _v)
    {
        v3 result = new v3(_v.x, _v.y, _v.z);
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static bool IsInside(bounds _b, v3 _pos)
    {
        return (_b.maxPoints.x > _pos.x && _b.maxPoints.y > _pos.y && _b.maxPoints.z > _pos.z)
            && (_b.minPoints.x < _pos.x && _b.minPoints.y < _pos.y && _b.minPoints.z < _pos.z);
    }


    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static v3 v3Addv3(v3 _a, v3 _b)
    {
        v3 result = _a;
        result.x += _b.x;
        result.y += _b.y;
        result.z += _b.z;
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static v3 v3Substractv3(v3 _a, v3 _b)
    {
        v3 result = _a;
        result.x -= _b.x;
        result.y -= _b.y;
        result.z -= _b.z;
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static v3 v3Mul(v3 _a, float _b)
    {
        v3 result = _a;
        result.x *= _b;
        result.y *= _b;
        result.z *= _b;
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static v3 v3Div(v3 _a, float _b)
    {
        v3 result = _a;
        result.x /= _b;
        result.y /= _b;
        result.z /= _b;
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static float v3Mag(v3 _v)
    {
        float result = Mathf.Sqrt(_v.x * _v.x + _v.y * _v.y + _v.z * _v.z);
        return result;
    }


    [System.Diagnostics.Contracts.Pure]
    public static v3 v3Normalize(v3 _v)
    {
        v3 result = _v;
        float mag = v3Mag(_v);
        result.x /= mag;
        result.y /= mag;
        result.z /= mag;
        return result;
    }

    [System.Diagnostics.Contracts.Pure]
    public static v3 v3FromDirAndMag(v3 _start, v3 _dir, float _mag)
    {
        v3 result = _dir;
        result = v3Normalize(result);
        result.x *= _mag;
        result.y *= _mag;
        result.z *= _mag;
        result = v3Addv3(_start, result);
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static bool v3Equality(v3 _a, v3 _b)
    {
        return _a.x == _b.x && _a.y == _b.y && _a.z == _b.z;
    }

    public static bool FastRaycast(ref world _world, v3 _start, v3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        NativeArray<raycast_result> hitResults = new NativeArray<raycast_result>(1, Allocator.TempJob);
        var runMyJob = new RaycastJob { gameWorld = _world, start = _start, dir = _dir, maxDist = _maxDist, hitRes = hitResults };
        runMyJob.Run();
        _hitResult = runMyJob.hitRes[0];
        hitResults.Dispose();
        return _hitResult.didHit;
    }

    [System.Diagnostics.Contracts.Pure]
    public static bool SlowRayCast(world _world, v3 _start, v3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        float stepDelta = .1f;
        float currStep = stepDelta;
        v3 checkPos = _start;
        v3 finalPos = v3FromDirAndMag(_start, _dir, _maxDist);
        _hitResult = new raycast_result();
        while (currStep < _maxDist)
        {
            for(int entityIndex = 0;
                entityIndex < _world.entityCount;
                ++entityIndex)
            {
                if (IsInside(_world.entities[entityIndex].bounds, checkPos))
                {
                    _hitResult.hitEntity = _world.entities[entityIndex];
                    _hitResult.hitPos = checkPos;
                    _hitResult.didHit = true;
                    return true;
                }
            }
            checkPos = v3FromDirAndMag(_start, _dir, currStep);
            currStep += stepDelta;
        }
        return false;
    }

    public static void AddEntityToWorld(ref world _w, ref entity _e)
    {
        if(_w.entityCount -1 == _w.maxEntities)
        {
            _w.maxEntities *= 2;
            NativeArray<entity> tmp = new NativeArray<entity>(_w.maxEntities, Allocator.Temp, NativeArrayOptions.ClearMemory);
            NativeArray<entity>.Copy(_w.entities, tmp);
            _w.entities.Dispose();
            _w.entities = new NativeArray<entity>(_w.maxEntities, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            NativeArray<entity>.Copy(tmp, _w.entities);
            tmp.Dispose();
        }
        _w.entities[_w.entityCount++] = _e;
    }

    [System.Diagnostics.Contracts.Pure]
    public static entity CreateCubePrimative(v3 _origin)
    {
        entity result = new entity();
        result.position = _origin;
        result.scale = new global::v3(1,1,1);
        float minX = result.position.x - result.scale.x;
        float maxX = result.position.x + result.scale.x;

        float minY = result.position.y - result.scale.y;
        float maxY = result.position.y + result.scale.y;

        float minZ = result.position.z - result.scale.z;
        float maxZ = result.position.z + result.scale.z;

        result.bounds.minPoints = new v3(minX, minY, minZ);
        result.bounds.maxPoints = new v3(maxX, maxY, maxZ);
        return result;
    }
}
