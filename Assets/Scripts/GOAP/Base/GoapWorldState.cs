using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 1 世界状态
/// 将世界状态分为「私有」和「共享」，我们就可以让角色更新「私有」部分，而全局系统更新「共享」部分。当需要角色规划时，我们就用位运算将该角色的「私有」与世界的「共享」进行整合，得到对于这个角色而言的当前世界状态。这样对于不同角色，它们就能得到对各自的而言的世界状态啦！如果去除注释，
/// 这个类的内容其实并不多，在使用时几乎只要用到SetAtomValue函数
/// worldState = new GoapWorldState();
/// worldState.SetAtomValue("血量健康", true);
/// worldState.SetAtomValue("大半夜", false, true);
/// </summary>
namespace GOAP
{
    /// <summary>
    /// 用位表示的世界状态
    /// </summary>
    public class GoapWorldState
    {
        public const int MAXATOMS = 64;//存储的状态数上限，由于用long类型存储，最多就是64（long类型为64位整数）
        public long Values => values;//世界状态值
        public long DontCare => dontCare;//标记未被使用的位
        public long Shared => shared;//判断共享状态位
        private readonly Dictionary<string, int> namesTable;//存储各个状态名字与其在values中的对应位，方便查找状态
        private int curNamsLen;//存储的已用状态的长度
        private long values;
        private long dontCare;
        private long shared;
        /// <summary>
        /// 初始化为空白世界状态
        /// </summary>
        public GoapWorldState()
        {
            //赋值0，可将二进制位全置0；赋值-1，可将二进制位全置1
            namesTable = new Dictionary<string, int>();
            values = 0L; //全置0，意为世界状态默认为false
            dontCare = -1L; //全置1，意为世界状态的位全没有被使用
            shared = -1L; //将shard的位全置1
            curNamsLen = 0;
        }
        /// <summary>
        /// 基于某世界状态的进一步创建，相当于复制状态设置但清空值
        /// </summary>
        public GoapWorldState(GoapWorldState worldState)
        {
            namesTable = new Dictionary<string, int>(worldState.namesTable);//复制状态名称与位的分配
            values = 0L;
            dontCare = -1L;
            curNamsLen = worldState.curNamsLen;//同样复制已使用的位长度
            shared = worldState.shared;//保留状态共享性的信息
        }
        /// <summary>
        /// 根据状态名，修改单个状态的值
        /// </summary>
        /// <param name="atomName">状态名</param>
        /// <param name="value">状态值</param>
        /// <param name="isShared">设置状态是否为共享</param>
        /// <returns>修改成功与否</returns>
        public bool SetAtomValue(string atomName, bool value = false, bool isShared = false)
        {
            var pos = GetIdxOfAtomName(atomName);//获取状态对应的位
            if (pos == -1) return false;//如果不存在该状态，就返回false
                                        //将该位 置为指定value
            var mask = 1L << pos;
            values = value ? (values | mask) : (values & ~mask);
            dontCare &= ~mask;//标记该位已被使用
            if (!isShared)//如果该状态不共享，则修改共享位信息
            {
                shared &= ~mask;
            }
            return true;//设置成功，返回true
        }
        /// <summary>
        /// 计算该世界状态与指定世界状态的相关度
        /// </summary>
        public int CalcCorrelation(GoapWorldState to)
        {
            var care = to.dontCare ^ -1L;
            var diff = (values & care) ^ (to.values & care);
            int dist = 0; //统计有多少位是相同的，以表示相关度
            for (int i = 0; i < MAXATOMS; ++i)
            {
                /*因为规划时找的是最小代价的动作，所以相关度越高理应代价越小
                这样才能被优先选取，故用--，而非++*/
                if ((diff & (1L << i)) != 0)
                    --dist;
            }
            return dist;
        }
        public void SetValues(long newValues)
        {
            values = newValues;
        }
        public void SetDontCare(long newDontCare)
        {
            dontCare = newDontCare;
        }
        public void Clear()
        {
            values = 0L;
            namesTable.Clear();
            curNamsLen = 0;
            dontCare = -1L;
        }
        /// <summary>
        /// 通过状态名获取单个状态在Values中的位，如果没包含会尝试添加
        /// </summary>
        /// <param name="atomName">状态名</param>
        /// <returns>状态所在位</returns> 	
        private int GetIdxOfAtomName(string atomName)
        {
            if (namesTable.TryGetValue(atomName, out int idx))
            {
                return idx;
            }
            if (curNamsLen < MAXATOMS)
            {
                namesTable.Add(atomName, curNamsLen);
                return curNamsLen++;
            }
            return -1;
        }
    }
}