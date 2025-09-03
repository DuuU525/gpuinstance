using UnityEngine;
using UnityEngine.Scripting;
using H3Unity;


    [Preserve]
    [ExecuteAlways]
    [AddComponentMenu("H3/Test Driver")]
    public class H3Test : MonoBehaviour
    {
        [SerializeField] private double lat = 35.0;
        [SerializeField] private double lng = -90.0;
        [SerializeField] private int resolution = 9;

        private void Start()
        {
            var index = H3.FromLatLng(lat, lng, resolution);
            Debug.Log($"[H3] LatLng → Index: {H3.ToHex(index)}");

            var pos = H3.ToLatLng(index);
            Debug.Log($"[H3] Index → LatLng: {pos.lat:F6}, {pos.lng:F6}");

            var ring = H3.Ring(index, 1);
            Debug.Log($"[H3] Ring count: {ring.Length}");

            var parent = H3.ToParent(index, 5);
            Debug.Log($"[H3] Parent (res=5): {H3.ToHex(parent)}");
        }
    }

