using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

public class game : MonoBehaviour
{
    private world gameWorld;

    public Mesh boxMesh;
    public Material material;
    public Camera mainCam;

    public static float3 up = new float3(0, 1, 0);
    public static float3 down = new float3(0, -1, 0);
    public static float3 left = new float3(-1, 0, 0);
    public static float3 right = new float3(1, 0, 0);
    public static float3 forward = new float3(0, 0, 1);
    public static float3 back = new float3(0, 0, -1);

    public static float3 ruf = Math.Float3Normalize(new float3(1, 1, 1));
    public static float3 rub = Math.Float3Normalize(new float3(1, 1, -1));
    public static float3 rdf = Math.Float3Normalize(new float3(1, -1, 1));
    public static float3 rdb = Math.Float3Normalize(new float3(1, -1, -1));
    public static float3 luf = Math.Float3Normalize(new float3(-1, 1, 1));
    public static float3 lub = Math.Float3Normalize(new float3(-1, 1, -1));
    public static float3 ldf = Math.Float3Normalize(new float3(-1, -1, 1));
    public static float3 ldb = Math.Float3Normalize(new float3(-1, -1, -1));

    private ComputeBuffer positionBuffer;
    public ComputeBuffer argsBuffer;
    private Vector4[] positions;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    private void OnDisable()
    {
    }

    private void Start()
    {
        mainCam = Camera.main;
        gameWorld = new world(30000);

        positions = new Vector4[gameWorld.maxEntities];
        positionBuffer = new ComputeBuffer(gameWorld.maxEntities, 16);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
#if true
        SpawnLotsOfCubes(ref gameWorld, gameWorld.maxEntities);

        float3 start = new float3(50, 0, -50);
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        raycast_result hitResult;
        Raycast.RaycastJob(ref gameWorld, new float3(50, 0, -50), forward, 10000, out hitResult);

        sw.Stop();
        Debug.Log($"Raycast took {sw.ElapsedMilliseconds}ms");
#endif 

#if false
        SpawnLotsOfGameObjects(gameWorld.maxEntities);
        System.Diagnostics.Stopwatch sw2 = System.Diagnostics.Stopwatch.StartNew();
        Physics.Raycast(new Vector3(50, 0, -5), Vector3.forward, 10000);
        sw2.Stop();
        Debug.Log($"Unity Raycast took {sw2.ElapsedMilliseconds}ms");
#endif 

#if true
        RayCastAlongSphere(ref gameWorld, new float3(0, 0, 0), 20f);
#endif
#if false
        BoundsTests(ref test);
#endif

#if false
        float3CreationTests();
#endif
#if false
        ProjectionTests();
#endif
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float3 dir = ray.direction;
        float3 pos = ray.origin;
        raycast_result result;
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            Raycast.RaycastJob(ref gameWorld, pos, dir, 1000, out result);
            sw.Stop();
            Debug.Log($"Raycast took {sw.ElapsedMilliseconds}ms and hit {result.hitPos.x}, {result.hitPos.y}, {result.hitPos.z}");
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = new Vector3(.35f, .25f, .25f);
            go.transform.position = result.hitPos;
            Debug.DrawLine(pos, result.hitPos, new Color32(122, 0, 122, 255), 10000);
        }

        float boxDimWithPadding = (gameWorld.entityCount * .5f) + gameWorld.entityCount;
        Bounds b = new Bounds(new Vector3(boxDimWithPadding / 2, 0, boxDimWithPadding / 2), new Vector3(boxDimWithPadding, 100, boxDimWithPadding));

