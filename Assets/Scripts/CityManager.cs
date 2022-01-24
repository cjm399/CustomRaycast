using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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

    private Vector3 groundPos = new Vector3(0,-.1f,0);

    public world gameWorld;
    private List<Mesh> debugMeshes;

    private int selectedEntityIndex = -1;

    private Mesh groundMesh;

    public static CityManager Instance;

    private void Awake()
    {
        if(Instance)
        {
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    public void SelectAllBuildingsInSphere(sphereBounds _s)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        List<entity> entities = new List<entity>();
        for(int i = 0; i < gameWorld.entityCount; ++i)
        {
            if(Collision.AABBSphereOverlap(gameWorld.entities[i].bounds, _s))
            {
                entities.Add(gameWorld.entities[i]);
                Debug.Log($"{gameWorld.entities[i].name} was in overlap sphere.");
            }
        }
        sw.Stop();
        Debug.Log($"Select All Buildings In Sphere Took {sw.ElapsedMilliseconds}ms");
    }

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
        MeshHelpers.CreateMeshesFromEpa(objData);
        Raycast.InitRaycastData(ref gameWorld);

        //Generate Ground plane.
        bounds ground = new bounds();
        ground.minPoints = new float3(-1000, -.1f, -1000);
        ground.maxPoints = new float3(1000, .1f, 1000);

        groundMesh = MeshHelpers.MakeCubeFromBounds(ground);
        groundMesh.RecalculateBounds();
        entity groundEntity = new entity();
        groundEntity.bounds.minPoints = groundMesh.bounds.min;
        groundEntity.bounds.maxPoints = groundMesh.bounds.max;
        groundEntity.name = "Ground";
        GameWorld.AddEntity(ref gameWorld, ref groundEntity);
    }

    private void OnDisable()
    {
        Raycast.boundsArray.Dispose();
        Raycast.partitionsArray.Dispose();
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
            selectedEntityIndex = result.hitEntityIndex;
            if (!result.didHit)
            {
                hitStr = "did not hit";
                drawColor = new Color32(255, 0, 0, 255);
                selectedEntityIndex = -1;
            }
            Debug.Log($"Raycast took {sw.ElapsedMilliseconds}ms and {hitStr}");
            Debug.DrawLine(pos, result.hitPos, drawColor, 10000);
        }

        if (selectedEntityIndex >= 0)
        {
            entity selectedEntity = gameWorld.entities[selectedEntityIndex];
            if(GameWorld.HasTag(ref selectedEntity, global::tag.BUILDING))
            {
                selectedBuildingTransform.position = selectedEntity.position;
                buildingUI.GetComponentInChildren<TMPro.TMP_Text>().text = selectedEntity.name;
                UIHelpers.WorldSpaceToScreenSpace(ref selectedBuildingTransform, ref buildingUI, ref dynamicBuildingCanvas);
            }
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
#if false //Draw the ground entity.
        Graphics.DrawMesh(groundMesh, groundPos, Quaternion.identity, debugMat, 1, Camera.main);
#endif
    }
}
