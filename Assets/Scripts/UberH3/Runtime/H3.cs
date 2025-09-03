using System;
using System.Text;
using UnityEngine.Scripting;

namespace H3Unity
{
    [Preserve]
    public static class H3
    {
        // ---- Indexing ----

        public static ulong FromLatLng(double latDeg, double lngDeg, int res)
        {
            var latlng = new LatLng
            {
                lat = H3Utils.DegsToRads(latDeg),
                lng = H3Utils.DegsToRads(lngDeg)
            };

            if (H3Native.latLngToCell(ref latlng, res, out ulong h3) != 0)
                throw new H3Exception("Failed to convert LatLng to H3 index");

            return h3;
        }

        public static LatLng ToLatLng(ulong h3)
        {
            if (H3Native.cellToLatLng(h3, out LatLng result) != 0)
                throw new H3Exception("Failed to convert H3 index to LatLng");

            return result;
        }

        // ---- Grid Operations ----

        public static ulong[] Ring(ulong origin, int k)
        {
            var output = new ulong[6 * k];

            if (H3Native.gridRingUnsafe(origin, k, output) != 0)
                throw new H3Exception("gridRingUnsafe failed", k);

            return output;
        }

        public static ulong[] Disk(ulong origin, int k)
        {
            var output = new ulong[1 + 3 * k * (k + 1)];

            if (H3Native.gridDisk(origin, k, output) != 0)
                throw new H3Exception("gridDisk failed", k);

            return output;
        }

        // ---- Hierarchy ----

        public static ulong ToParent(ulong h3, int parentRes)
        {
            if (H3Native.cellToParent(h3, parentRes, out ulong parent) != 0)
                throw new H3Exception("cellToParent failed", parentRes);

            return parent;
        }

        public static ulong[] ToChildren(ulong h3, int childRes)
        {
            if (H3Native.cellToChildrenSize(h3, childRes, out long size) != 0 || size <= 0)
                throw new H3Exception("cellToChildrenSize failed", childRes);

            var children = new ulong[size];

            if (H3Native.cellToChildren(h3, childRes, children) != 0)
                throw new H3Exception("cellToChildren failed", childRes);

            return children;
        }

        // ---- Metadata ----

        public static int GetResolution(ulong h3) => H3Utils.GetResolution(h3);
        public static int GetBaseCell(ulong h3) => H3Utils.GetBaseCellNumber(h3);

        public static bool IsValid(ulong h3) => H3Native.isValidCell(h3) != 0;

        // ---- Conversion ----

        public static string ToHex(ulong h3) => H3Utils.H3ToString(h3);

        public static ulong FromHex(string hex) => H3Utils.StringToH3(hex);

        // ---- Area & Distance ----

        public static double CellAreaKm2(ulong h3)
        {
            if (H3Native.cellAreaKm2(h3, out double area) != 0)
                throw new H3Exception("cellAreaKm2 failed");

            return area;
        }

        public static long GridDistance(ulong origin, ulong target)
        {
            if (H3Native.gridDistance(origin, target, out long dist) != 0)
                throw new H3Exception("gridDistance failed");

            return dist;
        }
    }
}
