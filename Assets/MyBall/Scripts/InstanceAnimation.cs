using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Mathematics;

public class InstanceAnimation : MonoBehaviour
{
    public GameObject modelAni;
    // Start is called before the first frame update
    void Start()
    {
        RecordAnimationMesh();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void InitModel()
    {

    }

    private void RecordAnimationMesh()
    {
        List<string> animationNames = new List<string>();
        var root = modelAni;
        var rendererModel = modelAni.GetComponentInChildren<Renderer>();
        var meshFileter = modelAni.GetComponentInChildren<MeshFilter>();
        var anim = CovertAnimation(modelAni);
        Mesh mesh = null;
        Material material = null;
        Material[] materials = null;
        if (meshFileter)
        {
            mesh = meshFileter.sharedMesh;
        }

        if (rendererModel)
        {
            material = rendererModel.sharedMaterial;
            materials = rendererModel.sharedMaterials;
            var renderer = rendererModel as SkinnedMeshRenderer;
            if (renderer && anim != null) //写入模型的网格数据
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

                    var ws = new BoneWeight[renderer.sharedMesh.boneWeights.Length];

                    for (int i = 0; i < renderer.sharedMesh.boneWeights.Length; i++)
                    {
                        BoneWeight ww = new BoneWeight();
                        var w = renderer.sharedMesh.boneWeights[i];
                        int n0 = anim.bones.IndexOf(CalculateTransformPath(renderer.bones[w.boneIndex0], root.transform));
                        ww.boneIndex0 = n0;
                        int n1 = anim.bones.IndexOf(CalculateTransformPath(renderer.bones[w.boneIndex0], root.transform));
                        ww.boneIndex1 = n1;
                        int n2 = anim.bones.IndexOf(CalculateTransformPath(renderer.bones[w.boneIndex0], root.transform));
                        ww.boneIndex2 = n2;
                        int n3 = anim.bones.IndexOf(CalculateTransformPath(renderer.bones[w.boneIndex0], root.transform));
                        ww.boneIndex3 = n3;

                        ww.weight0 = w.weight0;
                        ww.weight1 = w.weight1;
                        ww.weight2 = w.weight2;
                        ww.weight3 = w.weight3;

                        ws[i] = ww;
                    }
                    mesh.boneWeights = ws;
                    mesh.triangles = renderer.sharedMesh.triangles;
                }
            }
        }

