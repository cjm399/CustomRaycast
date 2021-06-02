using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


public static class Raycast
{
    [BurstCompile(CompileSynchronously = true, Debug = false, DisableSafetyChecks = true, FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
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

    public static bool FastRaycast(ref world _world, float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        NativeArray<raycast_result> hitResults = new NativeArray<raycast_result>(1, Allocator.TempJob);
        var runMyJob = new RaycastJob { gameWorld = _world, start = _start, dir = _dir, maxDist = _maxDist, hitRes = hitResults };
        runMyJob.Run();
        _hitResult = runMyJob.hitRes[0];
        hitResults.Dispose();

        return _hitResult.didHit;
    }


    [System.Diagnostics.Contracts.Pure]
    public static bool SlowRayCast(world _world, float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        float3 finalPos = Math.Float3FromDirAndMag(_start, _dir, _maxDist);
        _hitResult = new raycast_result();

        float3 checkPos = _start;
        float stepDelta = .1f;
        float currStep = stepDelta;
        while (currStep < _maxDist)
        {
            for (int entityIndex = 0;
                entityIndex < _world.entityCount;
                ++entityIndex)
            {
                if (Collision.IsInside(_world.entities[entityIndex].bounds, checkPos))
                {
                    _hitResult.hitEntity = _world.entities[entityIndex];
                    _hitResult.hitPos = checkPos;
                    _hitResult.didHit = true;
                    return true;
                }
            }
            checkPos = Math.Float3FromDirAndMag(_start, _dir, currStep);
            currStep += stepDelta;
        }
        return false;
    }

    public static bool RayCast(world _world, float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        _hitResult = new raycast_result();
        float3 finalPos = Math.Float3FromDirAndMag(_start, _dir, _maxDist);
        _hitResult.hitPos = finalPos;
        bounds raycastAABB = Collision.MakeBoundsFromVector(_start, finalPos);

        float closestSqrDistance = 100000000;
        int closestIndex = -1;
        NativeArray<int> entitiesInsideBB = new NativeArray<int>(_world.entityCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        int count = 0;
        for (int entityIndex = 0;
                entityIndex < _world.entityCount;
                ++entityIndex)
        {
            if (Collision.AABBOverlap(raycastAABB, _world.entities[entityIndex].bounds))
            {
                float3 pos = _world.entities[entityIndex].position;
                float a = _start.x - pos.x;
                a *= a;
                float b = _start.y - pos.y;
                b *= b;
                float c = _start.z - pos.z;
                c *= c;
                float sqrDist = a + b + c;
                if (sqrDist < closestSqrDistance)
                {
                    closestSqrDistance = sqrDist;
                    closestIndex = entityIndex;
                    entitiesInsideBB[count++] = entityIndex;
                }
            }
        }
        float3 checkPos = Math.Float3FromDirAndMag(_start, _dir, Mathf.Sqrt(closestSqrDistance) - 1);
        float stepDelta = .05f;
        float currStep = stepDelta;
        while (currStep < _maxDist)
        {
            for (int entityIndex = 0;
                entityIndex < count;
                ++entityIndex)
            {
                int realIndex = entitiesInsideBB[entityIndex];
                if (Collision.IsInside(_world.entities[realIndex].bounds, checkPos))
                {
                    _hitResult.hitEntity = _world.entities[realIndex];
                    _hitResult.hitPos = checkPos;
                    _hitResult.didHit = true;
                    return true;
                }
            }
            checkPos = Math.Float3FromDirAndMag(_start, _dir, currStep);
            currStep += stepDelta;
        }
        entitiesInsideBB.Dispose();
        return false;
    }

}
