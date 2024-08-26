using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 构造器
/// 构造器会自带规划器和执行器，并将任务的创建打包成函数。行为树一样，用栈的方式描述构建过程，提供一定可视化。
/// </summary>
namespace HTN
{
    public partial class HTNPlanBuilder
    {
        //规划器
        private HTNPlanner planner;
        //执行器
        private HTNPlanRunner runner;
        private readonly Stack<IBaseTask> taskStack;

        public HTNPlanBuilder()
        {
            taskStack = new Stack<IBaseTask>();
        }

        private void AddTask(IBaseTask task)
        {
            if (planner != null)//当前计划器不为空
            {
                //将新任务作为构造栈顶元素的子任务
                taskStack.Peek().AddNextTask(task);
            }
            else //如果计划器为空，意味着新任务是根任务，进行初始化
            {
                planner = new HTNPlanner(task as CompoundTask);
                runner = new HTNPlanRunner(planner);
            }
            //如果新任务是原子任务，就不需要进栈了，因为原子任务不会有子任务
            if (task is not PrimitiveTask)
            {
                taskStack.Push(task);
            }
        }
        //剩下的代码都很简单，我相信能直接看得懂
        public void RunPlan()
        {
            runner.RunPlan();
        }
        //要向复合任务添加一个方法，先调用Back后再添加（AddTask）
        //用Back调整栈顶的元素，我们可以自由地控制新任务作为谁的子任务
        public HTNPlanBuilder Back()
        {
            taskStack.Pop();
            return this;
        }
        public HTNPlanner End()
        {
            taskStack.Clear();
            return planner;
        }
        public HTNPlanBuilder CompoundTask()
        {
            var task = new CompoundTask();
            AddTask(task);
            return this;
        }
        public HTNPlanBuilder Method(System.Func<bool> condition)
        {
            var task = new Method(condition);
            AddTask(task);
            return this;
        }
    }
}