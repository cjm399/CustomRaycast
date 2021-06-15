using Unity.Collections;
using Unity.Mathematics;

public struct bounds
{
    public float3 minPoints;
    public float3 maxPoints;
    public float3 center;
}

public struct sphereBounds
{
    public float radius;
    public float3 center;
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
    public int tags;
}

public struct world
{
    public System.Collections.Generic.List<entity> entities;
    public int entityCount, maxEntities;

    public world(int _maxEntities)
    {
        maxEntities = _maxEntities;
        entityCount = 0;
        entities = new System.Collections.Generic.List<entity>(maxEntities);
    }
}

