using System;
using System.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Xml;
using System.IO;
using System.Collections;
using System.Data;

namespace MD5Encrypt
{
    /// <summary>
    /// MD5加密、解密DES算法
    /// </summary>
    public class DES : MD5Encrypt.IMD5Encrypt
    {
        #region 缺省密钥

        byte[] key = new byte[] { 0x63, 0x45, 0x87, 0x78, 0x68, 0x12, 0x34, 0x23 };
        byte[] iv = new byte[] { 0x63, 0x45, 0x87, 0x78, 0x68, 0x12, 0x34, 0x23 };

        #endregion

        #region 单例模式

        /// <summary>
        /// 事件:记录错误日志
        /// </summary>
        public event delegateMD5SengLog sendLog;

        /// <summary>
        /// MD5加密
        /// </summary>
        public DES()
        {
            this.sendLog += new delegateMD5SengLog(DES_sendLog);
        }

        void DES_sendLog(string strlog)
        {

        }

        static DES des = new DES();

        /// <summary>
        /// DES单例模式
        /// </summary>
        /// <returns>DES实例</returns>
        public static DES instance()
        {
            if (des == null)
            {
                des = new DES();
            }
            return des;
        }

        #endregion

        #region 使用缺省密钥加密、解密字符串,并返回字符串
        /// <summary>
        /// 使用缺省密钥加密字符串
        /// </summary>
        /// <param name="sInputString">需要加密的字符串</param>
        /// <returns>加密后字符串</returns>
        public string EncryptString(string sInputString)
        {
            byte[] data = Encoding.UTF8.GetBytes(sInputString);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = key;
            DES.IV = iv;
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
            return BitConverter.ToString(result);
        }

        /// <summary>
        /// 使用缺省密钥解密字符串
        /// </summary>
        /// <param name="sInputString">需要解密字符串</param>
        /// <returns>解密后字符串</returns>
        public string DecryptString(string sInputString)
        {
            string[] sInput = sInputString.Split("-".ToCharArray());
            byte[] data = new byte[sInput.Length];
            for (int i = 0; i < sInput.Length; i++)
            {
                data[i] = byte.Parse(sInput[i], NumberStyles.HexNumber);
            }
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = key;
            DES.IV = iv;
            ICryptoTransform desencrypt = DES.CreateDecryptor();
            byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
            return Encoding.UTF8.GetString(result);
        }

        #endregion

        #region 使用缺省密钥加密、解密XML文件文本,并返回XML文件文本

