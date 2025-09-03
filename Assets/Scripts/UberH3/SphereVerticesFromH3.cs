using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using H3Unity;
using Debug = UnityEngine.Debug;

public class SphereVerticesFromH3 : MonoBehaviour
{
    // 球体半径
    public float sphereRadius = 6000f;
    // H3网格分辨率（值越大，网格越密，顶点越多）
    public int h3Resolution = 0;

    // 存储球体表面顶点坐标
    public List<Vector3> sphereVertices = new List<Vector3>();
    public GameObject prefabCell;
    void Start()
    {
        sw = new();
        sw.Start();
        sw.Stop();
        // 获取球体表面的H3网格顶点坐标
        GenerateSphereVertices();
        Debug.Log($"已获取球体顶点数量: {sphereVertices.Count}");
        foreach (var pos in sphereVertices)
        {
            var tmpObj = GameObject.Instantiate(prefabCell);
            tmpObj.transform.position = pos;
            tmpObj.transform.localScale = Vector3.one;
            tmpObj.transform.SetParent(this.transform);
        }
    }

    public bool refreshBall;
    private Stopwatch sw;
    private float lockTime;
    private void Update()
    {
        if (refreshBall)
        {
            lockTime = Time.time;
            refreshBall = false;
            sw.Reset();
            // 获取球体表面的H3网格顶点坐标
            GenerateSphereVertices();
            sw.Stop();
            var costTime = Time.time - lockTime;
            Debug.Log($"已获取球体顶点数量: {sphereVertices.Count}   cost:[{sw.ElapsedMilliseconds}] time:[{costTime}]");
        }
    }

    /// <summary>
    /// 生成球体表面的顶点坐标（基于H3网格）
    /// </summary>
    void GenerateSphereVertices()
    {
        sphereVertices.Clear();

        // 遍历所有基础单元格（0-121）
        for (int baseCell = 0; baseCell < 122; baseCell++)
        {
            try
            {
                // 获取基础单元格的H3索引
                string baseCellHex = H3Utils.GetBaseCellHex(baseCell);
                ulong baseH3Index = H3.FromHex(baseCellHex);

                // 获取该基础单元格在指定分辨率下的子单元格
                ulong[] childCells = H3.ToChildren(baseH3Index, h3Resolution);

                // 处理每个子单元格的顶点
                foreach (ulong cell in childCells)
                {
                    // 获取单元格边界顶点（经纬度）
                    CellBoundary boundary = GetCellBoundary(cell);
                    
                    // 将经纬度顶点转换为3D坐标并添加到列表
                    for (int i = 0; i < boundary.numVerts; i++)
                    {
                        Vector3 vertex = LatLngToSpherePoint(boundary.verts[i], sphereRadius);
                        sphereVertices.Add(vertex);
                    }
                }
            }
            catch (H3Exception ex)
            {
                Debug.LogWarning($"处理基础单元格 {baseCell} 时出错: {ex.Message}");
            }
        }

        // 去重（避免不同单元格共享顶点重复计算）
        RemoveDuplicateVertices();
    }

    /// <summary>
    /// 获取H3单元格的边界顶点
    /// </summary>
    CellBoundary GetCellBoundary(ulong h3Index)
    {
        if (H3Native.cellToBoundary(h3Index, out CellBoundary boundary) != 0)
        {
            throw new H3Exception($"无法获取单元格 {h3Index} 的边界");
        }
        return boundary;
    }

    /// <summary>
    /// 将经纬度转换为球体表面3D坐标
    /// </summary>
    Vector3 LatLngToSpherePoint(LatLng latLng, float radius)
    {
        // 转换为弧度
        double latRad = latLng.lat; // H3返回的已经是弧度
        double lngRad = latLng.lng;

        // 球面坐标转笛卡尔坐标
        float x = (float)(radius * Mathf.Cos((float)latRad) * Mathf.Cos((float)lngRad));
        float y = (float)(radius * Mathf.Sin((float)latRad));
        float z = (float)(radius * Mathf.Cos((float)latRad) * Mathf.Sin((float)lngRad));

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// 移除重复的顶点（精度范围内）
    /// </summary>
    void RemoveDuplicateVertices()
    {
        HashSet<Vector3> uniqueVertices = new HashSet<Vector3>(new Vector3Comparer());
        foreach (var vert in sphereVertices)
        {
            uniqueVertices.Add(vert);
        }
        sphereVertices = new List<Vector3>(uniqueVertices);
    }

    // 用于Vector3去重的比较器（考虑浮点精度）
    private class Vector3Comparer : IEqualityComparer<Vector3>
    {
        private const float epsilon = 0.001f;

        public bool Equals(Vector3 a, Vector3 b)
        {
            return Mathf.Abs(a.x - b.x) < epsilon &&
                   Mathf.Abs(a.y - b.y) < epsilon &&
                   Mathf.Abs(a.z - b.z) < epsilon;
        }

        public int GetHashCode(Vector3 obj)
        {
            return obj.x.GetHashCode() ^ obj.y.GetHashCode() ^ obj.z.GetHashCode();
        }
    }

    /// <summary>
    /// 获取所有球体顶点
    /// </summary>
    public List<Vector3> GetSphereVertices()
    {
        return new List<Vector3>(sphereVertices);
    }
}