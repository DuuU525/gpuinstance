using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3Unity;

public class H3Ball : MonoBehaviour
{
    public H3Cell H3Cell;
    public double Lat;
    public double Lng;
    public int Res;
    public ulong h3LatLng;
    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        h3LatLng = H3.FromLatLng(Lat,Lng,Res);
        
        //GetCellVertices(h3LatLng);

        // Example();
        ExampleAllBallPoints();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static void GetCellVertices(ulong h3Index)
    {
        try
        {
            // 调用原生方法获取单元格边界（顶点坐标）
            int result = H3Native.cellToBoundary(h3Index, out CellBoundary boundary);
            if (result != 0)
            {
                throw new H3Exception("Failed to get cell boundary", result);
            }

            // 输出顶点信息（转换为度制）
            Debug.Log($"Cell {H3.ToHex(h3Index)} 顶点数量: {boundary.numVerts}");
            for (int i = 0; i < boundary.numVerts; i++)
            {
                double latDeg = H3Utils.RadsToDegs(boundary.verts[i].lat);
                double lngDeg = H3Utils.RadsToDegs(boundary.verts[i].lng);
                Debug.Log($"顶点 {i + 1}: 纬度 {latDeg:F6}, 经度 {lngDeg:F6}");
            }
        }
        catch (H3Exception ex)
        {
            Debug.Log($"获取顶点失败: {ex.Message}");
        }
    }

    #region 经纬度 转 世界坐标

    // 地球半径（单位：米，WGS84椭球长半轴）
    public const double EarthRadiusMeters = 6378137.0;

    /// <summary>
    /// 将经纬度（度）转换为世界坐标（三维笛卡尔坐标）
    /// </summary>
    /// <param name="latDeg">纬度（度，范围：-90 ~ 90）</param>
    /// <param name="lngDeg">经度（度，范围：-180 ~ 180）</param>
    /// <returns>世界坐标(x, y, z)，单位：米</returns>
    public static (double x, double y, double z) LatLngToWorldCoord(double latDeg, double lngDeg)
    {
        // 验证输入范围
        if (latDeg < -90 || latDeg > 90)
            throw new ArgumentOutOfRangeException(nameof(latDeg), "纬度必须在-90到90度之间");
        if (lngDeg < -180 || lngDeg > 180)
            throw new ArgumentOutOfRangeException(nameof(lngDeg), "经度必须在-180到180度之间");

        // 转换为弧度
        double latRad = H3Utils.DegsToRads(latDeg);
        double lngRad = H3Utils.DegsToRads(lngDeg);

        // 计算笛卡尔坐标
        double cosLat = Math.Cos(latRad);
        double x = EarthRadiusMeters * cosLat * Math.Cos(lngRad);
        double y = EarthRadiusMeters * cosLat * Math.Sin(lngRad);
        double z = EarthRadiusMeters * Math.Sin(latRad);

        return (x, y, z);
    }

    /// <summary>
    /// 将H3单元格的中心点经纬度转换为世界坐标
    /// </summary>
    /// <param name="h3Index">H3单元格索引</param>
    /// <returns>世界坐标(x, y, z)，单位：米</returns>
    public static (double x, double y, double z) H3CellToWorldCoord(ulong h3Index)
    {
        // 获取H3单元格中心点的经纬度（弧度）
        LatLng centerLatLng = H3.ToLatLng(h3Index);

        // 转换为度
        double latDeg = H3Utils.RadsToDegs(centerLatLng.lat);
        double lngDeg = H3Utils.RadsToDegs(centerLatLng.lng);

        // 转换为世界坐标
        return LatLngToWorldCoord(latDeg, lngDeg);
    }

    // 使用示例
    public static void Example()
    {
        // 1. 直接通过经纬度转换
        var worldCoord1 = LatLngToWorldCoord(39.9042, 116.4074); // 北京坐标
        Debug.Log($"北京世界坐标：X={worldCoord1.x:F2}, Y={worldCoord1.y:F2}, Z={worldCoord1.z:F2}");

        // 2. 通过H3单元格转换
        ulong h3Index = H3.FromLatLng(39.9042, 116.4074, 8); // 获取北京附近H3单元格
        var worldCoord2 = H3CellToWorldCoord(h3Index);
        Debug.Log($"H3单元格中心点世界坐标：X={worldCoord2.x:F2}, Y={worldCoord2.y:F2}, Z={worldCoord2.z:F2}");
    }
    #endregion

    #region 获得所有球面顶点

    /// <summary>
    /// 获取H3单元格的所有球面顶点坐标（度制）
    /// </summary>
    /// <param name="h3Index">H3单元格索引</param>
    /// <returns>顶点坐标列表（纬度、经度），按顺时针顺序排列</returns>
    /// <exception cref="H3Exception">当获取边界失败时抛出</exception>
    public static List<Tuple<double, double>> GetAllSphericalVertices(ulong h3Index)
    {
        // 调用原生方法获取单元格边界（包含顶点）
        int resultCode = H3Native.cellToBoundary(h3Index, out CellBoundary boundary);
        if (resultCode != 0)
        {
            throw new H3Exception($"获取单元格边界失败，错误码: {resultCode}");
        }

        // 解析顶点并转换为度制
        var vertices = new List<Tuple<double, double>>();
        for (int i = 0; i < boundary.numVerts; i++)
        {
            double latDeg = H3Utils.RadsToDegs(boundary.verts[i].lat); // 纬度（度）
            double lngDeg = H3Utils.RadsToDegs(boundary.verts[i].lng); // 经度（度）
            vertices.Add(Tuple.Create(latDeg, lngDeg));
        }

        return vertices;
    }

    // 使用示例
    public static void ExampleAllBallPoints()
    {
        // 示例：获取北京附近某单元格的顶点
        ulong h3Index = H3.FromLatLng(39.9042, 116.4074, 8); // 纬度、经度、分辨率
        try
        {
            var vertices = GetAllSphericalVertices(h3Index);
            Debug.Log($"单元格 {H3.ToHex(h3Index)} 的顶点坐标（度）：");
            for (int i = 0; i < vertices.Count; i++)
            {
                Debug.Log($"顶点 {i + 1}: 纬度 {vertices[i].Item1:F6}, 经度 {vertices[i].Item2:F6}");
            }
        }
        catch (H3Exception ex)
        {
            Debug.Log($"错误：{ex.Message}");
        }
    }

    #endregion
    
}
