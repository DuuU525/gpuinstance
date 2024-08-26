using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 5. A星寻路
/// 一切条件都准备好了，现在实现下用来「寻路」的类。首先，我们会进行反向搜索，意思是说，我们不会「起始状态-->目标状态」，而是「目标状态-->起始状态」，如果成功找到，就将得到的动作序列逆向执行。
/// 为什么这么麻烦？其实恰恰相反，这还是一种简化。如果真的「起始状态-->目标状态」，未必最终会找到目标状态（因为有可能能抵达的动作暂时条件不满足）；但反向搜索，必定会包含目标状态，也一定会找到一条路（因为总会抵达一个当前已经符合的世界状态，否则就是设计的有问题了），只不过可能不是最短的。
/// 我们也能接受这种结果，虽说非最优解，但这种不确定因素，也变相让AI增加了点随机性，更接近真实决策情况。
/// 它的整体搜索过程和A星寻路是一样的
/// </summary>
namespace GOAP
{
    /// <summary>
    /// Goap A星启发式搜索
    /// </summary>
    public static class GoapAstar
    {
        private static readonly MyHeap<GoapAstarNode> openList;
        private static readonly HashSet<GoapAstarNode> closeList;
        static GoapAstar()
        {
            openList = new MyHeap<GoapAstarNode>(GoapWorldState.MAXATOMS);
            closeList = new HashSet<GoapAstarNode>();
        }
        /// <summary>
        /// 根据给定初始世界状态和目标世界状态，从动作集中规划出可达成目标的动作
        /// </summary>
        /// <param name="from">初始世界状态</param>
        /// <param name="to">目标世界状态</param>
        /// <param name="actionSet">动作集</param>
        /// <returns>需执行的动作名称，弹出顺序即为执行顺序</returns>
        public static Queue<string> Plan(GoapWorldState from, GoapWorldState to, GoapActionSet actionSet)
        {
            openList.Clear();
            closeList.Clear();
            // 实际要的是from --> to，但在代码中寻找时是to --> from
            var n0 = new GoapAstarNode(to, null, 0, from, default); //创建一个目标状态节点
            openList.PushHeap(n0);
            while (!openList.IsEmpty)
            {
                var curState = openList.Top;
                var curCare = ~curState.WorldState.DontCare;
                closeList.Add(curState);
                openList.PopHeap();
                if ((curState.WorldState.Values & curCare) == (from.Values & curCare) || openList.IsFull)
                {
                    return GenerateFinalPlan(curState);
                }
                var neighbors = actionSet.GetPossibleTrans(curState, from, to, out List<string> actions);
                for (int i = 0; i < neighbors.Count; ++i)
                {
                    if (closeList.Contains(neighbors[i]))
                        continue;
                    var cost = curState.G + actionSet[actions[i]].Cost;
                    var isWithoutOpen = !openList.Contains(neighbors[i]);
                    if (isWithoutOpen || cost < neighbors[i].G)
                    {
                        neighbors[i].SetGCost(cost);
                        if (isWithoutOpen)
                        {
                            openList.PushHeap(neighbors[i]);
                        }
                    }
                }
            }
            return new Queue<string>();
        }
        /// <summary>
        /// 根据最终节点回溯，获取最终执行动作集
        /// </summary>
        /// <param name="endNode"></param>
        /// <returns>动作队列，弹出顺序即为执行顺序</returns>
        private static Queue<string> GenerateFinalPlan(GoapAstarNode endNode)
        {
            var planQueue = new Queue<string>();
            if (endNode.Parent == null)
            {
                return planQueue;
            }
            planQueue.Enqueue(endNode.FromActionName);
            var tpNode = endNode.Parent;
            while (tpNode.Parent != null)
            {
                planQueue.Enqueue(tpNode.FromActionName);
                tpNode = tpNode.Parent;
            }
            return planQueue;
        }
    }
}