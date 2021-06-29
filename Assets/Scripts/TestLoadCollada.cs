using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TestLoadCollada : MonoBehaviour
{
    public string colladaFile;
    public Material debugMat;
    public Material buildingMat;
    public Vector3 rotation;
    public Texture2D buildingMapper;
    List<Mesh> meshes = new List<Mesh>(20000);
    world gameWorld = new world(20000);

    public void Initialize()
    {
        string fileName = Path.Combine(Application.streamingAssetsPath, colladaFile);

        if (!File.Exists(fileName))
        {
            Debug.LogError("FILE COULD NOT BE FOUND!");
            return;
        }

        string colladaData = File.ReadAllText(fileName);

        MeshHelpers.CreateMeshesFromCollada(colladaData, ref meshes, ref gameWorld);

        InitializePropertyData propData = FindObjectOfType<InitializePropertyData>();

        int res = 0, ind = 0, com = 0;

        byte r = 0, g = 0;
        for(int i = 0; i < gameWorld.entityCount; ++i)
        {
            Color col = new Color32(r, g, 0, 255);
            r += 1;
            if(r >= 255)
            {
                g += 1;
                r = 0;
            }
            Color rand = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
            if(propData.placeIdToPropertyData.ContainsKey(gameWorld.entities[i].name))
            {
                int index = gameWorld.entities[i].vertexColorIndex;
                Color32 destColor = PropertyData.zoningMappingColors[propData.placeIdToPropertyData[gameWorld.entities[i].name].zone];
                buildingMapper.SetPixel(index % 256, index / 256, destColor);

                if (propData.placeIdToPropertyData[gameWorld.entities[i].name].zone == (int)Zones.Comercial)
                {
                    com += 1;
                }
                if (propData.placeIdToPropertyData[gameWorld.entities[i].name].zone == (int)Zones.Residential)
                {
                    res++;
                }
                if (propData.placeIdToPropertyData[gameWorld.entities[i].name].zone == (int)Zones.Industrial)
                {
                    ++ind;
                }
            }
            else
            {
                int index = gameWorld.entities[i].vertexColorIndex;
                buildingMapper.SetPixel(index % 256, index / 256, new Color(0,0,0,0));
                //Debug.LogError($"Could not find building {gameWorld.entities[i].name}");
            }
        }
        Debug.Log($"Ind: {ind} Res: {res} Com {com}");
        buildingMapper.Apply();
        string path = Path.Combine(Application.streamingAssetsPath, "testImg.png");
        File.WriteAllBytes(path, buildingMapper.EncodeToPNG());
    }


    private void Update()
    {
#if false
        for (int i = 0; i < meshes.Count; ++i)
        {
            Graphics.DrawMesh(meshes[i], gameWorld.entities[i].position, Quaternion.Euler(rotation), debugMat, 1, Camera.main);
        } 
#endif
    }
}
