#region 模块信息
//===================================================
// Copyright (C) 2020	
//
// 文件名(File Name):				VarByte.cs
// 作者(Author):					稀饭 
// 邮箱(e-mail):					1144000915@qq.com
// 创建时间(CreateTime):			2021-01-12 16:32:47 
// 修改者列表(modifier):		
// 模块描述(Module description):	创建脚本自动修改文件名、作者、创建时间
//===================================================
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class Pool
    {
        private static Dictionary<string, VartableBase> m_ParamDic = new();

        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <typeparam name="TData">泛型类型</typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetData<TData>(string key,TData value)
        {
            VartableBase itemBase = null;
            if (m_ParamDic.TryGetValue(key,out itemBase))
            {
                Vartable<TData> item = itemBase as Vartable<TData>;
                item.Value = value;
                m_ParamDic[key] = item;
            }
            else
            {
                //参数原来不存在
                Vartable<TData> item = new Vartable<TData>();
                item.Value = value;
                m_ParamDic[key] = item;
            }
        }
         /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="TData">泛型类型</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public TData GetData<TData>(string key)
        {
            VartableBase itemBase = null;
            if (m_ParamDic.TryGetValue(key, out itemBase))
            {
                Vartable<TData> item = itemBase as Vartable<TData>;
                return item.Value;
            }
            return default(TData);
        }

        internal static void EnqueueVarObject(VartableBase vartableBase)
        {
            
        }

        internal static T DequeueVarObject<T>()
        {
            throw new NotImplementedException();
        }
    }
}