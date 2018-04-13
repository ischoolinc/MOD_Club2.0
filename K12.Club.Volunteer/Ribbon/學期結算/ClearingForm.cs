using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using K12.Data;
using FISCA.UDT;
using FISCA.Data;

namespace K12.Club.Volunteer
{
    public partial class ClearingForm : BaseForm
    {
        /// <summary>
        /// UDT資料取得器
        /// </summary>
        private AccessHelper _AccessHelper { get; set; }

        BackgroundWorker BGW = new BackgroundWorker();

        List<ResultScoreRecord> InsertScoreList { get; set; }

        List<ResultScoreRecord> UPDateScoreList { get; set; }

        //List<ResultScoreRecord> DeleteScoreList { get; set; }

        public ClearingForm()
        {
            InitializeComponent();
        }

        private void ClearingForm_Load(object sender, EventArgs e)
        {
            _AccessHelper = new AccessHelper();

            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!BGW.IsBusy)
            {
                btnStart.Enabled = false;
                this.Text = "學期結算(系統結算中...)";
                BGW.RunWorkerAsync();
            }
            else
            {
                MsgBox.Show("系統忙碌中,稍後再試...");
            }
        }

        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {

            //取得目前選擇課程
            成績取得器 tool = new 成績取得器();
            tool.GetSCJoinByClubIDList(ClubAdmin.Instance.SelectedSource);

            //取得運算比例
            tool.SetWeightProportion();

            //社團ID : 社團幹部obj
            Dictionary<string, 社團幹部obj> CadreDic = new Dictionary<string, 社團幹部obj>();

            #region 處理幹部記錄

            foreach (CLUBRecord each in tool._ClubDic.Values)
            {
                if (!CadreDic.ContainsKey(each.UID))
                {
                    CadreDic.Add(each.UID, new 社團幹部obj(each));
                }
            }
            string qr = string.Join("','", tool._ClubDic.Keys);
            List<CadresRecord> list = _AccessHelper.Select<CadresRecord>("ref_club_id in ('" + qr + "')");
            foreach (CadresRecord cr in list)
            {
                if (!CadreDic[cr.RefClubID]._Cadre1.ContainsKey(cr.RefStudentID))
                {
                    CadreDic[cr.RefClubID]._Cadre1.Add(cr.RefStudentID, cr.CadreName);
                }
                else
                {
                    CadreDic[cr.RefClubID]._Cadre1[cr.RefStudentID] += "," + cr.CadreName;
                }
            }

            #endregion

            #region 結算回學務幹部
            List<string> dataList = new List<string>();
            foreach (社團幹部obj cadre in CadreDic.Values)
            {
                string clubID = cadre._Club.UID; 
                string clubName = cadre._Club.ClubName; 
                int _schoolYear = cadre._Club.SchoolYear;
                int _semester = cadre._Club.Semester;
                
                foreach (string _cadre in cadre._Cadre1.Keys) // StudentID，CadreName
                {
                    string studentID = _cadre; 
                    string cadreName = cadre._Cadre1[_cadre];

                    string data = string.Format(@"
SELECT
		'{0}'::TEXT AS ref_student_id
		, '{1}'::TEXT AS schoolyear
		, '{2}'::TEXT AS semester
        , '{3}'::TEXT AS cadre_name
		, '社團幹部'::TEXT AS referencetype
		, '{4}'::TEXT AS text -- 放社團名稱
                    ", studentID , _schoolYear , _semester, cadreName , clubName);

                    dataList.Add(data);
                }
            }
            string dataRow = string.Join("UNION ALL",dataList);
            #region SQL
            // 判斷條件: 學生ID、學年度、學期、社團名稱
            string sql = string.Format(@"
WITH data_row AS(
	{0}
) , run_update AS(
	UPDATE
		$behavior.thecadre 
	SET 
		cadrename = data_row.cadre_name
		, text = data_row.text
	FROM
		data_row
	WHERE
		$behavior.thecadre.referencetype = '社團幹部'
		AND data_row.ref_student_id = $behavior.thecadre.studentid
		AND data_row.schoolyear = $behavior.thecadre.schoolyear
		AND data_row.semester = $behavior.thecadre.semester
		AND data_row.text = $behavior.thecadre.text
	RETURNING $behavior.thecadre.*
) , run_insert AS(
	INSERT INTO $behavior.thecadre(
			studentid
			, schoolyear
			, semester
			, referencetype
			, cadrename
			, text
	)
	SELECT
		data_row.ref_student_id
		, data_row.schoolyear
		, data_row.semester
		, data_row.referencetype
		, data_row.cadre_name
		, data_row.text
	FROM
		data_row
		LEFT OUTER JOIN $behavior.thecadre AS cadre
			ON cadre.studentid = data_row.ref_student_id
			AND cadre.schoolyear = data_row.schoolyear
			AND cadre.semester = data_row.semester
			AND cadre.referencetype = '社團幹部'
			AND cadre.text = data_row.text
	WHERE
		cadre.uid IS  NULL 
	RETURNING $behavior.thecadre.*
) , delete_data AS(
	SELECT
		cadre.uid
	FROM
		$behavior.thecadre AS cadre
		LEFT OUTER JOIN data_row
			ON data_row.ref_student_id = cadre.studentid
			AND data_row.schoolyear = cadre.schoolyear
			AND data_row.semester = cadre.semester
			AND data_row.cadre_name = cadre.cadrename
			AND data_row.referencetype = cadre.referencetype
			AND data_row.text = cadre.text
	WHERE
		data_row.ref_student_id IS NULL
		AND cadre.referencetype = '社團幹部'
		AND cadre.text IS NOT NULL
) 
DELETE 
FROM
	$behavior.thecadre
WHERE
	uid IN (SELECT * FROM delete_data)
                ", dataRow);
            #endregion
            UpdateHelper up = new UpdateHelper();
            up.Execute(sql);

            #endregion


            Dictionary<string, ResultScoreRecord> ResultScoreDic = new Dictionary<string, ResultScoreRecord>();

            List<string> list_2 = new List<string>();
            foreach (List<SCJoin> each in tool._SCJoinDic.Values)
            {
                foreach (SCJoin scj in each)
                {
                    list_2.Add(scj.UID);
                }
            }
            //string uq = string.Join("','", list_2);
            string studentIDs = string.Join("','", tool._StudentDic.Keys);
            List<string> clubNameList = new List<string>();
            string schoolYear = "", semester = "";
            foreach (string clubID in tool._ClubDic.Keys)
            {
                string clubName = tool._ClubDic[clubID].ClubName;
                schoolYear = "" + tool._ClubDic[clubID].SchoolYear;
                semester = "" + tool._ClubDic[clubID].Semester;
                clubNameList.Add("'" + clubName + "'");
            }
            string clubNames = string.Join(",",clubNameList);
            // 201/03/22 羿均 更新: 修改讀取學期結算成績的KEY值為 studentID、clubName、schoolYear、semester
            string condition = string.Format("ref_student_id IN ('{0}') AND school_year = {1} AND semester = {2} AND club_name IN ({3}) ", studentIDs, schoolYear,semester, clubNames);
            List<ResultScoreRecord> ResultList = _AccessHelper.Select<ResultScoreRecord>(condition);
            //List<ResultScoreRecord> ResultList = _AccessHelper.Select<ResultScoreRecord>("ref_scjoin_id in ('" + uq + "')");
            foreach (ResultScoreRecord rsr in ResultList)
            {
                if (!ResultScoreDic.ContainsKey(rsr.RefStudentID))
                {
                    ResultScoreDic.Add(rsr.RefStudentID, rsr);
                }
            }

            UPDateScoreList = new List<ResultScoreRecord>();
            InsertScoreList = new List<ResultScoreRecord>();
            //DeleteScoreList = new List<ResultScoreRecord>();

            //_AccessHelper.DeletedValues(ResultList);

            foreach (List<SCJoin> scjList in tool._SCJoinDic.Values)
            {
                foreach (SCJoin scj in scjList)
                {
                    
                    if (ResultScoreDic.ContainsKey(scj.RefStudentID))
                    {
                        #region 如果有原資料
                        if (tool._StudentDic.ContainsKey(scj.RefStudentID))
                        {
                            //社團
                            CLUBRecord cr = tool._ClubDic[scj.RefClubID];
                            //學生
                            StudentRecord sr = tool._StudentDic[scj.RefStudentID];
                            //原有社團成績記錄
                            ResultScoreRecord update_rsr = ResultScoreDic[scj.RefStudentID];

                            update_rsr.SchoolYear = cr.SchoolYear;
                            update_rsr.Semester = cr.Semester;

                            update_rsr.RefClubID = cr.UID; //社團ID
                            update_rsr.RefStudentID = sr.ID; //學生ID
                            update_rsr.RefSCJoinID = scj.UID; //參與記錄ID

                            update_rsr.ClubName = cr.ClubName;

                            update_rsr.ClubLevel = cr.Level; //社團評等

                            #region 成績
                            if (!string.IsNullOrEmpty(scj.Score))
                            {
                                update_rsr.ResultScore = tool.GetDecimalValue(scj); //成績
                            }
                            else
                            {
                                //當成績已被清空,結算內容也被清空
                                update_rsr.ResultScore = null;
                            }
                            #endregion

                            #region 幹部
                            if (CadreDic.ContainsKey(cr.UID))
                            {
                                if (CadreDic[cr.UID]._Cadre1.ContainsKey(sr.ID))
                                {
                                    update_rsr.CadreName = CadreDic[cr.UID]._Cadre1[sr.ID];
                                }
                                else
                                {
                                    update_rsr.CadreName = "";
                                }
                            }
                            else
                            {
                                update_rsr.CadreName = "";
                            }
                            #endregion

                            UPDateScoreList.Add(update_rsr);
                        }
                        #endregion
                    }
                    else
                    {
                        #region 完全沒有成績記錄
                        if (tool._StudentDic.ContainsKey(scj.RefStudentID))
                        {
                            //社團
                            CLUBRecord cr = tool._ClubDic[scj.RefClubID];
                            //學生
                            StudentRecord sr = tool._StudentDic[scj.RefStudentID];

                            ResultScoreRecord rsr = new ResultScoreRecord();
                            rsr.SchoolYear = cr.SchoolYear;
                            rsr.Semester = cr.Semester;

                            rsr.RefClubID = cr.UID; //社團ID
                            rsr.RefStudentID = sr.ID; //學生ID
                            rsr.RefSCJoinID = scj.UID; //參與記錄ID

                            rsr.ClubName = cr.ClubName;

                            rsr.ClubLevel = cr.Level; //社團評等


                            if (!string.IsNullOrEmpty(scj.Score))
                            {
                                rsr.ResultScore = tool.GetDecimalValue(scj); //成績
                            }

                            #region 幹部
                            if (CadreDic.ContainsKey(cr.UID))
                            {
                                if (CadreDic[cr.UID]._Cadre1.ContainsKey(sr.ID))
                                {
                                    rsr.CadreName = CadreDic[cr.UID]._Cadre1[sr.ID];
                                }
                                else
                                {
                                    rsr.CadreName = "";
                                }
                            }
                            #endregion
                             
                            InsertScoreList.Add(rsr);
                            
                        }
                        #endregion
                    }
                }
            }

            try
            {
                _AccessHelper.InsertValues(InsertScoreList);
                _AccessHelper.UpdateValues(UPDateScoreList);
            }
            catch (Exception ex)
            {
                MsgBox.Show("新增社團成績發生錯誤!!\n" + ex.Message);
                e.Cancel = true;
                return;
            }

            #region 社團成績Log處理
            StringBuilder _sbLog = new StringBuilder();
            _sbLog.AppendLine("已進行社團結算：");
            if (InsertScoreList.Count > 0)
                _sbLog.AppendLine("共新增「" + InsertScoreList.Count + "」筆成績記錄");
            if (UPDateScoreList.Count > 0)
                _sbLog.AppendLine("共更新「" + UPDateScoreList.Count + "」筆成績記錄");
            _sbLog.AppendLine("");
            _sbLog.AppendLine("簡要明細如下：");
            if (InsertScoreList.Count > 0)
            {
                foreach (ResultScoreRecord each in InsertScoreList)
                {
                    if (tool._StudentDic.ContainsKey(each.RefStudentID))
                    {
                        if (string.IsNullOrEmpty(each.CadreName))
                        {
                            StudentRecord sr = tool._StudentDic[each.RefStudentID];
                            string de = each.ResultScore.HasValue ? each.ResultScore.Value.ToString() : "";
                            _sbLog.AppendLine(string.Format("學生新增「{0}」社團成績「{1}」", sr.Name, de));
                        }
                        else
                        {
                            StudentRecord sr = tool._StudentDic[each.RefStudentID];
                            string de = each.ResultScore.HasValue ? each.ResultScore.Value.ToString() : "";
                            _sbLog.AppendLine(string.Format("學生新增「{0}」社團成績「{1}」幹部記錄「{2}」", sr.Name, de, each.CadreName));
                        }
                    }
                }
            }

            if (UPDateScoreList.Count > 0)
            {
                _sbLog.AppendLine("");
                foreach (ResultScoreRecord each in UPDateScoreList)
                {
                    if (tool._StudentDic.ContainsKey(each.RefStudentID))
                    {
                        if (string.IsNullOrEmpty(each.CadreName))
                        {
                            StudentRecord sr = tool._StudentDic[each.RefStudentID];
                            string de = each.ResultScore.HasValue ? each.ResultScore.Value.ToString() : "";
                            _sbLog.AppendLine(string.Format("學生更新「{0}」社團成績「{1}」", sr.Name, de));
                        }
                        else
                        {
                            StudentRecord sr = tool._StudentDic[each.RefStudentID];
                            string de = each.ResultScore.HasValue ? each.ResultScore.Value.ToString() : "";
                            _sbLog.AppendLine(string.Format("學生更新「{0}」社團成績「{1}」幹部記錄「{2}」", sr.Name, de, each.CadreName));
                        }
                    }

                }
            }

            if (InsertScoreList.Count + UPDateScoreList.Count > 0)
            {
                try
                {
                    FISCA.LogAgent.ApplicationLog.Log("社團", "成績結算", _sbLog.ToString());
                }
                catch (Exception ex)
                {
                    MsgBox.Show("上傳Log記錄發生錯誤!!\n" + ex.Message);
                    e.Cancel = true;
                    return;
                }
            }

            #endregion
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Text = "學期結算";
            btnStart.Enabled = true;

            if (e.Cancelled)
            {
                FISCA.Presentation.Controls.MsgBox.Show("結算操作已中止!");
            }
            else
            {
                if (e.Error == null)
                {
                    if (InsertScoreList.Count + UPDateScoreList.Count > 0)
                    {
                        StringBuilder sb_8 = new StringBuilder();
                        sb_8.AppendLine("結算完成!!");
                        if (InsertScoreList.Count != 0)
                            sb_8.AppendLine("新增[" + InsertScoreList.Count + "]筆");
                        if (UPDateScoreList.Count != 0)
                            sb_8.AppendLine("更新[" + UPDateScoreList.Count + "]筆");

                        FISCA.Presentation.Controls.MsgBox.Show(sb_8.ToString());

                        this.Close();
                    }
                    else
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("結算失敗!!無成績記錄可供結算!!");
                    }
                }
                else
                {
                    FISCA.Presentation.Controls.MsgBox.Show("結算發生錯誤!!\n" + e.Error.Message);
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
