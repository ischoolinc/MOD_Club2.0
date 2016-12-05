using System.Collections.Generic;
using System.Xml;
using FISCA.UDT;
using SmartSchool.API.PlugIn;
using K12.Data;
using System.Data;
using FISCA.DSAUtil;
using System.Text;

namespace K12.Club.Volunteer.CLUB
{
    class ExportVolunteerRecord : SmartSchool.API.PlugIn.Export.Exporter
    {
        Dictionary<string, string> TeacherDic { get; set; }
        Dictionary<string, StudentRecord> StudentDic { get; set; }
        //建構子
        public ExportVolunteerRecord()
        {
            this.Image = null;
            this.Text = "匯出社團志願序";
        }

        //覆寫
        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            #region 志願數

            int 學生選填志願數 = GetVolunteerData.GetVolumnteerCount();
            StudentDic = new Dictionary<string, StudentRecord>();
            #endregion

            //Dictionary<string, int> IndexDic = new Dictionary<string, int>();
            //int j = 1;
            List<string> FieldList = new List<string>();
            FieldList.Add("學年度"); //目前欄位
            FieldList.Add("學期"); //目前欄位
            for (int x = 1; x <= 學生選填志願數; x++)
            {
                FieldList.Add(string.Format("志願{0}", x)); //目前欄位
                //IndexDic.Add(string.Format("志願{0}", x), j);
                //j++;
            }
            List<CLUBRecord> ClubList = tool._A.Select<CLUBRecord>();
            Dictionary<string, CLUBRecord> CLUBDic = new Dictionary<string, CLUBRecord>();
            foreach (CLUBRecord each in ClubList)
            {
                if (!CLUBDic.ContainsKey(each.UID))
                {
                    CLUBDic.Add(each.UID, each);
                }
            }

            //取得所選學生之社團志願序(學年度/學期/學生 為單位)
            wizard.ExportableFields.AddRange(FieldList);

            wizard.ExportPackage += (sender, e) =>
            {
                List<VolunteerRecord> VolUnDic = GetVolunteerData.GetStudentVolunteerDic(e.List);
                //取得學生清單
                List<StudentRecord> StudentList = Student.SelectByIDs(e.List);

                StudentList = SortClassIndex.K12Data_StudentRecord(StudentList);
                foreach (StudentRecord each in StudentList)
                {
                    if (!StudentDic.ContainsKey(each.ID))
                    {
                        StudentDic.Add(each.ID, each);
                    }
                }

                VolUnDic.Sort(SortByStudent);

                for (int i = 0; i < VolUnDic.Count; i++)
                {
                    #region MyRegion

                    VolunteerRecord vr = VolUnDic[i];
                    RowData row = new RowData();
                    row.ID = VolUnDic[i].RefStudentID;
                    if (!string.IsNullOrEmpty(vr.Content))
                    {

                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        DSXmlHelper dsx = new DSXmlHelper();
                        dsx.Load(vr.Content);
                        foreach (XmlElement each in dsx.BaseElement.SelectNodes("Club"))
                        {
                            string index = each.GetAttribute("Index");
                            string clubID = each.GetAttribute("Ref_Club_ID");
                            if (CLUBDic.ContainsKey(clubID))
                            {
                                if (!dic.ContainsKey("志願" + index))
                                {
                                    dic.Add("志願" + index, CLUBDic[clubID].ClubName);
                                }
                            }
                        }

                        foreach (string field in e.ExportFields)
                        {
                            if (wizard.ExportableFields.Contains(field))
                            {
                                switch (field)
                                {
                                    case "學年度": row.Add(field, "" + VolUnDic[i].SchoolYear); break;
                                    case "學期": row.Add(field, "" + VolUnDic[i].Semester); break;
                                }

                                if (dic.ContainsKey(field))
                                {
                                    row.Add(field, dic[field]);
                                }
                            }
                        }
                    }

                    e.Items.Add(row);

                    #endregion
                }
            };
        }

        /// <summary>
        /// 排序2個學生
        /// </summary>
        private int SortByStudent(VolunteerRecord vr1, VolunteerRecord vr2)
        {
            if (StudentDic.ContainsKey(vr1.RefStudentID) && StudentDic.ContainsKey(vr2.RefStudentID))
            {
                StudentRecord sr1 = StudentDic[vr1.RefStudentID];
                StudentRecord sr2 = StudentDic[vr2.RefStudentID];
                return SortClassIndex.SortStudentDouble(sr1, sr2);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 取得教師名稱
        /// </summary>
        /// <param name="TeacherID"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetTeacher()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            DataTable dt = tool._Q.Select("select id,teacher_name,nickname from teacher where status=1");

            foreach (DataRow row in dt.Rows)
            {
                string id = "" + row["id"];
                string teacher_name = "" + row["teacher_name"];
                string nickname = "" + row["nickname"];
                if (!dic.ContainsKey(id))
                {
                    if (string.IsNullOrEmpty(nickname))
                        dic.Add(id, teacher_name);
                    else
                        dic.Add(id, teacher_name + "(" + nickname + ")");
                }
            }

            return dic;
        }

        /// <summary>
        /// 傳入科別內容,回傳以"/"號分隔的科別資料
        /// </summary>
        private string GetRestrict(string RestrictXml)
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(RestrictXml))
            {
                DSXmlHelper dsx = new DSXmlHelper();
                dsx.Load(RestrictXml);
                foreach (XmlElement xml in dsx.BaseElement.SelectNodes("Dept"))
                {
                    list.Add(xml.InnerText);
                }
            }

            return string.Join("/", list);
        }
    }
}