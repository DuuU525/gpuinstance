using System.Collections;
using System.Collections.Generic;
namespace GOAP
{
    public class GoapAstarNode : IMyHeapItem<GoapAstarNode>
    {
        public int G => g;
        public GoapWorldState WorldState => worldState;
        public GoapAstarNode Parent => parent;//记录上一个节点，寻路完成后溯回出动作序列
        public string FromActionName => fromActionName;//记录上一个动作的名字
        public int HeapIndex { get; set; }
        private readonly GoapWorldState worldState;
        private readonly GoapAstarNode parent;
        private readonly int h;//与目标状态的相关度
        private int f;//启发值f
        private int g;//起始状态至此的累计动作代价
        private readonly string fromActionName;

        public GoapAstarNode(GoapWorldState curState, GoapAstarNode parent, int g, GoapWorldState goal, string fromActionName)
        {
            worldState = curState;
            this.parent = parent;
            this.g = g;
            this.fromActionName = fromActionName;
            h = curState.CalcCorrelation(goal);
            f = g + h;
        }
        public void SetGCost(int g)//设置g值
        {
            this.g = g;
            f = g + h;
        }
        public int CompareTo(GoapAstarNode other)
        {
            return f.CompareTo(other.f);//启发值比较
        }
        /* HashSet会比较GetHashCode以及从Equals返回的bool值，判断元素是否相等
        MyHeap也会根据Equals判断是否相等
        需要重写这两个方法*/
        public override int GetHashCode()
        {
            return (worldState.Values & ~worldState.DontCare).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }
    }
}