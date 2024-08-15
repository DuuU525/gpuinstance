using System.Collections.Generic;
using UnityEngine;

public class IcosphereGenerator : MonoBehaviour
{
    public bool isInstance;
    public int subdivisionLevel = 3;
    public Material material;
    private Mesh mesh;
    public float radiusBall = 1000f;

    private void Start()
    {
        mesh = GenerateIcosphere(subdivisionLevel);
        if (!isInstance)
        {
            CreateMeshObject(mesh);
        }
        else
        {
            InitMeshBuffers(mesh);
        }
        
        //for (int i = 0; i < 20; i++)
        //{
        //    float count = Mathf.Pow(3, i) * 20 * 6 + 12 * 5;
        //    Debug.Log(count + "*3*" + i.ToString());
        //}
        //12 * 5(边形) + (1600002(多边形) - 12(五边形)) * 6(边形) = 960 0000(triangle)
    }

    private Mesh GenerateIcosphere(int subdivisions)
    {
        // 生成正二十面体的顶点和三角形索引数组
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;
                                                                        
        vertices.Add(new Vector3(-1f, t, 0).normalized * radiusBall);
        vertices.Add(new Vector3(1f, t, 0).normalized * radiusBall);
        vertices.Add(new Vector3(-1f, -t, 0).normalized * radiusBall);
        vertices.Add(new Vector3(1, -t, 0).normalized * radiusBall);

        vertices.Add(new Vector3(0, -1f, t).normalized * radiusBall);
        vertices.Add(new Vector3(0, 1f, t).normalized * radiusBall);
        vertices.Add(new Vector3(0, -1f, -t).normalized * radiusBall);
        vertices.Add(new Vector3(0, 1f, -t).normalized * radiusBall);

        vertices.Add(new Vector3(t, 0, -1f).normalized * radiusBall);
        vertices.Add(new Vector3(t, 0, 1f).normalized * radiusBall);
        vertices.Add(new Vector3(-t, 0, -1f).normalized * radiusBall);
        vertices.Add(new Vector3(-t, 0, 1f).normalized * radiusBall);
        

        triangles.AddRange(new int[] {
            0, 11, 5,  0, 5, 1,   0, 1, 7,   0, 7, 10,  0, 10, 11,
            1, 5, 9,   5, 11, 4,  11, 10, 2,  10, 7, 6,   7, 1, 8,
            3, 9, 4,    3, 4, 2,   3, 2, 6,   3, 6, 8,    3, 8, 9,
            4, 9, 5,    2, 4, 11,  6, 2, 10,   8, 6, 7,    9, 8, 1
        });

        // 对正二十面体的面进行细分
        for (int i = 0; i < subdivisions; i++)
        {
            List<int> newTriangles = new List<int>();

            for (int j = 0; j < triangles.Count; j += 3)
            {
                int a = GetMiddlePoint(triangles[j], triangles[j + 1], vertices);
                int b = GetMiddlePoint(triangles[j + 1], triangles[j + 2], vertices);
                int c = GetMiddlePoint(triangles[j + 2], triangles[j], vertices);

                newTriangles.AddRange(new int[] { 
                    triangles[j], a, c, 
                    triangles[j + 1], b, a, 
                    triangles[j + 2], c, b, 
                    a, b, c });
            }

            triangles = newTriangles;
        }

        // 创建 Mesh 对象并设置顶点、法线和索引数据
        Mesh generatedMesh = new Mesh();
        generatedMesh.vertices = vertices.ToArray();
        generatedMesh.triangles = triangles.ToArray();
        Debug.Log($"三角形个数： {triangles.Count/3}  顶点个数： {vertices.Count}");
        //List<Color> listCol = new List<Color>();
        //for (int i = 0; i < vertices.Count; i++)
        //{
        //    listCol.Add(new Color(Random.Range(0,1), Random.Range(0, 1), Random.Range(0, 1),1));
        //}
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
        return generatedMesh;
    }


    private int GetMiddlePoint(int p1, int p2, List<Vector3> vertices)
    {
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = (point1 + point2) / 2.0f;

        int newIndex = vertices.Count;
        vertices.Add(middle.normalized * radiusBall);
        return newIndex;
    }

    private void CreateMeshObject(Mesh mesh)
    {
        GameObject sphereObject = new GameObject("Icosphere");
        MeshFilter meshFilter = sphereObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = sphereObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        meshRenderer.material = material;
        sphereObject.transform.localScale = Vector3.one;
    }

