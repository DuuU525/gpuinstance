using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;

namespace H3Unity
{
    [Preserve]
    [StructLayout(LayoutKind.Sequential)]
    public struct LatLng
    {
        public double lat;
        public double lng;
    }

    [Preserve]
    [StructLayout(LayoutKind.Sequential)]
    public struct CellBoundary
    {
        public int numVerts;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public LatLng[] verts;
    }

    [Preserve]
    [StructLayout(LayoutKind.Sequential)]
    public struct CoordIJ
    {
        public int i;
        public int j;
    }

    [Preserve]
    [StructLayout(LayoutKind.Sequential)]
    public struct GeoLoop
    {
        public int numVerts;
        public IntPtr verts; // Pointer to LatLng array
    }

    [Preserve]
    [StructLayout(LayoutKind.Sequential)]
    public struct GeoPolygon
    {
        public GeoLoop geoloop;
        public int numHoles;
        public IntPtr holes; // Pointer to GeoLoop array
    }
}