        if (mesh)
        {
            if(materials == null)
            {
                materials = new Material[1]{material};
            }
            MeshTemplate tempate = new MeshTemplate();
            tempate.Init(this, mesh, materials, rendererModel is SkinnedMeshRenderer);
            List<int> animationRefrection = new List<int>();
            if(anim != null)
            {
                for (int i = 0; i < anim.clips.Count; i++)
                {
                    var animName = anim.clipNames[i];
                    int id = animationNames.IndexOf(animName);
                    if(id == -1)
                    {
                        id = animationNames.Count;
                        animationNames.Add(animName);
                    }
                    while(id >= animationRefrection.Count)
                    {
                        animationRefrection.Add(-1);
                    }
                    animationRefrection[id] = i;
                }

                // tem
            }
        }
    }
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
    //GameObject转换成ModelAnimation
    ModelAnimation CovertAnimation(GameObject modelRoot)
    {
        ModelAnimation modelAnimation = new ModelAnimation();

        Animator animator = modelRoot.GetComponent<Animator>();

        if (!animator)
            return null;

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (!controller)
            return null;

        var clips = controller.animationClips;

        List<Transform> bones = new List<Transform>();
        var initTrans = new List<Matrix4x4>();
        Transform root = null;
        var skinmeshes = modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (var sr in skinmeshes)
        {
            if (sr && sr.sharedMesh)
            {
                for (int i = 0; i < sr.bones.Length; i++)
                {
                    if (!bones.Contains(sr.bones[i]))
                    {
                        bones.Add(sr.bones[i]);
                        initTrans.Add(sr.sharedMesh.bindposes[i]);
                    }
                }
                root = sr.transform.parent;
            }
        }

        MeshRenderer[] AllNoneSkinNodes = modelRoot.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var n in AllNoneSkinNodes)
        {
            if (!bones.Contains(n.transform))
            {
                bones.Add(n.transform);
                var init = Matrix4x4.TRS(
                modelRoot.transform.InverseTransformPoint(n.transform.position),
                Quaternion.Inverse(modelRoot.transform.rotation) * n.transform.rotation,
                n.transform.localScale);

                initTrans.Add(init.inverse);
            }
        }

        List<Matrix4x4[]> frames = new List<Matrix4x4[]>();

        for (int i = 0; i < clips.Length; i++)
        {
            ModelAnimation.AnimationClip clip = new ModelAnimation.AnimationClip();

            clip.startFrame = frames.Count;

            var name = clips[i].name;

            clip.looping = clips[i].isLooping ? 1 : 0;
            clip.len = clips[i].length;

            List<Vector3> transFrames = new List<Vector3>();
            List<Quaternion> rotsFrames = new List<Quaternion>();
            List<Vector3> scaleFrames = new List<Vector3>();

            for (float time = 0; time < clips[i].length; time += 0.1f)
            {
                frames.Add(DoFrame(animator, modelRoot, initTrans, bones, clips[i], time));
            }

            clip.endFrame = frames.Count;

            modelAnimation.clipNames.Add(name);
            modelAnimation.clips.Add(clip);
        }

        modelAnimation.transCount = bones.Count;
        modelAnimation.animationTrans = new Matrix4x4[frames.Count * bones.Count];
        for (int i = 0; i < frames.Count; i++)
        {
            for (int j = 0; j < bones.Count; j++)
            {
                modelAnimation.animationTrans[i * bones.Count + j] = frames[i][j];
            }
        }

        modelAnimation.animationBuffer = new ComputeBuffer(modelAnimation.animationTrans.Length, 64);
        modelAnimation.animationBuffer.SetData(modelAnimation.animationTrans);

        modelAnimation.animationClipInfoBuffer = new ComputeBuffer(modelAnimation.clips.Count, Marshal.SizeOf<ModelAnimation.AnimationClip>());
        modelAnimation.animationClipInfoBuffer.SetData(modelAnimation.clips.ToArray());

        foreach (var b in bones)
        {
            modelAnimation.bones.Add(CalculateTransformPath(b, modelRoot.transform));
        }

        return modelAnimation;
    }
    Matrix4x4[] DoFrame(Animator animator, GameObject sampleGO, List<Matrix4x4> initTrans, List<Transform> bones, AnimationClip clip, float time)
    {
        clip.SampleAnimation(sampleGO, time);

        var exposedTransforms = new Matrix4x4[bones.Count];
        for (int j = 0; j < bones.Count; j++)
        {
            //string path = AnimationUtility.CalculateTransformPath(bones[j], roots[j]);

            if (bones[j].gameObject.activeSelf)
            {
                Transform cur = bones[j];

                exposedTransforms[j] = Matrix4x4.TRS(
                    sampleGO.transform.InverseTransformPoint(cur.position),
                    Quaternion.Inverse(sampleGO.transform.rotation) * cur.rotation,
                    sampleGO.transform.InverseTransformVector(cur.localScale));

                exposedTransforms[j] = exposedTransforms[j] * initTrans[j];
            }
            else
            {
                exposedTransforms[j] = Matrix4x4.zero;
            }
        }

        return exposedTransforms;
    }
}

public class MeshTemplate
{
    public MeshTemplate()
    {

    }
    public void Init(InstanceAnimation _node, Mesh _mesh, Material[] _materials, bool _isSkin)
    {

    }
}


public class ModelAnimation
{
    public struct AnimationClip
    {
        public int startFrame;
        public int endFrame;
        public int looping;
        public float len;
    }

    public int transCount;
    public List<string> bones = new List<string>();

    public List<AnimationClip> clips = new List<AnimationClip>();
    public List<string> clipNames = new List<string>();

    public Matrix4x4[] animationTrans;

    public ComputeBuffer animationBuffer;
    public ComputeBuffer animationClipInfoBuffer;

    public void Destroy()
    {
        animationBuffer.Dispose();
        animationClipInfoBuffer.Dispose();
    }
}