using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 2 动作
/// 我们之前说过，动作包含一个「前提条件」，其实和HTN一样，它还包含一个「行为影响」，相当于之前图中道路指向的椭圆表示的状态。它们也都是世界状态，注意是世界状态，而不是单个状态！
/// 为什么不设置成单个？首先，「前提条件」和「行为影响」本身就可能是多个状态组合成的，用单个不合适；其次，将它们也设置成世界状态（64位的long类型），方便进行统一处理与位运算。Unity中的Layer不也是这样，对吧。
/// 只有当前世界状态与「前提条件」对应位的值相同时，才算满足前提条件，这个动作才有被选择的机会。而动作一旦执行成功，世界状态就会发送变化，对应位上的值会被赋值为「行为影响」所设置的值
/// 
/// 这个动作类的奇怪之处——它没有像OnRunning或OnUpdate之类的动作执行函数，这样一来要如何执行动作？是的，这个类主要是用来充当图的边，来连接各个状态，它会作为<string, GoapAction>字典中的值，并于一个动作名字符串绑定。我们会通过动作名，再查找另一个同样以动作名为键、但值为事件的字典，找到对应的事件，这个事件才是真正运行的动作函数。这样岂不多此一举？其实这是为了提高GOAP图的重用性。如果GOAP中的道路并不是真正的动作函数，而是用了动作名来标记。那么我们可以为多个角色设计同一种动作，但不同的表现。比如「攻击」动作，在弓箭手中就是射击函数，枪手中就是开火函数……这样一来，即便不同角色都可以使用同一张GOAP图，不用重复创建（除非有特殊需求）。这样是GOAP的一般做法，只用少数GOAP图，而不同角色可以共同使用一张GOAP图来进行互不干扰的规划。这可以省很多代码量，试想在有限状态机中，不做特殊处理你都无法让不同敌人共用「攻击」状态，就得不断写大同小异的代码。GOAP的这种将结构与逻辑分离的做法，就可以很方便地复用结构或进行定制化设计，也是其优势之一。
/// </summary>
namespace GOAP
{
    /// <summary>
    /// Goap动作，也是Goap图中的边
    /// </summary>
    public class GoapAction
    {
        public int Cost { get; private set; } //动作代价，作为AI规划的依据
        private readonly GoapWorldState precondition; //动作得以执行的前提条件
        private readonly GoapWorldState effect; //动作成功执行后带来的影响，体现在对世界状态的改变

        /// <summary>
        /// 根据给定世界状态样式创建「前提条件」和「行为影响」，
        /// 这为了让它们的位与世界状态保持一致，方便进行位运算
        /// </summary>
        /// <param name="baseState">作为基准的世界状态</param>
        /// <param name="cost">动作代价</param>
        public GoapAction(GoapWorldState baseState, int cost = 1)
        {
            Cost = cost;
            precondition = new GoapWorldState(baseState);
            effect = new GoapWorldState(baseState);
        }
        /// <summary>
        /// 判断是否满足动作执行的前提条件
        /// </summary>
        /// <param name="worldState">当前世界状态</param>
        /// <returns>是否满足前提</returns>
        public bool MetCondition(GoapWorldState worldState)
        {
            var care = ~precondition.DontCare;
            return (precondition.Values & care) == (worldState.Values & care);
        }

        //---------------------------------------------------------------
        /// <summary>
        /// 判断世界状态是否可由执行影响导致
        /// </summary>
        /// <param name="worldState">当前世界状态</param>
        /// <returns>是否能导致</returns>
        public bool MetEffect(GoapWorldState worldState)
        {
            var care = ~effect.DontCare;
            return (effect.Values & care) == (worldState.Values & care);
        }

        public GoapWorldState GetPrecondition()
        {
            return precondition;
        }
        //----------------------------------------------------------------

        /// <summary>
        /// 动作实际执行成功的影响
        /// </summary>
        /// <param name="worldState">实际世界状态</param>
        public void Effect_OnRun(GoapWorldState worldState)
        {
            worldState.SetValues((worldState.Values & effect.DontCare) | (effect.Values & ~effect.DontCare));
        }
        /// <summary>
        /// 设置动作前提条件，利用元组，方便一次性设置多个
        /// </summary>
        public GoapAction SetPrecontidion(params (string, bool)[] atomName)
        {
            foreach (var atom in atomName)
            {
                precondition.SetAtomValue(atom.Item1, atom.Item2);
            }
            return this;
        }
        /// <summary>
        /// 设置动作影响
        /// </summary>
        public GoapAction SetEffect(params (string, bool)[] atomName)
        {
            foreach (var atom in atomName)
            {
                effect.SetAtomValue(atom.Item1, atom.Item2);
            }
            return this;
        }
        public void Clear()
        {
            precondition.Clear();
            effect.Clear();
        }
    }
}