using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using FISCA.Data;
using FISCA.UDT;
using K12.Data;

namespace K12.Club.Volunteer
{
    public partial class CopyClub : BaseForm
    {
        BackgroundWorker BGW = new BackgroundWorker();

        //不複製欄位：社員 / 社長 / 副社長

        AccessHelper _AccessHelper = new AccessHelper();
        QueryHelper _QueryHelper = new QueryHelper();

        bool _CopyOtherStudent = false;
        bool _CopyCadresStudent = false;
        bool _CopyPresidentStudent = false;

        int _SchoolYear = 90;
        int _Semester = 1;
        bool IsNowLock = true;

        /// <summary>
        /// 所要拷貝的社團
        /// </summary>
        List<CLUBRecord> _CopyList = new List<CLUBRecord>();
        /// <summary>
        /// 所要複製的社團
        /// </summary>
        List<CLUBRecord> _NewInsertList = new List<CLUBRecord>();

        List<SCJoin> _InsertSCJList = new List<SCJoin>();

        List<string> _SkipList = new List<string>();

        List<CadresRecord> _InsertCadreList = new List<CadresRecord>();

        public CopyClub()
        {
            InitializeComponent();
        }

        private void CopyClub_Load(object sender, EventArgs e)
        {
            if (!CheckSelectClubIsIdentical())
                MsgBox.Show("欲複製之社團,不可來自不同學年度/學期");

            SetNextSchoolYear();

            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);
        }

        /// <summary>
        /// 設定學年度學期呈現資訊
        /// </summary>
        private void SetNextSchoolYear()
        {
            string TitleText = "所選擇的社團為　" + _SchoolYear + "學年度　第" + _Semester + "學期";
            lbHelp2.Text = TitleText;

            if (_Semester == 1)
            {
                _Semester++;
            }
            else
            {
                _SchoolYear++;
                _Semester--;
            }

            intSchoolYear.Value = _SchoolYear;
            intSemester.Value = _Semester;
        }

