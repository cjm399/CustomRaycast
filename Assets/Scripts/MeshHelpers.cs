using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public static class MeshHelpers
{
    public static void CreateMeshesFromEpa(string _objContent)
    {
        bool isValidObj = _objContent != null && _objContent.Length > 0;
        if (isValidObj)
        {
            entity curr = new entity();
            Mesh currMesh = new Mesh();
            List<Vector3> verts = new List<Vector3>(100);
            List<int> triangles = new List<int>(550);
            bool first = true;
            int vertexOffset = 0;
            int count = 0;

            int maxVertCount = 0;
            int maxTriCount = 0;

            //int tcount = 0;

            int remainingSize = _objContent.Length;
            int startSize = remainingSize;

            float systemMemory = Mathf.Clamp((float)SystemInfo.systemMemorySize, 512, (float)SystemInfo.systemMemorySize);

            //Only want to use half of avalible memory.
            systemMemory = (systemMemory / 1024f) / 2f;
            int lookupLength = (int)(100_000 * systemMemory);
            //WebGL sucks, so we'll use a quarter?
#if UNITY_WEBGL
            lookupLength = (int)(100_000 * systemMemory / 2);
#endif

            int lookupIndex = lookupLength;
            string[] lines = new string[lookupLength / 7]; //7 seems to be one of the shorter character counts for a line.
            string line = "";
            string function = "";
            //We can afford running slower than 30FPS here
            //Web GL seems to crash from running low on memory at slower FPS.
            float msPerFrame = 1000f / 3f;
            string[] points = new string[3];
            string[] colorVals = new string[4];
            while (remainingSize > 0)
            {
                lookupIndex = Mathf.Min(lookupLength, remainingSize - 1);
                char lookupChar = _objContent[lookupIndex];
                //Need to split into objects.
                while (lookupChar != 'o' && lookupIndex != remainingSize - 1)
                {
                    lookupChar = _objContent[++lookupIndex];
                }
                if (lookupChar == 'o')
                {
                    lookupIndex--;
                }
                string cluster = _objContent.Substring(0, lookupIndex);
                int linesLength = FastSplit(ref cluster, ref lines, '\n');
                //Debug.Log($"lines length : {linesLength}");
                //Debug.Log($"First function {lines[0].Split(' ')[0]}");

                for (int lineIndex = 0; lineIndex < linesLength; ++lineIndex)
                {
                    line = lines[lineIndex];
                    if (line[line.Length - 1] == '\r')
                    {
                        line = line.Substring(0, line.Length - 1);
                    }

                    function = line.Split(' ')[0];
                    //New object coming in!!!
                    if (function == "o")
                    {
                        count++;
                        if (!first)
                        {
                            curr.position.x /= (verts.Count);
                            curr.position.z /= (verts.Count);

                            for (int i = 0; i < verts.Count; ++i)
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

                            curr.bounds.minPoints.x += curr.position.x;
                            curr.bounds.maxPoints.x += curr.position.x;
                            curr.bounds.minPoints.y += curr.position.y;
                            curr.bounds.maxPoints.y += curr.position.y;
                            curr.bounds.minPoints.z += curr.position.z;
                            curr.bounds.maxPoints.z += curr.position.z;
                            curr.bounds.center = curr.position;

                            //currMesh = MakeCubeFromBounds(curr.bounds);
                            //currMesh.RecalculateNormals();

                            GameWorld.SetTags(ref curr, tag.BUILDING);
                            GameWorld.AddEntity(ref CityManager.Instance.gameWorld, ref curr);
                            vertexOffset += verts.Count;
                        }
                        else
                        {
                            first = false;
                        }
                        string objName = line.Substring(2);
                        curr = new entity();
                        curr.name = objName;
                        currMesh = new Mesh();
                        if (verts.Count > maxVertCount)
                            maxVertCount = verts.Count;
                        if (triangles.Count > maxTriCount)
                            maxTriCount = triangles.Count;
                        triangles.Clear();
                        verts.Clear();
                    }
                    //New vert coming in
                    else if (function == "v")
                    {
                        string sub = line.Substring(2);
                        FastSplit(ref sub, ref points, ' ');
                        float x = -1f * float.Parse(points[0]) / 100;
                        float y = float.Parse(points[1]) / 100;
                        float z = float.Parse(points[2]) / 100;

                        if (curr.position.y > y)
                        {
                            curr.position.y = y;
                        }
                        curr.position.x += x;
                        curr.position.z += z;
                        verts.Add(new Vector3(x, y, z));
                    }
                    else if (function == "c")
                    {
                        string sub = line.Substring(2);
                        FastSplit(ref sub, ref colorVals, ' ');
                        float rf = float.Parse(colorVals[0]);
                        float gf = float.Parse(colorVals[1]);
                        float bf = float.Parse(colorVals[2]);
                        float af = float.Parse(colorVals[3]);

                        int r = (int)(rf * 255.1f);
                        int g = (int)(gf * 255.1f);
                        int b = (int)(bf * 255.1f);
                        //A seems to not be reliable in FBX, seems to show always as 1 in the shader. DO NOT USE!
                        int a = (int)(af * 255.1f);
                        int vertexIndex = (b * 256 * 256) + (g * 256) + r;
                        curr.vertexColorIndex = vertexIndex;
                    }
                    else if (function == "f")
                    {
                        string sub = line.Substring(2);
                        FastSplit(ref sub, ref points, ' ');
                        int a = int.Parse(points[0]);
                        int b = int.Parse(points[1]);
                        int c = int.Parse(points[2]);
                        triangles.Add(a);
                        triangles.Add(b);
                        triangles.Add(c);
                    }
                    else if (function != "vc" && function[0] != '#')
                    {
                        Debug.Log($"Unknown function {function}");
                    }
                }

                if (lookupIndex + 1 != remainingSize)
                {
                    _objContent = _objContent.Substring(lookupIndex + 1);
                }
                else
                {
                    _objContent = "";
                }

                remainingSize = _objContent.Length;
            }

            #region Also make sure to create the last object.
            curr.position.x /= verts.Count;
            curr.position.z /= verts.Count;

            for (int i = 0; i < verts.Count; ++i)
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

            curr.bounds.minPoints.x += curr.position.x;
            curr.bounds.maxPoints.x += curr.position.x;
            curr.bounds.minPoints.y += curr.position.y;
            curr.bounds.maxPoints.y += curr.position.y;
            curr.bounds.minPoints.z += curr.position.z;
            curr.bounds.maxPoints.z += curr.position.z;
            //curr.bounds.center = curr.position;

            currMesh = MakeCubeFromBounds(curr.bounds);
            currMesh.RecalculateNormals();
            GameWorld.SetTags(ref curr, tag.BUILDING);
            GameWorld.AddEntity(ref CityManager.Instance.gameWorld, ref curr);
            vertexOffset += verts.Count;
            #endregion
        }
    }

    public static int FastSplit(ref string _str, ref string[] _splits, params char[] _hits)
    {
        int index = 0;
        int lastHit = -1;
        for (int i = 0; i < _str.Length; ++i)
        {
            for (int hitIndex = 0; hitIndex < _hits.Length; ++hitIndex)
            {
                if (_str[i] == _hits[hitIndex])
                {
                    int pos = lastHit + 1;
                    _splits[index++] = _str.Substring(pos, i - (pos));
                    lastHit = i;
                }
            }
        }
        int finalPos = lastHit + 1;
        _splits[index++] = _str.Substring(finalPos, _str.Length - finalPos);
        return index;
    }

    public static Mesh MakeCubeFromBounds(bounds _b)
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
}