        /// <summary>
        /// 使用缺省密钥加密XML文件
        /// </summary>
        /// <param name="xmlpath">需要加密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要加密的位数:1表示8位加密,2表示16位加密,3表示24位加密...</param>
        /// <returns>加密后XML文件文本字符串</returns>
        public string EncryptXML(string xmlpath, int count)
        {
            if (count <= 0)
                return null;
            XmlDocument doc = new XmlDocument();
            MemoryStream ms = new MemoryStream();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            try
            {
                doc.Load(xmlpath);

                doc = EncryptXmlDoc2XmlDoc(doc, count);

                doc.Save(ms);

                char[] OutChar = Encoding.UTF8.GetChars(ms.ToArray());
                foreach (char b in OutChar)
                {
                    sb.Append((char)b);
                }
            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 使用缺省密钥解密XML文件
        /// </summary>
        /// <param name="xmlpath">需要解密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要解密的位数:1表示解密8位加密,2表示解密16位加密,3表示解密24位加密...</param>
        /// <returns>解密后XML文件文本字符串</returns>
        public string DecryptXML(string xmlpath, int count)
        {
            if (count <= 0)
                return null;
            XmlDocument doc = new XmlDocument();
            MemoryStream ms = new MemoryStream();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            try
            {
                doc.Load(xmlpath);

                doc = DecrypXmlDoc2XmlDoc(doc, count);

                doc.Save(ms);

                char[] OutChar = Encoding.UTF8.GetChars(ms.ToArray());
                foreach (char b in OutChar)
                {
                    sb.Append((char)b);
                }
            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return sb.ToString();
        }

        #endregion

        #region 使用缺省密钥加密、解密XML文件文本,并返回DataSet


        /// <summary>
        /// 使用缺省密钥加密密XML文件,并返回DataSet
        /// </summary>
        /// <param name="xmlpath">需要加密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要解加密的位数:1表示8位加密,2表示16位加密,3表示24位加密...</param>
        /// <returns>DataSet</returns>
        public DataSet EncryptXML2DS(string xmlpath, int count)
        {

            if (count <= 0)
                return null;
            DataSet ds = new DataSet();

            try
            {
                ds.ReadXml(xmlpath);

                ds = (DataSet)EncryptDS2DS(ds, count);

            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return ds;
        }

        /// <summary>
        /// 使用缺省密钥解密XML文件,并返回DataSet
        /// </summary>
        /// <param name="xmlpath">需要解密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要解密的位数:1表示解密8位加密,2表示解密16位加密,3表示解密24位加密...</param>
        /// <returns>DataSet</returns>
        public DataSet DecryptXML2DS(string xmlpath, int count)
        {

            if (count <= 0)
                return null;
            DataSet ds = new DataSet();

            try
            {
                ds.ReadXml(xmlpath);

                ds = (DataSet)DecrypDS2DS(ds, count);

            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return ds;
        }

        #endregion

        #region 使用缺省密钥加密、解密XML文件文本,并返回XmlDocument

        /// <summary>
        /// 使用缺省密钥加密XML文件,并返回XmlDocument
        /// </summary>
        /// <param name="xmlpath">需要加密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要加密的位数:1表示8位加密,2表示16位加密,3表示24位加密...</param>
        /// <returns>XmlDocument</returns>
        public XmlDocument EncryptXML2XmlDoc(string xmlpath, int count)
        {
            if (count <= 0)
                return null;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(xmlpath);

                doc = EncryptXmlDoc2XmlDoc(doc, count);

            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return doc;
        }

        /// <summary>
        /// 使用缺省密钥解密XML文件,并返回XmlDocument
        /// </summary>
        /// <param name="xmlpath">需要解密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要解密的位数:1表示解密8位加密,2表示解密16位加密,3表示解密24位加密...</param>
        /// <returns>XmlDocument</returns>
        public XmlDocument DecryptXML2XmlDoc(string xmlpath, int count)
        {
            if (count <= 0)
                return null;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(xmlpath);

                doc = DecrypXmlDoc2XmlDoc(doc, count);

            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return doc;
        }

        #endregion

        #region 加密、解密DataSet,并返回DataSet

        /// <summary>
        ///  加密DataSet，并返回DataSet
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="count">加密位数</param>
        /// <returns>DataSet</returns>
        public DataSet EncryptDS2DS(DataSet ds, int count)
        {
            try
            {
                XmlDocument doc = (XmlDocument)GetXmlDocFormDS(ds);

                XmlElement xmlel = doc.DocumentElement;

                EncryptNodeValue((XmlNode)xmlel, count);

                ds = (DataSet)GetDSFormXmlDoc(doc);
            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return ds;
        }

        /// <summary>
        /// 解密DataSet，并返回DataSet
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="count">解密位数</param>
        /// <returns>DataSet</returns>
        public DataSet DecrypDS2DS(DataSet ds, int count)
        {
            try
            {
                XmlDocument doc = (XmlDocument)GetXmlDocFormDS(ds);

                XmlElement xmlel = doc.DocumentElement;

                DecrypNodeValue((XmlNode)xmlel, count);

                ds = (DataSet)GetDSFormXmlDoc(doc);
            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return ds;
        }

        #endregion

        #region 加密、解密DataSet,并返回XmlDocument

        /// <summary>
        ///  加密DataSet，并返回XmlDocument
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="count">加密位数</param>
        /// <returns>XmlDocument</returns>
        public XmlDocument EncryptDS2XmlDoc(DataSet ds, int count)
        {
            XmlDocument doc = (XmlDocument)GetXmlDocFormDS(ds);
            try
            {

                XmlElement xmlel = doc.DocumentElement;

                EncryptNodeValue((XmlNode)xmlel, count);
            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return doc;
        }

        /// <summary>
        /// 解密DataSet，并返回XmlDocument
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="count">解密位数</param>
        /// <returns>XmlDocument</returns>
        public XmlDocument DecrypDS2XmlDoc(DataSet ds, int count)
        {
            XmlDocument doc = (XmlDocument)GetXmlDocFormDS(ds);
            try
            {

                XmlElement xmlel = doc.DocumentElement;

                DecrypNodeValue((XmlNode)xmlel, count);
            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return doc;
        }

        #endregion

        #region 加密、解密XmlDocument,并返回DataSet

        /// <summary>
        ///  加密XmlDocument，并返回DataSet
        /// </summary>
        /// <param name="doc">XmlDocument</param>
        /// <param name="count">加密位数</param>
        /// <returns>DataSet</returns>
        public DataSet EncryptXmlDoc2DS(XmlDocument doc, int count)
        {
            DataSet ds;
            try
            {
                XmlElement xmlel = doc.DocumentElement;

                EncryptNodeValue((XmlNode)xmlel, count);

                ds = (DataSet)GetDSFormXmlDoc(doc);
            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return ds;
        }

        /// <summary>
        /// 解密XmlDocument，并返回DataSet
        /// </summary>
        /// <param name="doc">XmlDocument</param>
        /// <param name="count">解密位数</param>
        /// <returns>DataSet</returns>
        public DataSet DecrypXmlDoc2DS(XmlDocument doc, int count)
        {
            DataSet ds;
            try
            {
                XmlElement xmlel = doc.DocumentElement;

                DecrypNodeValue((XmlNode)xmlel, count);

                ds = (DataSet)GetDSFormXmlDoc(doc);

            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return ds;
        }
        #endregion

        #region 加密、解密XmlDocument,并返回XmlDocument

        /// <summary>
        ///  加密XmlDocument，并返回XmlDocument
        /// </summary>
        /// <param name="doc">XmlDocument</param>
        /// <param name="count">加密位数</param>
        /// <returns>XmlDocument</returns>
        public XmlDocument EncryptXmlDoc2XmlDoc(XmlDocument doc, int count)
        {
            try
            {
                XmlElement xmlel = doc.DocumentElement;

                EncryptNodeValue((XmlNode)xmlel, count);

            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return doc;
        }

        /// <summary>
        /// 解密XmlDocument，并返回XmlDocument
        /// </summary>
        /// <param name="doc">XmlDocument</param>
        /// <param name="count">解密位数</param>
        /// <returns>XmlDocument</returns>
        public XmlDocument DecrypXmlDoc2XmlDoc(XmlDocument doc, int count)
        {
            try
            {
                XmlElement xmlel = doc.DocumentElement;
                DecrypNodeValue((XmlNode)xmlel, count);

            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return doc;
        }

        #endregion

        #region XmlDocment,DataSet互转
        /// <summary>
        /// DataSet转换为XmlDocument
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <returns>XmlDocument</returns>
        private XmlDocument GetXmlDocFormDS(DataSet ds)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                using (MemoryStream mStrm = new MemoryStream())
                {

                    StreamReader sRead = new StreamReader(mStrm);

                    ds.WriteXml(mStrm, XmlWriteMode.IgnoreSchema);

                    mStrm.Seek(0, SeekOrigin.Begin);

                    doc.Load(sRead);
                }
            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return doc;

        }

        /// <summary>
        /// XmlDocument转换为DataSet
        /// </summary>
        /// <param name="doc">XmlDocument</param>
        /// <returns>DataSet</returns>
        private DataSet GetDSFormXmlDoc(XmlDocument doc)
        {
            DataSet ds = new DataSet();
            try
            {
                using (XmlNodeReader reader = new XmlNodeReader(doc))
                {
                    ds.ReadXml(reader);

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return ds;
        }

        #endregion

        #region 遍历XML文件

        /// <summary>
        /// 递归遍历XML元素，进行加密
        /// </summary>
        /// <param name="node">Xml节点</param>
        /// <param name="count">加密位数</param>
        private void EncryptNodeValue(XmlNode node, int count)
        {
            foreach (XmlNode subnode in node.ChildNodes)
            {
                if (subnode.Name.StartsWith("xs:"))
                    continue;

                if (subnode.Attributes != null)
                {
                    foreach (XmlAttribute attr in subnode.Attributes)
                    {
                        string s1 = attr.Value;
                        for (int j = 0; j < count; j++)
                        {
                            s1 = EncryptString(s1);
                        }
                        attr.Value = s1;
                    }
                }


                if (subnode.HasChildNodes)
                {
                    EncryptNodeValue(subnode, count);
                }
                else
                {
                    string s0 = subnode.InnerText;

                    if (subnode.NodeType == XmlNodeType.Text)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            s0 = EncryptString(s0);
                        }
                        subnode.InnerText = s0;
                    }

                }
            }
        }

        /// <summary>
        /// 递归遍历XML元素，进行解密
        /// </summary>
        /// <param name="node">Xml节点</param>
        /// <param name="count">解密位数</param>
        private void DecrypNodeValue(XmlNode node, int count)
        {
            foreach (XmlNode subnode in node.ChildNodes)
            {
                if (subnode.Name.StartsWith("xs:"))
                    continue;

                if (subnode.Attributes != null)
                {
                    foreach (XmlAttribute attr in subnode.Attributes)
                    {
                        string s1 = attr.Value;
                        for (int j = 0; j < count; j++)
                        {
                            s1 = DecryptString(s1);
                        }
                        attr.Value = s1;
                    }
                }

                if (subnode.HasChildNodes)
                {
                    DecrypNodeValue(subnode, count);
                }
                else
                {
                    string s0 = subnode.InnerText;
                    if (subnode.NodeType == XmlNodeType.Attribute)
                    {
                        foreach (XmlAttribute attr in subnode.Attributes)
                        {
                            string s1 = attr.Value;
                            for (int j = 0; j < count; j++)
                            {
                                s1 = DecryptString(s1);
                            }
                            attr.Value = s1;
                        }
                    }

                    if (subnode.NodeType == XmlNodeType.Text)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            s0 = DecryptString(s0);
                        }
                    }
                    subnode.InnerText = s0;
                }
            }
        }
        #endregion

        #region 保存XML文件
        /// <summary>
        /// 保存XML文件
        /// </summary>
        /// <param name="xmlpath">保存xml文件的路径(包括文件名)</param>
        /// <param name="textContent">xml文件文本的内容</param>
        public void SaveXml(string xmlpath, string textContent)
        {
            if (!xmlpath.EndsWith("xml"))
                return;

            XmlDocument doc = new XmlDocument();

            try
            {
                using (StreamWriter sw = new StreamWriter(xmlpath, false, Encoding.UTF8))
                {

                    sw.Write(textContent);

                    doc.Save(sw);

                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                sendLog(ex.ToString());
                throw new Exception("保存文件失败,请查看日志.");
            }
        }
        #endregion
    }
}
