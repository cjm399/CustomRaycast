using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public v3[] points;
}

public struct entity
{
    public v3 position;
    public v3 scale;
}

public struct world
{
    public entity[] entities;
    public int entityCount, maxEntities;

    public world(int _maxEntities)
    {
        maxEntities = _maxEntities;
        entityCount = 0;
        entities = new entity[maxEntities];
    }
}

public class game : MonoBehaviour
{
    private world gameWorld;

    private void Start()
    {
        gameWorld = new world(1000);
        entity test = CreateCubePrimative(new global::v3(0,0,0));
        AddEntityToWorld(ref gameWorld, ref test);
    }

    public static void AddEntityToWorld(ref world _w, ref entity _e)
    {
        if(_w.entityCount -1 == _w.maxEntities)
        {
            _w.maxEntities *= 2;
            System.Array.Resize<entity>(ref _w.entities, _w.maxEntities);
        }
        _w.entities[_w.entityCount++] = _e;
    }

    public static entity CreateCubePrimative(v3 _origin)
    {
        entity result = new entity();
        result.position = _origin;
        result.scale = new global::v3(1,1,1);
        return result;
    }
}
