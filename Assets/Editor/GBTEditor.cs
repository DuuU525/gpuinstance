/// <summary>
/// Mesh网格编程 —— 点击拓展菜单，创建正二十面体
/// Created by 杜子兮(duzixi.com) 2015.2.20
/// www.lanou3g.com All Rights Reserved.
/// </summary>

using UnityEngine;
using UnityEditor;          // 使用编译器类
using System.Collections;
using System.Collections.Generic;
//Vector3.cross() //右手 确定朝向
public class GBTEditor : EditorWindow
{
    //报错mesh
    public static void SaveMesh(Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh)
    {
        string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
        if (string.IsNullOrEmpty(path)) return;

        path = FileUtil.GetProjectRelativePath(path);

        Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;

        if (optimizeMesh)
            MeshUtility.Optimize(meshToSave);

        AssetDatabase.CreateAsset(meshToSave, path);
        AssetDatabase.SaveAssets();
    }

    // 注意：该类继承EditorWindow，只能包含静态成员

    static Mesh mesh;            // 网格
    static Vector3[] Vs;         // 模型顶点坐标数组
    static Vector2[] UVs;        // UV贴图坐标
    static Vector3[] normals;    // 法线
    static Vector4[] tangents;   // 切线
    static int[] Ts;             // 三角形的点序列

    // 添加菜单项，并放置最顶端
    [MenuItem("GameObject/Create Other/正二十面体", false, -30)]
    static void CreateRegular()
    {
        // 先按12个顶点开辟顶点数组
        Vs = new Vector3[12];

        // 正二十面体顶点公式（度娘可查）
        float m = Mathf.Sqrt(50 - 10 * Mathf.Sqrt(5)) / 10;
        float n = Mathf.Sqrt(50 + 10 * Mathf.Sqrt(5)) / 10;

        // 按公式顺序对顶点坐标赋值
        Vs[0] = new Vector3(m, 0, n);
        Vs[1] = new Vector3(m, 0, -n);
        Vs[2] = new Vector3(-m, 0, n);
        Vs[3] = new Vector3(-m, 0, -n);
        Vs[4] = new Vector3(0, n, m);
        Vs[5] = new Vector3(0, -n, m);
        Vs[6] = new Vector3(0, n, -m);
        Vs[7] = new Vector3(0, -n, -m);
        Vs[8] = new Vector3(n, m, 0);
        Vs[9] = new Vector3(-n, m, 0);
        Vs[10] = new Vector3(n, -m, 0);
        Vs[11] = new Vector3(-n, -m, 0);

        // 正二十面体三角形的点序列
        Ts = new int[] {
                6,4,8,
                9,4,6,
                6,3,9,
                6,1,3,
                6,8,1,

                8,10,1,
                8,0,10,
                8,4,0,

                4,2,0,
                4,9,2,

                9,11,2,
                9,3,11,

                3,1,7,
                1,10,7,
                10,0,5,

                0,2,5,
                2,11,5,
                3,7,11,
                5,11,7,
                10,5,7
            };

        CreateObject("MY_Icosahedron");
    }

    // 创建对象函数（这个功能提出来，方便以后扩展）
    static void CreateObjectByMesh(string name)
    {
        GameObject regular = new GameObject();  // 创建游戏对象
        regular.name = name;                    // 根据参数命名
        regular.AddComponent<MeshFilter>();     // 添加MeshFilter组件
        regular.AddComponent<MeshRenderer>();   // 添加MeshRenderer组件

        mesh = new Mesh();                      // 创建网格
        mesh.vertices = Vs;                     // 网格的顶点
        mesh.triangles = Ts;                    // 网格的三角形
        mesh.uv = UVs;                          // 网格的UV坐标
        mesh.normals = normals;                 // 网格的法线
        mesh.tangents = tangents;               // 网格的切线
        regular.GetComponent<MeshFilter>().mesh = mesh; // 添加网格

        SaveMesh(mesh, $"{name}_mesh", true, true);
    }

