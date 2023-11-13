using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.DocumentValidator;
using FISCA.DSAUtil;
using K12.Data;
using System.Xml;
using System.Data;

namespace K12.Club.Volunteer
{
    class CLUBImportBOT
    {
        public Dictionary<string, CLUBRecord> ClubDic { get; set; }

        public Dictionary<string, TeacherRecord> TeacherNameDic { get; set; }

        public Dictionary<string, TeacherRecord> TeacherIDDic { get; set; }

        //社團ID , 學生ID , 幹部紀錄
        public Dictionary<string, Dictionary<string, List<CadresRecord>>> CadreIDDic { get; set; }

        public Dictionary<string, StudDe> StudentDic { get; set; }

        public string GetLogString(CLUBRecord each)
        {
            StringBuilder log = new StringBuilder();
            log.AppendLine(string.Format("學年度「{0}」學期「{1}」社團名稱「{2}」", each.SchoolYear, each.Semester, each.ClubName));
            if (!string.IsNullOrEmpty(each.ClubNumber))
                log.AppendLine(string.Format("代碼「{0}」", each.ClubNumber));
            if (!string.IsNullOrEmpty(each.Location))
                log.AppendLine(string.Format("場地「{0}」", each.Location));
            if (!string.IsNullOrEmpty(each.ClubCategory))
                log.AppendLine(string.Format("類型「{0}」", each.ClubCategory));

            if (!string.IsNullOrEmpty(each.RefTeacherID))
            {
                if (TeacherIDDic.ContainsKey(each.RefTeacherID))
                {
                    log.AppendLine(string.Format("老師1「{0}」", GetTeacherName(TeacherIDDic[each.RefTeacherID])));
                }
            }
            if (!string.IsNullOrEmpty(each.RefTeacherID2))
            {
                if (TeacherIDDic.ContainsKey(each.RefTeacherID2))
                {
                    log.AppendLine(string.Format("老師2「{0}」", GetTeacherName(TeacherIDDic[each.RefTeacherID2])));
                }
            }

            if (!string.IsNullOrEmpty(each.RefTeacherID3))
            {
                if (TeacherIDDic.ContainsKey(each.RefTeacherID3))
                {
                    log.AppendLine(string.Format("老師3「{0}」", GetTeacherName(TeacherIDDic[each.RefTeacherID3])));
                }
            }
            if (!string.IsNullOrEmpty(each.About))
                log.AppendLine(string.Format("簡介「{0}」", each.About));
            if (!string.IsNullOrEmpty(each.GenderRestrict))
                log.AppendLine(string.Format("限制:性別「{0}」", each.GenderRestrict));
            if (!string.IsNullOrEmpty(GetDeptName(each.DeptRestrict)))
                log.AppendLine(string.Format("限制:科別「{0}」", GetDeptName(each.DeptRestrict)));
            if (each.Grade1Limit.HasValue)
                log.AppendLine(string.Format("限制:一年級人數「{0}」", each.Grade1Limit.Value.ToString()));
            if (each.Grade2Limit.HasValue)
                log.AppendLine(string.Format("限制:二年級人數「{0}」", each.Grade2Limit.Value.ToString()));
            if (each.Grade3Limit.HasValue)
                log.AppendLine(string.Format("限制:三年級人數「{0}」", each.Grade3Limit.Value.ToString()));
            if (each.Limit.HasValue)
                log.AppendLine(string.Format("限制:人數上限「{0}」", each.Limit.Value.ToString()));

            return log.ToString();
        }

        public void SetClub(IRowStream Row, CLUBRecord club)
        {
            club.ClubNumber = Row.GetValue("代碼");
            club.Location = Row.GetValue("場地");
            club.ClubCategory = Row.GetValue("類型");

            club.Level = Row.GetValue("評等");

            club.RefTeacherID = checkTeacherName("" + Row.GetValue("老師1"));
            club.RefTeacherID2 = checkTeacherName("" + Row.GetValue("老師2"));
            club.RefTeacherID3 = checkTeacherName("" + Row.GetValue("老師3"));

            club.About = Row.GetValue("簡介");
            club.GenderRestrict = Row.GetValue("限制:性別");
            club.DeptRestrict = GetDeptXML(Row.GetValue("限制:科別"));

            int x = 0;
            if (int.TryParse("" + Row.GetValue("限制:一年級人數"), out x))
                club.Grade1Limit = x;
            else
                club.Grade1Limit = null;

            if (int.TryParse("" + Row.GetValue("限制:二年級人數"), out x))
                club.Grade2Limit = x;
            else
                club.Grade2Limit = null;

            if (int.TryParse("" + Row.GetValue("限制:三年級人數"), out x))
                club.Grade3Limit = x;
            else
                club.Grade3Limit = null;

            if (int.TryParse("" + Row.GetValue("限制:人數上限"), out x))
                club.Limit = x;
            else
                club.Limit = null;
        }

