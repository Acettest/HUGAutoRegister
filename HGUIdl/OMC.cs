#region 描述
//-----------------------------------------------------------------------------
// 文 件 名: OMC
// 作    者：d.w
// 创建时间：2014/6/12 16:10:54
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

namespace HGU.Idl
{
    public class OMC
    {
        #region IFields
        private string m_city;                                         //地市
        private string m_manufacturer;                                 //厂商
        private string m_omcName;                                      //网管 
        #endregion

        #region IProperties
        public string City
        {
            get { return m_city; }
        }

        public string Manufacturer
        {
            get { return m_manufacturer; }
        }
        public string OmcName
        {
            get { return m_omcName; }
        }
        #endregion

        #region IConstructors
        public OMC(string city, string manufacturer, string omcName)
        {
            m_city = city;
            m_manufacturer = manufacturer;
            m_omcName = omcName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            if (this.GetType() != obj.GetType())
                return false;
            return CompareMembers(this, obj as OMC);
        }

        public static bool CompareMembers(OMC left, OMC right)
        {
            if (left == null || right == null)
                return false;
            if (left.m_city != right.m_city ||
                left.m_manufacturer != right.m_manufacturer ||
                left.m_omcName != right.m_omcName)
                return false;
            return true;
        }
        #endregion
    }

    public class OMCAddInfo
    {
        #region IFields
        private OMC m_omc;
        private string m_omcIP;                            //tl1 ip
        private int m_omcPort;                             //tl1 端口
        private string m_user;
        private string m_pwd;
        #endregion

        #region IProperties
        public OMC Omc
        {
            get { return m_omc; }
        }

        public string OmcIP
        {
            get { return m_omcIP; }
        }

        public int OmcPort
        {
            get { return m_omcPort; }
        }

        public string User
        {
            get { return m_user; }
        }

        public string Pwd
        {
            get { return m_pwd; }
        }
        #endregion

        #region IConstructor
        public OMCAddInfo(OMC omc, string omcIP, int omcPort, string user, string pwd)
        {
            m_omc = omc;
            m_omcIP = omcIP;
            m_omcPort = omcPort;
            m_user = user;
            m_pwd = pwd;
        }
        #endregion
    }
}
