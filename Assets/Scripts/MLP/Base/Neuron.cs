using System;
using System.Collections.Generic;
using UnityEngine;

namespace JufGame.AI.ANN
{
    [Serializable] // 方便在编辑器页面查看
    public class Neuron
    {
        //神经元权重列表，末位放置偏置b
        public float[] Weights => weights;
        //加权和
        public float Sum => sum;
        //为各个权重分配的额外参数
        public Dictionary<string, float[]> WeightParams{ get; private set; }
        //为神经元本身分配的额外参数
        public Dictionary<string, float> Params{ get; private set; }
        [SerializeField]private float[] weights;
        private float sum;
        public Neuron(int weightCount)
        {
            weights = new float[weightCount + 1];//末尾放偏置
        }
        /// <summary>
        /// 初始化训练所需参数列表，仅在训练时调用
        /// </summary>
        public void InitCache()
        {
            Params = new Dictionary<string, float>
            {
                ["Error"] = 0,//该值用来记录，每次更新时的累计损失
            };
            WeightParams = new Dictionary<string, float[]>
            {
                //记录权重待变化值
                ["Delta"] = new float[weights.Length],
                //Momentum和Adam中，用于记录权重变化的「动量」
                ["m"] = new float[weights.Length],
                //AdaGrad和Adam中，用于记录权重独立学习率
                ["v"] = new float[weights.Length],           
            };
        }
        //计算Sum
        public float CalcSum(float[] input)
        {
            int i;
            sum = 0;
            for(i = 0; i < input.Length; ++i)
            {
                sum += weights[i] * input[i];//加权和
            }
            sum += weights[i];//加上权重
            return Sum;
        }
    }
}