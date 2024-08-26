using UnityEngine;

namespace HTN
{
    public class TestHTN : MonoBehaviour
    {
        //节选自某个小游戏里的一个小怪的行动
        // protected override void Start()
        // {
        //     base.Start();
        //     trigger = Para.HeathValue * 0.5f;
        //     hTN.CompoundTask()
        //             .Method(() => isHurt)
        //                 .Enemy_Hurt(this)
        //                 .Enemy_Die(this)
        //                 .Back()
        //             .Method(() => curHp <= trigger)
        //                 .Enemy_Combo(this, 3)
        //                 .Enemy_Rest(this, "victory")
        //                 .Back()
        //             .Method(() => HTNWorld.GetWorldState<float>("PlayerHp") > 0)
        //                 .Enemy_Check(this)
        //                 .Enemy_Track(this, PlayerTrans)
        //                 .Enemy_Atk(this)
        //                 .Back()
        //             .Method(() => true)
        //                 .Enemy_Idle(this, 3f)
        //             .End();
        // }
        /// <summary>
        /// Enemy_Check、Enemy_Atk都是实际开发实现的具体原子行为
        /// HTN擅长规划，其实并不擅长时时决策，所以在实际开发时，建议与有限状态机结合。
        /// 将受伤、死亡这类需要时时反馈的事交给状态机，HTN本身也可以放进一个状态，来进行复杂行为。
        /// 而不是将受伤、死亡也当成原子任务，因为这样做就要你为各个行为设计受伤中断，代码就会比较繁冗
        /// 
        /// “状态机+其它”的复合决策模型并不罕见，GOAP也经常以这种形式出现。
        /// 
        ///  最后分享一些设计原子任务的心得：
        /// 1 如果一个原子任务有一定的运行过程，可以用一个bool值在Operator函数内部判断是否完成了动作。
        /// 2 因为我们的世界状态是用字符串来读取的，如果我们想获取某个士兵的血量该怎么办？有很多士兵在，该如何区分？可以用Unity的GetInstanceID()获取唯一的ID+“血量”，组合成字符串来区分，其它类似情况同理。例如：
        /// HTNWorld.AddState(GetInstanceID() + "currentHp", () => currentHp, (v) => currentHp = (float)v);
        /// HTNWorld.AddState(GetInstanceID() + "IsHurt", () => isHurt, (v) => { isHurt = (bool)v; });
        /// HTNWorld.AddState(GetInstanceID() + "IsDie", () => curHp <= 0, (v) => { });
        /// </summary>
        
    }
}