using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GenerateBuildings : MonoBehaviour
{
    public string objFile = "buildings_obj.obj";
    public Mesh mesh;
    public Material mat;

    private world gameWorld;

    private List<Mesh> meshes = new List<Mesh>(5000);

    private void Start()
    {
        string fileName = Path.Combine(Application.streamingAssetsPath, objFile);

        if (!File.Exists(fileName))
        {
            Debug.LogError("FILE COULD NOT BE FOUND!");
            return;
        }

        string objData = File.ReadAllText(fileName);

        ParseObj(objData);
    }

    private void OnDisable()
    {
        gameWorld.entities.Dispose();
    }

    void ParseObj(string _obj)
    {
        string[] lines = _obj.Split(new string[] { System.Environment.NewLine, "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        entity curr = new entity();
        Mesh currMesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();
        List < entity > entities = new List<entity>();
        bool first = true;
        foreach(string line in lines)
        {
            char function = line[0];
            //New object coming in!!!
            if(function == 'o')
            {
                if(!first)
                {
                    entities.Add(curr);
                    currMesh.SetVertices(verts);
                    currMesh.SetTriangles(triangles, 0);
                    meshes.Add(currMesh);
                }
                else
                {
                    first = false;
                }
                int endNameIndex = line.IndexOf("_Mesh");
                string objName = line.Substring(2, endNameIndex -2);
                print(objName);
                curr = new entity();
                currMesh = new Mesh();
                verts = new List<Vector3>();
                triangles = new List<int>();
                triangles.Clear();
                verts.Clear();
            }
            //New vert coming in
            else if (function == 'v')
            {
                string[] points = line.Substring(2).Split(' ');
                float x = float.Parse(points[0]);
                float y = float.Parse(points[1]);
                float z = float.Parse(points[2]);

                curr.bounds = SetBounds(ref curr.bounds, x, y, z);

                if(curr.position.y > y)
                {
                    curr.position.y = y;
                }
                curr.position.x += x;
                curr.position.z += z;
                curr.position.x /= 2.0f;
                curr.position.z /= 2.0f;
                verts.Add(new Vector3(x, y, z));
            }
            else if(function == 'f')
            {
                string[] points = line.Substring(2).Split(' ');
                int a = int.Parse(points[0]);
                int b = int.Parse(points[1]);
                int c = int.Parse(points[2]);
                triangles.AddRange(new int[] { a -1, b -1, c -1 });
            }
        }
        gameWorld = new world(entities.Count);
        for (int i = 0; i < entities.Count; ++i)
        {
            gameWorld.entities[i] = entities[i];
            gameWorld.entityCount++;

        }
    }

    public bounds SetBounds(ref bounds _b, float _x, float _y, float _z)
    {
        bounds result = _b;
        if(_b.minPoints.x > _x)
        {
            result.minPoints.x = _x;
        }
        else if(_b.maxPoints.x < _x)
        {
            result.maxPoints.x = _x;
        }

        if (_b.minPoints.y > _y)
        {
            result.minPoints.y = _y;
        }
        else if (_b.maxPoints.y < _y)
        {
            result.maxPoints.y = _y;
        }

        if (_b.minPoints.z > _z)
        {
            result.minPoints.z = _z;
        }
        else if (_b.maxPoints.z < _z)
        {
            result.maxPoints.z = _z;
        }

        return result;
    }

    private void Update()
    {
        for(int i = 0; i < gameWorld.entityCount; ++i)
        {
            Graphics.DrawMesh(meshes[i], gameWorld.entities[i].position, Quaternion.identity, mat, 1, Camera.main);  
        }
    }
}