        /// <summary>
        /// 檢查所選社團是否為相同學年度/學期
        /// </summary>
        private bool CheckSelectClubIsIdentical()
        {
            bool IsTrue = true;
            string stringUDT = string.Join("','", ClubAdmin.Instance.SelectedSource);
            _CopyList = _AccessHelper.Select<CLUBRecord>("uid in ('" + stringUDT + "')");
            if (_CopyList.Count > 0)
            {
                _SchoolYear = _CopyList[0].SchoolYear;
                _Semester = _CopyList[0].Semester;

                foreach (CLUBRecord each in _CopyList)
                {
                    if (_SchoolYear == each.SchoolYear && _Semester == each.Semester)
                        continue;

                    IsTrue = false;
                }
            }
            return IsTrue;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!BGW.IsBusy)
            {
                btnStart.Enabled = false;
                intSchoolYear.Enabled = false;
                intSemester.Enabled = false;

                _CopyOtherStudent = checkBoxX1.Checked;
                _CopyCadresStudent = checkBoxX2.Checked;
                _CopyPresidentStudent = checkBoxX3.Checked;

                _SchoolYear = intSchoolYear.Value;
                _Semester = intSemester.Value;

                IsNowLock = checkBoxX4.Checked;

                BGW.RunWorkerAsync();
            }
            else
            {
                MsgBox.Show("忙碌中請稍後再試...");
            }
        }

        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            //檢查目標學年期,是否有相同名稱之社團資料
            //如果有則移除該社團
            //並透過覆製功能進行新增動作
            _SkipList = GetExistClub();

            var copyClubList = new List<CLUBRecord>();
            var copyClubIDList = new List<string>();


            foreach (var item in _CopyList)
            {
                //如果是重覆社團則不處理
                if (!_SkipList.Contains(item.ClubName))
                {
                    copyClubList.Add(item);
                    copyClubIDList.Add(item.UID);
                }
            }

            if (copyClubList.Count == 0) return;

            int LogSchoolYear = 90;
            int LogSemester = 1;

            if (copyClubList.Count > 0)
            {
                LogSchoolYear = copyClubList[0].SchoolYear;
                LogSemester = copyClubList[0].Semester;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("已進行複製社團作業：");
            sb.AppendLine(string.Format("由學年度「{0}」學期「{1}」複製至學年度「{2}」學期「{3}」", LogSchoolYear.ToString(), LogSemester.ToString(), _SchoolYear.ToString(), _Semester.ToString()));
            if (_CopyOtherStudent)
            {
                sb.AppendLine("(已勾選複製學生選項)");
            }
            sb.AppendLine("");
            sb.AppendLine("新學期社團清單如下：");

            foreach (CLUBRecord each in copyClubList)
            {
                #region 複製社團基本資料

                //如果是重覆社團則不處理
                if (_SkipList.Contains(each.ClubName))
                    continue;

                CLUBRecord cr = new CLUBRecord();
                cr.About = each.About;
                cr.ClubNumber = each.ClubNumber;
                cr.ClubCategory = each.ClubCategory;
                cr.ClubName = each.ClubName;
                cr.DeptRestrict = each.DeptRestrict;
                cr.GenderRestrict = each.GenderRestrict;
                cr.Grade1Limit = each.Grade1Limit;
                cr.Grade2Limit = each.Grade2Limit;
                cr.Grade3Limit = each.Grade3Limit;
                cr.Limit = each.Limit;
                cr.Location = each.Location;
                cr.Photo1 = each.Photo1;
                cr.Photo2 = each.Photo2;
                cr.RefTeacherID = each.RefTeacherID;
                cr.RefTeacherID2 = each.RefTeacherID2;
                cr.RefTeacherID3 = each.RefTeacherID3;
                cr.Level = each.Level;

                if (_CopyPresidentStudent)
                {
                    //社長,副社長
                    cr.President = each.President;
                    cr.VicePresident = each.VicePresident;
                }

                //使用者所設定之學年度學期
                cr.SchoolYear = _SchoolYear;
                cr.Semester = _Semester;
                _NewInsertList.Add(cr);

                sb.AppendLine(string.Format("學年度「{0}」學期「{1}」社團名稱「{2}」", cr.SchoolYear.ToString(), cr.Semester.ToString(), cr.ClubName));


                #endregion

            }

            //新增之社團ID
            List<string> newIDList = new List<string>();

            try
            {
                newIDList = _AccessHelper.InsertValues(_NewInsertList);
            }
            catch (Exception ex)
            {
                SmartSchool.ErrorReporting.ReportingService.ReportException(ex);
                e.Cancel = true;
                return;
            }

            FISCA.LogAgent.ApplicationLog.Log("社團", "複製社團", sb.ToString());

            if (_CopyOtherStudent || _CopyCadresStudent || _CopyPresidentStudent)
            {
                #region 複製社團學生

                //取得社團
                List<CLUBRecord> newClubRecord = _AccessHelper.Select<CLUBRecord>(string.Format("UID in('{0}')", string.Join("','", newIDList)));

                Dictionary<string, List<CadresRecord>> studentCadreDic = new Dictionary<string, List<CadresRecord>>();

                Dictionary<string, List<SCJoin>> studentSCJDic = new Dictionary<string, List<SCJoin>>();

                //取得原有社團之學生社團記錄
                List<SCJoin> scjStudentList = _AccessHelper.Select<SCJoin>(string.Format("ref_club_id in ('{0}')", string.Join("','", copyClubIDList)));

                foreach (SCJoin each in scjStudentList)
                {
                    if (!studentSCJDic.ContainsKey(each.RefClubID))
                    {
                        studentSCJDic.Add(each.RefClubID, new List<SCJoin>());
                    }
                    studentSCJDic[each.RefClubID].Add(each);
                }

                //取得原有社團之學生幹部記錄
                List<CadresRecord> cadreStudentList = _AccessHelper.Select<CadresRecord>(string.Format("ref_club_id in ('{0}')", string.Join("','", copyClubIDList)));

                foreach (CadresRecord each in cadreStudentList)
                {
                    if (!studentCadreDic.ContainsKey(each.RefClubID))
                    {
                        studentCadreDic.Add(each.RefClubID, new List<CadresRecord>());
                    }
                    studentCadreDic[each.RefClubID].Add(each);
                }

                _InsertSCJList = new List<SCJoin>();
                List<CadresRecord> newCadresRecordList = new List<CadresRecord>();

                foreach (CLUBRecord newClubRec in newClubRecord)
                {
                    foreach (CLUBRecord copyClubRec in copyClubList)
                    {
                        if (newClubRec.ClubName == copyClubRec.ClubName)
                        {
                            if (studentSCJDic.ContainsKey(copyClubRec.UID))
                            {
                                foreach (var scjRec in studentSCJDic[copyClubRec.UID])
                                {
                                    if (_CopyPresidentStudent
                                        && (copyClubRec.President == scjRec.RefStudentID
                                        || copyClubRec.VicePresident == scjRec.RefStudentID))
                                    {
                                        #region 複製社長副社長
                                        SCJoin scj = new SCJoin();
                                        scj.RefStudentID = scjRec.RefStudentID;
                                        scj.RefClubID = newClubRec.UID;

                                        //鎖定狀態 - 2021/4/15(鎖定狀態,依原鎖定狀態)
                                        if (IsNowLock)
                                            scj.Lock = scjRec.Lock;
                                        else
                                            scj.Lock = true;

                                        _InsertSCJList.Add(scj);
                                        #endregion
                                        continue;
                                    }
                                    if (_CopyCadresStudent
                                        && studentCadreDic.ContainsKey(copyClubRec.UID))
                                    {
                                        bool match = false;
                                        #region 複製社團幹部
                                        foreach (var cadresRec in studentCadreDic[copyClubRec.UID])
                                        {
                                            if (cadresRec.RefStudentID == scjRec.RefStudentID)
                                            {
                                                CadresRecord cadre = new CadresRecord();
                                                cadre.RefStudentID = cadresRec.RefStudentID; //學生
                                                cadre.CadreName = cadresRec.CadreName; //幹部名稱
                                                cadre.RefClubID = newClubRec.UID;
                                                newCadresRecordList.Add(cadre);

                                                SCJoin scj = new SCJoin();
                                                scj.RefStudentID = scjRec.RefStudentID;
                                                scj.RefClubID = newClubRec.UID;

                                                //鎖定狀態 - 2021/4/15(鎖定狀態,依原鎖定狀態)
                                                if (IsNowLock)
                                                    scj.Lock = scjRec.Lock;
                                                else
                                                    scj.Lock = true;

                                                _InsertSCJList.Add(scj);
                                                match = true;
                                                break;
                                            }
                                        }
                                        #endregion
                                        if (match)
                                            continue;
                                    }

                                    if (_CopyOtherStudent)
                                    {
                                        #region 複製一般社員
                                        SCJoin scj = new SCJoin();
                                        scj.RefStudentID = scjRec.RefStudentID;
                                        scj.RefClubID = newClubRec.UID;

                                        //鎖定狀態 - 2021/4/15(鎖定狀態,依原鎖定狀態)
                                        if (IsNowLock)
                                            scj.Lock = scjRec.Lock;
                                        else
                                            scj.Lock = true;

                                        _InsertSCJList.Add(scj);
                                        #endregion
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }


                try
                {
                    _AccessHelper.InsertValues(_InsertSCJList);
                    _AccessHelper.InsertValues(newCadresRecordList);
                }
                catch (Exception ex)
                {
                    SmartSchool.ErrorReporting.ReportingService.ReportException(ex);
                    e.Cancel = true;
                    return;
                }

                #endregion
            }
        }

        private List<string> GetExistClub()
        {
            List<string> existList = new List<string>();
            List<CLUBRecord> list = _AccessHelper.Select<CLUBRecord>(string.Format("school_year={0} and semester={1}", _SchoolYear, _Semester));

            foreach (CLUBRecord each1 in _CopyList)
            {
                foreach (CLUBRecord each2 in list)
                {
                    if (each1.ClubName == each2.ClubName)
                    {
                        existList.Add(each2.ClubName);
                        break;
                    }
                }
            }
            return existList;
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MsgBox.Show("複製社團操作已中止!!");
                return;
            }

            if (e.Error != null)
            {
                MsgBox.Show("複製社團失敗!\n" + e.Error.Message);
                SmartSchool.ErrorReporting.ReportingService.ReportException(e.Error);
                return;
            }

            if (_SkipList.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("共" + _NewInsertList.Count + "個社團複製成功!!\n");
                sb.AppendLine("共" + _SkipList.Count + "個重覆社團,已略過處理!!");
                if (_CopyOtherStudent)
                {
                    sb.AppendLine("已同步建立" + _InsertSCJList.Count + "名學生的社團參與記錄!!");
                }

                if (_CopyCadresStudent)
                {
                    sb.AppendLine("已同步建立學生的社團幹部記錄!!");
                }
                if (_CopyPresidentStudent)
                {

                    sb.AppendLine("已同步複製社長、副社長社團參與紀錄");
                }

                sb.AppendLine(string.Join(",", _SkipList));

                MsgBox.Show(sb.ToString());
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("共" + _NewInsertList.Count + "個社團複製成功!!\n");
                if (_CopyOtherStudent)
                {
                    sb.AppendLine("已同步建立" + _InsertSCJList.Count + "名學生的社團參與記錄!!");
                }

                if (_CopyCadresStudent)
                {
                    sb.AppendLine("已同步建立學生的社團幹部記錄!!");
                }
                if (_CopyPresidentStudent)
                {

                    sb.AppendLine("已同步複製社長、副社長社團參與紀錄");
                }

                MsgBox.Show(sb.ToString());
            }

            ClubEvents.RaiseAssnChanged();
            this.Close();

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBoxX1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxX1.Checked == true)
            {
                checkBoxX2.Checked = true;
            }
        }

        private void checkBoxX2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxX2.Checked == true)
            {
                checkBoxX3.Checked = true;
            }
        }
    }
}
