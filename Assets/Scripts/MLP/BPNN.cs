using UnityEngine;

namespace JufGame.AI.ANN
{
    [System.Serializable]
    public class BPNN : Training
    {
        public float[][] OutputSet => outputSet;
        [SerializeField] private float meanError = float.MaxValue;
        [SerializeField] private LossFunc.Type errorFunc;
        [SerializeField] private UpdateWFunc.Type updateWFunc;
        private float[][] outputSet;
        public BPNN(NeuralNet initedNet, LossFunc.Type errorFunc, UpdateWFunc.Type updateWFunc, int maxEpochs): base(initedNet,maxEpochs)
        {
            this.errorFunc = errorFunc;
            this.updateWFunc = updateWFunc;
        }
        public void SetOutput(float[][] outputSet)
        {
            this.outputSet = outputSet;
        }
        public override bool IsTrainEnd()//判断是否训练完成
        {
            return meanError < TrainingNet.TargetError 
                || maxEpochs < TrainingNet.CurEpochs;
        }
        public override void Train()
        {
            meanError = float.MaxValue;
            while(!IsTrainEnd())
            {
                Train_OneTime();
            }
        }
        public override void Train_OneTime()
        {
            int samplesCount = inputSet.GetLength(0);//记下样本数量
            ++TrainingNet.CurEpochs;//更新迭代次数
            meanError = 0;
            for(int i = 0; i < samplesCount; ++i)
            {
                ForWard(i);
                Backpropagation();
                UpdateWFunc.CalcDelta(TrainingNet, inputSet[i]);
            }
            UpdateWFunc.UpdateNetWeights( updateWFunc, TrainingNet, samplesCount);
            meanError /= samplesCount;//取样本误差均值作为本次迭代的误差
            #if UNITY_EDITOR
            Debug.Log($"误差：{meanError}");//调试时用的
            #endif
        }
        private void ForWard(int trainIndex)
        {
            var outLayer = TrainingNet.OutLayer;
            TrainingNet.CalcNet(inputSet[trainIndex]);
            meanError = LossFunc.Calc(errorFunc, outputSet[trainIndex], outLayer);
            /*这里图省事，将反向传播的第一步一并计算了*/
            LossFunc.Diff(errorFunc, outputSet[trainIndex], outLayer);//损失函数求导
            for(int i = 0; i < outLayer.Neurons.Length; ++i)//输出层激活函数求导
            {
                outLayer.Neurons[i].Params["Error"] *= ActivationFunc.Diff(TrainingNet.outAcFunc, outLayer, i);
            }
        }
        private void Backpropagation()
        {
            var lastLayer = TrainingNet.OutLayer;
            for(int i = TrainingNet.HdnLayers.Length - 1; i > -1; --i)
            {
                var curLayer = TrainingNet.HdnLayers[i];
                for(int j = 0; j < curLayer.Neurons.Length; ++j)
                {
                    var curNeuron = curLayer.Neurons[j];
                    //每次计算损失时要清零，避免上次迭代结果产生的干扰
                    curNeuron.Params["Error"] = 0;
                    for(int k = 0; k < lastLayer.Neurons.Length; ++k)
                    {
                        var lastNeuron = lastLayer.Neurons[k];
                        curNeuron.Params["Error"] += lastNeuron.Params["Error"] * lastNeuron.Weights[j];
                    }
                    curNeuron.Params["Error"] *= ActivationFunc.Diff(TrainingNet.hdnAcFunc, curLayer, j);
                }
                lastLayer = curLayer;
            }
        }
    }
}