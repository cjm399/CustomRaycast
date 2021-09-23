using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

public static class Raycast

{
    public static NativeArray<bounds> boundsArray = new NativeArray<bounds>(30_000, Allocator.Persistent, NativeArrayOptions.ClearMemory);

    public static readonly float2 areaSize = new float2(100, 100);
    public static readonly int partitionCastcades = 3;
    public static readonly float partitionGranularity = 1f / 4f;
    public static readonly float2 partitionSize = areaSize * partitionGranularity;
    public static readonly int partitionsCount = (int)System.Math.Round(1f / partitionGranularity);
    public static readonly int topLevelPartitions = partitionsCount * partitionsCount;

    public static NativeArray<bounds> partitionsArray = new NativeArray<bounds>(topLevelPartitions +
        (topLevelPartitions * topLevelPartitions) +
        (topLevelPartitions * topLevelPartitions * topLevelPartitions * topLevelPartitions)
        , Allocator.Persistent, NativeArrayOptions.ClearMemory);

    [BurstCompile(CompileSynchronously = true, Debug = false, DisableSafetyChecks = true, FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
    private struct RaycastBurstJob : IJob
    {
        public float3 start;
        public float3 dir;
        public float maxDist;
        public int entityCount;
        public NativeArray<raycast_result> hitRes;
        public NativeArray<bounds> bounds;
        public NativeArray<bounds> partitions;
        public int partitionsSize;

        public void Execute()
        {
            raycast_result hit = new raycast_result();
            PartitionRayCast(bounds, entityCount, partitions, partitionsSize, start, dir, maxDist, out hit);
            //RayCast(bounds, entityCount, start, dir, maxDist, out hit);
            hitRes[0] = hit;
        }
    }

    public static void InitRaycastData(ref world _world)
    {
        if (!boundsArray.IsCreated)
        {
            UnityEngine.Debug.Log("Bounds array not created!");
        }
        if (!partitionsArray.IsCreated)
        {
            UnityEngine.Debug.Log("Partitions array not created!");
        }

        float2 areaHalfSize = areaSize / 2f;
        float2 partitionHalfSize = partitionSize / 2f;
        float2 partitionQuarterSize = partitionSize / 4f;
        float2 partitionEighthSize = partitionSize / 8f;
        int indexOffset = 0;

        for (int partIndex = 0; partIndex < partitionsCount * partitionsCount; ++partIndex)
        {
            int x = partIndex % partitionsCount;
            int y = partIndex / partitionsCount;

            float xCenter = (-areaHalfSize.x + partitionHalfSize.x) + (x * partitionSize.x);
            float xMin = xCenter - partitionHalfSize.x;
            float xMax = xCenter + partitionHalfSize.x;

            float yCenter = (-areaHalfSize.y + partitionHalfSize.y) + (y * partitionSize.y);
            float yMin = yCenter - partitionHalfSize.y;
            float yMax = yCenter + partitionHalfSize.y;
            bounds b = new bounds();
            b.center = new float3(xCenter, 5, yCenter);
            b.maxPoints = new float3(xMax, 10, yMax);
            b.minPoints = new float3(xMin, 0, yMin);
            partitionsArray[indexOffset++] = b;

            //casdcade level 2
            for (int cpIndex = 0; cpIndex < partitionsCount * partitionsCount; ++cpIndex)
            {
                int cx = cpIndex % partitionsCount;
                int cy = cpIndex / partitionsCount;

                float cpx = (xCenter - partitionQuarterSize.x) + (cx * partitionHalfSize.x);
                float cpxMin = cpx - partitionQuarterSize.x;
                float cpxMax = cpx + partitionQuarterSize.x;

                float cpy = (yCenter - partitionQuarterSize.y) + (cy * partitionHalfSize.y);
                float cpyMin = cpy - partitionQuarterSize.y;
                float cpyMax = cpy + partitionQuarterSize.y;

                bounds cb = new bounds();
                cb.center = new float3(cpx, 50, cpy);
                cb.maxPoints = new float3(cpxMax, 100, cpyMax);
                cb.minPoints = new float3(cpxMin, 0, cpyMin);
                partitionsArray[(partIndex * topLevelPartitions) + topLevelPartitions + cpIndex] = cb;

                //casdcade level 3
                for (int cp2Index = 0; cp2Index < partitionsCount * partitionsCount; ++cp2Index)
                {
                    int cx2 = cp2Index % partitionsCount;
                    int cy2 = cp2Index / partitionsCount;

                    float cp2x = (cpx - partitionEighthSize.x) + (cx2 * partitionQuarterSize.x);
                    float cp2xMin = cp2x - partitionEighthSize.x;
                    float cp2xMax = cp2x + partitionEighthSize.x;

                    float cp2y = (cpy - partitionEighthSize.y) + (cy2 * partitionQuarterSize.y);
                    float cp2yMin = cp2y - partitionEighthSize.y;
                    float cp2yMax = cp2y + partitionEighthSize.y;

                    bounds cb2 = new bounds();
                    cb2.center = new float3(cp2x, 50, cp2y);
                    cb2.maxPoints = new float3(cp2xMax, 100, cp2yMax);
                    cb2.minPoints = new float3(cp2xMin, 0, cp2yMin);
                    partitionsArray[(((partIndex * topLevelPartitions) + topLevelPartitions + cpIndex) * topLevelPartitions) + topLevelPartitions + cp2Index] = cb2;
                }
            }
        }

        for (int i = 0; i < _world.entityCount; ++i)
        {
            boundsArray[i] = _world.entities[i].bounds;
        }
    }

    public static bool RaycastJob(ref world _world, float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        _hitResult = new raycast_result();
        NativeArray<raycast_result> hitResults = new NativeArray<raycast_result>(1, Allocator.TempJob);
        var runMyJob = new RaycastBurstJob
        {
            bounds = boundsArray,
            entityCount = _world.entityCount,
            partitions = partitionsArray,
            partitionsSize = topLevelPartitions,
            start = _start,
            dir = _dir,
            maxDist = _maxDist,
            hitRes = hitResults,
        };

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

    public static bool PartitionRayCast(NativeArray<bounds> _bounds, int _entityCount,
        NativeArray<bounds> _partitions, int _paritionsInPartition,
        float3 _start, float3 _dir, float _maxDist, out raycast_result _hitResult)
    {
        _hitResult = new raycast_result();
        float3 finalPos = Math.Float3FromDirAndMag(_start, _dir, _maxDist);
        _hitResult.hitPos = finalPos;

        int parititionsCount = _partitions.Length;
        int perPartition = _paritionsInPartition;

        int partitionSide = (int)System.Math.Sqrt(parititionsCount) / 2;
        int partSideCube = (int)System.Math.Pow(System.Math.Sqrt(partitionSide), 3);

        NativeArray<int> entitiesInsideBB = new NativeArray<int>(_entityCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        NativeArray<int> hits = new NativeArray<int>(partSideCube * 2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        NativeArray<int> partitionsInRaycast = new NativeArray<int>(partSideCube * 2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        NativeArray<int> partitionsInRaycast2 = new NativeArray<int>(partSideCube * 2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        int entitiesCount = 0;
        int hitPartitionsCount = 0;

        float3 checkPos = _start;
        float stepDelta = 10f;
        float currStep = stepDelta;
        //Build Level 1 partitions
        while (currStep < _maxDist)
        {
            for (int partitionIndex = 0;
                partitionIndex < perPartition;
                ++partitionIndex)
            {
                bool contained = false;
                for (int i = 0; i < hitPartitionsCount; ++i)
                {
                    if (hits[i] == partitionIndex)
                    {
                        contained = true;
                        break;
                    }
                }

                if (!contained && Collision.IsInside(_partitions[partitionIndex], checkPos))
                {
                    hits[hitPartitionsCount] = partitionIndex;
                    int baseIndex = (partitionIndex * perPartition) + perPartition;
                    for (int partition = 0; partition < perPartition; ++partition)
                    {
                        partitionsInRaycast[(hitPartitionsCount * perPartition) + partition] = baseIndex + partition;
                    }
                    ++hitPartitionsCount;
                }
            }
            checkPos = Math.Float3FromDirAndMag(_start, _dir, currStep);
            currStep += stepDelta;
        }

        //Build level 2 partitions
        int endCount = hitPartitionsCount * perPartition;
        hitPartitionsCount = 0;
        checkPos = _start;
        stepDelta = 5f;
        currStep = stepDelta;
        while (currStep < _maxDist)
        {
            for (int partitionIndex = 0;
                partitionIndex < endCount;
                ++partitionIndex)
            {
                bool contained = false;
                for (int i = 0; i < hitPartitionsCount; ++i)
                {
                    if (hits[i] == partitionIndex)
                    {
                        contained = true;
                        break;
                    }
                }

                if (!contained && Collision.IsInside(_partitions[partitionsInRaycast[partitionIndex]], checkPos))
                {
                    hits[hitPartitionsCount] = partitionIndex;
                    int baseIndex = (partitionsInRaycast[partitionIndex] * perPartition) + perPartition;
                    for (int partition = 0; partition < perPartition; ++partition)
                    {
                        partitionsInRaycast2[(hitPartitionsCount * perPartition) + partition] = baseIndex + partition;
                    }
                    ++hitPartitionsCount;
                }
            }
            checkPos = Math.Float3FromDirAndMag(_start, _dir, currStep);
            currStep += stepDelta;
        }

        //Build level 3 partitions
        endCount = hitPartitionsCount * perPartition;
        hitPartitionsCount = 0;
        checkPos = _start;
        stepDelta = 2.5f;
        currStep = stepDelta;
        while (currStep < _maxDist)
        {
            for (int partitionIndex = 0;
                partitionIndex < endCount;
                ++partitionIndex)
            {
                bool contained = false;
                for (int i = 0; i < hitPartitionsCount; ++i)
                {
                    if (hits[i] == partitionIndex)
                    {
                        contained = true;
                        break;
                    }
                }

                if (!contained && Collision.IsInside(_partitions[partitionsInRaycast2[partitionIndex]], checkPos))
                {
                    hits[hitPartitionsCount] = partitionIndex;
                    partitionsInRaycast[hitPartitionsCount] = partitionsInRaycast2[partitionIndex];
                    ++hitPartitionsCount;
                }
            }
            checkPos = Math.Float3FromDirAndMag(_start, _dir, currStep);
            currStep += stepDelta;
        }

        for (int entityIndex = 0;
                entityIndex < _entityCount;
                ++entityIndex)
        {
            for (int partitionIndex = 0; partitionIndex < hitPartitionsCount; ++partitionIndex)
            {
                if (Collision.AABBOverlap(_partitions[partitionsInRaycast[partitionIndex]], _bounds[entityIndex]))
                {
                    entitiesInsideBB[entitiesCount++] = entityIndex;
                    break;
                }
            }
        }

        checkPos = _start;
        stepDelta = .05f;
        currStep = stepDelta;
        while (currStep < _maxDist)
        {
            for (int entityIndex = 0;
                entityIndex < entitiesCount;
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
        partitionsInRaycast.Dispose();
        partitionsInRaycast2.Dispose();
        hits.Dispose();
        return false;
    }

}
