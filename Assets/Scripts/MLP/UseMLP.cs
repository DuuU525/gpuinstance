using System.Collections;
using System.Collections.Generic;
using JufGame.AI.ANN;
using UnityEngine;

public class UseMLP : MonoBehaviour
{
    public NeuralNet net;
    private float[][] inSet = //异或运算的输入
    {
        new float[]{1, 0},
        new float[]{1, 1},
        new float[]{0, 0},
        new float[]{0, 1},
    };
    private void Awake()
    {
        Training.DebugNetRes(net, inSet);
    }
}