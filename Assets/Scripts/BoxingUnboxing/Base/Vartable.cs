#region 模块信息
//===================================================
// Copyright (C) 2020	
//
// 文件名(File Name):				Vartable.cs
// 作者(Author):					稀饭 
// 邮箱(e-mail):					1144000915@qq.com
// 创建时间(CreateTime):			2021-01-11 15:20:05 
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
    /// <summary>
    /// 变量泛型基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Vartable<T> : VartableBase
    {

        /// <summary>
        /// 当前存储的真实值
        /// </summary>
        public T Value;

        /// <summary>
        /// 变量类型
        /// </summary>
        public override Type Type
        {
            get
            {
                return typeof(T);
            }
        }


    }
}

