using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 6.代理器
/// 创建一个「代理器」，它用来整合了上述内容，并统筹运行
/// </summary>
namespace GOAP
{

    /// <summary>
    /// 运行结果状态枚举（和往期决策方法使用的一样）
    /// </summary>
    public enum EStatus
    {
        Failure, Success, Running, Aborted, Invalid
    }
    public class GoapAgent
    {
        private readonly GoapActionSet actionSet; //动作集
        private readonly GoapWorldState curSelfState; //当前自身状态，主要是存储私有状态
        private readonly Dictionary<string, Func<EStatus>> actionFuncs; //各动作名字对应的动作函数
        private Queue<string> actionPlan;//存储规划出的动作序列

        private EStatus curState;//存储当前动作的执行结果
        private bool canContinue;//是否能够继续执行，记录动作序列全部是否执行完了
        private GoapAction curAction;//记录当前执行的动作
        private Func<EStatus> curActionFunc;//记录当前运行的动作函数

        /// <summary>
        /// 初始化代理器
        /// </summary>
        /// <param name="baseWorldState">世界状态，用来复制成自身状态</param>
        /// <param name="actionSet">动作集</param>
        public GoapAgent(GoapWorldState baseWorldState, GoapActionSet actionSet)
        {
            curSelfState = new GoapWorldState(baseWorldState);
            curSelfState.SetValues(baseWorldState.Values);
            curSelfState.SetDontCare(baseWorldState.DontCare);
            actionFuncs = new Dictionary<string, Func<EStatus>>();
            this.actionSet = actionSet;
        }
        /// <summary>
        /// 修改自身状态值
        /// </summary>
        public bool SetAtomValue(string stateName, bool value)
        {
            return curSelfState.SetAtomValue(stateName, value);
        }
        /// <summary>
        /// 为动作名设置对应的动作函数
        /// </summary>
        public void SetActionFunc(string actionName, Func<EStatus> func)
        {
            actionFuncs.Add(actionName, func);
        }
        /// <summary>
        /// 规划GOAP并运行
        /// </summary>
        /// <param name="curWorldState"></param>
        /// <param name="goal"></param>
        public void RunPlan(GoapWorldState curWorldState, GoapWorldState goal)
        {
            UpdateSelfState(curWorldState);//将自身的私有状态与世界的共享状态融合，得到真正的「当前世界状态」
            if (curState == EStatus.Failure) //当前状态为「失败」，就表示动作执行失败
            {
                //那就重新规划，找出新的动作序列
                actionPlan = GoapAstar.Plan(curSelfState, goal, actionSet);
            }
            if (curState == EStatus.Success)//执行结果为「成功」，表示动作顺利执行完
            {
                curAction.Effect_OnRun(curWorldState); //动作就会对全局世界状态造成影响
                /*这同样要更新自身状态，以防这次改变的是「私有」状态，全局世界状态可是只维护「共享」部分。
                所以需要自身状态也记录下这次影响，即便是共享状态也没关系，反正下次会与世界的共享状态融合*/
                curSelfState.SetValues(curWorldState.Values);
            }
            //如果执行结果不是「运行中」，就表示上个动作要么成功了，要么失败了。都该取出动作序列中新的动作来执行
            if (curState != EStatus.Running)
            {
                canContinue = actionPlan.TryDequeue(out string curActionName);
                if (canContinue)//如果成功取出动作，就根据动作名，选出对应函数和动作
                {
                    curActionFunc = actionFuncs[curActionName];
                    curAction = actionSet[curActionName];
                }
            }
            curState = canContinue && curAction.MetCondition(curSelfState) ? curActionFunc() : EStatus.Failure;
        }
        /// <summary>
        /// 更新自身状态的共享部分与当前世界状态同步
        /// </summary>
        private void UpdateSelfState(GoapWorldState curWorldState)
        {
            curSelfState.SetValues(curWorldState.Values & curWorldState.Shared | curSelfState.Values & ~curWorldState.Shared);
        }
    }
}