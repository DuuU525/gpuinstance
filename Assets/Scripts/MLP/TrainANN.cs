using UnityEngine;
using JufGame.AI.ANN;
public class TrainANN : MonoBehaviour
{
    public int inputCount;
    public BPNN bp;
    public InitWFunc.Type initW;
    
    private float[][] inSet = //异或运算的输入
    {
        new float[]{1, 0},
        new float[]{1, 1},
        new float[]{0, 0},
        new float[]{0, 1},
    };
    private float[][] outSet = //异或运算的输出
    {
        new float[]{1},
        new float[]{0},
        new float[]{0},
        new float[]{1},
    };

    private void Awake()
    {
        bp.SetInput(inSet);//为训练器设置训练输入集
        bp.SetOutput(outSet);//为训练器设置训练输出集
    }
    private void Start()
    {
        bp.TrainingNet.InitWeights(inputCount, initW);//初始化权重
        bp.TrainingNet.InitCache();//初始化额外参数存储
    }

    private void Update()
    {
        if(bp.IsTrainEnd())//如果训练结束，就打印训练完成的神经网络 对训练输入集输出
        {
            Training.DebugNetRes(bp.TrainingNet, inSet);
            return;
        }
        bp.Train_OneTime();//没有训练结束，就每帧训练一次
    }
}