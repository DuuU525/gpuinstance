using System;

namespace JufGame.AI.ANN
{
    public static class UpdateWFunc
    {
        private const float MinDelta = 1e-7f;
        private const float beta1 = 0.9f;
        private const float beta2 = 0.999f;
        private delegate void UpdateLayer(Neuron curNeuron, float learningRate, int curEpochs, int samplesCount);
        public enum Type
        {
            SGD, Momentum, AdaGrad, Adam
        }
        public static void UpdateNetWeights(Type type, NeuralNet net, int samplesCount)
        {
            UpdateLayer updateLayerFunc = type switch
            {
                Type.Momentum => Momentum_UpdateW,
                Type.AdaGrad => AdaGrad_UpdateW,
                Type.Adam => Adam_UpdateW,
                _ => SGD_UpdateW,
            };
            var curLayer = net.OutLayer;
            for(int j = 0; j < curLayer.Neurons.Length; ++j)
            {
                updateLayerFunc(curLayer.Neurons[j], net.LearningRate, net.CurEpochs ,samplesCount);
            }
            for(int i = 0; i < net.HdnLayers.Length; ++i)
            {
                curLayer = net.HdnLayers[i];
                for(int j = 0; j < curLayer.Neurons.Length; ++j)
                {
                    updateLayerFunc(curLayer.Neurons[j], net.LearningRate, net.CurEpochs, samplesCount);
                }	
            }
        }

        #region 各参数损失贡献计算
        //计算各参数对损失的贡献程度（也是各参数的变化的值）
        public static void CalcDelta(NeuralNet net, float[] input)
        {
            var lastInput = input;
            for(int i = 0; i < net.HdnLayers.Length; ++i)
            {
                var curLayer = net.HdnLayers[i];
                CalcLayerDelta(curLayer, lastInput);
                lastInput = curLayer.Output;
            }
            CalcLayerDelta(net.OutLayer, lastInput);
        }
        private static void CalcLayerDelta(Layer curLayer, float[] lastInput)
        {
            for(int j = 0, k; j < curLayer.Neurons.Length; ++j)
            {
                var curNeuron = curLayer.Neurons[j];
                for(k = 0; k < lastInput.Length; ++k)
                {
                    //通过反向传播时神经元的损失，计算每个权重的贡献贡献并累加
                    curNeuron.WeightParams["Delta"][k] += curNeuron.Params["Error"] * lastInput[k];
                }
                //同理计算偏置的损失贡献
                curNeuron.WeightParams["Delta"][k] += curNeuron.Params["Error"];
            }
        }
        #endregion

        #region SGD
        private static void SGD_UpdateW(Neuron curNeuron, float learningRate, int curEpochs, int samplesCount)
        {
            for(int k = 0; k < curNeuron.Weights.Length; ++k)
            {
                var gradient = curNeuron.WeightParams["Delta"][k] / samplesCount;
                curNeuron.Weights[k] -= learningRate * gradient;
                curNeuron.WeightParams["Delta"][k] = 0;
            }	
        }
        #endregion

        #region Momentum
        private static void Momentum_UpdateW(Neuron curNeuron, float learningRate, int curEpochs, int samplesCount)
        {
            for(int k = 0; k < curNeuron.Weights.Length; ++k)
            {
                var gradient = curNeuron.WeightParams["Delta"][k] / samplesCount;
                curNeuron.WeightParams["m"][k] = beta1 * curNeuron.WeightParams["m"][k] - learningRate * gradient;
                curNeuron.Weights[k] += curNeuron.WeightParams["m"][k];
                curNeuron.WeightParams["Delta"][k] = 0;
            }
        }
        #endregion

        #region AdaGrad
        private static void AdaGrad_UpdateW(Neuron curNeuron, float learningRate, int curEpochs, int samplesCount)
        {
            for(int k = 0; k < curNeuron.Weights.Length; ++k)
            {
                var gradient = curNeuron.WeightParams["Delta"][k] / samplesCount;
                curNeuron.WeightParams["v"][k] += gradient * gradient;
                curNeuron.Weights[k] -= learningRate * gradient / MathF.Sqrt(curNeuron.WeightParams["v"][k] + MinDelta);
                curNeuron.WeightParams["Delta"][k] = 0;
            }
        }
        #endregion

        #region  Adam
        private static void Adam_UpdateW(Neuron curNeuron, float learningRate, int curEpochs, int samplesCount)
        {
            for(int k = 0; k < curNeuron.Weights.Length; ++k)
            {
                var gradient = curNeuron.WeightParams["Delta"][k] / samplesCount;
                curNeuron.WeightParams["m"][k] = beta1 * curNeuron.WeightParams["m"][k] + (1 - beta1) * gradient;
                curNeuron.WeightParams["v"][k] = beta2 * curNeuron.WeightParams["v"][k] + (1 - beta2) * gradient * gradient;
                var mHat = curNeuron.WeightParams["m"][k] / (1 - MathF.Pow(beta1, curEpochs));
                var vHat = curNeuron.WeightParams["v"][k] / (1 - MathF.Pow(beta2, curEpochs));
                curNeuron.Weights[k] -= learningRate * mHat / (MathF.Sqrt(vHat) + MinDelta);
                curNeuron.WeightParams["Delta"][k] = 0;
            }
        }
        #endregion
    }
}