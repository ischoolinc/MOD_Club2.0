using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Data;
using System.Xml;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 取得學生本期的志願序內容
    /// </summary>
    class GetVolunteer
    {
        /// <summary>
        /// 學生本期志願序清單
        /// </summary>
        public List<VolunteerRecord> VolList { get; set; }

        /// <summary>
        /// 學生ID:學生志願序Record
        /// </summary>
        public Dictionary<string, VolunteerRecord> VolDic { get; set; }

        /// <summary>
        /// 學生ID:學生Record
        /// </summary>
        public Dictionary<string, StudentRecord> StudentDic { get; set; }

        /// <summary>
        /// 社團ID:社團Record
        /// </summary>
        public Dictionary<string, CLUBRecord> ClubDic { get; set; }

        public GetVolunteer()
        {
            VolDic = new Dictionary<string, VolunteerRecord>();

            //取得學生本期所有的志願序內容
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("school_year={0} and semester={1}", School.DefaultSchoolYear, School.DefaultSemester);
            VolList = tool._A.Select<VolunteerRecord>(sb.ToString());

            foreach (VolunteerRecord each in VolList)
            {
                if (!VolDic.ContainsKey(each.RefStudentID))
                {
                    VolDic.Add(each.RefStudentID, each);
                }
            }

            #region 整理出相關聯的學生與社團資料

            List<string> StudentIDList = new List<string>();
            List<string> ClubIDList = new List<string>();

            foreach (VolunteerRecord vr in VolList)
            {
                if (!StudentIDList.Contains(vr.RefStudentID))
                {
                    StudentIDList.Add(vr.RefStudentID);
                }

                XmlElement xml = XmlHelper.LoadXml(vr.Content);
                foreach (XmlElement each in xml.SelectNodes("Club"))
                {
                    if (!ClubIDList.Contains(each.GetAttribute("Ref_Club_ID")))
                    {
                        ClubIDList.Add(each.GetAttribute("Ref_Club_ID"));
                    }
                }
            } 

            #endregion

            StudentDic = GetVolunteerData.GetStudent(StudentIDList);

            ClubDic = tool.GetClub(ClubIDList);
        }
    }
}
