using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameWorld
{

    public static bool AddEntity(ref world _w, ref entity _e)
    {
        if (_w.entityCount - 1 == _w.maxEntities)
        {
            _w.maxEntities *= 2;
        }
        _w.entities.Add(_e);
        ++_w.entityCount;
        return true;
    }
}
