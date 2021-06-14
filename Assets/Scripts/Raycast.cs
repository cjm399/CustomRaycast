using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

public static class Raycast
{
    public static NativeArray<bounds> boundsArray = new NativeArray<bounds>(30_000, Allocator.Persistent, NativeArrayOptions.ClearMemory);

    [BurstCompile(CompileSynchronously = true, Debug = false, DisableSafetyChecks = true, FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
    private struct RaycastBurstJob : IJob
    {
        public float3 start;
        public float3 dir;
        public float maxDist;
        public int entityCount;
        public NativeArray<raycast_result> hitRes;
        public NativeArray<bounds> bounds;

        public void Execute()
        {
            raycast_result hit = new raycast_result();
#if false
            SlowRayCast(bounds, entityCount, start, dir, maxDist, out hit); 
#else
            RayCast(bounds, entityCount, start, dir, maxDist, out hit);
#endif
            hitRes[0] = hit;
        }
    }

    public static bool RaycastJob(ref world _world, float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        _hitResult = new raycast_result();
        if (!boundsArray.IsCreated)
        {
            UnityEngine.Debug.Log("Not created!");
            return false;
        }
        NativeArray<raycast_result> hitResults = new NativeArray<raycast_result>(1, Allocator.TempJob);
        for(int i = 0; i < _world.entityCount; ++i)
        {
            boundsArray[i] = _world.entities[i].bounds;
        }
        var runMyJob = new RaycastBurstJob { bounds = boundsArray, entityCount = _world.entityCount, start = _start, dir = _dir, maxDist = _maxDist, hitRes = hitResults };
        runMyJob.Run();
        _hitResult = runMyJob.hitRes[0];
        hitResults.Dispose();

        return _hitResult.didHit;
    }

    [System.Diagnostics.Contracts.Pure]
    public static bool SlowRayCast(NativeArray<bounds> _bounds, int _entityCount, float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        float3 finalPos = Math.Float3FromDirAndMag(_start, _dir, _maxDist);
        _hitResult = new raycast_result();

        float3 checkPos = _start;
        float stepDelta = .1f;
        float currStep = stepDelta;
        while (currStep < _maxDist)
        {
            for (int entityIndex = 0;
                entityIndex < _entityCount;
                ++entityIndex)
            {
                if (Collision.IsInside(_bounds[entityIndex], checkPos))
                {
                    _hitResult.hitEntityIndex = entityIndex;
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

    //TODO(chris): Improve performance here.
    public static bool RayCast(NativeArray<bounds> _bounds, int _entityCount, float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        _hitResult = new raycast_result();
        float3 finalPos = Math.Float3FromDirAndMag(_start, _dir, _maxDist);
        _hitResult.hitPos = finalPos;
        bounds raycastAABB = Collision.MakeBoundsFromVector(_start, finalPos);

        NativeArray<int> entitiesInsideBB = new NativeArray<int>(_entityCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        int count = 0;
        for (int entityIndex = 0;
                entityIndex < _entityCount;
                ++entityIndex)
        {
            if (Collision.AABBOverlap(raycastAABB, _bounds[entityIndex]))
            {
                entitiesInsideBB[count++] = entityIndex;
            }
        }
        float3 checkPos = _start;
        float stepDelta = .05f;
        float currStep = stepDelta;
        while (currStep < _maxDist)
        {
            for (int entityIndex = 0;
                entityIndex < count;
                ++entityIndex)
            {
                int realIndex = entitiesInsideBB[entityIndex];
                if (Collision.IsInside(_bounds[realIndex], checkPos))
                {
                    _hitResult.hitEntityIndex = realIndex;
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
