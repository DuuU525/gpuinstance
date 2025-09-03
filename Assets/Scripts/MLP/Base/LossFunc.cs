using System;

namespace JufGame.AI.ANN
{
    public static class LossFunc
    {
        private const float MinDelta = 1e-7f;
        public enum Type
        {
            MeanSqurad, CrossEntropy,
        }
        public static float Calc(Type type, float[] targetOut, Layer outLayer)
        {
            return type switch
            {
                Type.MeanSqurad => MeanSquradErr_Calc(targetOut, outLayer),
                _ => CrossEntropy_Calc(targetOut, outLayer),
            };
        }
        public static void Diff(Type type, float[] targetOut, Layer outLayer)
        {
            switch(type)
            {
                case Type.MeanSqurad:
                    MeanSquradErr_Diff(targetOut, outLayer);
                    break;
                case Type.CrossEntropy:
                    CrossEntropy_Diff(targetOut, outLayer);
                    break;
            };
        }

        private static float MeanSquradErr_Calc(float[] targetOut, Layer outLayer)
        {
            var errSum = 0.0f;
            for(int i = 0; i < targetOut.Length; ++i)
            {
                errSum += MathF.Pow(outLayer.Output[i] - targetOut[i], 2);
            }
            return errSum / (2 * targetOut.Length);
        }
        private static void MeanSquradErr_Diff(float[] targetOut, Layer outLayer)
        {
            for(int i = 0; i < targetOut.Length; ++i)
            {
                var curNeuron = outLayer.Neurons[i];
                curNeuron.Params["Error"] = outLayer.Output[i] - targetOut[i];
            }
        }
                
        private static float CrossEntropy_Calc(float[] targetOut, Layer outLayer)
        {
            var errSum = 0.0f;
            for(int i = 0; i < targetOut.Length; ++i)
            {
                //加上一个极小值再取log，放置出现log(0)报错
                errSum -= targetOut[i] * MathF.Log(outLayer.Output[i] + MinDelta);
            }
            return errSum;
        }
        private static void CrossEntropy_Diff(float[] targetOut, Layer outLayer)
        {
            for(int i = 0; i < targetOut.Length; ++i)
            {
                var curNeuron = outLayer.Neurons[i];
                //用Output[i]的前提：神经网络的输出经过了softmax处理
                curNeuron.Params["Error"] = outLayer.Output[i] - targetOut[i];
            }
        }
    }
}