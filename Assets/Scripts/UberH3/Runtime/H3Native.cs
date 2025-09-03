using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;

namespace H3Unity
{
    [Preserve]
    public static class H3Native
    {
        private const string DLL = "h3";

        // -------------------- Indexing --------------------

        [DllImport(DLL, EntryPoint = "h3_latLngToCell", CallingConvention = CallingConvention.Cdecl)]
        public static extern int latLngToCell(ref LatLng g, int res, out ulong h3);

        [DllImport(DLL, EntryPoint = "h3_cellToLatLng", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cellToLatLng(ulong h3, out LatLng outLatLng);

        [DllImport(DLL, EntryPoint = "h3_cellToBoundary", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cellToBoundary(ulong h3, out CellBoundary boundary);

        // -------------------- Grid Operations --------------------

        [DllImport(DLL, EntryPoint = "h3_gridDisk", CallingConvention = CallingConvention.Cdecl)]
        public static extern int gridDisk(ulong origin, int k, [Out] ulong[] outHexes);

        [DllImport(DLL, EntryPoint = "h3_gridDiskUnsafe", CallingConvention = CallingConvention.Cdecl)]
        public static extern int gridDiskUnsafe(ulong origin, int k, [Out] ulong[] outHexes);

        [DllImport(DLL, EntryPoint = "h3_gridRingUnsafe", CallingConvention = CallingConvention.Cdecl)]
        public static extern int gridRingUnsafe(ulong origin, int k, [Out] ulong[] outHexes);

        [DllImport(DLL, EntryPoint = "h3_gridDiskDistances", CallingConvention = CallingConvention.Cdecl)]
        public static extern int gridDiskDistances(ulong origin, int k, [Out] ulong[] outHexes, [Out] int[] distances);

        // -------------------- Polygon Coverage --------------------

        [DllImport(DLL, EntryPoint = "h3_maxPolygonToCellsSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int maxPolygonToCellsSize(ref GeoPolygon polygon, int res, uint flags, out long outSize);

        [DllImport(DLL, EntryPoint = "h3_polygonToCells", CallingConvention = CallingConvention.Cdecl)]
        public static extern int polygonToCells(ref GeoPolygon polygon, int res, uint flags, [Out] ulong[] outHexes);

        // -------------------- Validation & Conversion --------------------

        [DllImport(DLL, EntryPoint = "h3_isValidCell", CallingConvention = CallingConvention.Cdecl)]
        public static extern int isValidCell(ulong h3);

        [DllImport(DLL, EntryPoint = "h3_getResolution", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getResolution(ulong h3);

        [DllImport(DLL, EntryPoint = "h3_getBaseCellNumber", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getBaseCellNumber(ulong h3);

        [DllImport(DLL, EntryPoint = "h3_stringToH3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int stringToH3(string str, out ulong h3);

        [DllImport(DLL, EntryPoint = "h3_h3ToString", CallingConvention = CallingConvention.Cdecl)]
        public static extern int h3ToString(ulong h3, [Out] byte[] str, UIntPtr size);

        // -------------------- Geometry --------------------

        [DllImport(DLL, EntryPoint = "h3_degsToRads", CallingConvention = CallingConvention.Cdecl)]
        public static extern double degsToRads(double degrees);

        [DllImport(DLL, EntryPoint = "h3_radsToDegs", CallingConvention = CallingConvention.Cdecl)]
        public static extern double radsToDegs(double radians);

        [DllImport(DLL, EntryPoint = "h3_greatCircleDistanceKm", CallingConvention = CallingConvention.Cdecl)]
        public static extern double greatCircleDistanceKm(ref LatLng a, ref LatLng b);

        [DllImport(DLL, EntryPoint = "h3_cellAreaKm2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cellAreaKm2(ulong h3, out double area);

        [DllImport(DLL, EntryPoint = "h3_gridDistance", CallingConvention = CallingConvention.Cdecl)]
        public static extern int gridDistance(ulong origin, ulong h3, out long distance);

        // -------------------- Hierarchy --------------------

        [DllImport(DLL, EntryPoint = "h3_cellToParent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cellToParent(ulong h3, int parentRes, out ulong parent);

        [DllImport(DLL, EntryPoint = "h3_cellToChildrenSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cellToChildrenSize(ulong h3, int childRes, out long outSize);

        [DllImport(DLL, EntryPoint = "h3_cellToChildren", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cellToChildren(ulong h3, int childRes, [Out] ulong[] children);

        // -------------------- Set Operations --------------------

        [DllImport(DLL, EntryPoint = "h3_compactCells", CallingConvention = CallingConvention.Cdecl)]
        public static extern int compactCells([In] ulong[] input, [Out] ulong[] output, long count);

        [DllImport(DLL, EntryPoint = "h3_uncompactCellsSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uncompactCellsSize([In] ulong[] input, long count, int res, out long outSize);

        [DllImport(DLL, EntryPoint = "h3_uncompactCells", CallingConvention = CallingConvention.Cdecl)]
        public static extern int uncompactCells([In] ulong[] input, long count, [Out] ulong[] output, long outCount, int res);
        // -------------------- Cell Metrics --------------------

        [DllImport(DLL, EntryPoint = "h3_getHexagonEdgeLengthAvgM", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getHexagonEdgeLengthAvgM(int res, out double lengthM);

        [DllImport(DLL, EntryPoint = "h3_getHexagonAreaAvgM2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getHexagonAreaAvgM2(int res, out double areaM2);

        [DllImport(DLL, EntryPoint = "h3_edgeLengthM", CallingConvention = CallingConvention.Cdecl)]
        public static extern int edgeLengthM(ulong edge, out double lengthM);

        [DllImport(DLL, EntryPoint = "h3_isValidDirectedEdge", CallingConvention = CallingConvention.Cdecl)]
        public static extern int isValidDirectedEdge(ulong edge);

        [DllImport(DLL, EntryPoint = "h3_getDirectedEdgeOrigin", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getDirectedEdgeOrigin(ulong edge, out ulong origin);

        [DllImport(DLL, EntryPoint = "h3_getDirectedEdgeDestination", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getDirectedEdgeDestination(ulong edge, out ulong destination);
    }
}
