using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameInfoDebugger : MonoBehaviour
{
    private int DrawCall;
    private int Tris;
    private int Verts;
    private int Batches;
    private GUIStyle fontStyle;
    // Start is called before the first frame update
    //void Start()
    //{
    //    DrawCall = UnityStats.drawCalls;
    //    Tris = UnityStats.triangles;
    //    Verts = UnityStats.vertices;
    //    Batches = UnityStats.batches;
    //    fontStyle = new GUIStyle();
    //    fontStyle.normal.background = null;
    //    fontStyle.normal.textColor = Color.white;
    //    fontStyle.fontSize = 30;
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    //private void OnGUI()
    //{
    //    GUILayout.BeginVertical();
    //    GUILayout.Label($"DrawCall : {UnityStats.drawCalls}", fontStyle);
    //    GUILayout.Label($"Tris : {UnityStats.triangles}", fontStyle);
    //    GUILayout.Label($"Verts : {UnityStats.vertices}", fontStyle);
    //    GUILayout.Label($"Batches : {UnityStats.batches}", fontStyle);
    //    GUILayout.EndVertical();
    //}
}
