using UnityEngine;
using System.Collections.Generic;
using H3Unity;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class H3SphereRenderer : MonoBehaviour
{
    [Header("球体设置")]
    [Tooltip("球体半径")]
    public float sphereRadius = 10f;
    
    [Header("H3网格设置")]
    [Tooltip("H3分辨率 (0-15，值越大网格越密)")]
    [Range(0, 15)] public int h3Resolution = 3;
    
    [Tooltip("是否只生成部分区域用于测试")]
    public bool useTestArea = true;
    
    [Header("渲染设置")]
    [Tooltip("实例化使用的材质")]
    public Material instanceMaterial;
    
    [Tooltip("六边形网格大小缩放")]
    public float hexScale = 1f;

    // 基础六边形网格原型
    public Mesh hexMesh;
    
    // 实例化数据
    private List<Matrix4x4> instanceMatrices = new List<Matrix4x4>();
    private List<Color> instanceColors = new List<Color>();
    
    // 存储所有H3单元格索引
    private List<ulong> allHexIndices = new List<ulong>();
    public Material mHex;
    void Start()
    {
        // 初始化基础六边形网格
        hexMesh = CreateHexagonMesh();
        GameObject objHexMesh = new();
        objHexMesh.AddComponent<MeshFilter>().mesh = hexMesh;
        objHexMesh.AddComponent<MeshRenderer>().sharedMaterial = mHex;
        objHexMesh.transform.SetParent(this.transform);
        // 生成H3网格数据
        GenerateH3Grid();
        
        // 准备实例化数据
        PrepareInstanceData();
        
        Debug.Log($"已生成 {allHexIndices.Count} 个H3单元格，准备实例化绘制");
    }

    void Update()
    {
        if (hexMesh != null && instanceMaterial != null && instanceMatrices.Count > 0)
        {
            // 使用实例化绘制所有六边形
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            //propertyBlock.SetVectorArray("_InstanceColor", instanceColors.ConvertAll(c => (Vector4)c).ToArray());
            
            Graphics.DrawMeshInstanced(
                hexMesh,
                0,
                instanceMaterial,
                instanceMatrices,
                propertyBlock,
                ShadowCastingMode.Off,
                false
            );
        }
    }

    /// <summary>
    /// 创建基础六边形网格作为实例原型
    /// </summary>
    private Mesh CreateHexagonMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "HexagonPrototype";

        // 六边形顶点 (中心 + 6个顶点)
        List<Vector3> vertices = new List<Vector3>
        {
            Vector3.zero // 中心顶点
        };

        // 计算六边形的6个顶点
        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.PI / 3f;
            vertices.Add(new Vector3(
                Mathf.Cos(angle),
                0,
                Mathf.Sin(angle)
            ));
        }

        // 三角形索引
        int[] triangles = new int[18]; // 6个三角形 × 3个顶点
        for (int i = 0; i < 6; i++)
        {
            int baseIndex = i * 3;
            triangles[baseIndex] = 0;
            triangles[baseIndex + 1] = i + 1;
            triangles[baseIndex + 2] = (i + 1) % 6 + 1;
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }

    /// <summary>
    /// 生成H3网格索引
    /// </summary>
    private void GenerateH3Grid()
    {
        allHexIndices.Clear();
        
        if (useTestArea)
        {
            // 生成测试区域（以某个点为中心的局部网格）
            ulong centerHex = H3.FromLatLng(0, 0, h3Resolution); // 赤道中心
            int radius = 5; // 网格半径
            allHexIndices.AddRange(H3.Disk(centerHex, radius));
        }
        else
        {
            // 生成全球网格（遍历所有122个基础单元格）
            for (int baseCell = 0; baseCell < 122; baseCell++)
            {
                try
                {
                    // 获取基础单元格的H3索引
                    string baseCellHex = H3Utils.GetBaseCellHex(baseCell);
                    ulong baseIndex = H3.FromHex(baseCellHex);
                    
                    // 获取指定分辨率的子单元格
                    if (h3Resolution == 0)
                    {
                        allHexIndices.Add(baseIndex);
                    }
                    else
                    {
                        ulong[] children = H3.ToChildren(baseIndex, h3Resolution);
                        allHexIndices.AddRange(children);
                    }
                }
                catch (H3Exception ex)
                {
                    Debug.LogWarning($"处理基础单元格 {baseCell} 时出错: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 准备实例化所需的矩阵和颜色数据
    /// </summary>
    private void PrepareInstanceData()
    {
        instanceMatrices.Clear();
        instanceColors.Clear();
        
        foreach (ulong hexIndex in allHexIndices)
        {
            if (!H3.IsValid(hexIndex)) continue;
            
            try
            {
                // 获取单元格中心经纬度
                LatLng center = H3.ToLatLng(hexIndex);
                
                // 转换为3D球面坐标
                Vector3 position = LatLngToSpherePoint(center, sphereRadius);
                
                // 计算旋转（使六边形平面与球面相切）
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, position.normalized);
                
                // 计算缩放（基于单元格大小）
                float cellSize = GetHexCellSize(hexIndex);
                float scale = cellSize * hexScale;
                
                // 创建转换矩阵
                Matrix4x4 matrix = Matrix4x4.TRS(
                    position,
                    rotation,
                    Vector3.one * scale
                );
                
                instanceMatrices.Add(matrix);
                
                // 添加随机颜色（可根据需要修改为基于位置的颜色）
                instanceColors.Add(new Color(
                    Mathf.PerlinNoise(position.x * 0.1f, position.y * 0.1f),
                    Mathf.PerlinNoise(position.y * 0.1f, position.z * 0.1f),
                    Mathf.PerlinNoise(position.z * 0.1f, position.x * 0.1f)
                ));
            }
            catch (H3Exception ex)
            {
                Debug.LogWarning($"处理H3单元格 {hexIndex} 时出错: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 将经纬度转换为球体表面3D坐标
    /// </summary>
    private Vector3 LatLngToSpherePoint(LatLng latLng, float radius)
    {
        // H3返回的是弧度，直接使用
        float latRad = (float)latLng.lat;
        float lngRad = (float)latLng.lng;
        
        // 球坐标转笛卡尔坐标
        float x = radius * Mathf.Cos(latRad) * Mathf.Cos(lngRad);
        float y = radius * Mathf.Sin(latRad);
        float z = radius * Mathf.Cos(latRad) * Mathf.Sin(lngRad);
        
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// 获取六边形单元格的大小（用于缩放）
    /// </summary>
    private float GetHexCellSize(ulong hexIndex)
    {
        // 获取单元格边缘长度（米）并转换为适合的缩放值
        double edgeLength = 1;//H3Native.edgeLengthM(hexIndex);
        return (float)(edgeLength / sphereRadius * 2f);
    }

    void OnDrawGizmosSelected()
    {
        // 绘制球体线框作为参考
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Vector3.zero, sphereRadius);
    }
}
