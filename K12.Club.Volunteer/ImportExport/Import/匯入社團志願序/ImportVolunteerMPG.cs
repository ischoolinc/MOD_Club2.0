using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.Import;
using FISCA.Presentation.Controls;
using Campus.DocumentValidator;
using K12.Data;
using FISCA.DSAUtil;
using System.Xml;
using System.Data;

namespace K12.Club.Volunteer
{
    class ImportVolunteerMPG : ImportWizard
    {
        //設定檔
        private ImportOption mOption;

        int 學生選填志願數 { get; set; }

        VolunteerImportBOT Importbot = new VolunteerImportBOT();

        Dictionary<string, LogVolunteer> Log_Dic = new Dictionary<string, LogVolunteer>();

        public override string GetValidateRule()
        {
            //依據使用者設定的志願序數量
            //來產生志願序的驗證數量
            學生選填志願數 = GetVolunteerData.GetVolumnteerCount();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version='1.0' encoding='utf-8' ?>");
            sb.AppendLine("<?xml-stylesheet type='text/xsl' href='format.xsl' ?>");

            DSXmlHelper dsx = new DSXmlHelper();
            dsx.Load(Properties.Resources.ImportVolunteerRecordValRule);
            for (int x = 1; x <= 學生選填志願數; x++)
            {
                XmlElement Xmle = dsx.AddElement("./FieldList", "Field");
                Xmle.SetAttribute("Required", "True");
                Xmle.SetAttribute("Name", "志願" + x);
                Xmle.SetAttribute("EmptyAlsoValidate", "False");
                Xmle.SetAttribute("Description", "學生的志願項目");
            }

            StringBuilder Rule = new StringBuilder();
            Rule.Append(sb);
            Rule.Append(dsx.BaseElement.OuterXml);

            return Rule.ToString();
        }

