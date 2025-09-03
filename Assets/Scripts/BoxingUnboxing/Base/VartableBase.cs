#region 模块信息
//===================================================
// Copyright (C) 2020	
//
// 文件名(File Name):				VartableBase.cs
// 作者(Author):					稀饭 
// 邮箱(e-mail):					1144000915@qq.com
// 创建时间(CreateTime):			2021-01-11 15:20:13 
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
	/// 变量基类
	/// </summary>
	public abstract class VartableBase  {

		/// <summary>
		/// 获取变量类型
		/// </summary>
		public abstract Type Type
        {
			get;
        }

		/// <summary>
		/// 引用计数
		/// </summary>
		public byte ReferenceCount
        {
			get;
			private set;
        }

		/// <summary>
		/// 变量引用计数增加
		/// </summary>
		public void Retain()
        {
			ReferenceCount++;
		}

		/// <summary>
		/// 变量引用计数减少 变量回池
		/// </summary>
		public void Release()
        {
			ReferenceCount--;
            if (ReferenceCount<1)
            {
				//回池操作
				YouYou.Pool.EnqueueVarObject(this);
			}
		}
	}
}

