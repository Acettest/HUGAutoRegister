#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: YWT_HuaWei_Adapter
// 作    者：d.w
// 创建时间：2014/7/1 15:31:57
// 描    述：
// 版    本：1.0.0.0
//-----------------------------------------------------------------------------
// 历史更新纪录
//-----------------------------------------------------------------------------
// 版    本：           修改时间：           修改人：           
// 修改内容：
//-----------------------------------------------------------------------------
// Copyright (C) 2009-2014 www.chaselwang.com . All Rights Reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HGU.Tl1Wrapper;
using HGU.CommandsBuilder;
using HGUAdapterBase;

namespace RemotableYWT_HuaWei_Adapter
{
    class HGU_HuaWei_Adapter : HGUAsynAdapter
    {
        protected override void InitCommandBuilder()
        {
            m_commBuilder = new HuaWeiCommandsBuilder();
        }
    }
}
