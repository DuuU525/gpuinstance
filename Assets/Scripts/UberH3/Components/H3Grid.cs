using UnityEngine;
using UnityEngine.Scripting;
using H3Unity;


    [Preserve]
    [ExecuteAlways]
    [AddComponentMenu("H3/Grid")]
    public class H3Grid : MonoBehaviour
    {
        [Header("Origin")]
        [Tooltip("Hex index that serves as grid origin")]
        [SerializeField]
        private string originHex;

        [Header("Coverage Settings")]
        [Tooltip("Radius for disk or ring coverage")]
        [Range(0, 10)]
        [SerializeField]
        public int radius = 1;

        [Tooltip("Use ring instead of disk")]
        [SerializeField]
        private bool useRing;

        [Header("Gizmo Settings")]
        [SerializeField]
        private Color gizmoColor = Color.green;

        [SerializeField]
        private float gizmoSize = 0.1f;

        public ulong Origin => H3Utils.StringToH3(originHex);

        public ulong[] Coverage =>
            useRing ? H3.Ring(Origin, radius) : H3.Disk(Origin, radius);

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            foreach (var h3 in Coverage)
            {
                if (!H3.IsValid(h3)) continue;
                var latlng = H3.ToLatLng(h3);
                var pos = new Vector3((float)latlng.lng, 0f, (float)latlng.lat);
                Gizmos.DrawSphere(pos, gizmoSize);
            }
        }

        public void SetOriginFromLatLng(double latDeg, double lngDeg, int res)
        {
            originHex = H3.ToHex(H3.FromLatLng(latDeg, lngDeg, res));
        }
    }