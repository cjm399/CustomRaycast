using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSelector : MonoBehaviour
{
    public Transform unityRaycastTransform;
    public float radius;

    sphereBounds selectionSphere;

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        selectionSphere.radius = radius;

        if(Input.GetKeyDown(KeyCode.Mouse2))
        {
            if (Physics.Raycast(ray, out hit, 10000))
            {
                unityRaycastTransform.position = hit.point;
                selectionSphere.center = unityRaycastTransform.position;
                CityManager.Instance.SelectAllBuildingsInSphere(selectionSphere);
            }
        }
    }
}
