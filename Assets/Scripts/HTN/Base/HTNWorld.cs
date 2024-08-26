using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// 世界状态
/// 世界状态实现的难点在于：
/// 状态数据的类型是多种多样的，该用什么来统一保存？
/// 状态数据会时时变化，如何保证存储的数据也会同步更新？
/// 对于问题1，我们可以用 <string, object> 的字典来解决。毕竟C#中，object类是所有数据类型的老祖宗。那问题2呢，假设用这种字典存储了某个角色的血量，那这个角色就算血量变成0了，字典里存储的也只是刚存进去时的那个值而不是0。而且反过来，我们修改字典里的这个血量值，也不会影响实际角色的血量……除非，这些值能像属性一样……
/// 这是可以做到的！但要用到两个字典，一个用来模仿属性的get，一个用来模仿属性的set。分别用值类型为System.Action< object > 和 System.Func< object >的字典就可以了。
/// </summary>
namespace HTN
{
    //世界状态只有一个即可，我们将其设为静态类
    public static class HTNWorld
    {
        //读 世界状态的字典
        private static readonly Dictionary<string, Func<object>> get_WorldState;
        //写 世界状态的字典
        private static readonly Dictionary<string, Action<object>> set_WorldState;

        static HTNWorld()
        {
            get_WorldState = new Dictionary<string, Func<object>>();
            set_WorldState = new Dictionary<string, Action<object>>();
        }
        //添加一个状态，需要传入状态名、读取函数和写入函数
        public static void AddState(string key, Func<object> getter, Action<object> setter)
        {
            get_WorldState[key] = getter;
            set_WorldState[key] = setter;
        }
        //根据状态名移除某个世界状态
        public static void RemoveState(string key)
        {
            get_WorldState.Remove(key);
            set_WorldState.Remove(key);
        }
        //修改某个状态的值
        public static void UpdateState(string key, object value)
        {
            //就是通过写入字典修改的
            set_WorldState[key].Invoke(value);
        }
        //读取某个状态的值，利用泛型，可以将获取的object转为指定的类型
        public static T GetWorldState<T>(string key)
        {
            return (T)get_WorldState[key].Invoke();
        }
        //复制一份当前世界状态的值（这个主要是用在规划中）
        public static Dictionary<string, object> CopyWorldState()
        {
            var copy = new Dictionary<string, object>();
            foreach (var state in get_WorldState)
            {
                copy.Add(state.Key, state.Value.Invoke());
            }
            return copy;
        }
    }
}