        /// <summary>
        /// 取得XML結構之科別清單狀態
        /// </summary>
        public string GetDeptXML(string Dept)
        {
            if (!string.IsNullOrEmpty(Dept))
            {
                string[] DeptList = Dept.Split('/');
                DSXmlHelper dsXml = new DSXmlHelper("Department");
                foreach (string each in DeptList)
                {
                    dsXml.AddElement("Dept");
                    dsXml.AddText("Dept", each);
                }
                return dsXml.BaseElement.OuterXml;
            }
            else
            {
                DSXmlHelper dsXml = new DSXmlHelper("Department");
                return dsXml.BaseElement.OuterXml;
            }
        }

        public string GetDeptName(string xml)
        {
            List<string> list = new List<string>();
            DSXmlHelper dsXml = new DSXmlHelper();
            dsXml.Load(xml);
            foreach (XmlElement each in dsXml.BaseElement.SelectNodes("Dept"))
            {
                list.Add(each.InnerText);
            }

            return string.Join("/", list);

        }


        /// <summary>
        /// 老師是否存在,則傳回老師ID
        /// </summary>
        public string checkTeacherName(string name)
        {
            if (TeacherNameDic.ContainsKey(name))
                return TeacherNameDic[name].ID;
            else
                return "";
        }

        /// <summary>
        /// 取得老師清單 Name:Record
        /// </summary>
        public void GetTeacherDic()
        {
            TeacherIDDic = new Dictionary<string, TeacherRecord>();
            TeacherNameDic = new Dictionary<string, TeacherRecord>();

            List<TeacherRecord> TeacherList = K12.Data.Teacher.SelectAll();
            foreach (TeacherRecord each in TeacherList)
            {
                if (each.Status == TeacherRecord.TeacherStatus.一般)
                {
                    #region 老師名稱
                    string teacherName = "";
                    if (string.IsNullOrEmpty(each.Nickname))
                    {
                        teacherName = each.Name;
                    }
                    else
                    {
                        teacherName = each.Name + "(" + each.Nickname + ")";
                    }

                    if (!TeacherNameDic.ContainsKey(teacherName))
                    {
                        TeacherNameDic.Add(teacherName, each);
                    }
                    #endregion

                    //建立老師對照 ID:Record
                    if (!TeacherIDDic.ContainsKey(each.ID))
                    {
                        TeacherIDDic.Add(each.ID, each);
                    }
                }
            }
        }

        /// <summary>
        /// 取得幹部清單 Name:Record
        /// </summary>
        public void GetCadreDic()
        {
            CadreIDDic = new Dictionary<string, Dictionary<string, List<CadresRecord>>>();

            List<CadresRecord> CadresList = tool._A.Select<CadresRecord>();

            foreach (CadresRecord each in CadresList)
            {

                //建立對照 ID:Record
                if (!CadreIDDic.ContainsKey(each.RefClubID))
                {
                    CadreIDDic.Add(each.RefClubID, new Dictionary<string, List<CadresRecord>>());
                }

                if (!CadreIDDic[each.RefClubID].ContainsKey(each.RefStudentID))
                {
                    CadreIDDic[each.RefClubID].Add(each.RefStudentID, new List<CadresRecord>());
                }

                CadreIDDic[each.RefClubID][each.RefStudentID].Add(each);
            }
        }

        /// <summary>
        /// 傳入老師Record,回傳包含老師暱稱的名字
        /// </summary>
        public string GetTeacherName(TeacherRecord tr)
        {
            if (string.IsNullOrEmpty(tr.Nickname))
            {
                return tr.Name;
            }
            else
            {
                return tr.Name + "(" + tr.Nickname + ")";
            }
        }

        /// <summary>
        /// 取得社團清單 school + semester + name:Record
        /// </summary>
        public void GetCLUBDic()
        {
            ClubDic = new Dictionary<string, CLUBRecord>();
            List<CLUBRecord> CLUBList = tool._A.Select<CLUBRecord>();
            foreach (CLUBRecord each in CLUBList)
            {
                string CourseKey = each.SchoolYear + "," + each.Semester + "," + each.ClubName;
                if (!ClubDic.ContainsKey(CourseKey))
                {
                    ClubDic.Add(CourseKey, each);
                }
            }
        }

        /// <summary>
        /// 以學生學號取得學生清單
        /// </summary>
        public void GetStudentIDList(List<string> StudentNumberList)
        {
            StudentDic = new Dictionary<string, StudDe>();

            StringBuilder sb_log = new StringBuilder();
            sb_log.Append(@"select student.id,student.student_number,student.name,student.seat_no,class.class_name from student join class on student.ref_class_id=class.id where student.student_number in ('{0}')");

            DataTable dt = tool._Q.Select(string.Format(sb_log.ToString(), string.Join("','", StudentNumberList)));
            foreach (DataRow row in dt.Rows)
            {
                string number = "" + row["student_number"];

                StudDe stuf = new StudDe(row);
                if (!StudentDic.ContainsKey(stuf.number))
                {
                    StudentDic.Add(stuf.number, stuf);
                }
            }
        }

