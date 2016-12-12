using Campus.DocumentValidator;
using Campus.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.General.ImportClubScore.ImportExport.Import.匯入社團學期成績
{
    public class FrmImportClubScore : ImportWizard
    {
        // 設定檔
        private ImportOption _Option;

        private LogHelper _LogHelper;

        // 新增
        private List<DAO.ResultScoreRecord> _InsertRecList;
        // 更新
        private List<DAO.ResultScoreRecord> _UpdateRecList;

        public override ImportAction GetSupportActions()
        {
            //新增或更新
            return ImportAction.InsertOrUpdate;
        }

        public override string GetValidateRule()
        {
            return Properties.Resources.ImportCLUBScoreRule;
        }

        public override void Prepare(ImportOption Option)
        {
            _Option = Option;
            _InsertRecList = new List<DAO.ResultScoreRecord>();
            _UpdateRecList = new List<DAO.ResultScoreRecord>();
            _LogHelper = new LogHelper();
        }

        public override string Import(List<Campus.DocumentValidator.IRowStream> Rows)
        {
            if (_Option.Action == ImportAction.InsertOrUpdate)
            {
                _InsertRecList.Clear();
                _UpdateRecList.Clear();

                // Key: 系統ID, Value: 學號
                Dictionary<string, string> studentIdDic = new Dictionary<string,string>();

                // 取得學生的系統ID
                foreach (IRowStream row in Rows)
                {
                    string studentNumber = Utility.GetIRowValueString(row, Global._ColStudentNumber);

                    if (string.IsNullOrEmpty(studentNumber)) continue;

                    if (Global._AllStudentNumberIDTemp.ContainsKey(studentNumber))
                    {
                        string studentId = Global._AllStudentNumberIDTemp[studentNumber];
                    
                        if (!studentIdDic.ContainsKey(studentId))
                            studentIdDic.Add(studentId, studentNumber);
                    }
                }

                // 透過學生ID取得社團分數
                List<DAO.ResultScoreRecord> clubScoreList = DAO.FDQuery.GetClubScoreByStudentIdList(studentIdDic.Keys.ToList());

                int totalCount = 0;
                #region 處理每一筆資料是新增或更新
                
                // 判斷每一筆資料是要新增還是更新
                foreach (IRowStream row in Rows)
                {
                    totalCount++;
                    this.ImportProgress = totalCount;
                    if (Utility.CheckRowData(row) == false) continue;

                    DAO.ResultScoreRecord rec = new DAO.ResultScoreRecord();

                    string studentId = "";
                    string studentNumber = Utility.GetIRowValueString(row, Global._ColStudentNumber);
                    int schoolYear = Utility.GetIRowValueInt(row, Global._ColScholYear);
                    int semester = Utility.GetIRowValueInt(row, Global._ColSemester);
                    string clubName = Utility.GetIRowValueString(row, Global._ColClubName);
                    decimal? clubScore = Utility.GetIRowValueDecimal(row, Global._ColClubScore);
                    string cadreName = Utility.GetIRowValueString(row, Global._ColCadreName);
                    string clubLevel = Utility.GetIRowValueString(row, Global._ColClubLevel);

                    // 透過學號換成學生ID
                    if (Global._AllStudentNumberIDTemp.ContainsKey(studentNumber))
                    {
                        studentId = Global._AllStudentNumberIDTemp[studentNumber];
                    }
                    else
                    {
                        // 如果無法取得學生ID, 就換到下一筆
                        continue;
                    }

                    // 判斷此筆資料是否已在DB
                    rec = Utility.GetClubScoreRecord(clubScoreList, studentId, schoolYear, semester, clubName);

                    // 新增資料
                    if (rec == null)
                    {
                        rec = new DAO.ResultScoreRecord();

                        #region 處理新增資料
                        // 學生系統ID
                        rec.RefStudentID = studentId;
                        // 社團參考
                        rec.RefClubID = "";
                        // 社團參與記錄參考
                        rec.RefSCJoinID = "";
                        // 學年度
                        rec.SchoolYear = schoolYear;
                        // 學期
                        rec.Semester = semester;
                        // 社團名稱
                        rec.ClubName = clubName;
                        // 社團學期成績
                        rec.ResultScore = clubScore;
                        // 社團幹部
                        rec.CadreName = cadreName;
                        //社團評等
                        rec.ClubLevel = clubLevel;

                        #endregion

                        // 加入準備新增的列表
                        _InsertRecList.Add(rec);
                    }
                    // 更新資料
                    else
                    {
                        // 先儲存原本的資料
                        _LogHelper.SaveOldRecForLog(rec);

                        #region 處理更新資料
                        // 學生系統ID, 社團參考, 社團參與記錄參考, 學年度, 學期, 社團名稱 無法更新

                        // 社團學期成績
                        if (_Option.SelectedFields.Contains(Global._ColClubScore))
                            rec.ResultScore = clubScore;
                    
                        // 社團幹部
                        if (_Option.SelectedFields.Contains(Global._ColCadreName))
                            rec.CadreName = cadreName;

                        if (_Option.SelectedFields.Contains(Global._ColClubLevel))
                            rec.ClubLevel = clubLevel;

                        #endregion

                        // 儲存更新後的資料
                        _LogHelper.SaveNewRecForLog(rec);

                        // 加入準備更新的列表
                        _UpdateRecList.Add(rec);
                    }

                }   // end of 判斷每一筆資料是要新增還是更新

                #endregion

                #region 新增或修改DB資料
                // 執行新增
                if (_InsertRecList.Count > 0)
                {
                    Global._A.InsertValues(_InsertRecList);

                    #region 處理Log
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("新增匯入社團學期成績：");
                    foreach ( DAO.ResultScoreRecord rec in _InsertRecList)
                    {
                        string studentNumber = "";
                        if (studentIdDic.ContainsKey(rec.RefStudentID))
                            studentNumber = studentIdDic[rec.RefStudentID];

                        sb.AppendLine(_LogHelper.ComposeInsertLogString(rec, studentNumber));
                    }

                    FISCA.LogAgent.ApplicationLog.Log("學生.社團學期成績-匯入", "新增匯入", sb.ToString());
                    #endregion
                }
                // 執行更新
                if (_UpdateRecList.Count > 0)
                {
                    Global._A.UpdateValues(_UpdateRecList);

                    #region 處理Log
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("更新匯入社團學期成績：");
                    foreach (DAO.ResultScoreRecord rec in _UpdateRecList)
                    {
                        string studentNumber = "";
                        if (studentIdDic.ContainsKey(rec.RefStudentID))
                            studentNumber = studentIdDic[rec.RefStudentID];
                        if (_LogHelper.ClubScorePairDic.ContainsKey(rec.UID))
                            sb.AppendLine(_LogHelper.ComposeUpdateLogString(_LogHelper.ClubScorePairDic[rec.UID], studentNumber));
                    }

                    FISCA.LogAgent.ApplicationLog.Log("學生.社團學期成績-匯入", "更新匯入", sb.ToString());
                    #endregion
                }
                #endregion

                // TODO ClubEvents.RaiseAssnChanged();

            }   // end of if (_Option.Action == ImportAction.InsertOrUpdate)

            return "";
        }

    }
}
