using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 原子任务
/// 原子任务是一个抽象类，相当于行为树中的动作节点，用于开发者自定义的最小单元任务。
/// 一般就是像「开火」、「奔跑」之类的简单动作。
/// 值得注意的是，这里的条件判断和执行影响都要分两种情况，
/// 一种是规划时，
/// 一种是实际执行时，因为规划时我们使用的并不是真正的世界状态，而是一份模拟的世界状态副本。
/// </summary>
namespace HTN
{
    public abstract class PrimitiveTask : IBaseTask
    {
        //原子任务不可以再分解为子任务，所以AddNextTask方法不必实现
        void IBaseTask.AddNextTask(IBaseTask nextTask)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 执行前判断条件是否满足，传入null时直接修改HTNWorld
        /// </summary>
        /// <param name="worldState">用于plan的世界状态副本</param>
        public bool MetCondition(Dictionary<string, object> worldState = null)
        {
            if (worldState == null)//实际运行时
            {
                return MetCondition_OnRun();
            }
            else//模拟规划时，若能满足条件就直接进行Effect
            {
                if (MetCondition_OnPlan(worldState))
                {
                    Effect_OnPlan(worldState);
                    return true;
                }
                return false;
            }
        }
        //判断规划时条件
        protected virtual bool MetCondition_OnPlan(Dictionary<string, object> worldState)
        {
            return true;
        }
        //判断运行时条件
        protected virtual bool MetCondition_OnRun()
        {
            return true;
        }

        //任务的具体运行逻辑，交给具体类实现
        public abstract EStatus Operator();

        /// <summary>
        /// 执行成功后的影响，传入null时直接修改HTNWorld
        /// </summary>
        /// <param name="worldState">用于plan的世界状态副本</param>
        public void Effect(Dictionary<string, object> worldState = null)
        {
            Effect_OnRun();
        }
        protected virtual void Effect_OnPlan(Dictionary<string, object> worldState)
        {
            ;
        }
        protected virtual void Effect_OnRun()
        {
            ;
        }
    }
}