        //positionBuffer.SetData(positions);
        //material.SetBuffer("positionBuffer", positionBuffer);
        Graphics.DrawMeshInstancedIndirect(boxMesh, 0, material, b, argsBuffer);
    }

    private void SpawnLotsOfGameObjects(int _spawnCount)
    {
        int dimSizeX = (int)Mathf.Floor(Mathf.Sqrt(_spawnCount));
        int dimSizeY = dimSizeX;
        float paddingX = .5f;
        float paddingY = .5f;
        for (int y = 0; y < dimSizeY; ++y)
        {
            for (int x = 0; x < dimSizeX; ++x)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(x + paddingX, y + paddingY, 0);
                paddingX += .25f;
            }
            paddingY += .25f;
            paddingX = .5f;
        }
    }

    private void SpawnLotsOfCubes(ref world _w, int _spawnCount)
    {
        int dimSizeX = (int)Mathf.Floor(Mathf.Sqrt(_spawnCount));
        int dimSizeY = dimSizeX;
        float paddingX = .5f;
        float paddingY = .5f;
        int boxIndex = 0;
        for (int y = 0; y < dimSizeY; ++y)
        {
            for (int x = 0; x < dimSizeX; ++x)
            {
                entity curr = CreateCubePrimative(new float3(x + paddingX, 0, y + paddingY));
                GameWorld.AddEntity(ref _w, ref curr);
                paddingX += .25f;
                positions[boxIndex++] = new Vector4(curr.position.x, curr.position.y, curr.position.z, 1);
            }
            paddingY += .25f;
            paddingX = .5f;
        }
        positionBuffer.SetData(positions);
        material.SetBuffer("positionBuffer", positionBuffer);
        if (boxMesh != null)
        {
            args[0] = (uint)boxMesh.GetIndexCount(0);
            args[1] = (uint)gameWorld.entityCount;
            args[2] = (uint)boxMesh.GetIndexStart(0);
            args[3] = (uint)boxMesh.GetBaseVertex(0);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);
    }

    private static void RayCastAlongSphere(ref world _w, float3 _origin, float _radius)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        float diameter = _radius * 2;
        float yStep = _radius/25;
        float cStep = 360 / 25;
        raycast_result hitResult;
        int count = 0;

        for (float y = -_radius;
            y <= _radius;
            y += yStep)
        {
            float absY = Mathf.Abs(y);
            float ratio = (_radius - absY);
            for (float c = 0;
                c < 360;
                c += cStep)
            {
                float x = Mathf.Sin(c) * ratio;
                float z = Mathf.Cos(c) * ratio;
                float3 start = new float3(x, y, z);
                start = Math.Float3Normalize(start);
                start = start * _radius;
                float3 dir = Math.Float3Normalize(new float3(0, 0, 0) - start);
                if (Raycast.RaycastJob(ref _w, start, dir, _radius, out hitResult))
                {
                    raycast_result result2;
                    float3 newStart = start + Math.Float3FromDirAndMag(start, dir * 1, _radius * 2);
                    //newStart = float3Addfloat3(hitResult.hitPos, float3FromDirAndMag(hitResult.hitPos, dir, 1));
                    if (Raycast.RaycastJob(ref _w, newStart, dir* 1, _radius * 2, out result2))
                    {
                        Debug.DrawLine(hitResult.hitPos, result2.hitPos, Color.green, Mathf.Infinity);
                    }
                    count++;
                }
                count++;
            }
        }
        sw.Stop();
        Debug.Log($"ElapsedTime: {sw.ElapsedMilliseconds}ms for {count} raycasts");
    }

    private static void BoundsTests(ref entity _e)
    {
        Debug.Assert(Collision.IsInside(_e.bounds, new float3(0, 0, 0)));
        Debug.Assert(Collision.IsInside(_e.bounds, new float3(.5f, .5f, .5f)));
        Debug.Assert(Collision.IsInside(_e.bounds, new float3(-.5f, -.5f, -.5f)));
        Debug.Assert(!Collision.IsInside(_e.bounds, new float3(1, 1, 1)));
        Debug.Assert(!Collision.IsInside(_e.bounds, new float3(-1, -1, -1)));
        Debug.Assert(!Collision.IsInside(_e.bounds, new float3(2, 2, 2)));
        Debug.Assert(!Collision.IsInside(_e.bounds, new float3(-2, -2, -2)));
    }

    private static void float3CreationTests()
    {
#if false
        float3 a = Math.Float3FromDirAndMag(new float3(0,0,0), up, 1f);
        float3 b = Math.Float3FromDirAndMag(new float3(0,0,0), down, 2f);
        float3 c = Math.Float3FromDirAndMag(new float3(0,0,0), left, 3f);
        float3 d = Math.Float3FromDirAndMag(new float3(0,0,0), right, 4f);
        float3 e = Math.Float3FromDirAndMag(new float3(0,0,0), forward, 5f);
        float3 f = Math.Float3FromDirAndMag(new float3(0,0,0), back, 6f);

        Debug.DrawLine(new Vector3(0, 0, 0), a, Color.red, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), b, Color.green, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), c, Color.blue, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), d, Color.magenta, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), e, Color.yellow, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), f, Color.black, Mathf.Infinity);

