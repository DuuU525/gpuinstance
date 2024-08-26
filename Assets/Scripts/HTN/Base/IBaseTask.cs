using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 任务类接口
/// 「复合任务」、「方法」和「原子任务」它们有共通之处，我们把这些共通之处以接口的形式提炼出来，可以简化我们在规划环节的代码逻辑。
/// </summary>
namespace HTN
{
    //用于描述运行结果的枚举（如果有看上一篇行为树的话，也可以直接用行为树的EStatus）
    public enum EStatus
    {
        Failure, Success, Running,
    }
    public interface IBaseTask
    {
        //判断是否满足条件
        bool MetCondition(Dictionary<string, object> worldState);
        //添加子任务
        void AddNextTask(IBaseTask nextTask);
    }
}