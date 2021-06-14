using Unity.Collections;
using Unity.Mathematics;

public struct bounds
{
    public float3 minPoints;
    public float3 maxPoints;
}

public struct raycast_result
{
    public bool didHit;
    public int hitEntityIndex;
    public float3 hitPos;
}

public struct entity
{
    public float3 position;
    public float3 scale;
    public bounds bounds;
    public string name;
}

public struct world
{
    public System.Collections.Generic.List<entity> entities;
    public int entityCount, maxEntities;

    public world(int _maxEntities)
    {
        maxEntities = _maxEntities;
        entityCount = 0;
        entities = new System.Collections.Generic.List<entity>(maxEntities);//new NativeArray<entity>(maxEntities, Allocator.Persistent, NativeArrayOptions.ClearMemory);
    }
}

