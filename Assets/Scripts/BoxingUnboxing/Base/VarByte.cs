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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public class VarByte : Vartable<byte> {
		/// <summary>
		/// 分配一个对象
		/// </summary>
		/// <returns></returns>
		public static VarByte Alloc()
		{
			VarByte var = YouYou.Pool.DequeueVarObject<VarByte>();
			var.Value = 0;
			var.Retain();
			return var;
		}
		/// <summary>
		/// 分配一个对象
		/// </summary>
		/// <param name="value">初始值</param>
		/// <returns></returns>
		public static VarByte Alloc(byte value)
		{
			VarByte var = Alloc();
			var.Value = value;
			return var;
		}

		/// <summary>
		/// VarByte -> byte
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator byte(VarByte value)
		{
			return value.Value;
		}
	}
}

