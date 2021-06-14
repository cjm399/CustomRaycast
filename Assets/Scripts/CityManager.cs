using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Mathematics;

public class CityManager : MonoBehaviour
{
    public string objFile = "buildings_obj.obj";
    public Vector3 rotation;
    public bool debugRender = false;
    public Material debugMat;
    public Canvas dynamicBuildingCanvas;
    public RectTransform buildingUI;
    public Transform selectedBuildingTransform;

    private world gameWorld;
    private List<Mesh> debugMeshes;

    private int selectedEntity = -1;

    private void Start()
    {
        string fileName = Path.Combine(Application.streamingAssetsPath, objFile);

        if (!File.Exists(fileName))
        {
            Debug.LogError("FILE COULD NOT BE FOUND!");
            return;
        }

        string objData = File.ReadAllText(fileName);
        int objLen = (int)(objData.Split('o').Length * 1.2f);
        debugMeshes = new List<Mesh>(objLen);

        gameWorld = new world(objLen);
        MeshHelpers.CreateMeshesFromObj(objData, ref debugMeshes, ref gameWorld);
    }

    private void OnDisable()
    {
        Raycast.boundsArray.Dispose();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float3 dir = ray.direction;
            float3 pos = ray.origin;
            raycast_result result;
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            Raycast.RaycastJob(ref gameWorld, pos, dir, 1000, out result);
            //Raycast.SlowRayCast(gameWorld, pos, dir, 1000, out result);
            sw.Stop();
            string hitStr = $"hit {gameWorld.entities[result.hitEntityIndex].name} entity as position {result.hitPos}";
            Color drawColor = new Color32(122, 0, 122, 255);
            selectedEntity = result.hitEntityIndex;
            if (!result.didHit)
            {
                hitStr = "did not hit";
                drawColor = new Color32(255, 0, 0, 255);
                selectedEntity = -1;
            }
            Debug.Log($"Raycast took {sw.ElapsedMilliseconds}ms and {hitStr}");
            Debug.DrawLine(pos, result.hitPos, drawColor, 10000);
        }

        if (selectedEntity >= 0)
        {
            selectedBuildingTransform.position = gameWorld.entities[selectedEntity].position;
            buildingUI.GetComponentInChildren<TMPro.TMP_Text>().text = gameWorld.entities[selectedEntity].name;
            UIHelpers.WorldSpaceToScreenSpace(ref selectedBuildingTransform, ref buildingUI, ref dynamicBuildingCanvas);
            //UIHelpers.SnapUiToBuilding(ref selectedBuildingTransform, ref buildingUI, ref dynamicBuildingCanvas);
        }

        if (debugRender)
        {
            Camera cam = Camera.main;
            Quaternion rot = Quaternion.Euler(rotation);
            for (int i = 0; i < gameWorld.entityCount; ++i)
            {
                Graphics.DrawMesh(debugMeshes[i], gameWorld.entities[i].position, rot, debugMat, 1, cam);
            }
        }
    }
}
