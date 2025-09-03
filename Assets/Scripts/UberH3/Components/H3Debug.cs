using UnityEngine;
using UnityEngine.Scripting;
using H3Unity;

    [Preserve]
    [ExecuteAlways]
    [AddComponentMenu("H3/Debug")]
    public class H3Debug : MonoBehaviour
    {
        [Tooltip("Hex index to visualize")]
        [SerializeField]
        public string hexIndex;

        [Header("Gizmo Settings")]
        [SerializeField] public Color gizmoColor = Color.magenta;
        [SerializeField] public float gizmoSize = 0.2f;

        private void OnDrawGizmos()
        {
            if (string.IsNullOrEmpty(hexIndex)) return;

            var h3 = H3Utils.StringToH3(hexIndex);
            if (!H3.IsValid(h3)) return;

            var latlng = H3.ToLatLng(h3);
            var pos = new Vector3((float)latlng.lng, 0f, (float)latlng.lat);

            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(pos, gizmoSize);
        }

        public void SetHex(ulong index)
        {
            hexIndex = H3.ToHex(index);
        }

        public void SetFromLatLng(double latDeg, double lngDeg, int res)
        {
            SetHex(H3.FromLatLng(latDeg, lngDeg, res));
        }
    }
