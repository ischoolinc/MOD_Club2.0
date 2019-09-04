using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.DocumentValidator;
using Campus.Import;
using FISCA.Data;
using System.Data;
using Campus.Validator;
using FISCA.UDT;
using FISCA.Presentation.Controls;
using FISCA.Presentation;
using System.Windows.Forms;
using System.ComponentModel;

namespace K12.Club.Volunteer
{
    class ImportSCJoinData : ImportWizard
    {
        private ImportOption _Option;
        private BackgroundWorker BGW = new BackgroundWorker();
        private QueryHelper _qh = new QueryHelper();
        private AccessHelper _access = new AccessHelper();

        private Dictionary<string, SCJoin> _dicSCJoinByKey = new Dictionary<string, SCJoin>();
        private Dictionary<string, string> _dicClubIDByKey = new Dictionary<string, string>();
        private Dictionary<string, string> _dicStudentIDByStudentNumber = new Dictionary<string, string>();
        private Dictionary<string, CLUBRecord> _dicClubDataByID = new Dictionary<string, CLUBRecord>();
        private Dictionary<string, string> _dicStudentNameByStudentNumber = new Dictionary<string, string>();

        List<string> _listInsertLog = new List<string>();
        List<string> _listUpdateLog = new List<string>();

        public ImportSCJoinData()
        {
            this.CustomValidate = (Rows, Messages) =>
            {
                CustomValidator(Rows, Messages);
            };

            #region 取得社團學生參與紀錄
            {
                List<SCJoin> listSCJoin = this._access.Select<SCJoin>();

                //2019/9/4 - 增加判斷,學號不為空值
                string sql = @"
SELECT
    scj.uid
    , scj.ref_club_id
    , scj.ref_student_id
    , club.school_year
    , club.semester
    , club.club_name
    , student.student_number
FROM
    $k12.scjoin.universal AS scj
    LEFT OUTER JOIN $k12.clubrecord.universal AS club
        ON club.uid = scj.ref_club_id::BIGINT
    LEFT OUTER JOIN student 
        ON student.id = scj.ref_student_id::BIGINT
where student.student_number is not null
";
                DataTable dt = this._qh.Select(sql);

                foreach (DataRow row in dt.Rows)
                {
                    // 社團學生參與紀錄整理
                    SCJoin scj = listSCJoin.Where(x => x.UID == "" + row["uid"]).First();
                    string key = string.Format("{0}_{1}_{2}_{3}", "" + row["school_year"], "" + row["semester"], "" + row["club_name"], "" + row["student_number"]);

                    //2019/9/4 - Dylan修正增加判斷,避免加入重複資料
                    //(tip:為何資料會重複,也是要確認的問題)
                    if (!_dicSCJoinByKey.ContainsKey(key))
                    {
                        this._dicSCJoinByKey.Add(key, scj);
                    }

                    // 學生編號整理

                }
            }
            #endregion

            #region 取得社團資料
            {
                List<CLUBRecord> listClubRecord = this._access.Select<CLUBRecord>();
                foreach (CLUBRecord cr in listClubRecord)
                {
                    // 社團編號整理
                    string clubKey = string.Format("{0}_{1}_{2}", cr.SchoolYear, cr.Semester, cr.ClubName);

                    //2019/9/4 - Dylan修正增加判斷,避免加入重複資料
                    if (!_dicClubIDByKey.ContainsKey(clubKey))
                    {
                        this._dicClubIDByKey.Add(clubKey, cr.UID);
                    }

                    if (!_dicClubDataByID.ContainsKey(cr.UID))
                    {
                        this._dicClubDataByID.Add(cr.UID, cr);
                    }
                }
            }
            #endregion

            #region 取得學生資料
            {
                string sql = @"
SELECT
    id
    , name
    , student_number
FROM
    student
WHERE
    status IN(1,2)
    AND student_number IS NOT NULL
";
                DataTable dt = this._qh.Select(sql);
                foreach (DataRow row in dt.Rows)
                {
                    //2019/9/4 - Dylan修正增加判斷,避免加入重複資料
                    string student_number = "" + row["student_number"];
                    if (!_dicStudentIDByStudentNumber.ContainsKey(student_number))
                    {
                        this._dicStudentIDByStudentNumber.Add(student_number, "" + row["id"]);
                    }

                    if (!_dicStudentNameByStudentNumber.ContainsKey(student_number))
                    {
                        this._dicStudentNameByStudentNumber.Add(student_number, "" + row["name"]);
                    }
                }
            }
            #endregion

        }

        /// <summary>
        /// 設定匯入的行為
        /// </summary>
        /// <returns></returns>
        public override ImportAction GetSupportActions()
        {
            return ImportAction.InsertOrUpdate;
        }

        /// <summary>
        /// 取得XML設定檔
        /// </summary>
        /// <returns></returns>
        public override string GetValidateRule()
        {
            return Properties.Resources.SCJoinValidate;
        }

