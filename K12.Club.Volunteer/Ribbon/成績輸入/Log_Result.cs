using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Data;
using System.Xml;
using FISCA.DSAUtil;

namespace K12.Club.Volunteer
{
    class Log_Result
    {

        public StudentRecord _stud { get; set; }

        public ClassRecord _class { get; set; }

        public SCJoin _sch { get; set; }

        /// <summary>
        /// 舊的資料
        /// </summary>
        public Dictionary<string, string> _OldItemDic { get; set; }

        /// <summary>
        /// 修改內容
        /// </summary>
        public Dictionary<string, string> _NewItemDic { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Log_Result(List<string> list, SCJoin sch)
        {
            _OldItemDic = new Dictionary<string, string>();
            _NewItemDic = new Dictionary<string, string>();
            IsChange = false;
            _sch = sch;
            foreach (string each in list)
            {
                _OldItemDic.Add(each, "");
                _NewItemDic.Add(each, "");
            }

            if (!string.IsNullOrEmpty(sch.Score))
            {
                XmlElement xml = DSXmlHelper.LoadXml(sch.Score);

                foreach (XmlElement each in xml.SelectNodes("Item"))
                {
                    string name = each.GetAttribute("Name");
                    string Score = each.GetAttribute("Score");
                    if (_OldItemDic.ContainsKey(name))
                    {
                        _OldItemDic[name] = Score;
                        _NewItemDic[name] = Score;
                    }
                }
            }
        }

        public bool IsChange { get; set; }

        /// <summary>
        /// 取得該筆Log調整內容
        /// </summary>
        public string GetLogString(成績取得器 GetPoint)
        {
            StringBuilder sb = new StringBuilder();

            string SeatNo = "";
            string CLUBName = "";
            if (_stud.SeatNo.HasValue)
                SeatNo = _stud.SeatNo.Value.ToString();

            if (GetPoint._ClubDic.ContainsKey(_sch.RefClubID))
            {
                CLUBName = GetPoint._ClubDic[_sch.RefClubID].ClubName;
            }

            sb.AppendLine(string.Format("社團「{0}」班級「{1}」座號「{2}」學生「{3}」", CLUBName, _class.Name, SeatNo, _stud.Name));

            foreach (string each in _OldItemDic.Keys)
            {
                if (_OldItemDic[each] != _NewItemDic[each])
                {
                    sb.AppendLine(string.Format("「{0}」由「{1}」修改為「{2}」", each, _OldItemDic[each], _NewItemDic[each]));
                }
            }

            return sb.ToString();
        }
    }
}
