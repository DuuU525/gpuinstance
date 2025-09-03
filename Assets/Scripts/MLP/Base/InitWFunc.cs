using System;

namespace JufGame.AI.ANN
{
    public static class InitWFunc
    { 
        public enum Type
		{
			Random, Xavier, He, None
		}
		public static void InitWeights(Type initWFunc, Neuron neuron)
		{
			switch(initWFunc)
			{
				case Type.Xavier:
					XavierInitWeights(neuron.Weights);
					break;
				case Type.He:
					HeInitWeights(neuron.Weights);
					break;
				case Type.Random:
					RandomInitWeights(neuron.Weights);
					break;
				default:
					break;
			}
		}
        private static void RandomInitWeights(float[] weightsList)
        {
            var rand = new Random();
            for (int i = 0; i < weightsList.Length; ++i)
            {
                //使用较小的标准差，适合普通的随机初始化
                weightsList[i] = (float)(rand.NextGaussian() * 0.01); 
            }
        }
        private static void XavierInitWeights(float[] weightsList)
        {
            var rand = new Random();
            var scale = 1f / MathF.Sqrt(weightsList.Length);
            for (int i = 0; i < weightsList.Length; ++i)
            {
                weightsList[i] = (float)(rand.NextDouble() * 2 * scale - scale);
            }
        }
        private static void HeInitWeights(float[] weightsList)
        {
            var rand = new Random();
            var stdDev = MathF.Sqrt(2f / weightsList.Length); //计算标准差
            for (int i = 0; i < weightsList.Length; ++i)
            {   
                //生成服从正态分布的随机数，并乘以标准差
                weightsList[i] = (float)(rand.NextGaussian() * stdDev); 
            }
        }
        // 用于生成服从标准正态分布的随机数的辅助方法
        private static double NextGaussian(this Random rand)
        {
            double u1 = 1.0 - rand.NextDouble(); // 生成 [0, 1) 之间的随机数
            double u2 = 1.0 - rand.NextDouble();
            // 使用 Box-Muller 变换生成正态分布的随机数
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); 
        }
    }
}