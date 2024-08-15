using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using System;


public class InstanceModel : MonoBehaviour
{
    public int instanceCount;
    public GameObject modelAni;
    public Material modelMaterial;
    private Mesh instanceMesh;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private MeshTemplate meshTemp;
    private ModelAnimation modelAniData;
    // Start is called before the first frame update
    void Start()
    {
        InitMatixs();//随机位置朝向矩阵
        modelAniData = new ModelAnimation(modelAni);
        RecordMesh(modelAni, modelAni, modelAniData, ref meshTemp);//采样模型网格
        instanceMesh = meshTemp.mesh;
        SetArgs();
    }
    private List<float4x4> listTrans = new List<float4x4>();
    private List<float> listRotas = new List<float>();
    private void InitMatixs()
    {
        listTrans = new List<float4x4>();
        for (int i = 0; i < instanceCount; i++)
        {
            float rota = 0;
            listTrans.Add(RandomTrans(ref rota));
            listRotas.Add(rota);
        }
    }
    private void SetArgs()
    {
        argsBuffer?.Release();
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(0);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(0);
            args[3] = (uint)instanceMesh.GetBaseVertex(0);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);
    }

    private Matrix4x4 RandomTrans(ref float rot)
    {
        // return new Matrix4x4();
        var pos = new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
        rot = UnityEngine.Random.Range(0, 360);
        var rotate = Quaternion.AngleAxis(rot, Vector3.up); //Quaternion.LookRotation(pos) * Quaternion.AngleAxis(90.0f, Vector3.right) * Quaternion.AngleAxis(rot, Vector3.up);
        var size = Vector3.one;
        return Matrix4x4.TRS(pos, rotate, size);
    }

    public float speedMove = 30;
    private void UpdateMovePos()
    {
        for (int i = 0; i < listTrans.Count; i++)
        {
            var trans = listTrans[i];
            var rotaRandom = listRotas[i];
            var pos = ExtractPosition(trans);
            var rota = ExtractRotation(trans);
            var scale = ExtractScale(trans);
            var dir = Quaternion.AngleAxis(rotaRandom, Vector3.up);

            pos += dir * Vector3.forward * Time.deltaTime * speedMove;
            listTrans[i] = Matrix4x4.TRS(pos, rota, scale);
        }
    }

    private void UpdateAnimMatrix()
    {
        //  modelAniData.transCount
        //  modelAniData.animationBuffer
        //  modelAniData.animationClipInfoBuffer
    }

    #region Tools 
    //矩阵获得坐标
    public Vector3 ExtractPosition(Matrix4x4 matrix)
    {
        return new Vector3(matrix.m03, matrix.m13, matrix.m23);
    }
    //矩阵获得大小
    public Vector3 ExtractScale(Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
    //矩阵获得旋转
    private Quaternion ExtractRotation(Matrix4x4 matrix)
    {
        return Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
    }

    #endregion

    ComputeBuffer transBuffer;
    //移动变换矩阵
    private void UpdateTransBuffers()
    {
        transBuffer?.Release();
        transBuffer = new ComputeBuffer(instanceCount, 64);
        transBuffer.SetData(listTrans.ToArray());
        modelMaterial.SetBuffer("_transBuffer", transBuffer);
    }

    public float timeBlendAnim = 0.35f;
    private float frameDraw = 0;
    //动画buffer
    private void UpdateAnimBuffers()
    {
        modelMaterial.SetFloat("_AnimBlendTime", timeBlendAnim);
        modelMaterial.SetInt("_ModelTransCount", modelAniData.transCount);
        modelMaterial.SetBuffer("_AnimBuffer", modelAniData.animationBuffer);
        modelMaterial.SetBuffer("_AnimClipBuffer", modelAniData.animationClipInfoBuffer);
    }
    void Update()
    {
        if (instanceMesh == null)
        {
            return;
        }
        //UpdateMovePos();
        UpdateTransBuffers();
        UpdateAnimBuffers();
        SetArgs();
        Graphics.DrawMeshInstancedIndirect(
            instanceMesh,
            0,
            modelMaterial,
            new Bounds(Vector3.zero, new Vector3(3000.0f, 3000.0f, 3000.0f)),
            argsBuffer,
            0,
            null,
            UnityEngine.Rendering.ShadowCastingMode.On,
            true);
    }
    //采样网格
    public void RecordMesh(GameObject root, GameObject c, ModelAnimation modelAni, ref MeshTemplate templateMesh)
    {
        var r = c.GetComponent<Renderer>();
        var m = c.GetComponent<MeshFilter>();

        Mesh mesh = null;
        Material material = null;
        Material[] materials = null;

        if (m)//网格
        {
            mesh = m.sharedMesh;
        }
        if (r)//
        {
            material = r.sharedMaterial;
            materials = r.sharedMaterials;
            if (r is SkinnedMeshRenderer renderer && modelAni.HasBones())
            {
                if (renderer.sharedMesh.boneWeights.Length > 0)
                {
                    mesh = new Mesh();
                    mesh.vertices = renderer.sharedMesh.vertices;
                    mesh.normals = renderer.sharedMesh.normals;
                    mesh.colors = renderer.sharedMesh.colors;
                    mesh.uv = renderer.sharedMesh.uv;
                    mesh.uv2 = renderer.sharedMesh.uv2;
                    mesh.uv3 = renderer.sharedMesh.uv3;
                    mesh.uv4 = renderer.sharedMesh.uv4;
                    mesh.uv5 = renderer.sharedMesh.uv5;
                    mesh.uv6 = renderer.sharedMesh.uv6;
                    mesh.uv7 = renderer.sharedMesh.uv7;
                    mesh.uv8 = renderer.sharedMesh.uv8;
                    mesh.tangents = renderer.sharedMesh.tangents;
                    mesh.triangles = renderer.sharedMesh.triangles;

                    var ws = new BoneWeight[renderer.sharedMesh.boneWeights.Length];

                    for (int i = 0; i < renderer.sharedMesh.boneWeights.Length; i++)
                    {
                        BoneWeight ww = new BoneWeight();
                        var w = renderer.sharedMesh.boneWeights[i];
                        int n0 = modelAni.GetIndexByClipName(renderer.bones[w.boneIndex0], root.transform);
                        ww.boneIndex0 = n0;
                        int n1 = modelAni.GetIndexByClipName(renderer.bones[w.boneIndex1], root.transform);
                        ww.boneIndex1 = n1;
                        int n2 = modelAni.GetIndexByClipName(renderer.bones[w.boneIndex2], root.transform);
                        ww.boneIndex2 = n2;
                        int n3 = modelAni.GetIndexByClipName(renderer.bones[w.boneIndex3], root.transform);
                        ww.boneIndex3 = n3;

                        ww.weight0 = w.weight0;
                        ww.weight1 = w.weight1;
                        ww.weight2 = w.weight2;
                        ww.weight3 = w.weight3;

                        ws[i] = ww;
                    }
                    mesh.boneWeights = ws;
                }
            }
        }

        if (mesh)
        {
            if (materials == null)
            {
                materials = new Material[1] { material };
            }

            MeshTemplate template = new MeshTemplate();
            List<string> listAnimName = new List<string>();
            template.Init(this, mesh, materials, true);
            List<int> animationRefection = new List<int>();
            if (modelAni.HasBones())
            {
                for (int i = 0; i < modelAni.listClips.Count; i++)
                {
                    var nameAnim = modelAni.listClipNames[i];
                    int id = listAnimName.IndexOf(nameAnim);
                    if (id == -1)
                    {
                        id = listAnimName.Count;
                        listAnimName.Add(nameAnim);
                    }
                    while (id >= animationRefection.Count)
                    {
                        animationRefection.Add(-1);
                    }
                    animationRefection[id] = i;
                }
                template.ModelTransId = modelAni.GetIndexByClipName(c.transform, root.transform);
            }
            template.animationRefection = animationRefection.ToArray();
            //instanceMesh = mesh;   
            templateMesh = template;
        }
        for (int i = 0; i < c.transform.childCount; i++)
        {
            RecordMesh(root, c.transform.GetChild(i).gameObject, modelAni, ref templateMesh);
        }
    }

    public class MeshTemplate
    {
        private InstanceModel modelNode;
        public Mesh mesh;
        private Material[] materials;
        private bool isSkin;
        public int ModelTransId;
        public int[] animationRefection;
        public MeshTemplate()
        {

        }
        public void Init(InstanceModel _node, Mesh _mesh, Material[] _materials, bool _isSkin)
        {
            modelNode = _node;
            if (_mesh)
            {
                this.mesh = _mesh;
                this.isSkin = _isSkin;
                this.materials = new Material[mesh.subMeshCount];
                for (var i = 0; i < mesh.subMeshCount; i++)
                {
                    this.materials[i] = Material.Instantiate(_node.modelMaterial);
                    this.materials[i].CopyPropertiesFromMaterial(_materials[i]);
                    this.materials[i].enableInstancing = true;
                }
            }
        }

        private int idAnimPlaying;
        private ModelAnimation modelAnim;
        //改变模型动画
        public void ChangeInstanceAnimate(int animID, float animTime)
        {
            if (animID != -1)
            {
                idAnimPlaying = animID;
            }
            //modelAnim.GetAnimationClips(animID);

        }
        public void Draw(ModelInstancePage _modelTransBuffer, int _modelTransBufferIndex)
        {

        }
    }

    public class ModelInstancePage
    {
        public ComputeBuffer modelTransBuffer;
        public ComputeBuffer modelClicpBuffer;

    }

    public struct ModelAnimationClip
    {
        public int startFrame;
        public int endFrame;
        public int looping;
        public float len;
    }


    public class ModelAnimation
    {
        public List<string> listBones = new List<string>();
        public ComputeBuffer animationClipInfoBuffer;
        public ComputeBuffer animationBuffer;
        public Matrix4x4[] animationTrans;
        public int transCount;
        public List<string> listClipNames = new List<string>();
        public List<ModelAnimationClip> listClips = new List<ModelAnimationClip>();
        public ModelAnimation(GameObject _modelRoot)
        {
            CoverAnimation(_modelRoot);
        }

        //是否有骨骼
        public bool HasBones()
        {
            return transCount > 0;
        }

        // public ModelAnimationClip ModelAnimationClip(int idAnimation)
        // {
        //     return listClips[idAnimation];
        // }

        //采样网格
        private void SampleMeshs(GameObject modelRoot, ref List<Transform> bones, ref List<Matrix4x4> listTrans)
        {
            var skinMeshes = modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skinMesh in skinMeshes)
            {
                if (skinMesh && skinMesh.sharedMesh)
                {
                    for (int i = 0; i < skinMesh.bones.Length; i++)
                    {
                        if (!bones.Contains(skinMesh.bones[i]))
                        {
                            bones.Add(skinMesh.bones[i]);
                            listTrans.Add(skinMesh.sharedMesh.bindposes[i]);
                        }
                    }
                }
            }

            MeshRenderer[] allNoneSkinNodes = modelRoot.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var skinNode in allNoneSkinNodes)
            {
                if (!bones.Contains(skinNode.transform))
                {
                    bones.Add(skinNode.transform);
                    var pos = modelRoot.transform.InverseTransformPoint(skinNode.transform.position);
                    var qua = Quaternion.Inverse(modelRoot.transform.rotation) * skinNode.transform.rotation;
                    var scale = skinNode.transform.localScale;
                    Matrix4x4 matrix = Matrix4x4.TRS(pos, qua, scale);
                    listTrans.Add(matrix.inverse);
                }
            }
        }

        //采样动画clip
        private void SampleClips(GameObject modelRoot, ref List<Transform> bones, ref List<Matrix4x4> listTrans, ref List<Matrix4x4[]> frames)
        {
            Animator animator = modelRoot.GetComponent<Animator>();
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            var clips = controller.animationClips;
            frames = new List<Matrix4x4[]>();
            for (int i = 0; i < clips.Length; i++)
            {
                var dataClip = clips[i];
                ModelAnimationClip clip = new ModelAnimationClip();
                clip.startFrame = frames.Count;
                var nameClip = dataClip.name;
                clip.looping = dataClip.isLooping ? 1 : 0;
                clip.len = dataClip.length;//动画片段总时长
                for (float frame = 0; frame < clip.len; frame += 0.1f)//采样动画clip播放时间内每隔0.1f记录一次矩阵
                {
                    Matrix4x4[] matrixes = SampleClipFrame(modelRoot, listTrans, bones, dataClip, frame);
                    frames.Add(matrixes);
                }
                clip.endFrame = frames.Count;

                this.listClipNames.Add(nameClip);
                this.listClips.Add(clip);
            }
        }
        //记录骨骼动画的数据
        private void CoverAnimation(GameObject modelRoot)
        {
            Animator animator = modelRoot.GetComponent<Animator>();
            if (animator == null)
            {
                return;
            }

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller == null)
            {
                return;
            }

            List<Transform> bones = new List<Transform>();
            List<Matrix4x4> listTrans = new List<Matrix4x4>();

            List<Matrix4x4[]> frames = new List<Matrix4x4[]>();
            SampleMeshs(modelRoot, ref bones, ref listTrans);
            SampleClips(modelRoot, ref bones, ref listTrans, ref frames);//0.1s采样一次

            animationTrans = new Matrix4x4[frames.Count * bones.Count];//所有帧对应的骨骼的矩阵
            for (int i = 0; i < frames.Count; i++)
            {
                for (int j = 0; j < bones.Count; j++)
                {
                    animationTrans[i * bones.Count + j] = frames[i][j];
                }
            }
            animationBuffer = new ComputeBuffer(animationTrans.Length, 64);
            animationBuffer.SetData(animationTrans);
            animationClipInfoBuffer = new ComputeBuffer(listClips.Count, Marshal.SizeOf<ModelAnimationClip>());
            animationClipInfoBuffer.SetData(listClips.ToArray());

            transCount = bones.Count;
            foreach (var bone in bones)//记录所有骨骼的节点路径
            {
                string pathCalculateTrans = CalculateTransformPath(bone, modelRoot.transform);
                listBones.Add(pathCalculateTrans);
            }
        }

        //节点路径
        string CalculateTransformPath(Transform target, Transform root)
        {
            Transform transform = target;
            string path = transform.name;
            while (transform.parent != null && transform.parent != root)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }


        //获得动画帧下的矩阵数组
        private Matrix4x4[] SampleClipFrame(GameObject modelRoot, List<Matrix4x4> initTrans, List<Transform> bones, AnimationClip dataClip, float frame)
        {
            dataClip.SampleAnimation(modelRoot, frame);
            var expossedTransforms = new Matrix4x4[bones.Count];
            for (int i = 0; i < bones.Count; i++)
            {
                var nodeBone = bones[i];
                if (nodeBone.gameObject.activeSelf)
                {
                    var pos = modelRoot.transform.InverseTransformPoint(nodeBone.position);
                    var qua = Quaternion.Inverse(modelRoot.transform.rotation) * nodeBone.rotation;
                    var scale = modelRoot.transform.InverseTransformVector(nodeBone.localScale);
                    var matrix = Matrix4x4.TRS(pos, qua, scale);
                    expossedTransforms[i] = matrix * initTrans[i];
                }
                else
                {
                    expossedTransforms[i] = Matrix4x4.zero;
                }
            }
            return expossedTransforms;
        }

        //获得骨骼索引
        internal int GetIndexByClipName(Transform target, Transform root)
        {
            string path = CalculateTransformPath(target, root);
            return listBones.IndexOf(path);
        }
    }
}

