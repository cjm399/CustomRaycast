using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;


public struct bounds
{
    public float3 minPoints;
    public float3 maxPoints;
}

public struct raycast_result
{
    public bool didHit;
    public entity hitEntity;
    public float3 hitPos;
}

public struct entity
{
    public float3 position;
    public float3 scale;
    public bounds bounds;
}

public struct world
{
    public NativeArray<entity> entities;
    public int entityCount, maxEntities;
    public static int VOXEL_SIZE = 10;

    public world(int _maxEntities)
    {
        maxEntities = _maxEntities;
        entityCount = 0;
        entities = new NativeArray<entity>(maxEntities, Allocator.Persistent, NativeArrayOptions.ClearMemory);
    }
}

public class game : MonoBehaviour
{
    private world gameWorld;

    public Mesh boxMesh;
    public Material material;
    public Camera mainCam;

    public static float3 up = new float3(0, 1, 0);
    public static float3 down = new float3(0, -1, 0);
    public static float3 left = new float3(-1, 0, 0);
    public static float3 right = new float3(1, 0, 0);
    public static float3 forward = new float3(0, 0, 1);
    public static float3 back = new float3(0, 0, -1);

    public static float3 ruf = Float3Normalize(new float3(1, 1, 1));
    public static float3 rub = Float3Normalize(new float3(1, 1, -1));
    public static float3 rdf = Float3Normalize(new float3(1, -1, 1));
    public static float3 rdb = Float3Normalize(new float3(1, -1, -1));
    public static float3 luf = Float3Normalize(new float3(-1, 1, 1));
    public static float3 lub = Float3Normalize(new float3(-1, 1, -1));
    public static float3 ldf = Float3Normalize(new float3(-1, -1, 1));
    public static float3 ldb = Float3Normalize(new float3(-1, -1, -1));

    //[BurstCompile(CompileSynchronously = true, Debug = false, DisableSafetyChecks = true, FloatMode =FloatMode.Fast, FloatPrecision =FloatPrecision.Low)]
    private struct RaycastJob : IJob
    {
        public float3 start;
        public float3 dir;
        public float maxDist;
        public NativeArray<raycast_result> hitRes;
        public world gameWorld;

        public void Execute()
        {
            raycast_result hit = new raycast_result();
#if false
            SlowRayCast(gameWorld, start, dir, maxDist, out hit); 
#else
            RayCast(gameWorld, start, dir, maxDist, out hit);
#endif
            hitRes[0] = hit;
        }
    }

    private void OnDisable()
    {
        gameWorld.entities.Dispose();
    }

