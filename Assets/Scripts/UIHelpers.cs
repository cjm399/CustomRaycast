using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIHelpers
{
    public static void WorldSpaceToScreenSpace(ref Transform _trans, ref RectTransform _rectTrans, ref Canvas _canvas)
    {
        Camera cam = Camera.main;
        float width = _rectTrans.rect.xMax - _rectTrans.rect.xMin;
        float height = _rectTrans.rect.yMax - _rectTrans.rect.yMin;

        Debug.Log($"Pos {_trans.position} vs the mesh center: {_trans.GetComponent<MeshFilter>().mesh.bounds.center}");

        Vector3 screenPos = cam.WorldToViewportPoint(_trans.TransformPoint(Vector3.zero));

        screenPos.x = Mathf.Clamp01(screenPos.x);
        screenPos.y = Mathf.Clamp01(screenPos.y);
        _rectTrans.anchorMin = screenPos;
        _rectTrans.anchorMax = new Vector2(.5f + (screenPos.x - .5f), .5f + (screenPos.y - .5f));
        _rectTrans.anchoredPosition = Vector2.zero;

        int canvasWidth = (int)(_canvas.pixelRect.xMax / _canvas.scaleFactor);
        int canvasHeight = (int)(_canvas.pixelRect.yMax / _canvas.scaleFactor);

        Vector2 uiExtents = new Vector2(width / canvasWidth, height / canvasHeight) / 2;

        screenPos = cam.WorldToViewportPoint(_trans.TransformPoint(Vector3.zero));

        if (screenPos.z < 0)
        {
            screenPos.y *= -1;
        }

        screenPos.x = Mathf.Clamp(screenPos.x, uiExtents.x, 1 - uiExtents.x);
        screenPos.y = Mathf.Clamp(screenPos.y, uiExtents.y, 1 - uiExtents.y);

        if (screenPos.z < 0)
        {
            screenPos.x = 1 - screenPos.x;
            screenPos.x = Mathf.Clamp(screenPos.x, uiExtents.x, 1 - uiExtents.x);
        }

        _rectTrans.anchorMin = screenPos;
        _rectTrans.anchorMax = screenPos;
    }
}