    //三角面个数
    public int instanceCount;
    //实例单元
    public Mesh instanceMesh;
    //实例材质
    public Material instanceMaterial;
    public Camera camWork;
    //筛选不在摄像机中的实例
    public ComputeShader clipCamera;
    //缓存面的位置
    private ComputeBuffer positionBuffer;
    //缓存所有面上组合顶点的索引
    private ComputeBuffer trianglesBuffer;
    //缓存所有顶点坐标
    private ComputeBuffer verticesBuffer;
    //绘制的缓存数据
    private ComputeBuffer argsBuffer;
    //筛选显示的面
    private ComputeBuffer clipBuffer;
    //cs
    private int kernel;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private void InitMeshBuffers(Mesh mesh)
    {
        kernel = clipCamera.FindKernel("ClipCamera");
        instanceCount = mesh.triangles.Length / 3;//三角形个数         
        clipBuffer = new ComputeBuffer(instanceCount, sizeof(uint), ComputeBufferType.Append);
        UpdateBuffers();
        UpdateClip();
    }
    //剪切出可视
    private void UpdateClip()
    {   
        if(clipBuffer == null)
        {
            return;
        }
        clipBuffer.SetCounterValue(0);
        clipCamera.SetBuffer(kernel, "cullresult", clipBuffer);
        clipCamera.SetVector("cameraPos", camWork.transform.position);
        clipCamera.SetVectorArray("plance", GetFrustumPlane(camWork));
        clipCamera.SetInt("instanceCount", instanceCount);
        clipCamera.SetBuffer(kernel, "posInfo", positionBuffer);
        clipCamera.Dispatch(kernel, 1 + (instanceCount / 1024), 1, 1);
        ComputeBuffer.CopyCount(clipBuffer, argsBuffer, sizeof(uint));
    }
    private void UpdateBuffers()
    {
        ReleaseBuffer();

        var trianglesMesh = mesh.triangles;
        var vertices = mesh.vertices;
        positionBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 3);
        trianglesBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 3);
        verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        Vector3[] positions = new Vector3[instanceCount];
        Vector3[] triangles = new Vector3[instanceCount];
        for (int i = 0; i < instanceCount; i ++)
        {
            var idx1 = trianglesMesh[i * 3]; //索引
            var idx2 = trianglesMesh[i * 3 + 1];
            var idx3 = trianglesMesh[i * 3 + 2];

            var pos1 = vertices[idx1];
            var pos2 = vertices[idx2];
            var pos3 = vertices[idx3];
            var center = (pos1 + pos2 + pos3) / 3f;
            positions[i] = center;
            triangles[i] = new Vector3(idx1, idx2, idx3);
        }
        positionBuffer.SetData(positions, 0, 0, instanceCount);
        trianglesBuffer.SetData(triangles, 0, 0, instanceCount);
        verticesBuffer.SetData(vertices, 0, 0, vertices.Length);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);
        instanceMaterial.SetBuffer("trianglesBuffer", trianglesBuffer);
        instanceMaterial.SetBuffer("verticesBuffer", verticesBuffer);
        instanceMaterial.SetBuffer("clipBuffer", clipBuffer);

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(0);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(0);
            args[3] = (uint)instanceMesh.GetBaseVertex(0);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);
    }
    private void ReleaseBuffer()
    {
        positionBuffer?.Release();
        trianglesBuffer?.Release();
        verticesBuffer?.Release();
        argsBuffer?.Release();
    }
    private void Update()
    {
        if(!isInstance)
            return;
        if(instanceMesh == null)
        {
            InitInstanceMesh();
            return;
        }
        UpdateClip();
        UpdateBuffers();
        // Render
        Graphics.DrawMeshInstancedIndirect(
            instanceMesh,
            0,
            instanceMaterial,
            new Bounds(Vector3.zero, new Vector3(3000.0f, 3000.0f, 3000.0f)),
            argsBuffer,
            0,
            null,
            UnityEngine.Rendering.ShadowCastingMode.On,
            true);
    }
    private void InitInstanceMesh()
    {
        instanceMesh = new Mesh();

        instanceMesh.vertices = new Vector3[3]
        {
            Vector3.right,
            Vector3.forward,
            Vector3.up,
        };

        instanceMesh.colors = new Color[3]
        {
            Color.white,
            Color.white,
            Color.white,
        };

        instanceMesh.uv = new Vector2[3]
        {
            Vector2.zero,
            Vector2.right,
            new Vector2(0.5f, 1),
        };

        instanceMesh.triangles = new int[3] { 0, 1, 2 };
        instanceMesh.RecalculateNormals();
        instanceMesh.RecalculateTangents();
    }


    private void OnDisable()
    {
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = null;

        if (trianglesBuffer != null)
            trianglesBuffer.Release();
        trianglesBuffer = null;
        
        if (verticesBuffer != null)
            verticesBuffer.Release();
        verticesBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;

        if (clipBuffer != null)
            clipBuffer.Release();
        clipBuffer = null;
    }


    #region Utils 视锥剪裁
    public static Vector4[] GetFrustumPlane(Camera camera)
    {
        Vector4[] planes = new Vector4[6];
        Transform transform = camera.transform;
        Vector3 cameraPosition = transform.position;
        Vector3[] points = GetCameraFarClipPlanePoint(camera);
        //顺时针
        planes[0] = GetPlane(cameraPosition, points[0], points[2]);//left
        planes[1] = GetPlane(cameraPosition, points[3], points[1]);//right
        planes[2] = GetPlane(cameraPosition, points[1], points[0]);//bottom
        planes[3] = GetPlane(cameraPosition, points[2], points[3]);//up
        planes[4] = GetPlane(-transform.forward, transform.position + transform.forward * camera.nearClipPlane);//near
        planes[5] = GetPlane(transform.forward, transform.position + transform.forward * camera.farClipPlane);//far
        return planes;
    }
    //获取视锥体远平面的四个点
    public static Vector3[] GetCameraFarClipPlanePoint(Camera camera)
    {
        Vector3[] points = new Vector3[4];
        Transform transform = camera.transform;
        float distance = camera.farClipPlane;
        float halfFovRad = Mathf.Deg2Rad * camera.fieldOfView * 0.5f;
        float upLen = distance * Mathf.Tan(halfFovRad);
        float rightLen = upLen * camera.aspect;
        Vector3 farCenterPoint = transform.position + distance * transform.forward;
        Vector3 up = upLen * transform.up;
        Vector3 right = rightLen * transform.right;
        points[0] = farCenterPoint - up - right;//left-bottom
        points[1] = farCenterPoint - up + right;//right-bottom
        points[2] = farCenterPoint + up - right;//left-up
        points[3] = farCenterPoint + up + right;//right-up
        return points;
    }
    //三点确定一个平面
    public static Vector4 GetPlane(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
        return GetPlane(normal, a);
    }
    //一个点和一个法向量确定一个平面
    public static Vector4 GetPlane(Vector3 normal, Vector3 point)
    {
        return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, point));
    }
    #endregion
}