    [MenuItem("GameObject/Create Other/三角形")]
    static void InitTriangle()
    {
        Vs = new Vector3[8];
        //顶点
        Vs[0] = new Vector3(0, 0, 0);
        Vs[1] = new Vector3(1, 0, 0);
        Vs[2] = new Vector3(0, 0, 1);
        //面
        Ts = new int[] {
            0,2,1,
            };

        CreateObject("MY_Triangle");
    }
    [MenuItem("GameObject/Create Other/正方体面体")]
    static void InitCube()
    {
        Vs = new Vector3[8];
        //顶点
        Vs[0] = new Vector3(1, 1, 1);
        Vs[1] = new Vector3(1, -1, 1);
        Vs[2] = new Vector3(1, -1, -1);
        Vs[3] = new Vector3(1, 1, -1);
        Vs[4] = new Vector3(-1, 1, -1);
        Vs[5] = new Vector3(-1, 1, 1);
        Vs[6] = new Vector3(-1, -1, 1);
        Vs[7] = new Vector3(-1, -1, -1);

        //面
        Ts = new int[] {
            0,1,2,
            0,2,3,

            4,3,2,
            4,2,7,

            6,5,4,
            6,4,7,

            5,6,1,
            5,1,0,

            3,4,5,
            3,5,0,

            7,2,1,
            7,1,6,
            };

        CreateObject("MYCube");
    }

    [MenuItem("GameObject/Create Other/正六边形")]
    static void InitRegularHexagon()
    {
        GenVertexs(6);

        //面
        Ts = new int[] {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            };

        CreateObject("MYHexagon");
    }

    /// <summary>
    /// 获得n正多边形所有顶点坐标
    /// </summary>
    /// <param name="_n">边数</param>
    /// <param name="_r">半径</param>
    /// <returns></returns>
    static List<Vector2> GetVertexsPos(int _n, float _r = 1.0f)
    {
        List<Vector2> list = new List<Vector2>();
        for (int i = 0; i < _n; i++)
        {
            list.Add(new Vector2(_r * Mathf.Cos(2 * Mathf.PI * i / _n), _r * Mathf.Sin(2 * Mathf.PI * i / _n)));
        }
        return list;
    }

    static void GenVertexs(int n)
    {
        Vs = new Vector3[n];
        var vertexs = GetVertexsPos(n);
        vertexs.Reverse();
        //顶点
        for (int i = 0; i < n; i++)
        {
            Vs[i] = new Vector3(vertexs[i].x, 0, vertexs[i].y);
        }
    }

    [MenuItem("GameObject/Create Other/正五边形")]
    static void InitRegularPentagon()
    {
        GenVertexs(5);
        //面
        Ts = new int[] {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            };

        CreateObject("MY_Pentagon");
    }

    static void CreateObject(string _name)
    {
        // 根据面的顺序，重新创建新的顶点数组，用于计算顶点法线
        Vector3[] newVs = new Vector3[Ts.Length];
        for (int i = 0; i < newVs.Length; i++)
        {
            Debug.Log(Vs[Ts[i]]);
            newVs[i] = Vs[Ts[i]];
        }
        Vs = newVs;
        UVs = new Vector2[Vs.Length];
        normals = new Vector3[Vs.Length];
        tangents = new Vector4[Vs.Length];

        // 根据新的点，设置三角面的顶点ID并计算点法线
        for (int i = 0; i < Ts.Length - 2; i += 3)
        {
            Vector3 normal = Vector3.Cross(Vs[i + 1] - Vs[i], Vs[i + 2] - Vs[i]);  // 计算点的法线
            for (int j = 0; j < 3; j++)
            {
                Ts[i + j] = i + j;        // 重新设置面的顶点ID
                normals[i + j] = normal;  // 点的法线赋值
            }
        }

        // 设置每个点的切线和UV
        for (int i = 0; i < Vs.Length; i++)
        {
            tangents[i] = new Vector4(-1, 0, 0, -1);    // 切线
            UVs[i] = new Vector2(Vs[i].x, Vs[i].y);     // UV坐标
        }

        CreateObjectByMesh(_name);
    }
}