    private void Start()
    {
        mainCam = Camera.main;
        gameWorld = new world(30000);

#if true
        SpawnLotsOfCubes(ref gameWorld, gameWorld.maxEntities);

        float3 start = new float3(50, 0, -50);
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        raycast_result hitResult;
        FastRaycast(ref gameWorld, new float3(50, 0, -50), forward, 10000, out hitResult);

#if false
        bounds raycastAABB = MakeBoundsFromVector(start, Float3FromDirAndMag(start, forward, 10000));
        int2[] voxels = GetVoxels(world.VOXEL_SIZE, raycastAABB);
        foreach (int2 set in voxels)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = new Vector3(set.x * 10, 1f, set.y * 10);
        } 
#endif
        //int2 vx = GetVoxel(world.VOXEL_SIZE, start);
        sw.Stop();
        Debug.Log($"Raycast took {sw.ElapsedMilliseconds}ms");
#endif 

#if false
        SpawnLotsOfGameObjects(gameWorld.maxEntities);
        System.Diagnostics.Stopwatch sw2 = System.Diagnostics.Stopwatch.StartNew();
        Physics.Raycast(new Vector3(50, 0, -5), Vector3.forward, 10000);
        sw2.Stop();
        Debug.Log($"Unity Raycast took {sw2.ElapsedMilliseconds}ms");
#endif 

#if false
        RayCastAlongSphere(ref gameWorld, new float3(0, 0, 0), 20f);
#endif
#if false
        BoundsTests(ref test);
#endif

#if false
        float3CreationTests();
#endif
#if false
        ProjectionTests();
#endif
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float3 dir = ray.direction;
        float3 pos = ray.origin;
        raycast_result result;
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            FastRaycast(ref gameWorld, pos, dir, 1000, out result);
            sw.Stop();
            Debug.Log($"Raycast took {sw.ElapsedMilliseconds}ms and hit {result.hitPos.x}, {result.hitPos.y}, {result.hitPos.z}");
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = new Vector3(.35f, .25f, .25f);
            go.transform.position = result.hitPos;
            Debug.DrawLine(pos, result.hitPos, new Color32(122, 0, 122, 255), 10000);
        }

        for (int i = 0; i < gameWorld.entityCount; ++i)
        {
            Graphics.DrawMesh(boxMesh, gameWorld.entities[i].position, Quaternion.identity, material, 1, mainCam);
        }
    }

    private void SpawnLotsOfGameObjects(int _spawnCount)
    {
        int dimSizeX = (int)Mathf.Floor(Mathf.Sqrt(_spawnCount));
        int dimSizeY = dimSizeX;
        float paddingX = .5f;
        float paddingY = .5f;
        for (int y = 0; y < dimSizeY; ++y)
        {
            for (int x = 0; x < dimSizeX; ++x)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(x + paddingX, y + paddingY, 0);
                paddingX += .25f;
            }
            paddingY += .25f;
            paddingX = .5f;
        }
    }

    private void SpawnLotsOfCubes(ref world _w, int _spawnCount)
    {
        int dimSizeX = (int)Mathf.Floor(Mathf.Sqrt(_spawnCount));
        int dimSizeY = dimSizeX;
        float paddingX = .5f;
        float paddingY = .5f;
        for (int y = 0; y < dimSizeY; ++y)
        {
            for (int x = 0; x < dimSizeX; ++x)
            {
                entity curr = CreateCubePrimative(new float3(x + paddingX, 0, y + paddingY));
                AddEntityToWorld(ref _w, ref curr);
                paddingX += .25f;
            }
            paddingY += .25f;
            paddingX = .5f;
        }
    }

    private static void RayCastAlongSphere(ref world _w, float3 _origin, float _radius)
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
                float3 start = new float3(x, y, z);
                start = Float3Normalize(start);
                start = start * _radius;
                float3 dir = Float3Normalize(new float3(0, 0, 0) - start);
                if (FastRaycast(ref _w, start, dir, _radius, out hitResult))
                {
                    raycast_result result2;
                    float3 newStart = start + Float3FromDirAndMag(start, dir * 1, _radius * 2);
                    //newStart = float3Addfloat3(hitResult.hitPos, float3FromDirAndMag(hitResult.hitPos, dir, 1));
                    if (FastRaycast(ref _w, newStart, dir* 1, _radius * 2, out result2))
                    {
                        Debug.DrawLine(hitResult.hitPos, result2.hitPos, Color.green, Mathf.Infinity);
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
        Debug.Assert(IsInside(_e.bounds, new float3(0, 0, 0)));
        Debug.Assert(IsInside(_e.bounds, new float3(.5f, .5f, .5f)));
        Debug.Assert(IsInside(_e.bounds, new float3(-.5f, -.5f, -.5f)));
        Debug.Assert(!IsInside(_e.bounds, new float3(1, 1, 1)));
        Debug.Assert(!IsInside(_e.bounds, new float3(-1, -1, -1)));
        Debug.Assert(!IsInside(_e.bounds, new float3(2, 2, 2)));
        Debug.Assert(!IsInside(_e.bounds, new float3(-2, -2, -2)));
    }

    private static void float3CreationTests()
    {
#if false
        float3 a = Float3FromDirAndMag(new float3(0,0,0), up, 1f);
        float3 b = Float3FromDirAndMag(new float3(0,0,0), down, 2f);
        float3 c = Float3FromDirAndMag(new float3(0,0,0), left, 3f);
        float3 d = Float3FromDirAndMag(new float3(0,0,0), right, 4f);
        float3 e = Float3FromDirAndMag(new float3(0,0,0), forward, 5f);
        float3 f = Float3FromDirAndMag(new float3(0,0,0), back, 6f);

        Debug.DrawLine(new Vector3(0, 0, 0), a, Color.red, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), b, Color.green, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), c, Color.blue, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), d, Color.magenta, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), e, Color.yellow, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), f, Color.black, Mathf.Infinity);

