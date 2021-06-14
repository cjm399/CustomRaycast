using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GenerateBuildings : MonoBehaviour
{
    public string objFile = "buildings_obj.obj";
    public Material mat;
    public bool shouldRender = true;

    public Vector3 rotation;

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
    }

    void ParseObj(string _obj)
    {
        string[] lines = _obj.Split(new string[] { System.Environment.NewLine, "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        entity curr = new entity();
        Mesh currMesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();
        List <entity> entities = new List<entity>();
        bool first = true;
        int vertexOffset = 0;
        int count = 0;
        foreach(string line in lines)
        {
            char function = line[0];
            //New object coming in!!!
            if(function == 'o')
            {
                count++;
                if (!first)
                {
                    curr.position.x /= verts.Count;
                    curr.position.z /= verts.Count;

                    for(int i = 0; i < verts.Count; ++i)
                    {
                        Vector3 newPos = verts[i];
                        newPos.x -= curr.position.x;
                        newPos.y -= curr.position.y;
                        newPos.z -= curr.position.z;
                        verts[i] = newPos;
                    }

                    currMesh.SetVertices(verts);
                    currMesh.SetTriangles(triangles, 0);
                    currMesh.RecalculateNormals();
                    currMesh.RecalculateBounds();
                    curr.bounds.minPoints = currMesh.bounds.min;
                    curr.bounds.maxPoints = currMesh.bounds.max;

                    currMesh = MakeCubeFromBounds(curr.bounds);
                    currMesh.RecalculateNormals();
                    entities.Add(curr);
                    meshes.Add(currMesh);
                    vertexOffset += verts.Count;
                }
                else
                {
                    first = false;
                }
                int endNameIndex = line.IndexOf("_Mesh");
                string objName = line.Substring(2, endNameIndex -2);
                //print(objName);
                curr = new entity();
                currMesh = new Mesh();
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
                verts.Add(new Vector3(x, y, z));
            }
            else if(function == 'f')
            {
                string[] points = line.Substring(2).Split(' ');
                //obj vert index starts at 1.
                int a = int.Parse(points[0]) -1 - vertexOffset;
                int b = int.Parse(points[1]) -1 - vertexOffset;
                int c = int.Parse(points[2]) -1 - vertexOffset;
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);
            }
        }
        Debug.Log(count);
        gameWorld = new world(entities.Count);
        for (int i = 0; i < entities.Count; ++i)
        {
            gameWorld.entities[i] = entities[i];
            gameWorld.entityCount++;

        }
    }

    public Mesh MakeCubeFromBounds(bounds _b)
    {
        Mesh result = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        float minx = _b.minPoints.x;
        float miny = _b.minPoints.y;
        float minz = _b.minPoints.z;
        float maxx = _b.maxPoints.x;
        float maxy = _b.maxPoints.y;
        float maxz = _b.maxPoints.z;

        verts.Add(new Vector3(minx, miny, minz));
        verts.Add(new Vector3(minx, miny, maxz));
        verts.Add(new Vector3(minx, maxy, minz));
        verts.Add(new Vector3(minx, maxy, maxz));
        verts.Add(new Vector3(maxx, miny, minz));
        verts.Add(new Vector3(maxx, miny, maxz));
        verts.Add(new Vector3(maxx, maxy, minz));
        verts.Add(new Vector3(maxx, maxy, maxz));

        tris.AddRange(new int[] { 0, 1, 2 });
        tris.AddRange(new int[] { 1, 3, 2 });
        tris.AddRange(new int[] { 0, 2, 6 });
        tris.AddRange(new int[] { 0, 6, 4 });
        tris.AddRange(new int[] { 4, 6, 7 });
        tris.AddRange(new int[] { 4, 7, 5 });
        tris.AddRange(new int[] { 5, 7, 3 });
        tris.AddRange(new int[] { 5, 3, 1 });
        tris.AddRange(new int[] { 2, 3, 6 });
        tris.AddRange(new int[] { 3, 7, 6 });
        tris.AddRange(new int[] { 0, 4, 5 });
        tris.AddRange(new int[] { 0, 5, 1 });

        result.SetVertices(verts);
        result.SetTriangles(tris, 0);
        result.RecalculateNormals();
        return result;

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
        if(shouldRender)
        {
            for (int i = 0; i < gameWorld.entityCount; ++i)
            {
                Graphics.DrawMesh(meshes[i], gameWorld.entities[i].position, Quaternion.Euler(rotation), mat, 1, Camera.main);  
            }
        }
    }
}
