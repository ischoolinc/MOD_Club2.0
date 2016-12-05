using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.DocumentValidator;
using FISCA.DSAUtil;
using K12.Data;
using System.Xml;

namespace K12.Club.Volunteer
{
    class VolunteerImportBOT
    {
        /// <summary>
        /// 單筆Log
        /// </summary>
        public string GetLogString(Dictionary<string, CLUBRecord> ClubDic, VolunteerRecord each, StudentRecord sr)
        {
            StringBuilder log = new StringBuilder();
            string classname = sr.Class != null ? sr.Class.Name : "";
            string SeatNo = sr.SeatNo.HasValue ? sr.SeatNo.Value.ToString() : "";
            log.AppendLine(string.Format("班級「{0}」座號「{1}」姓名「{2}」學號「{3}」", classname, SeatNo, sr.Name, sr.StudentNumber));
            if (!string.IsNullOrEmpty(each.Content))
            {
                log.AppendLine(string.Format("志願序「{0}」", GetVoluntString(ClubDic, each.Content)));
            }
            else
            {
                log.AppendLine(string.Format("志願序「{0}」", "「無」"));
            }

            return log.ToString();
        }

        public string GetVoluntString(Dictionary<string, CLUBRecord> ClubDic, string name)
        {
            StringBuilder sb = new StringBuilder();
            DSXmlHelper dsx = new DSXmlHelper();
            dsx.Load(name);
            foreach (XmlElement xml in dsx.BaseElement.SelectNodes("Club"))
            {
                string id = xml.GetAttribute("Ref_Club_ID");
                if (ClubDic.ContainsKey(id))
                {
                    sb.Append(string.Format("志願{0}「{1}」", xml.GetAttribute("Index"), ClubDic[id].ClubName));
                }
                else
                {
                    sb.Append(string.Format("志願{0}「錯誤的社團ID!!」", xml.GetAttribute("Index")));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 取得比較後的更新Log
        /// </summary>
        public string SetLog(LogVolunteer log)
        {
            //檢查與確認資料是否被修改
            StringBuilder sb = new StringBuilder();
            if (log.lo_Vol.Content != log.New_Vol.Content)
            {
                sb.AppendLine("志願序由：");
                sb.AppendLine(a(log.lo_Vol.Content, log));
                sb.AppendLine("修改為：");
                sb.AppendLine(a(log.New_Vol.Content, log));
            }
            else
            {
                sb.AppendLine("志願序「未修改」");
            }

            return sb.ToString();
        }

        public string a(string name, LogVolunteer log)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(name))
            {
                DSXmlHelper dsx = new DSXmlHelper();
                dsx.Load(name);
                foreach (XmlElement xml in dsx.BaseElement.SelectNodes("Club"))
                {
                    string id = xml.GetAttribute("Ref_Club_ID");
                    if (log.ClubDic.ContainsKey(id))
                    {
                        sb.Append(string.Format("志願{0}「{1}」", xml.GetAttribute("Index"), log.ClubDic[id].ClubName));
                    }
                    else
                    {
                        sb.Append(string.Format("志願{0}「錯誤的社團ID!!」", xml.GetAttribute("Index")));
                    }
                }
            }
            else
            {
                sb.Append("無志願");
            }

            return sb.ToString();
        }
    }
}
