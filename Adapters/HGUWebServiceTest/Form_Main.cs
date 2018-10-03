using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace YWTWebServiceTest
{
    public partial class Form_Main : Form
    {
        public Form_Main()
        {
            InitializeComponent();
            this.Load += new EventHandler(Form_Main_Load);
            this.button_ok.Click += new EventHandler(button_ok_Click);
            this.button_off.Click += new EventHandler(button_off_Click);
            this.button_go.Click += new EventHandler(button_go_Click);
        }
        HGUWebServiceTest.localhost.HGUWebService m_service;
        void Form_Main_Load(object sender, EventArgs e)
        {
            //这里为了触发tabControl1表现页改变事件，进而根据标签页禁用某些文本框
            tabControl1.SelectTab(1);
            tabControl1.SelectTab(0);
            ChangeONUID();
            try
            {

                string title = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
                string template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>NewBroadband</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
 <!--MVLAN-->
  <MVLAN>{MVLAN}</MVLAN>
 <!--FENUMBER-->
  <FENUMBER>{FENumber}</FENUMBER>
 <!--POTSNumber -->
  <POTSNUMBER>{POTSNUMBER}</POTSNUMBER>
</Body>
";
                textBox1.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>NewIMS</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
 <!--MVLAN-->
  <MVLAN>{MVLAN}</MVLAN>
 <!--FENUMBER-->
  <FENUMBER>{FENumber}</FENUMBER>
 <!--POTSNumber -->
  <POTSNUMBER>{POTSNUMBER}</POTSNUMBER>
</Body>
";
                textBox2.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>AddBroadband</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
 <!--FENUMBER-->
  <FENUMBER>{FENumber}</FENUMBER>
 <!--POTSNumber -->
  <POTSNUMBER>{POTSNUMBER}</POTSNUMBER>
</Body>
";
                textBox3.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>AddIMS</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
  <FENUMBER>{FENumber}</FENUMBER>
 <!--POTSNumber -->
  <POTSNUMBER>{POTSNUMBER}</POTSNUMBER>
</Body>
";
                textBox4.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>DelBroadband</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
</Body>
";
                textBox5.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>DelIMS</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
</Body>
";
                textBox6.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>DelONU</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
</Body>
";
                textBox7.Text = template;

                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
</Body>
";
                textBox8.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>RelocateBroadband</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
 <!--MVLAN-->
  <MVLAN>{MVLAN}</MVLAN>
 <!--FENUMBER-->
  <FENUMBER>{FENumber}</FENUMBER>
 <!--POTSNumber -->
  <POTSNUMBER>{POTSNUMBER}</POTSNUMBER>
 <!--Old地市-->
  <OldCity>{OldCity}</OldCity>
  <!--Old厂家-->
  <OldManufacturer>{OldManufacturer}</OldManufacturer>
  <!--Old网管名称-->
  <OldOMCName>{OldOMCName}</OldOMCName>
  <!--OldOLT IP-->
  <OldOLTID>{OldOLTID}</OldOLTID>
  <!--OldPON口-->
  <OldPONID>{OldPONID}</OldPONID>
  <!--OldONU LOID-->
  <OldONUID>{OldONUID}</OldONUID>
  <!--Old电话号码-->
  <OldPhoneNumber>{OldPhoneNumber}</OldPhoneNumber>
  <!--OldSVLAN-->
  <OldSVLAN>{OldSVLAN}</OldSVLAN>
  <!--OldCVLAN-->
  <OldCVLAN>{OldCVLAN}</OldCVLAN>
 <!--OldMVLAN-->
  <OldMVLAN>{OldMVLAN}</OldMVLAN>
 <!--FENUMBER-->
  <OldFENUMBER>{OldFENumber}</OldFENUMBER>
 <!--POTSNumber -->
  <OldPOTSNUMBER>{OldPOTSNUMBER}</OldPOTSNUMBER>
 <!--IsContainIMS-->
  <IsContainIMS>{IsContainIMS}</IsContainIMS>
 <!--IMSSvlan-->
  <IMSSvlan>{IMSSvlan}</IMSSvlan>
 <!--IMSCvlan -->
  <IMSCvlan>{IMSCvlan}</IMSCvlan>
<!--IsContainIPTV-->
  <IsContainIPTV>{IsContainIPTV}</IsContainIPTV>
 <!--IPTVSvlan-->
  <IPTVSvlan>{IPTVSvlan}</IPTVSvlan>
 <!--IPTVCvlan -->
  <IPTVCvlan>{IPTVCvlan}</IPTVCvlan>
</Body>
";
                textBox9.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>RelocateIMS</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
 <!--MVLAN-->
  <MVLAN>{MVLAN}</MVLAN>
 <!--FENUMBER-->
  <FENUMBER>{FENumber}</FENUMBER>
 <!--POTSNumber -->
  <POTSNUMBER>{POTSNUMBER}</POTSNUMBER>
 <!--Old地市-->
  <OldCity>{OldCity}</OldCity>
  <!--Old厂家-->
  <OldManufacturer>{OldManufacturer}</OldManufacturer>
  <!--Old网管名称-->
  <OldOMCName>{OldOMCName}</OldOMCName>
  <!--OldOLT IP-->
  <OldOLTID>{OldOLTID}</OldOLTID>
  <!--OldPON口-->
  <OldPONID>{OldPONID}</OldPONID>
  <!--OldONU LOID-->
  <OldONUID>{OldONUID}</OldONUID>
  <!--Old电话号码-->
  <OldPhoneNumber>{OldPhoneNumber}</OldPhoneNumber>
  <!--OldSVLAN-->
  <OldSVLAN>{OldSVLAN}</OldSVLAN>
  <!--OldCVLAN-->
  <OldCVLAN>{OldCVLAN}</OldCVLAN>
 <!--OldMVLAN-->
  <OldMVLAN>{OldMVLAN}</OldMVLAN>
 <!--FENUMBER-->
  <OldFENUMBER>{OldFENumber}</OldFENUMBER>
 <!--POTSNumber -->
  <OldPOTSNUMBER>{OldPOTSNUMBER}</OldPOTSNUMBER>
</Body>
";
                textBox10.Text = template;
                template = title + @"             
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>NewIPTV</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
 <!--MVLAN-->
  <MVLAN>{MVLAN}</MVLAN>
 <!--FENUMBER-->
  <FENUMBER>{FENumber}</FENUMBER>
 <!--POTSNumber -->
  <POTSNUMBER>{POTSNUMBER}</POTSNUMBER>
</Body>
";
                textBox11.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>AddIPTV</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
  <FENUMBER>{FENumber}</FENUMBER>
 <!--POTSNumber -->
  <POTSNUMBER>{POTSNUMBER}</POTSNUMBER>
</Body>
";
                textBox12.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>DelIPTV</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
</Body>
";
                textBox13.Text = template;
                template = title + @"
<Body>
  <!--工单号-->
  <TaskID>{TaskID}</TaskID>
  <!--工单类型-->
  <TaskType>RelocateIPTV</TaskType>
  <!--地市-->
  <City>{City}</City>
  <!--厂家-->
  <Manufacturer>{Manufacturer}</Manufacturer>
  <!--网管名称-->
  <OMCName>{OMCName}</OMCName>
  <!--OLT IP-->
  <OLTID>{OLTID}</OLTID>
  <!--PON口-->
  <PONID>{PONID}</PONID>
  <!--ONU LOID-->
  <ONUID>{ONUID}</ONUID>
  <!--电话号码-->
  <PhoneNumber>{PhoneNumber}</PhoneNumber>
  <!--SVLAN-->
  <SVLAN>{SVLAN}</SVLAN>
  <!--CVLAN-->
  <CVLAN>{CVLAN}</CVLAN>
 <!--MVLAN-->
  <MVLAN>{MVLAN}</MVLAN>
 <!--FENUMBER-->
  <FENUMBER>{FENumber}</FENUMBER>
 <!--POTSNumber -->
  <POTSNUMBER>{POTSNUMBER}</POTSNUMBER>
 <!--Old地市-->
  <OldCity>{OldCity}</OldCity>
  <!--Old厂家-->
  <OldManufacturer>{OldManufacturer}</OldManufacturer>
  <!--Old网管名称-->
  <OldOMCName>{OldOMCName}</OldOMCName>
  <!--OldOLT IP-->
  <OldOLTID>{OldOLTID}</OldOLTID>
  <!--OldPON口-->
  <OldPONID>{OldPONID}</OldPONID>
  <!--OldONU LOID-->
  <OldONUID>{OldONUID}</OldONUID>
  <!--Old电话号码-->
  <OldPhoneNumber>{OldPhoneNumber}</OldPhoneNumber>
  <!--OldSVLAN-->
  <OldSVLAN>{OldSVLAN}</OldSVLAN>
  <!--OldCVLAN-->
  <OldCVLAN>{OldCVLAN}</OldCVLAN>
 <!--OldMVLAN-->
  <OldMVLAN>{OldMVLAN}</OldMVLAN>
 <!--FENUMBER-->
  <OldFENUMBER>{OldFENumber}</OldFENUMBER>
 <!--POTSNumber -->
  <OldPOTSNUMBER>{OldPOTSNUMBER}</OldPOTSNUMBER>
</Body>
";
                textBox14.Text = template;

                m_service = new HGUWebServiceTest.localhost.HGUWebService();
                m_service.Timeout = 30000;
                button_ok.Enabled = true;
            }
            catch (Exception ex)
            {
                button_ok.Enabled = false;
                textBox_result_xml.Text = ex.ToString();
            }
        }

        void button_off_Click(object sender, EventArgs e)
        {
            textBox_xml.Text = "";
            textBox_result_xml.Text = "";
        }

        void button_ok_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox_xml.Text == "")
                {
                    MessageBox.Show("请选择模板！");
                    return;
                }
                if (DialogResult.OK == MessageBox.Show("确定提交？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
                {
                    if (tabControl1.SelectedIndex != 7)
                    {
                        if (tabControl1.SelectedIndex != 8 && tabControl1.SelectedIndex != 9)
                        {
                            MessageBox.Show("开机+拆机工单！");

                            string taskid = "Test" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            string xml = textBox_xml.Text.Replace("{TaskID}", taskid);
                            textBox_result_xml.Text = "工单号:" + Environment.NewLine + taskid + Environment.NewLine +
                                "返回报文:" + Environment.NewLine + m_service.SendTask(xml);
                            MessageBox.Show("提交成功" + Environment.NewLine + "工单号:" + taskid);
                        }
                        else
                        {
                            MessageBox.Show("移机请点击移机报文按钮！");
                        }
                    }
                    else
                    {
                        MessageBox.Show("回滚工单！");

                        textBox_result_xml.Text = m_service.RollbackTask(textBox_xml.Text);
                        MessageBox.Show("提交成功");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("提交失败");
                textBox_result_xml.Text = ex.ToString();
            }
        }

        void button_go_Click(object sender, EventArgs e)
        {
            int i = tabControl1.SelectedIndex;
            textBox_result_xml.Text = "";
            switch (i)
            {
                case 0:
                    textBox_xml.Text = ReplaceTemplate(textBox1.Text);
                    break;
                case 1:
                    textBox_xml.Text = ReplaceTemplate(textBox2.Text);
                    break;
                case 2:
                    textBox_xml.Text = ReplaceTemplate(textBox3.Text);
                    break;
                case 3:
                    textBox_xml.Text = ReplaceTemplate(textBox4.Text);
                    break;
                case 4:
                    textBox_xml.Text = ReplaceTemplate(textBox5.Text);
                    break;
                case 5:
                    textBox_xml.Text = ReplaceTemplate(textBox6.Text);
                    break;
                case 6:
                    textBox_xml.Text = ReplaceTemplate(textBox7.Text);
                    break;
                case 7:
                    textBox_xml.Text = textBox8.Text;
                    break;
                case 8:
                    textBox_xml.Text = ReplaceTemplate(textBox9.Text); ;
                    break;
                case 9:
                    textBox_xml.Text = ReplaceTemplate(textBox10.Text);
                    break;
                case 10:
                    textBox_xml.Text = ReplaceTemplate(textBox11.Text);
                    break;
                case 11:
                    textBox_xml.Text = ReplaceTemplate(textBox12.Text);
                    break;
                case 12:
                    textBox_xml.Text = ReplaceTemplate(textBox13.Text);
                    break;
                case 13:
                    textBox_xml.Text = ReplaceTemplate(textBox14.Text);
                    break;
                default:
                    textBox_xml.Text = "";
                    textBox_result_xml.Text = i.ToString();
                    break;
            }
        }

        string ReplaceTemplate(string xml)
        {
            xml = xml.Replace("{City}", textBox_City.Text);
            xml = xml.Replace("{Manufacturer}", textBox_Manufacturer.Text);
            xml = xml.Replace("{OMCName}", textBox_OMCName.Text);
            string[] oltpon = textBox_OLTPON.Text.Split(',');
            string olt = oltpon[0].Split(':')[1];
            string pon = oltpon[1].Split(':')[1];
            xml = xml.Replace("{OLTID}", olt);
            xml = xml.Replace("{PONID}", pon);
            xml = xml.Replace("{ONUID}", textBox_ONUID.Text);

            if (xml.Contains("NewBroadband") == true || xml.Contains("AddBroadband") == true || xml.Contains("DelBroadband") == true)
            {
                xml = xml.Replace("{SVLAN}", textBox_bbSvlan.Text);
                xml = xml.Replace("{CVLAN}", textBox_bbCvlan.Text);
                xml = xml.Replace("{PhoneNumber}", textBox_bbPhone.Text);
            }
            if (xml.Contains("NewIMS") == true || xml.Contains("AddIMS") == true || xml.Contains("DelIMS") == true)
            {
                xml = xml.Replace("{SVLAN}", textBox_SVLAN.Text);
                xml = xml.Replace("{CVLAN}", textBox_CVLAN.Text);
                xml = xml.Replace("{PhoneNumber}", textBox_PhoneNumber.Text);
            }
            if (xml.Contains("NewIPTV") == true || xml.Contains("AddIPTV") == true || xml.Contains("DelIPTV") == true)
            {
                xml = xml.Replace("{SVLAN}", txtTVSvlan.Text);
                xml = xml.Replace("{CVLAN}", txtTVCvlan.Text);
                xml = xml.Replace("{PhoneNumber}", textBox_bbPhone.Text);
            }
            if (xml.Contains("DelONU") == true)
            {
                xml = xml.Replace("{PhoneNumber}", textBox_bbPhone.Text);
            }
            xml = xml.Replace("{MVLAN}", txtMVLAN.Text);
            xml = xml.Replace("{FENumber}", txtFENumber.Text);
            xml = xml.Replace("{POTSNUMBER}", txtPOTSNumber.Text);

            xml = xml.Replace("{OldCity}", textBox_oldCity.Text);
            xml = xml.Replace("{OldManufacturer}", textBox_oldManufacturer.Text);
            xml = xml.Replace("{OldOMCName}", textBox_oldOMCName.Text);
            string[] oldoltpon = textBox_oldOLTPON.Text.Split(',');
            string oldolt = oldoltpon[0].Split(':')[1];
            string oldpon = oldoltpon[1].Split(':')[1];
            xml = xml.Replace("{OldOLTID}", oldolt);
            xml = xml.Replace("{OldPONID}", oldpon);
            xml = xml.Replace("{OldONUID}", textBox_oldONUID.Text);


            if (xml.Contains("RelocateBroadband") == true)
            {
                //只移宽带
                xml = xml.Replace("{SVLAN}", textBox_bbSvlan.Text);
                xml = xml.Replace("{CVLAN}", textBox_bbCvlan.Text);
                xml = xml.Replace("{PhoneNumber}", textBox_bbPhone.Text);

                xml = xml.Replace("{OldSVLAN}", textBox_oldbbSvlan.Text);
                xml = xml.Replace("{OldCVLAN}", textBox_oldbbCvlan.Text);
                xml = xml.Replace("{textBox_oldbbPhone}", textBox_bbPhone.Text);

                if (txtIsContainIms.Text == "Y")
                {
                    //移宽带+IMS
                    xml = xml.Replace("{IMSSvlan}", textBox_SVLAN.Text);
                    xml = xml.Replace("{IMSCvlan}", textBox_CVLAN.Text);
                    xml = xml.Replace("{PhoneNumber}", textBox_bbPhone.Text);

                }
                if (txtIsContainIPTV.Text == "Y")
                {
                    //移宽带+IPTV
                    xml = xml.Replace("{IPTVSvlan}", txtIPTVSVLAN.Text);
                    xml = xml.Replace("{IPTVCvlan}", txtIPTVCVLAN.Text);
                }

            }
            if (xml.Contains("RelocateIMS") == true)
            {
                //移IMS
                xml = xml.Replace("{SVLAN}", textBox_SVLAN.Text);
                xml = xml.Replace("{CVLAN}", textBox_CVLAN.Text);
                xml = xml.Replace("{PhoneNumber}", textBox_PhoneNumber.Text);

                xml = xml.Replace("{OldSVLAN}", textBox_oldSVLAN.Text);
                xml = xml.Replace("{OldCVLAN}", textBox_oldCVLAN.Text);
                xml = xml.Replace("{OldPhoneNumber}", textBox_oldPhoneNumber.Text);
            }
            if (xml.Contains("RelocateIPTV") == true )
            {
                xml = xml.Replace("{SVLAN}", txtTVSvlan.Text);
                xml = xml.Replace("{CVLAN}", txtTVCvlan.Text);

                xml = xml.Replace("{OldSVLAN}", txtTVOldSvlan.Text);
                xml = xml.Replace("{OldCVLAN}", txtOldTVCVlan.Text);
            }



            xml = xml.Replace("{OldMVLAN}", txtoldMVLAN.Text);
            xml = xml.Replace("{OldFENumber}", txtoldFENumber.Text);
            xml = xml.Replace("{OldPOTSNUMBER}", txtoldPOTSNumber.Text);

            xml = xml.Replace("{IsContainIMS}", txtIsContainIms.Text);
            xml = xml.Replace("{IMSSvlan}", txtIMSSVlan.Text);
            xml = xml.Replace("{IMSCvlan}", txtIMSCVLAN.Text);
            return xml;

        }

        /// <summary>
        /// 移机报文信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox_xml.Text == "")
                {
                    MessageBox.Show("请选择模板！");
                    return;
                }
                if (DialogResult.OK == MessageBox.Show("确定提交？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
                {
                    if (tabControl1.SelectedIndex == 8 || tabControl1.SelectedIndex == 9 || tabControl1.SelectedIndex == 13)
                    {
                        MessageBox.Show("移机工单！");

                        string taskid = "Test" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        string xml = textBox_xml.Text.Replace("{TaskID}", taskid);
                        textBox_result_xml.Text = "工单号:" + Environment.NewLine + taskid + Environment.NewLine +
                            "返回报文:" + Environment.NewLine + m_service.RelocateTask(xml);
                        MessageBox.Show("提交成功" + Environment.NewLine + "工单号:" + taskid);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("提交失败");
                textBox_result_xml.Text = ex.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string title = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
            string xml = title + @"
<BODY><TaskID>TK_233100246530909_20171120102855</TaskID><TaskType>RelocateBroadband</TaskType><City>铜陵</City><Manufacturer>华为</Manufacturer><OMCName>铜陵华为GPON</OMCName><OLTID>10.220.247.23 

</OLTID><PONID>NA-0-11-1</PONID><ONUID>34562100628170000000</ONUID><MVLAN>3700</MVLAN><SVLAN>1665</SVLAN><CVLAN>2706</CVLAN><PhoneNumber>15005698988</PhoneNumber><FENUMBER>4</FENUMBER><POTSNUMBER>1</POTSNUMBER><OldCity>铜陵</OldCity><OldManufacturer>华为</OldManufacturer><OldOMCName>铜陵华为GPON</OldOMCName><OldOLTID>10.220.254.30 

</OldOLTID><OldPONID>NA-0-2-5</OldPONID><OldONUID>34562100628170000000</OldONUID><OldMVLAN>3700</OldMVLAN><OldSVLAN>1635</OldSVLAN><OldCVLAN>2703</OldCVLAN><OldPhoneNumber>15005698988</OldPhoneNumber><OldFENUMBER>4</OldFENUMBER><OldPOTSNUMBER>1</OldPOTSNUMBER><IsContainIMS>N</IsContainIMS><IMSSvlan>0</IMSSvlan><IMSCvlan>0</IMSCvlan></BODY>

";
            m_service.RelocateTask(xml);
        }
        static List<Control> noNeedFillTextBoxs = null;
        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (noNeedFillTextBoxs == null)
            {
                noNeedFillTextBoxs = new List<Control>
                {
                        textBox_oldbbCvlan,
                        textBox_oldbbPhone,
                        textBox_oldbbSvlan,
                        textBox_oldCity,
                        textBox_oldCVLAN,
                        textBox_oldManufacturer,
                        textBox_oldOLTPON,
                        textBox_oldOMCName,
                        textBox_oldONUID,
                        textBox_oldPhoneNumber,
                        textBox_oldSVLAN,
                        txtoldFENumber,
                        txtoldMVLAN,
                        txtoldPOTSNumber,
                        txtOldTVCVlan,
                        txtTVOldSvlan,
                        panel1
            };
            }
            if (e.TabPage.Text.Contains("移机"))
            {
                foreach (var item in noNeedFillTextBoxs)
                {
                    item.Enabled = true;
                }
                
            }
            else
            {
                foreach (var item in noNeedFillTextBoxs)
                {
                    item.Enabled = false;
                }
                
            }

            

            if (e.TabPage.Text.StartsWith("IMS"))
            {
                tabPage18.Parent = null;
                tabPage9.Parent = null;
                tabPage10.Parent = tabControl2;
            }
            else if(e.TabPage.Text.StartsWith("IPTV"))
            {
                tabPage9.Parent = null;
                tabPage10.Parent = null;
                tabPage18.Parent = tabControl2;
            }
            else if (e.TabPage.Text.StartsWith("宽带"))
            {
                tabPage18.Parent = null;
                tabPage10.Parent = null;
                tabPage9.Parent = tabControl2;
            }
            else
            {
                tabPage18.Parent = null;
                tabPage10.Parent = null;
                tabPage9.Parent = null ;
            }
                   
        }

        private void ChangeONUID()
        {
            textBox_ONUID.Text = DateTime.Now.ToString("yyyyMMddHHmmssffff")+GetRandomString(2);
            /*
            string onuid = string.Empty;
            for (int i = 0; i < 5; i++)
            {
                Random random = new Random(GetRandomSeed());
                string tempOnuId = random.Next(1000, 9999).ToString();
                onuid += tempOnuId;
            }
             textBox_ONUID.Text = onuid;
            */
        }
        //获取随机种子
        /*
        static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        */
        static string  GetRandomString(int length)
        {
            if (length <= 0)
                return "0";
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            int seed = BitConverter.ToInt32(bytes, 0);
            Random random = new Random(seed);
            return  random.Next(10*(length-1), 10*length).ToString();//random.net 包含最小值不包含最大值
        }

        #region OMCName拼接
        private void textBox_Manufacturer_TextChanged(object sender, EventArgs e)//ManufacturerTempConf
        {
            textBox_OMCName.Text = textBox_City.Text + textBox_Manufacturer.Text + "GPON";
        }

        private void textBox_City_TextChanged(object sender, EventArgs e)
        {
            textBox_OMCName.Text = textBox_City.Text + textBox_Manufacturer.Text + "GPON";
        }
        #endregion

        #region 鼠标悬浮提示
        private void label7_MouseHover(object sender, EventArgs e)
        {
            DisplayTips(this.label7, "对应BusinVLANFenPei表中管理业务UVLAN");
        }
        private void txtFENumber_MouseHover(object sender, EventArgs e)
        {
            DisplayTips(txtFENumber, "从ManufacturerTempConf中获取，默认为0");
        }
        private void txtPOTSNumber_MouseHover(object sender, EventArgs e)
        {
            DisplayTips(txtPOTSNumber, "从ManufacturerTempConf中获取，默认为0");
        }
        private void textBox_bbSvlan_MouseHover(object sender, EventArgs e)
        {
            DisplayTips(textBox_bbSvlan, "从Task表中获取，填SVLAN");
        }

        private void textBox_bbCvlan_MouseHover(object sender, EventArgs e)
        {
            DisplayTips(textBox_bbCvlan, "从Task表中获取，填CVLAN,值位于2200与2700之间");
        }
        private void label11_MouseHover(object sender, EventArgs e)
        {
            DisplayTips(label11, "VLAN包括SVLAN和CVLAN，有固定范围限制，可从Task表中取值确保值正确");
        }

        private void label14_MouseHover(object sender, EventArgs e)
        {
            DisplayTips(label14, "VLAN包括SVLAN和CVLAN，有固定范围限制，可从Task表中取值确保值正确");
        }

        private void txtIsContainIms_MouseHover(object sender, EventArgs e)
        {
            DisplayTips(txtIsContainIms, @"只识别Y\N，填N时下面的文本框均不需要填写");
        }
        void DisplayTips(Control control, string tips)
        {
            // 创建the ToolTip 
            ToolTip toolTip1 = new ToolTip();

            // 设置显示样式
            toolTip1.AutoPopDelay = 5000;//提示信息的可见时间
            toolTip1.InitialDelay = 500;//事件触发多久后出现提示
            toolTip1.ReshowDelay = 500;//指针从一个控件移向另一个控件时，经过多久才会显示下一个提示框
            toolTip1.ShowAlways = true;//是否显示提示框

            //  设置伴随的对象.
            toolTip1.SetToolTip(control, tips);
        }

        #endregion

        private void button3_Click(object sender, EventArgs e)
        {
            ChangeONUID();
        }
    }

}