#else
        float3 a = Math.Float3FromDirAndMag(new float3(0, 0, 0), ruf, 1f);
        float3 b = Math.Float3FromDirAndMag(new float3(0, 0, 0), rub, 2f);
        float3 c = Math.Float3FromDirAndMag(new float3(0, 0, 0), rdf, 3f);
        float3 d = Math.Float3FromDirAndMag(new float3(0, 0, 0), rdb, 4f);
        float3 e = Math.Float3FromDirAndMag(new float3(0, 0, 0), luf, 5f);
        float3 f = Math.Float3FromDirAndMag(new float3(0, 0, 0), lub, 6f);
        float3 g = Math.Float3FromDirAndMag(new float3(0, 0, 0), ldf, 7f);
        float3 h = Math.Float3FromDirAndMag(new float3(0, 0, 0), ldb, 8f);

        Debug.DrawLine(new Vector3(0, 0, 0), a, Color.red, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), b, Color.green, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), c, Color.blue, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), d, Color.magenta, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), e, Color.yellow, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), f, Color.black, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), g, Color.white, Mathf.Infinity);
        Debug.DrawLine(new Vector3(0, 0, 0), h, Color.gray, Mathf.Infinity);
#endif
    }

    private void ProjectionTests()
    {
        float3 start = new float3(10, 0, 0);
        float3 end = Math.Float3FromDirAndMag(start, ruf, 10000f);
        float3 rayCast = start - end;
        Debug.DrawLine(start, end, Color.white, Mathf.Infinity);

        float3 a = Math.Float3FromDirAndMag(new float3(0, 0, 0), ruf, 1f);
        float3 b = Math.Float3FromDirAndMag(new float3(0, 0, 0), rub, 2f);
        float3 c = Math.Float3FromDirAndMag(new float3(0, 0, 0), rdf, 3f);
        float3 d = Math.Float3FromDirAndMag(new float3(0, 0, 0), rdb, 4f);
        float3 e = Math.Float3FromDirAndMag(new float3(0, 0, 0), luf, 5f);
        float3 f = Math.Float3FromDirAndMag(new float3(0, 0, 0), lub, 6f);
        float3 g = Math.Float3FromDirAndMag(new float3(0, 0, 0), ldf, 7f);
        float3 h = Math.Float3FromDirAndMag(new float3(0, 0, 0), ldb, 8f);

        
        DebugProjectOntoRay(rayCast, a, Color.red, start);
        DebugProjectOntoRay(rayCast, b, Color.blue, start);
        DebugProjectOntoRay(rayCast, c, Color.magenta, start);
        DebugProjectOntoRay(rayCast, d, Color.grey, start);
        DebugProjectOntoRay(rayCast, e, Color.yellow, start);
        DebugProjectOntoRay(rayCast, f, Color.cyan, start);
        DebugProjectOntoRay(rayCast, g, new Color32(122, 0, 122, 255), start);
        DebugProjectOntoRay(rayCast, h, new Color32(255, 165, 0, 255), start);
    }

    private void DebugProjectOntoRay(float3 raycast, float3 other, Color _col, float3 _offset)
    {
        Debug.DrawLine(new Vector3(0, 0, 0), other, _col, Mathf.Infinity);
        float3 proj = Math.Float3Projection(raycast, other);
        Debug.DrawLine(other, proj, Color.green, Mathf.Infinity);
    }

    [System.Diagnostics.Contracts.Pure]
    public static entity CreateCubePrimative(float3 _origin)
    {
        entity result = new entity();
        result.position = _origin;
        result.scale = new float3(1,1,1);
        float minX = result.position.x - result.scale.x;
        float maxX = result.position.x + result.scale.x;

        float minY = result.position.y - result.scale.y;
        float maxY = result.position.y + result.scale.y;

        float minZ = result.position.z - result.scale.z;
        float maxZ = result.position.z + result.scale.z;

        result.bounds.minPoints = new float3(minX, minY, minZ);
        result.bounds.maxPoints = new float3(maxX, maxY, maxZ);
        return result;
    }

}