        public override string Import(List<Campus.DocumentValidator.IRowStream> Rows)
        {
            if (mOption.Action == ImportAction.InsertOrUpdate)
            {
                #region 學號:學生系統編號
                Dictionary<string, string> StudentDic = new Dictionary<string, string>();
                List<string> StudentNumberList = new List<string>();
                foreach (IRowStream Row in Rows)
                {
                    string StudentNumber = Row.GetValue("學號");
                    if (!StudentNumberList.Contains(StudentNumber))
                    {
                        StudentNumberList.Add(StudentNumber);
                    }
                }

                DataTable dt = tool._Q.Select(string.Format("select id,student_number from student where student_number in ('{0}')", string.Join("','", StudentNumberList)));
                //學生ID清單
                List<string> StudentIDList = new List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    string id = "" + row["id"];
                    string number = "" + row["student_number"];
                    if (!StudentDic.ContainsKey(number))
                    {
                        StudentDic.Add(number, id);
                    }
                    if (!StudentIDList.Contains(id))
                    {
                        StudentIDList.Add(id);
                    }
                }

                //學生Log專用的記錄
                List<StudentRecord> StudentList = K12.Data.Student.SelectByIDs(StudentIDList);
                Dictionary<string, StudentRecord> StudentLogDic = new Dictionary<string, StudentRecord>();
                foreach (StudentRecord each in StudentList)
                {
                    if (!StudentLogDic.ContainsKey(each.ID))
                    {
                        StudentLogDic.Add(each.ID, each);
                    }
                }
                #endregion

                Dictionary<string, CLUBRecord> ClubDic = new Dictionary<string, CLUBRecord>();
                Dictionary<string, CLUBRecord> ClubLogDic = new Dictionary<string, CLUBRecord>();
                List<CLUBRecord> CLUBList = tool._A.Select<CLUBRecord>();
                foreach (CLUBRecord each in CLUBList)
                {
                    string CourseKey = each.SchoolYear + "," + each.Semester + "," + each.ClubName;
                    if (!ClubDic.ContainsKey(CourseKey))
                    {
                        ClubDic.Add(CourseKey, each);
                    }
                    if (!ClubLogDic.ContainsKey(each.UID))
                    {
                        ClubLogDic.Add(each.UID, each);
                    }
                }

                //系統內是否已經有志願序清單
                //學年度學期學號 : 志願序
                Dictionary<string, VolunteerRecord> VolunteerDic = new Dictionary<string, VolunteerRecord>();
                List<VolunteerRecord> vrList = tool._A.Select<VolunteerRecord>(string.Format("ref_student_id in ('{0}')", string.Join("','", StudentDic.Values)));
                foreach (VolunteerRecord each in vrList)
                {
                    string kkey = each.SchoolYear + "," + each.Semester + "," + each.RefStudentID;
                    if (!VolunteerDic.ContainsKey(kkey))
                    {
                        VolunteerDic.Add(kkey, each);
                    }

                    //Log
                    if (!Log_Dic.ContainsKey(kkey))
                    {
                        LogVolunteer im = new LogVolunteer();
                        im.lo_Vol = each.CopyExtension();
                        im.ClubDic = ClubLogDic;
                        Log_Dic.Add(kkey, im);
                    }
                }

                List<VolunteerRecord> VolunteerInsertList = new List<VolunteerRecord>();
                List<VolunteerRecord> VolunteerUpdateList = new List<VolunteerRecord>();

                foreach (IRowStream Row in Rows)
                { //教師名稱
                    string StudentNumber = Row.GetValue("學號");
                    //取得學生ID
                    if (StudentDic.ContainsKey(StudentNumber))
                    {
                        string SchoolYear = Row.GetValue("學年度");
                        string Semester = Row.GetValue("學期");

                        string jkey = SchoolYear + "," + Semester + "," + StudentDic[StudentNumber];
                        if (!VolunteerDic.ContainsKey(jkey))
                        {
                            #region 取得志願XML
                            DSXmlHelper dsx = new DSXmlHelper("xml");
                            for (int x = 1; x <= 學生選填志願數; x++)
                            {
                                //依學年度+學期+社團名稱 找到社團
                                string CLUBName = Row.GetValue("志願" + x);
                                if (!string.IsNullOrEmpty(CLUBName))
                                {
                                    string CourseKey = SchoolYear + "," + Semester + "," + CLUBName;

                                    if (ClubDic.ContainsKey(CourseKey))
                                    {
                                        string clubID = ClubDic[CourseKey].UID;
                                        dsx.AddElement("Club");
                                        dsx.SetAttribute("Club", "Index", "" + x);
                                        dsx.SetAttribute("Club", "Ref_Club_ID", clubID);
                                    }
                                }
                            }
                            #endregion

                            //建立Record
                            VolunteerRecord Vol = new VolunteerRecord();
                            Vol.SchoolYear = int.Parse(SchoolYear);
                            Vol.Semester = int.Parse(Semester);
                            Vol.RefStudentID = StudentDic[StudentNumber];
                            Vol.Content = dsx.BaseElement.OuterXml;

                            VolunteerInsertList.Add(Vol);
                        }
                        else
                        {
                            #region 取得志願XML
                            DSXmlHelper dsx = new DSXmlHelper("xml");
                            for (int x = 1; x <= 學生選填志願數; x++)
                            {
                                //依學年度+學期+社團名稱 找到社團
                                string CLUBName = Row.GetValue("志願" + x);
                                if (!string.IsNullOrEmpty(CLUBName))
                                {
                                    string CourseKey = SchoolYear + "," + Semester + "," + CLUBName;

                                    if (ClubDic.ContainsKey(CourseKey))
                                    {
                                        string clubID = ClubDic[CourseKey].UID;
                                        dsx.AddElement("Club");
                                        dsx.SetAttribute("Club", "Index", "" + x);
                                        dsx.SetAttribute("Club", "Ref_Club_ID", clubID);
                                    }
                                }
                            }
                            #endregion

                            VolunteerRecord Vol = VolunteerDic[jkey];
                            Vol.Content = dsx.BaseElement.OuterXml;
                            VolunteerUpdateList.Add(Vol);

                            if (Log_Dic.ContainsKey(jkey))
                            {
                                Log_Dic[jkey].New_Vol = Vol;
                            }
                        }
                    }
                }

                if (VolunteerInsertList.Count > 0)
                {
                    StringBuilder mstrLog1 = new StringBuilder();
                    mstrLog1.AppendLine("新增社團志願序：");
                    foreach (VolunteerRecord each in VolunteerInsertList)
                    {
                        if (StudentLogDic.ContainsKey(each.RefStudentID))
                        {
                            mstrLog1.AppendLine(Importbot.GetLogString(ClubLogDic, each, StudentLogDic[each.RefStudentID]));
                        }
                    }
                    tool._A.InsertValues(VolunteerInsertList);
                    FISCA.LogAgent.ApplicationLog.Log("社團", "新增匯入志願序", mstrLog1.ToString());
                }

                if (VolunteerUpdateList.Count > 0)
                {
                    StringBuilder mstrLog2 = new StringBuilder();
                    mstrLog2.AppendLine("更新社團志願序：");
                    foreach (VolunteerRecord each in VolunteerUpdateList)
                    {
                        string CourseKey = each.SchoolYear + "," + each.Semester + "," + each.RefStudentID;
                        if (Log_Dic.ContainsKey(CourseKey))
                        {
                            if (StudentLogDic.ContainsKey(each.RefStudentID))
                            {
                                string classname = StudentLogDic[each.RefStudentID].Class != null ? StudentLogDic[each.RefStudentID].Class.Name : "";
                                string SeatNo = StudentLogDic[each.RefStudentID].SeatNo.HasValue ? StudentLogDic[each.RefStudentID].SeatNo.Value.ToString() : "";
                                mstrLog2.AppendLine(string.Format("班級「{0}」座號「{1}」姓名「{2}」學號「{3}」", classname, SeatNo, StudentLogDic[each.RefStudentID].Name, StudentLogDic[each.RefStudentID].StudentNumber));
                            }
                            mstrLog2.AppendLine(Importbot.SetLog(Log_Dic[CourseKey]));
                        }
                    }
                    tool._A.UpdateValues(VolunteerUpdateList);
                    FISCA.LogAgent.ApplicationLog.Log("社團", "更新匯入志願序", mstrLog2.ToString());
                }
            }

            return "";
        }

        /// <summary>
        /// 準備資料
        /// </summary>
        public override void Prepare(ImportOption Option)
        {
            mOption = Option;
        }

        public override ImportAction GetSupportActions()
        {
            //新增或更新
            return ImportAction.InsertOrUpdate;
        }
    }
}