        public string SetLog(ImputLog log)
        {
            //檢查與確認資料是否被修改
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("學年度「{0}」學期「{1}」社團名稱「{2}」", log.New_club.SchoolYear, log.New_club.Semester, log.New_club.ClubName));
            if (log.lo_CLUB.ClubNumber != log.New_club.ClubNumber)
                sb.AppendLine(ByOne("代碼", log.lo_CLUB.ClubNumber, log.New_club.ClubNumber));

            if (log.lo_CLUB.Location != log.New_club.Location)
                sb.AppendLine(ByOne("場地", log.lo_CLUB.Location, log.New_club.Location));

            if (log.lo_CLUB.ClubCategory != log.New_club.ClubCategory)
                sb.AppendLine(ByOne("類型", log.lo_CLUB.ClubCategory, log.New_club.ClubCategory));

            if (log.lo_CLUB.Level != log.New_club.Level)
                sb.AppendLine(ByOne("評等", log.lo_CLUB.Level, log.New_club.Level));

            if (log.lo_CLUB.RefTeacherID != log.New_club.RefTeacherID)
                ByTeacher("老師1", log.lo_CLUB.RefTeacherID, log.New_club.RefTeacherID);

            if (log.lo_CLUB.RefTeacherID2 != log.New_club.RefTeacherID2)
                ByTeacher("老師2", log.lo_CLUB.RefTeacherID2, log.New_club.RefTeacherID2);

            if (log.lo_CLUB.RefTeacherID3 != log.New_club.RefTeacherID3)
                ByTeacher("老師3", log.lo_CLUB.RefTeacherID3, log.New_club.RefTeacherID3);

            if (log.lo_CLUB.About != log.New_club.About)
                sb.AppendLine(ByOne("簡介", log.lo_CLUB.About, log.New_club.About));

            if (log.lo_CLUB.DeptRestrict != log.New_club.DeptRestrict)
                sb.AppendLine(ByOne("科別", GetDeptName(log.lo_CLUB.DeptRestrict), GetDeptName(log.New_club.DeptRestrict)));

            if (log.lo_CLUB.GenderRestrict != log.New_club.GenderRestrict)
                sb.AppendLine(ByOne("限制:性別", log.lo_CLUB.GenderRestrict, log.New_club.GenderRestrict));

            if (log.lo_CLUB.Grade1Limit != log.New_club.Grade1Limit)
                sb.AppendLine(ByOne("限制:一年級人數", ByInet(log.lo_CLUB.Grade1Limit), ByInet(log.New_club.Grade1Limit)));

            if (log.lo_CLUB.Grade2Limit != log.New_club.Grade2Limit)
                sb.AppendLine(ByOne("限制:二年級人數", ByInet(log.lo_CLUB.Grade2Limit), ByInet(log.New_club.Grade2Limit)));

            if (log.lo_CLUB.Grade3Limit != log.New_club.Grade3Limit)
                sb.AppendLine(ByOne("限制:二年級人數", ByInet(log.lo_CLUB.Grade3Limit), ByInet(log.New_club.Grade3Limit)));

            if (log.lo_CLUB.Grade3Limit != log.New_club.Grade3Limit)
                sb.AppendLine(ByOne("限制:二年級人數", ByInet(log.lo_CLUB.Grade3Limit), ByInet(log.New_club.Grade3Limit)));
            sb.AppendLine("");
            return sb.ToString();
        }

        public string ByInet(int? a)
        {
            if (a.HasValue)
                return a.Value.ToString();
            else
                return "";
        }

        public string ByTeacher(string a, string b, string c)
        {
            string teacherA = "";
            string teacherB = "";
            if (TeacherIDDic.ContainsKey(b))
                teacherA = GetTeacherName(TeacherIDDic[b]);

            if (TeacherIDDic.ContainsKey(c))
                teacherB = GetTeacherName(TeacherIDDic[c]);

            return ByOne(a, teacherA, teacherB);
        }

        public string ByOne(string a, string b, string c)
        {
            return string.Format("「{0}」由「{1}」修改為「{2}」", a, b, c);
        }
    }

    class StudDe
    {
        public StudDe(DataRow row)
        {
            id = "" + row["id"];
            number = "" + row["student_number"];
            Name = "" + row["name"];
            ClassName = "" + row["class_name"];
            seat_no = "" + row["seat_no"];
        }
        public string seat_no { get; set; }
        public string ClassName { get; set; }
        public string Name { get; set; }
        public string number { get; set; }
        public string id { get; set; }
    }
}
