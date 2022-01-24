using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Static
{
    public delegate void NoArgs();
    public delegate void Byte_Delegate(byte _b);
    public delegate void Float_Delegate(float _val);
    public delegate void GameObjectBoolDelegate(GameObject _go, bool _val);


    [System.Diagnostics.Contracts.Pure]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void ToggleCanvasGroup(ref CanvasGroup _cg, bool _targetValue = true)
    {
        _cg.alpha = BoolAsPercent(_targetValue);
        _cg.blocksRaycasts = _targetValue;
        _cg.interactable = _targetValue;
    }

    [System.Diagnostics.Contracts.Pure]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static void ToggleCanvasGroup(ref CanvasGroup _cg)
    {
        bool isVisible = !PercentAsBool(_cg.alpha);
        _cg.alpha = BoolAsPercent(isVisible);
        _cg.blocksRaycasts = isVisible;
        _cg.interactable = isVisible;
    }

    [System.Diagnostics.Contracts.Pure]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool isHoveringUIElement()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool result = false;
        if (results.Count > 0)
        {
            for(int i = 0; i < results.Count; ++i)
            {
                result |= results[0].gameObject.layer == 5; // 5 is Unity's UI layer
            }
        }

        return result;
    }

    [System.Diagnostics.Contracts.Pure]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string FormatAsMoney(ulong _value)
    {
        return string.Format("{0:C0}", _value);
    }

    [System.Diagnostics.Contracts.Pure]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string FormatAsMoney(long _value)
    {
        return string.Format("{0:C0}", _value);
    }

    [System.Diagnostics.Contracts.Pure]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string FormatAsMoney(float _value)
    {
        return string.Format("{0:C0}", _value);
    }

    [System.Diagnostics.Contracts.Pure]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string FormatAsNumber(float _value)
    {
        return _value.ToString("N0");
    }

    [System.Diagnostics.Contracts.Pure]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool PercentAsBool(float _percent)
    {
        return _percent >= 1;
    }

    [System.Diagnostics.Contracts.Pure]
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static float BoolAsPercent(bool _bool)
    {
        return (_bool ? 1 : 0);
    }
}