#else
        float3 a = Float3FromDirAndMag(new float3(0, 0, 0), ruf, 1f);
        float3 b = Float3FromDirAndMag(new float3(0, 0, 0), rub, 2f);
        float3 c = Float3FromDirAndMag(new float3(0, 0, 0), rdf, 3f);
        float3 d = Float3FromDirAndMag(new float3(0, 0, 0), rdb, 4f);
        float3 e = Float3FromDirAndMag(new float3(0, 0, 0), luf, 5f);
        float3 f = Float3FromDirAndMag(new float3(0, 0, 0), lub, 6f);
        float3 g = Float3FromDirAndMag(new float3(0, 0, 0), ldf, 7f);
        float3 h = Float3FromDirAndMag(new float3(0, 0, 0), ldb, 8f);

        Debug.DrawLine(new Vector3(0, 0, 0), a, Color.red, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), b, Color.green, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), c, Color.blue, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), d, Color.magenta, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), e, Color.yellow, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), f, Color.black, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), g, Color.white, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), h, Color.gray, Mathf.Infinity);
#endif
    }

    private void ProjectionTests()
    {
        float3 start = new float3(10, 0, 0);
        float3 end = Float3FromDirAndMag(start, ruf, 10000f);
        float3 rayCast = start - end;
        Debug.DrawLine(start, end, Color.white, Mathf.Infinity);

        float3 a = Float3FromDirAndMag(new float3(0, 0, 0), ruf, 1f);
        float3 b = Float3FromDirAndMag(new float3(0, 0, 0), rub, 2f);
        float3 c = Float3FromDirAndMag(new float3(0, 0, 0), rdf, 3f);
        float3 d = Float3FromDirAndMag(new float3(0, 0, 0), rdb, 4f);
        float3 e = Float3FromDirAndMag(new float3(0, 0, 0), luf, 5f);
        float3 f = Float3FromDirAndMag(new float3(0, 0, 0), lub, 6f);
        float3 g = Float3FromDirAndMag(new float3(0, 0, 0), ldf, 7f);
        float3 h = Float3FromDirAndMag(new float3(0, 0, 0), ldb, 8f);

        
        DebugProjectOntoRay(rayCast, a, Color.red, start);
        DebugProjectOntoRay(rayCast, b, Color.blue, start);
        DebugProjectOntoRay(rayCast, c, Color.magenta, start);
        DebugProjectOntoRay(rayCast, d, Color.grey, start);
        DebugProjectOntoRay(rayCast, e, Color.yellow, start);
        DebugProjectOntoRay(rayCast, f, Color.cyan, start);
        DebugProjectOntoRay(rayCast, g, new Color32(122, 0, 122, 255), start);
        DebugProjectOntoRay(rayCast, h, new Color32(255, 165, 0, 255), start);
    }

    private void DebugProjectOntoRay(float3 raycast, float3 other, Color _col, float3 _offset)
    {
        Debug.DrawLine(new Vector3(0, 0, 0), other, _col, Mathf.Infinity);
        float3 proj = Float3Projection(raycast, other);
        Debug.DrawLine(other, proj, Color.green, Mathf.Infinity);
    }

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

    public static bool FastRaycast(ref world _world, float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        raycast_result hitResults;
        RayCast(_world, _start, _dir, _maxDist, out hitResults);
#if false
        NativeArray<raycast_result> hitResults = new NativeArray<raycast_result>(1, Allocator.TempJob);
        var runMyJob = new RaycastJob { gameWorld = _world, start = _start, dir = _dir, maxDist = _maxDist, hitRes = hitResults };
        runMyJob.Run();
        _hitResult = runMyJob.hitRes[0];
        hitResults.Dispose();

        return _hitResult.didHit;
#else
        _hitResult = hitResults;
        return hitResults.didHit;
#endif
    }

    [System.Diagnostics.Contracts.Pure]
    public static bool SlowRayCast(world _world, float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        float3 finalPos = Float3FromDirAndMag(_start, _dir, _maxDist);
        _hitResult = new raycast_result();

        float3 checkPos = _start;
        float stepDelta = .1f;
        float currStep = stepDelta;
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
            checkPos = Float3FromDirAndMag(_start, _dir, currStep);
            currStep += stepDelta;
        }
        return false;
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

    public static bool RayCast(world _world, float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        _hitResult = new raycast_result();
        float3 finalPos = Float3FromDirAndMag(_start, _dir, _maxDist);
        _hitResult.hitPos = finalPos;
        bounds raycastAABB = MakeBoundsFromVector(_start, finalPos);

        //NativeArray<int> entitiesInsideBB = new NativeArray<int>(_world.entityCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        //int count = 0;
        for (int entityIndex = 0;
                entityIndex < _world.entityCount;
                ++entityIndex)
        {
            entity e = _world.entities[entityIndex];
            if(AABBOverlap(raycastAABB, e.bounds))
            {
                float3 proj = Float3Projection(finalPos, e.position);
                Debug.DrawLine(_start, finalPos, Color.red, Mathf.Infinity);
                Debug.DrawLine(_start, proj, Color.white, Mathf.Infinity);
                Debug.DrawLine(e.position, proj, Color.green, Mathf.Infinity);

                float magProj = Float3Mag(proj);
                if (IsInside(e.bounds, proj))
                {
                    if(Float3Mag(_hitResult.hitPos) > magProj)
                    {
                        _hitResult.didHit = true;
                        _hitResult.hitEntity = e;
                        //TODO(chris):Figure out if we need more precision here, we can back out until hitting the surface
                        _hitResult.hitPos = proj;
                    }
                }
            }
        }

        /*float stepDelta = .1f;
        float currStep = stepDelta;
        float3 checkPos = Float3FromDirAndMag(_start, _dir, currStep);
        while (currStep < _maxDist)
        {
            for (int entityIndex = 0;
                entityIndex < count;
                ++entityIndex)
            {
                int realIndex = entitiesInsideBB[entityIndex];

                if (IsInside(_world.entities[realIndex].bounds, checkPos))
                {
                    _hitResult.hitEntity = _world.entities[realIndex];
                    _hitResult.hitPos = checkPos;
                    _hitResult.didHit = true;
                    return true;
                }
            }
            checkPos = Float3FromDirAndMag(_start, _dir, currStep);
            currStep += stepDelta;
        }*/
        //entitiesInsideBB.Dispose();
        return _hitResult.didHit;
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
    public static entity CreateCubePrimative(float3 _origin)
    {
        entity result = new entity();
        result.position = _origin;
        result.scale = new float3(1,1,1);
        float minX = result.position.x - result.scale.x;
        float maxX = result.position.x + result.scale.x;

        float minY = result.position.y - result.scale.y;
        float maxY = result.position.y + result.scale.y;

        float minZ = result.position.z - result.scale.z;
        float maxZ = result.position.z + result.scale.z;

        result.bounds.minPoints = new float3(minX, minY, minZ);
        result.bounds.maxPoints = new float3(maxX, maxY, maxZ);
        return result;
    }

    [System.Diagnostics.Contracts.Pure]
    public static int2[] GetVoxels(int _voxelSize, bounds _b)
    {
        int x1 = Mathf.FloorToInt(_b.maxPoints.x);
        int z1 = Mathf.FloorToInt(_b.maxPoints.z);
        int x2 = Mathf.FloorToInt(_b.minPoints.x);
        int z2 = Mathf.FloorToInt(_b.minPoints.z);

        int xDim = x1 - x2 + 1;
        int zDim = z1 - z2 + 1;

        int2[] result = new int2[xDim*zDim];
        for(int i = 0; i < xDim; ++i)
        {
            for(int j = 0; j < zDim; ++j)
            {
                result[(zDim * i) + j] = new int2(xDim, zDim);
            }
        }
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public static int2 GetVoxel(int _voxelSize, float3 _pos)
    {
        int2 result = new int2(Mathf.FloorToInt(_pos.x/_voxelSize), Mathf.FloorToInt(_pos.z/_voxelSize));
        return result;
    }
}
