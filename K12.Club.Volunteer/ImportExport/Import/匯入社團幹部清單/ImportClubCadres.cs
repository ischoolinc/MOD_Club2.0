using Campus.DocumentValidator;
using Campus.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K12.Club.Volunteer
{
    class ImportClubCadres : ImportWizard
    {
        private ImportOption mOption;
        CLUBImportBOT Importbot = new CLUBImportBOT();

        public override string Import(List<IRowStream> Rows)
        {
            if (mOption.Action == ImportAction.InsertOrUpdate)
            {
                StringBuilder sb_log = new StringBuilder();

                //加入 社長/副社長
                List<CLUBRecord> ClubUpdateList = new List<CLUBRecord>();

                //加入 其他社團幹部
                List<CadresRecord> CadreInsertList = new List<CadresRecord>();

                //取得學號對應ID
                List<string> studentNumber = new List<string>();
                foreach (IRowStream Row in Rows)
                {
                    string StudentNumber = Row.GetValue("學號");
                    if (!studentNumber.Contains(StudentNumber))
                        studentNumber.Add(StudentNumber);
                }

                Importbot.GetStudentIDList(studentNumber);

                foreach (IRowStream Row in Rows)
                {
                    //教師名稱
                    string SchoolYear = Row.GetValue("學年度");
                    string Semester = Row.GetValue("學期");
                    string CLUBName = Row.GetValue("社團名稱");
                    string CadreName = Row.GetValue("幹部名稱");
                    string StudentNumber = Row.GetValue("學號");

                    string name = SchoolYear + "," + Semester + "," + CLUBName;
                    if (Importbot.ClubDic.ContainsKey(name))
                    {
                        CLUBRecord record = Importbot.ClubDic[name];

                        if (Importbot.StudentDic.ContainsKey(StudentNumber))
                        {
                            StudDe stud = Importbot.StudentDic[StudentNumber];

                            if (CadreName == "社長")
                            {
                                if (record.President != stud.id)
                                {
                                    record.President = stud.id;
                                    ClubUpdateList.Add(record);
                                    sb_log.AppendLine(string.Format("學年度「{0}」學期「{1}」社團名稱「{2}」", SchoolYear, Semester, CLUBName));
                                    sb_log.AppendLine(string.Format("班級「{0}」座號「{1}」姓名「{2}」幹部名稱「{3}」", stud.ClassName, stud.seat_no, stud.Name, CadreName));
                                    sb_log.AppendLine("");
                                }
                                else
                                {
                                    sb_log.AppendLine(string.Format("學年度「{0}」學期「{1}」社團名稱「{2}」", SchoolYear, Semester, CLUBName));
                                    sb_log.AppendLine(string.Format("學生「{0}」已是社長(未調整)", stud.Name));
                                    sb_log.AppendLine("");
                                }
                            }
                            else if (CadreName == "副社長")
                            {
                                if (record.VicePresident != stud.id)
                                {
                                    record.VicePresident = stud.id;
                                    ClubUpdateList.Add(record);

                                    sb_log.AppendLine(string.Format("學年度「{0}」學期「{1}」社團名稱「{2}」", SchoolYear, Semester, CLUBName));
                                    sb_log.AppendLine(string.Format("班級「{0}」座號「{1}」姓名「{2}」幹部名稱「{3}」", stud.ClassName, stud.seat_no, stud.Name, CadreName));
                                    sb_log.AppendLine("");
                                }
                                else
                                {
                                    sb_log.AppendLine(string.Format("學年度「{0}」學期「{1}」社團名稱「{2}」", SchoolYear, Semester, CLUBName));
                                    sb_log.AppendLine(string.Format("學生「{0}」已是副社長(未調整)", stud.Name));
                                    sb_log.AppendLine("");
                                }
                            }
                            else
                            {
                                //其他幹部直接新增
                                CadresRecord cadres = new CadresRecord();
                                cadres.CadreName = CadreName;
                                cadres.RefClubID = record.UID;
                                cadres.RefStudentID = stud.id;

                                CadreInsertList.Add(cadres);

                                sb_log.AppendLine(string.Format("學年度「{0}」學期「{1}」社團名稱「{2}」", SchoolYear, Semester, CLUBName));
                                sb_log.AppendLine(string.Format("班級「{0}」座號「{1}」姓名「{2}」幹部名稱「{3}」", stud.ClassName, stud.seat_no, stud.Name, CadreName));
                                sb_log.AppendLine("");
                            }
                        }
                    }
                }

                if (ClubUpdateList.Count > 0)
                {
                    tool._A.UpdateValues(ClubUpdateList);
                }

                if (CadreInsertList.Count > 0)
                {
                    tool._A.InsertValues(CadreInsertList);
                }

                if (ClubUpdateList.Count > 0 || CadreInsertList.Count > 0)
                    FISCA.LogAgent.ApplicationLog.Log("社團", "匯入社團幹部", sb_log.ToString());
            }

            return "";
        }

        public override string GetValidateRule()
        {
            return Properties.Resources.ImportClubCadresValRule;
        }

        public override ImportAction GetSupportActions()
        {
            //新增或更新
            return ImportAction.InsertOrUpdate;
        }

        public override void Prepare(ImportOption Option)
        {
            mOption = Option;
            //取得社團基本資料
            Importbot.ClubDic = Importbot.GetCLUBDic();

            Importbot.CadreIDDic = Importbot.GetCadreDic();

        }
    }
}
