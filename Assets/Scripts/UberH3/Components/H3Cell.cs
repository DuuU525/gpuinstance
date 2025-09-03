using System;
using UnityEngine;
using UnityEngine.Scripting;
using H3Unity;

    [Preserve]
    [ExecuteAlways]
    [AddComponentMenu("H3/Cell")]
    public class H3Cell : MonoBehaviour
    {
        [Tooltip("Hex index for this cell")]
        [SerializeField]
        private string hexIndex;

        public ulong Index => H3Utils.StringToH3(hexIndex);

        public LatLng Position => H3.ToLatLng(Index);

        public void SetIndex(ulong index)
        {
            hexIndex = H3.ToHex(index);
        }

        public void SetFromLatLng(double latDeg, double lngDeg, int res)
        {
            SetIndex(H3.FromLatLng(latDeg, lngDeg, res));
        }

        private void OnDrawGizmos()
        {
            var latlng = Position;
            var pos = new Vector3((float)latlng.lng, 0f, (float)latlng.lat);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(pos, 0.1f);
        }
    }