        /// <summary>
        /// 客製驗證規則
        /// </summary>
        /// <param name="Rows"></param>
        /// <param name="Messages"></param>
        private void CustomValidator(List<IRowStream> Rows, RowMessages Messages)
        {
            // 取得資料庫社團資料
            string sql = @"
SELECT
    uid
    , school_year
    , semester
    , club_name
FROM
    $k12.clubrecord.universal
";

            DataTable dt = this._qh.Select(sql);
            IEnumerable<DataRow> clubs = dt.Rows.Cast<DataRow>();

            // 資料檢查
            Rows.ForEach((x) =>
            {
                string schoolYear = x.GetValue("學年度").Trim();
                string semester = x.GetValue("學期").Trim();
                string clubName = x.GetValue("社團名稱").Trim();

                if (clubs.Where(y => (y["school_year"] + "").Trim() == schoolYear).Where(y => (y["semester"] + "").Trim() == semester).Where(y => (y["club_name"] + "").Trim() == clubName).Count() == 0)
                {
                    Messages[x.Position].MessageItems.Add(new MessageItem(Campus.Validator.ErrorType.Error, Campus.Validator.ValidatorType.Row, "此學年度、學期、社團名稱不存在系統。"));
                }
            });
        }

        public override string Import(List<IRowStream> Rows)
        {
            List<SCJoin> listInsertSCJoin = new List<SCJoin>();
            List<SCJoin> listUpdateSCJoin = new List<SCJoin>();

            if (this._Option.Action == ImportAction.InsertOrUpdate)
            {
                // 資料整理
                parseData(Rows, listInsertSCJoin, listUpdateSCJoin);

                // 資料存入資料庫
                saveData(listInsertSCJoin, listUpdateSCJoin);

                //ClubEvents.RaiseAssnChanged();
            }
            return "";
        }

        private void parseData(List<IRowStream> Rows, List<SCJoin> listInsertSCJoin, List<SCJoin> listUpdateSCJoin)
        {
            // 每一筆社團參於學生資料
            foreach (IRowStream row in Rows)
            {
                // 確認社團參與學生資料是新增或修改
                string schoolYear = row.GetValue("學年度");
                string semester = row.GetValue("學期");
                string clubName = row.GetValue("社團名稱");
                string studentNumber = row.GetValue("學號");
                string studentName = this._dicStudentNameByStudentNumber[studentNumber];
                string key = string.Format("{0}_{1}_{2}_{3}", schoolYear, semester, clubName, studentNumber);
                string clubKey = string.Format("{0}_{1}_{2}", schoolYear, semester, clubName);
                string newClubName = this._dicClubDataByID[this._dicClubIDByKey[clubKey]].ClubName;

                if (this._dicSCJoinByKey.ContainsKey(key)) // 更新
                {
                    SCJoin scj = this._dicSCJoinByKey[key];

                    // log資料
                    string originClubName = this._dicClubDataByID[scj.RefClubID].ClubName;
                    string log = string.Format("「{0}」學生「{1}」學年度「{2}」學期原參與社團「{3}」變更為「{4}」", studentName, schoolYear, semester, originClubName, newClubName);
                    this._listUpdateLog.Add(log);

                    // 更新資料
                    scj.RefClubID = this._dicClubIDByKey[clubKey];
                    scj.RefStudentID = this._dicStudentIDByStudentNumber[studentNumber];
                    listUpdateSCJoin.Add(scj);
                }
                else // 新增
                {
                    // log資料
                    string log = string.Format("「{0}」學生「{1}」學年度「{2}」學期參與社團「{3}」", studentName, schoolYear, semester, newClubName);
                    this._listInsertLog.Add(log);

                    // 新增資料
                    SCJoin scj = new SCJoin();
                    scj.RefClubID = this._dicClubIDByKey[clubKey];
                    scj.RefStudentID = this._dicStudentIDByStudentNumber[studentNumber];
                    listInsertSCJoin.Add(scj);
                }
            }
        }

        private void saveData(List<SCJoin> listInsertSCJoin, List<SCJoin> listUpdateSCJoin)
        {
            if (listInsertSCJoin.Count() > 0)
            {
                try
                {
                    this._access.InsertValues(listInsertSCJoin);

                    StringBuilder logs = new StringBuilder();
                    logs.AppendLine("新增匯入社團參與學生: ");
                    foreach (string log in this._listInsertLog)
                    {
                        logs.AppendLine(log);
                    }
                    FISCA.LogAgent.ApplicationLog.Log("社團", "新增匯入", logs.ToString());
                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }
            if (listUpdateSCJoin.Count() > 0)
            {
                try
                {
                    this._access.UpdateValues(listUpdateSCJoin);

                    StringBuilder logs = new StringBuilder();
                    logs.AppendLine("更新匯入社團參與學生: ");
                    foreach (string log in this._listUpdateLog)
                    {
                        logs.AppendLine(log);
                    }
                    FISCA.LogAgent.ApplicationLog.Log("社團", "更新匯入", logs.ToString());
                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }
        }

        private string GetLogString(SCJoin scj)
        {
            StringBuilder log = new StringBuilder();

            log.AppendLine(string.Format("社團編號「{0}」學生編號「{1}」", scj.RefClubID, scj.RefStudentID));

            return "";
        }

        /// <summary>
        /// 使用者選擇匯入模式
        /// </summary>
        /// <param name="Option"></param>
        public override void Prepare(ImportOption Option)
        {
            this._Option = Option;
        }
    }
}
