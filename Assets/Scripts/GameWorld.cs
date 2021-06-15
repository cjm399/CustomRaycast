using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum tag
{
    BUILDING = 1,
}

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

    public static void SetTags(ref entity _e, params tag[] _tags)
    {
        _e.tags = 0;
        for (int i = 0; i < _tags.Length; ++i)
        {
            _e.tags |= (int)_tags[i];
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool HasTag(ref entity _e, tag _tag)
    {
        bool result = (_e.tags & (int)_tag) != 0;
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void AddTag(ref entity _e, tag _tag)
    {
        _e.tags |= (int)_tag;
    }

    public static void RemoveTag(ref entity _e, tag _tag)
    {
        _e.tags &= ~(int)_tag;
    }
}
