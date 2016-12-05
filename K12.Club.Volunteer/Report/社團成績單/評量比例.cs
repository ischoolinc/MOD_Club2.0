using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;
using System.Xml;
using FISCA.DSAUtil;

namespace K12.Club.Volunteer
{
    class 評量比例
    {
        public WeightProportion _wp { get; set; }

        /// <summary>
        /// 評量名稱與比例
        /// </summary>
        public Dictionary<string, int> ProportionDic = new Dictionary<string, int>();

        /// <summary>
        /// 評量名稱與定位
        /// </summary>
        public Dictionary<string, int> ColumnDic = new Dictionary<string, int>();

        public 評量比例()
        {
            List<WeightProportion> wpList = tool._A.Select<WeightProportion>();
            if (wpList.Count > 0)
            {
                _wp = wpList[0];
            }
            else
            {
                return;
            }

            if (_wp != null)
            {
                if (!string.IsNullOrEmpty(_wp.Proportion))
                {
                    int Columnindex = 0;
                    XmlElement xml = DSXmlHelper.LoadXml(_wp.Proportion);
                    foreach (XmlElement xml2 in xml.SelectNodes("Item"))
                    {
                        //記錄位置
                        ColumnDic.Add(xml2.GetAttribute("Name"), Columnindex);
                        //記錄比例
                        ProportionDic.Add(xml2.GetAttribute("Name"), int.Parse(xml2.GetAttribute("Proportion")));

                        Columnindex++;
                    }
                }
            }
        }
    }
}
