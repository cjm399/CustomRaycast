using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TestLoadCollada : MonoBehaviour
{
    public string colladaFile;
    public Material debugMat;
    public Vector3 rotation;
    List<Mesh> meshes = new List<Mesh>(20000);
    world gameWorld = new world(20000);

    void Start()
    {
        string fileName = Path.Combine(Application.streamingAssetsPath, colladaFile);

        if (!File.Exists(fileName))
        {
            Debug.LogError("FILE COULD NOT BE FOUND!");
            return;
        }

        string colladaData = File.ReadAllText(fileName);


        MeshHelpers.CreateMeshesFromCollada(colladaData, ref meshes, ref gameWorld);
    }

    private void Update()
    {
        for(int i = 0; i < meshes.Count; ++i)
        {
            Graphics.DrawMesh(meshes[i], gameWorld.entities[i].position, Quaternion.Euler(rotation), debugMat, 1, Camera.main);
        }
    }
}
