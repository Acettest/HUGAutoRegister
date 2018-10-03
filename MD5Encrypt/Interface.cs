using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;

namespace MD5Encrypt
{
    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="strlog">异常字符串</param>
    public delegate void delegateMD5SengLog(string strlog);

    /// <summary>
    /// MD5接口
    /// </summary>
    public interface IMD5Encrypt
    {
        
        /// <summary>
        /// 事件:记录错误日志
        /// </summary>
        event delegateMD5SengLog sendLog;

        /// <summary>
        /// 使用缺省密钥加密字符串
        /// </summary>
        /// <param name="sInputString">需要加密的字符串</param>
        /// <returns>加密后字符串</returns>
        string EncryptString(string sInputString);

        /// <summary>
        /// 使用缺省密钥解密字符串
        /// </summary>
        /// <param name="sInputString">需要解密字符串</param>
        /// <returns>解密后字符串</returns>
        string DecryptString(string sInputString);

        /// <summary>
        /// 使用缺省密钥加密XML文件
        /// </summary>
        /// <param name="xmlpath">需要加密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要加密的位数:1表示8位加密,2表示16位加密,3表示24位加密...</param>
        /// <returns>加密后XML文件文本字符串</returns>
        string EncryptXML(string xmlpath, int count);

        /// <summary>
        /// 使用缺省密钥解密XML文件
        /// </summary>
        /// <param name="xmlpath">需要解密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要解密的位数:1表示解密8位加密,2表示解密16位加密,3表示解密24位加密...</param>
        /// <returns>解密后XML文件文本字符串</returns>
        string DecryptXML(string xmlpath, int count);


        /// <summary>
        /// 使用缺省密钥加密密XML文件,并返回DataSet
        /// </summary>
        /// <param name="xmlpath">需要加密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要解加密的位数:1表示8位加密,2表示16位加密,3表示24位加密...</param>
        /// <returns>DataSet</returns>
        DataSet EncryptXML2DS(string xmlpath, int count);

        /// <summary>
        /// 使用缺省密钥解密XML文件,并返回DataSet
        /// </summary>
        /// <param name="xmlpath">需要解密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要解密的位数:1表示解密8位加密,2表示解密16位加密,3表示解密24位加密...</param>
        /// <returns>DataSet</returns>
        DataSet DecryptXML2DS(string xmlpath, int count);

        /// <summary>
        /// 使用缺省密钥加密XML文件,并返回XmlDocument
        /// </summary>
        /// <param name="xmlpath">需要加密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要加密的位数:1表示8位加密,2表示16位加密,3表示24位加密...</param>
        /// <returns>XmlDocument</returns>
        XmlDocument EncryptXML2XmlDoc(string xmlpath, int count);

        /// <summary>
        /// 使用缺省密钥解密XML文件,并返回XmlDocument
        /// </summary>
        /// <param name="xmlpath">需要解密的XML文件完整路径(包括文件名)</param>
        /// <param name="count">需要解密的位数:1表示解密8位加密,2表示解密16位加密,3表示解密24位加密...</param>
        /// <returns>XmlDocument</returns>
        XmlDocument DecryptXML2XmlDoc(string xmlpath, int count);

        /// <summary>
        ///  加密DataSet，并返回DataSet
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="count">加密位数</param>
        /// <returns>DataSet</returns>
        DataSet EncryptDS2DS(DataSet ds, int count);

        /// <summary>
        /// 解密DataSet，并返回DataSet
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="count">解密位数</param>
        /// <returns>DataSet</returns>
        DataSet DecrypDS2DS(DataSet ds, int count);

        /// <summary>
        ///  加密DataSet，并返回XmlDocument
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="count">加密位数</param>
        /// <returns>XmlDocument</returns>
        XmlDocument EncryptDS2XmlDoc(DataSet ds, int count);

        /// <summary>
        /// 解密DataSet，并返回XmlDocument
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="count">解密位数</param>
        /// <returns>XmlDocument</returns>
        XmlDocument DecrypDS2XmlDoc(DataSet ds, int count);

        /// <summary>
        ///  加密XmlDocument，并返回DataSet
        /// </summary>
        /// <param name="doc">XmlDocument</param>
        /// <param name="count">加密位数</param>
        /// <returns>DataSet</returns>
        DataSet EncryptXmlDoc2DS(XmlDocument doc, int count);

        /// <summary>
        /// 解密XmlDocument，并返回DataSet
        /// </summary>
        /// <param name="doc">XmlDocument</param>
        /// <param name="count">解密位数</param>
        /// <returns>DataSet</returns>
        DataSet DecrypXmlDoc2DS(XmlDocument doc, int count);

        /// <summary>
        ///  加密XmlDocument，并返回XmlDocument
        /// </summary>
        /// <param name="doc">XmlDocument</param>
        /// <param name="count">加密位数</param>
        /// <returns>XmlDocument</returns>
        XmlDocument EncryptXmlDoc2XmlDoc(XmlDocument doc, int count);

        /// <summary>
        /// 解密XmlDocument，并返回XmlDocument
        /// </summary>
        /// <param name="doc">XmlDocument</param>
        /// <param name="count">解密位数</param>
        /// <returns>XmlDocument</returns>
        XmlDocument DecrypXmlDoc2XmlDoc(XmlDocument doc, int count);

        /// <summary>
        /// 保存XML文件
        /// </summary>
        /// <param name="xmlpath">保存xml文件的路径(包括文件名)</param>
        /// <param name="textContent">xml文件文本的内容</param>
        void SaveXml(string xmlpath, string textContent);
    }
}
