using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 4 动作集
/// 照理说，动作集不过是动作的合集，单独将它也制成一个类，是为了方便「动作序列」规划，主要体现在GetPossibleTrans函数，根据传入的节点的世界状态，在合集中遍历出「前提条件」满足的动作
/// </summary>
namespace GOAP
{
    public class GoapActionSet
    {
        private readonly Dictionary<string, GoapAction> actionSet;
        public GoapActionSet()
        {
            actionSet = new Dictionary<string, GoapAction>();
        }
        public GoapAction this[string idx]
        {
            get => actionSet[idx];
        }
        public GoapActionSet AddAction(string actionName, GoapAction newAction)
        {
            actionSet.Add(actionName, newAction);
            return this;
        }
        /// <summary>
        /// 根据当前节点搜索可进一步执行的动作
        /// </summary>
        /// <param name="curNode">当前图节点</param>
        /// <param name="from">起始状态，用于启发函数计算</param>
        /// <param name="to">目标状态，同样用于启发函数计算</param>
        /// <param name="actionNames">用于存储找到的可行动作的名字，有名字方便找到动作函数</param>
        /// <returns>找到的所有可行动作</returns>
        public List<GoapAstarNode> GetPossibleTrans(GoapAstarNode curNode, GoapWorldState from, GoapWorldState to, out List<string> actionNames)
        {
            var curState = curNode.WorldState;
            var neighbors = new List<GoapAstarNode>();
            actionNames = new List<string>();
            foreach (var act in actionSet)
            {
                if (act.Value.MetEffect(curState)) //如果动作的影响能造就当前世界状态，就选中
                {
                    actionNames.Add(act.Key);
                    var nextState = act.Value.GetPrecondition(); //获得动作的条件，以便倒推
                    neighbors.Add(new GoapAstarNode(nextState, curNode, from.CalcCorrelation(nextState), to, act.Key));
                }
            }
            return neighbors;
        }
    }
}