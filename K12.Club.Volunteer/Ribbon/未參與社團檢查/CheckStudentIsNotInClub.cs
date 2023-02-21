using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using FISCA.UDT;
using FISCA.Data;
using K12.Data;
using DevComponents.DotNetBar;

namespace K12.Club.Volunteer
{
    public partial class CheckStudentIsNotInClub : BaseForm
    {
        /// <summary>
        /// 開啟畫面即檢查學生
        /// 取得本學期的社團資料
        /// </summary>
        BackgroundWorker BGW = new BackgroundWorker();

        /// <summary>
        /// 儲存學生的社團分配記錄
        /// </summary>
        BackgroundWorker BGWSave = new BackgroundWorker();

        //UDT物件
        private AccessHelper _AccessHelper = new AccessHelper();
        private QueryHelper _QueryHelper = new QueryHelper();

        List<CLUBRecord> CLUBRecordList = new List<CLUBRecord>();

        StringBuilder sb_log = new StringBuilder();
        Random xy01 = new Random();
        /// <summary>
        /// 已選社團學生
        /// 學生ID & 修課紀錄
        /// </summary>
        Dictionary<string, SCJoin> StudentScjoinDic;

        /// <summary>
        /// 社團 & 社團修課學生
        /// </summary>
        Dictionary<string, List<SCJoin>> ScjDic;

        /// <summary>
        /// 未選社團學生
        /// </summary>
        List<StudRecord> IsStudentList;
        Dictionary<string, StudRecord> IsStudentDic;

        /// <summary>
        /// 社團 vs 社團學生
        /// </summary>
        Dictionary<string, List<string>> ClubAddDic = new Dictionary<string, List<string>>();

        //人為設定選社學年
        string seting_school_year = "";

        //人為設定選社學期
        string seting_school_semester = "";

        public CheckStudentIsNotInClub()
        {
            InitializeComponent();

            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);

            BGWSave.DoWork += new DoWorkEventHandler(BGWSave_DoWork);
            BGWSave.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGWSave_RunWorkerCompleted);

            K12.Presentation.NLDPanels.Student.TempSourceChanged += new EventHandler(Student_TempSourceChanged);

            //labelX1.Text = string.Format("{0}學年度　第{1}學期　未選社清單：", School.DefaultSchoolYear, School.DefaultSemester);

            labelX3.Text = string.Format("待處理學生：共{0}人", K12.Presentation.NLDPanels.Student.TempSource.Count);
        }

        void Student_TempSourceChanged(object sender, EventArgs e)
        {
            labelX3.Text = string.Format("待處理學生：共{0}人", K12.Presentation.NLDPanels.Student.TempSource.Count);
        }

        private void CheckStudentIsNotInClub_Load(object sender, EventArgs e)
        {
            //畫面開啟
            //1.即檢查本學年度學期,是否有未選社團學生
            //每個Row內存學生的ID

            //2.取得目前學年度/學期的可選擇社團
            //每個Row內存社團的Club UID

            //3.

            #region 因應需要支援跨學期選社，在這邊做檢查，防止使用者沒有設定 選社學年、學期
            AccessHelper _AccessHelper = new AccessHelper();
            List<UDT.OpenSchoolYearSemester> opensemester = new List<UDT.OpenSchoolYearSemester>();

            opensemester = _AccessHelper.Select<UDT.OpenSchoolYearSemester>();



            //填入之前的紀錄
            if (opensemester.Count > 0)
            {
                seting_school_year = opensemester[0].SchoolYear;
                seting_school_semester = opensemester[0].Semester;

                labelX1.Text = string.Format("選社學年度  {0}學年度　第{1}學期 未選社清單：", seting_school_year, seting_school_semester);
            }
            else
            {
                MsgBox.Show("沒有設定 選社學年、選社學期，請至'選社開放時間'功能內設定。");

                this.Close();
                return;
            }
            #endregion 

            if (!BGW.IsBusy)
            {
                btnSave.Enabled = false;
                BGW.RunWorkerAsync();
            }
            else
            {
                MsgBox.Show("請重開本畫面");
            }
        }

        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {

            //取得本學期社團資料
            CLUBRecordList.Clear();

            //舊的  會載入 系統系統學期的社團清單
            //CLUBRecordList = _AccessHelper.Select<CLUBRecord>(string.Format("school_year={0} and semester={1}", School.DefaultSchoolYear, School.DefaultSemester));

            //新的 是載入 人為設定選社學年、學期
            CLUBRecordList = _AccessHelper.Select<CLUBRecord>(string.Format("school_year={0} and semester={1}", seting_school_year, seting_school_semester));

            //取得本學期,所有社團參與記錄
            List<string> ClubRefIDList = new List<string>();
            foreach (CLUBRecord record in CLUBRecordList)
            {
                if (!ClubRefIDList.Contains(record.UID))
                {
                    ClubRefIDList.Add(record.UID);
                    ClubAddDic.Add(record.UID, new List<string>());
                }
            }

            //取得學校所有學生記錄
            //學生記錄來自於社團ID
            StudentScjoinDic = new Dictionary<string, SCJoin>();
            ScjDic = new Dictionary<string, List<SCJoin>>();

            string ClubIdString = string.Join("','", ClubRefIDList);
            List<SCJoin> Scjoin = _AccessHelper.Select<SCJoin>(string.Format("ref_club_id in ('{0}')", ClubIdString));
            foreach (SCJoin join in Scjoin)
            {
                //學生修社紀錄
                if (!StudentScjoinDic.ContainsKey(join.RefStudentID))
                {
                    StudentScjoinDic.Add(join.RefStudentID, join);
                }

                //社團修課人數

                if (!ScjDic.ContainsKey(join.RefClubID))
                {
                    ScjDic.Add(join.RefClubID, new List<SCJoin>());
                }
                ScjDic[join.RefClubID].Add(join);
            }

            //取得學校內所有一般生記錄
            //班級/座號/學號/姓名
            //(沒有班級之學生,不列入記錄
            DataTable studentDT = _QueryHelper.Select(@"select student.id,class.class_name,
student.seat_no,student.student_number,student.name,class.grade_year,
student.gender ,dept_Class.name as dept_name_class,dept_Stud.name as dept_name_stud 
from student join class on student.ref_class_id=class.id 
left join dept dept_Class on dept_Class.id=class.ref_dept_id
left join dept dept_Stud on dept_Stud.id=student.ref_dept_id
where student.status in (1,2) and class.grade_year in (1,2,3,7,8,9) 
ORDER BY class.grade_year,class.class_name,student.seat_no");

            IsStudentList = new List<StudRecord>();
            IsStudentDic = new Dictionary<string, StudRecord>();
            foreach (DataRow row in studentDT.Rows)
            {
                StudRecord stud = new StudRecord(row);
                //依據社團參與記錄進行資料篩選
                if (!StudentScjoinDic.ContainsKey(stud.id))
                {
                    IsStudentList.Add(stud);

                }

                //所有學生,都加入清單
                if (!IsStudentDic.ContainsKey(stud.id))
                {
                    IsStudentDic.Add(stud.id, stud);
                }
            }
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnSave.Enabled = true;
            if (e.Cancelled)
            {
                MsgBox.Show("檢查作業已停止!");
                return;
            }
            if (e.Error != null)
            {
                SmartSchool.ErrorReporting.ReportingService.ReportException(e.Error);
                MsgBox.Show("檢查學生社團參與記錄發生錯誤\n" + e.Error.Message);
                return;
            }

            #region 學生

            // 舊的 抓取 系統 學年度
            //labelX1.Text = string.Format("{0}學年度　第{1}學期　未選社清單(共{2}人)：", School.DefaultSchoolYear, School.DefaultSemester, IsStudentList.Count);

            // 新的 抓取 人為設定選社學年度
            labelX1.Text = string.Format("{0}學年度　第{1}學期　未選社清單(共{2}人)：", seting_school_year, seting_school_semester, IsStudentList.Count);


            foreach (StudRecord stud in IsStudentList)
            {
                DataGridViewRow dataRow = new DataGridViewRow();
                dataRow.CreateCells(dataGridViewX1);
                dataRow.Tag = stud;
                dataRow.Cells[colGrade_year.Index].Value = stud.grade_year;
                dataRow.Cells[colClass.Index].Value = stud.class_name;
                dataRow.Cells[colSeat_no.Index].Value = stud.seat_no;
                dataRow.Cells[colName.Index].Value = stud.name;
                dataRow.Cells[colGan.Index].Value = stud.gender;
                dataRow.Cells[colStudentNumber.Index].Value = stud.student_number;

                dataGridViewX1.Rows.Add(dataRow);
            }
            #endregion

            #region 社團

            foreach (CLUBRecord record in CLUBRecordList)
            {
                int count = 0;
                if (ScjDic.ContainsKey(record.UID))
                {
                    count = ScjDic[record.UID].Count;
                }

                if (record.Limit.HasValue)
                {
                    if (count < record.Limit.Value)
                    {

                        ButtonItem btnItem = new ButtonItem();
                        btnItem.Text = record.ClubName + string.Format("(人數:{0}/{1})", count, record.Limit.Value);
                        btnItem.Tag = record;
                        btnItem.OptionGroup = "itmPnlTimeName";
                        btnItem.ButtonStyle = eButtonStyle.ImageAndText;
                        btnItem.Click += new EventHandler(btnItem_Click);
                        btnItem.ForeColor = Color.Blue;

                        itmPnlTimeName.Items.Add(btnItem);
                    }
                    else
                    {
                        ButtonItem btnItem = new ButtonItem();
                        btnItem.Text = record.ClubName + string.Format("(人數:{0}/{1}) 額滿", count, record.Limit.Value);
                        btnItem.Tag = record;
                        btnItem.OptionGroup = "itmPnlTimeName";
                        btnItem.ButtonStyle = eButtonStyle.ImageAndText;
                        btnItem.Click += new EventHandler(btnItem_Click);
                        btnItem.Enabled = false;
                        btnItem.ForeColor = Color.Gainsboro;

                        itmPnlTimeName.Items.Add(btnItem);
                    }
                }
                else
                {
                    ButtonItem btnItem = new ButtonItem();
                    btnItem.Text = record.ClubName + string.Format("(人數:{0}/{1})", count, "無限制");
                    btnItem.Tag = record;
                    btnItem.OptionGroup = "itmPnlTimeName";
                    btnItem.ButtonStyle = eButtonStyle.ImageAndText;
                    btnItem.Click += new EventHandler(btnItem_Click);

                    itmPnlTimeName.Items.Add(btnItem);
                }
            }

            itmPnlTimeName.ResumeLayout();
            itmPnlTimeName.Refresh();
            #endregion
        }

        void btnItem_Click(object sender, EventArgs e)
        {
            if (itmPnlTimeName.SelectedItems.Count == 1)
            {
                List<CLUBRecord> clubList = new List<CLUBRecord>();
                //取得目前所選擇的Button
                ButtonItem Buttonitem = itmPnlTimeName.SelectedItems[0] as ButtonItem;

                //取得課程Record
                CLUBRecord club = (CLUBRecord)Buttonitem.Tag;
                clubList.Add(club);

                //該課程目前有多少選課學生
                int count = 0;
                if (ScjDic.ContainsKey(club.UID))
                {
                    count = ScjDic[club.UID].Count;
                }

                //
                foreach (DataGridViewRow row in dataGridViewX1.SelectedRows)
                {
                    //先清除原本的
                    if (row.Cells[colSelectClub.Index].Tag != null)
                    {
                        CLUBRecord reClub = (CLUBRecord)row.Cells[colSelectClub.Index].Tag;
                        StudRecord stud = (StudRecord)row.Tag;
                        if (ClubAddDic.ContainsKey(reClub.UID))
                        {
                            ClubAddDic[reClub.UID].Remove(stud.id);
                        }

                        if (!clubList.Contains(reClub))
                        {
                            clubList.Add(reClub);
                        }
                    }

                    //填入學生所選的社團資料
                    row.Cells[colSelectClub.Index].Value = "" + club.ClubName;
                    row.Cells[colSelectClub.Index].Tag = club;

                    //統計,此按鈕造成新增多少學生(不重複)
                    if (ClubAddDic.ContainsKey(club.UID))
                    {
                        //不包含此學生則加入
                        StudRecord stud = (StudRecord)row.Tag;
                        if (!ClubAddDic[club.UID].Contains(stud.id))
                        {
                            ClubAddDic[club.UID].Add(stud.id);
                        }
                    }
                }

                RefreshButtonItem(clubList);
            }
        }

        private void 清除指定社團ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DataGridViewRow> list = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dataGridViewX1.SelectedRows)
            {
                list.Add(row);
            }
            ReSetupClub(list);
        }

        /// <summary>
        /// 重新整理修課有多少學生
        /// </summary>
        private void ReSetupClub(List<DataGridViewRow> rowList)
        {
            List<CLUBRecord> clublist = new List<CLUBRecord>();
            foreach (DataGridViewRow row in rowList)
            {
                //取得課程Record
                CLUBRecord club = (CLUBRecord)row.Cells[colSelectClub.Index].Tag;
                clublist.Add(club);
                //該課程目前有多少選課學生
                int count = 0;
                if (ScjDic.ContainsKey(club.UID))
                {
                    count = ScjDic[club.UID].Count;
                }

                row.Cells[colSelectClub.Index].Value = null;
                row.Cells[colSelectClub.Index].Tag = null;

                //統計,此按鈕造成新增多少學生(不重複)
                if (ClubAddDic.ContainsKey(club.UID))
                {
                    //包含此學生,則移除學生
                    StudRecord stud = (StudRecord)row.Tag;
                    if (ClubAddDic[club.UID].Contains(stud.id))
                    {
                        ClubAddDic[club.UID].Remove(stud.id);
                    }

                    club.NewCount--;
                    if (stud.grade_year == "1" || stud.grade_year == "7")
                    {
                        club.NewGrade1Limit--;
                    }
                    else if (stud.grade_year == "2" || stud.grade_year == "8")
                    {
                        club.NewGrade2Limit--;
                    }
                    else if (stud.grade_year == "3" || stud.grade_year == "9")
                    {
                        club.NewGrade3Limit--;
                    }
                }

            }

            RefreshButtonItem(clublist);
        }

        /// <summary>
        /// 重新處理畫面顯示
        /// </summary>
        private void RefreshButtonItem(List<CLUBRecord> ClubList)
        {
            foreach (CLUBRecord club in ClubList)
            {
                foreach (ButtonItem Buttonitem in itmPnlTimeName.Items)
                {
                    CLUBRecord record = (CLUBRecord)Buttonitem.Tag;
                    if (club.UID == record.UID)
                    {
                        //使用者若使用過按鈕分配,就會儲存在 ClubAddDic
                        if (ClubAddDic.ContainsKey(record.UID))
                        {
                            int count = 0;

                            if (ScjDic.ContainsKey(record.UID))
                            {
                                count += ScjDic[record.UID].Count;
                            }
                            count += ClubAddDic[record.UID].Count;

                            if (record.Limit.HasValue)
                            {
                                if (count == record.Limit.Value)
                                {
                                    Buttonitem.Text = record.ClubName + string.Format("(人數:{0}/{1}) +{2} 額滿", count, record.Limit.Value, ClubAddDic[record.UID].Count);
                                    Buttonitem.ForeColor = Color.Blue;
                                }
                                else if (count > record.Limit.Value)
                                {
                                    Buttonitem.Text = record.ClubName + string.Format("(人數:{0}/{1}) +{2} 超額", count, record.Limit.Value, ClubAddDic[record.UID].Count);
                                    Buttonitem.ForeColor = Color.Red;
                                }
                                else if (ClubAddDic[record.UID].Count == 0)
                                {
                                    Buttonitem.Text = record.ClubName + string.Format("(人數:{0}/{1})", count, record.Limit.Value);
                                    Buttonitem.ForeColor = Color.Blue;
                                }
                                else
                                {
                                    Buttonitem.Text = record.ClubName + string.Format("(人數:{0}/{1}) +{2}", count, record.Limit.Value, ClubAddDic[record.UID].Count);
                                    Buttonitem.ForeColor = Color.Blue;
                                }
                            }
                            else
                            {
                                Buttonitem.Text = record.ClubName + string.Format("(人數:{0}/{1}) +{2}", count, "無限制", ClubAddDic[record.UID].Count);
                                Buttonitem.ForeColor = Color.Blue;
                            }
                        }
                    }
                }
            }

            itmPnlTimeName.ResumeLayout();
            itmPnlTimeName.Refresh();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //將指定好的學生
            //建立社團參與記錄
            //並加入該社團內

            btnSave.Enabled = false;

            List<SCJoin> SCJoinList = new List<SCJoin>();
            sb_log = new StringBuilder();
            sb_log.AppendLine(string.Format("「{0}」學年度 第「{1}」學期 未選社團學生分發：", seting_school_year, seting_school_semester));

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.Cells[colSelectClub.Index].Tag != null)
                {
                    StudRecord sr = (StudRecord)row.Tag;
                    CLUBRecord cr = (CLUBRecord)row.Cells[colSelectClub.Index].Tag;

                    SCJoin sc = new SCJoin();
                    sc.RefClubID = cr.UID;
                    sc.RefStudentID = sr.id;
                    SCJoinList.Add(sc);

                    sb_log.AppendLine(string.Format("班級「{0}」學生「{1}」社團指定為「{2}」", sr.class_name, sr.name, cr.ClubName));
                }
            }

            BGWSave.RunWorkerAsync(SCJoinList);
        }

        void BGWSave_DoWork(object sender, DoWorkEventArgs e)
        {

            List<SCJoin> SCJoinList = (List<SCJoin>)e.Argument;

            try
            {
                _AccessHelper.InsertValues(SCJoinList);
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                SmartSchool.ErrorReporting.ReportingService.ReportException(ex);
                return;
            }

            FISCA.LogAgent.ApplicationLog.Log("未選社團分發作業", "分發", sb_log.ToString());
        }

        void BGWSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnSave.Enabled = true;

            if (e.Cancelled)
            {
                MsgBox.Show("儲存作業發生錯誤已停止!");
                return;
            }
            if (e.Error != null)
            {
                SmartSchool.ErrorReporting.ReportingService.ReportException(e.Error);
                MsgBox.Show("儲存作業發生錯誤!\n" + e.Error.Message);
                return;
            }

            MsgBox.Show("學生加入社團成功!!");

            ClubEvents.RaiseAssnChanged();
            this.Close();


        }

        private void 加入待處理學生ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>();
            foreach (DataGridViewRow row in dataGridViewX1.SelectedRows)
            {
                StudRecord sr = (StudRecord)row.Tag;
                list.Add(sr.id);
            }
            K12.Presentation.NLDPanels.Student.AddToTemp(list);
        }

        private void btnOutPut_Click(object sender, EventArgs e)
        {
            #region 匯出
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = "匯出未選社團學生清單";
            saveFileDialog1.Filter = "Excel (*.xlsx)|*.xlsx";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

            DataGridViewExport export = new DataGridViewExport(dataGridViewX1);
            export.Save(saveFileDialog1.FileName);

            if (new CompleteForm().ShowDialog() == DialogResult.Yes)
                System.Diagnostics.Process.Start(saveFileDialog1.FileName);
            #endregion
        }

        private void btnStartAuto_Click(object sender, EventArgs e)
        {

            DialogResult dr = MsgBox.Show("本功能將依據「社團選社限制」進行亂數分配\n確認開始?", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.No)
            {
                MsgBox.Show("已取消");
                return;
            }



            //開始自動分配
            //1.取得系統內的各社團的條件
            //a.人數上限(目前社團人數)
            //b.科別限制
            //c.男女限制
            //d.年級限制
            List<CLUBRecord> InsertClubLimit = new List<CLUBRecord>();
            foreach (CLUBRecord club in CLUBRecordList)
            {
                //取得目前社團修課人數
                int count = 0;
                if (ScjDic.ContainsKey(club.UID))
                {
                    count += ScjDic[club.UID].Count;
                }

                if (ClubAddDic.ContainsKey(club.UID))
                {
                    count += ClubAddDic[club.UID].Count;
                }

                //目前人數
                club.NewCount = count;
                club.RandomIndex = xy01.Next(1, 99999);

                //本社團,是否有"人數上限"限制
                if (club.Limit.HasValue)
                {
                    //目前社團人數,是否已超過人數上限
                    if (count < club.Limit.Value)
                    {
                        InsertClubLimit.Add(club);
                    }
                }
                else
                {
                    InsertClubLimit.Add(club);
                }

                //如果社團已經有修課學生
                if (ScjDic.ContainsKey(club.UID))
                {
                    //判斷與分類目前社團修課人數
                    foreach (SCJoin scj in ScjDic[club.UID])
                    {
                        //取得學生資料
                        if (IsStudentDic.ContainsKey(scj.RefStudentID))
                        {
                            StudRecord stud = IsStudentDic[scj.RefStudentID];

                            if (stud.grade_year == "1" || stud.grade_year == "7")
                            {
                                club.NewGrade1Limit++;
                            }
                            else if (stud.grade_year == "2" || stud.grade_year == "8")
                            {
                                club.NewGrade2Limit++;
                            }
                            else if (stud.grade_year == "3" || stud.grade_year == "9")
                            {
                                club.NewGrade3Limit++;
                            }
                        }
                    }
                }
            }
            //社團順序也亂數
            InsertClubLimit.Sort(SortRandomClub);

            //這一段主要是要將學生進行亂數排序
            List<StudRecord> StudList = new List<StudRecord>();
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                //沒有設定過才進行分配
                if (row.Cells[colSelectClub.Index].Value == null)
                {
                    //學生基本資料
                    StudRecord stud = (StudRecord)row.Tag;
                    stud.row = row;
                    stud.RandomIndex = xy01.Next(1, 99999);
                    StudList.Add(stud);
                }
            }
            StudList.Sort(SortRandomStud);

            //開始分配作業
            foreach (StudRecord stud in StudList)
            {
                DataGridViewRow row = stud.row;

                //重新亂數
                foreach (CLUBRecord club in InsertClubLimit)
                {
                    club.RandomIndex = xy01.Next(1, 99999);
                }
                InsertClubLimit.Sort(SortRandomClub);

                //可以分配的社團
                foreach (CLUBRecord club in InsertClubLimit)
                {
                    //這個社團,有人數上限
                    if (club.Limit.HasValue)
                    {
                        //人數未滿
                        if (club.Limit.Value > club.NewCount)
                        {
                            //社團有科別限制
                            if (club.GetDeptRestrictList.Count > 0)
                            {
                                if (stud.dept_name_stud != "")
                                {
                                    //學生科別
                                    if (club.GetDeptRestrictList.Contains(stud.dept_name_stud))
                                    {
                                        NowRunRow(row, club, stud);
                                    }
                                    else
                                    {
                                        //科別條件不符合
                                    }
                                }
                                else if (stud.dept_name_class != "")
                                {
                                    //班級科別
                                    if (club.GetDeptRestrictList.Contains(stud.dept_name_class))
                                    {
                                        NowRunRow(row, club, stud);
                                    }
                                    else
                                    {
                                        //科別條件不符合
                                    }
                                }
                                else
                                {
                                    //有科別限制,但學生身上無科別
                                }
                            }
                            else
                            {
                                NowRunRow(row, club, stud);
                            }
                        }
                        else
                        {
                            //這個社團人數上限已滿
                        }
                    }
                    else
                    {
                        //沒有人數上限
                        //如果沒有設定過,才進行分配
                        if (row.Cells[colSelectClub.Index].Value == null)
                        {
                            NowRunRow(row, club, stud);
                        }
                    }
                }
            }

            //重新處理畫面顯示
            RefreshButtonItem(InsertClubLimit);

            dataGridViewX1.Sort(new RowComparer());
        }

        private class RowComparer : System.Collections.IComparer
        {
            public RowComparer()
            {

            }

            public int Compare(object x, object y)
            {
                DataGridViewRow DataGridViewRow1 = (DataGridViewRow)x;
                DataGridViewRow DataGridViewRow2 = (DataGridViewRow)y;

                //指定社團名稱
                string name1 = ("" + DataGridViewRow1.Cells[6].Value).PadLeft(20, 'z');
                string name2 = ("" + DataGridViewRow2.Cells[6].Value).PadLeft(20, 'z');
                //年級
                name1 += ("" + DataGridViewRow1.Cells[0].Value).PadLeft(5, '0');
                name2 += ("" + DataGridViewRow2.Cells[0].Value).PadLeft(5, '0');
                //班級
                name1 += ("" + DataGridViewRow1.Cells[1].Value).PadLeft(10, '0');
                name2 += ("" + DataGridViewRow2.Cells[1].Value).PadLeft(10, '0');
                //座號
                name1 += ("" + DataGridViewRow1.Cells[2].Value).PadLeft(5, '0');
                name2 += ("" + DataGridViewRow2.Cells[2].Value).PadLeft(5, '0');
                return name1.CompareTo(name2);
            }
        }


        private void NowRunRow(DataGridViewRow row, CLUBRecord club, StudRecord stud)
        {
            //如果沒有設定過,才進行分配
            if (row.Cells[colSelectClub.Index].Value == null)
            {
                //當學生是一年級時
                if (stud.grade_year == "1" || stud.grade_year == "7")
                {

                    if (club.Grade1Limit.HasValue)
                    {
                        if (club.Grade1Limit.Value > club.NewGrade1Limit)
                        {
                            if (club.GenderRestrict != "")
                            {
                                //性別判斷
                                if (club.GenderRestrict == stud.gender)
                                {
                                    NowSetRow(row, club, stud, "1");
                                }
                            }
                            else
                            {
                                NowSetRow(row, club, stud, "1");
                            }
                        }
                    }
                    else
                    {
                        NowSetRow(row, club, stud, "1");
                    }
                }
                else if (stud.grade_year == "2" || stud.grade_year == "8")
                {
                    if (club.Grade2Limit.HasValue)
                    {
                        if (club.Grade2Limit.Value > club.NewGrade2Limit)
                        {
                            if (club.GenderRestrict != "")
                            {
                                //性別判斷
                                if (club.GenderRestrict == stud.gender)
                                {
                                    NowSetRow(row, club, stud, "2");
                                }
                            }
                            else
                            {
                                NowSetRow(row, club, stud, "2");
                            }
                        }
                    }
                    else
                    {
                        NowSetRow(row, club, stud, "2");
                    }
                }
                else if (stud.grade_year == "3" || stud.grade_year == "9")
                {
                    if (club.Grade3Limit.HasValue)
                    {
                        //三年級人數上限還沒滿
                        if (club.Grade3Limit.Value > club.NewGrade3Limit)
                        {
                            if (club.GenderRestrict != "")
                            {
                                //性別判斷
                                if (club.GenderRestrict == stud.gender)
                                {
                                    NowSetRow(row, club, stud, "3");
                                }
                            }
                            else
                            {
                                NowSetRow(row, club, stud, "3");
                            }
                        }
                    }
                    else
                    {
                        NowSetRow(row, club, stud, "3");
                    }
                }
            }
        }

        private void NowSetRow(DataGridViewRow row, CLUBRecord club, StudRecord stud, string v)
        {
            row.Cells[colSelectClub.Index].Value = "" + club.ClubName;
            row.Cells[colSelectClub.Index].Tag = club;
            club.NewCount++;

            //新加入多少人
            if (ClubAddDic.ContainsKey(club.UID))
            {
                ClubAddDic[club.UID].Add(stud.id);
            }

            if (v == "1")
            {
                club.NewGrade1Limit++;
            }
            else if (v == "2")
            {
                club.NewGrade2Limit++;
            }
            else if (v == "3")
            {
                club.NewGrade3Limit++;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private int SortRandomStud(StudRecord x, StudRecord y)
        {
            return x.RandomIndex.CompareTo(y.RandomIndex);
        }

        private int SortRandomClub(CLUBRecord x, CLUBRecord y)
        {
            return x.RandomIndex.CompareTo(y.RandomIndex);
        }

        private void btnSendMessageStud_Click(object sender, EventArgs e)
        {
            DialogResult dr = MsgBox.Show(string.Format("請確認要對所選「{0}」名學生,\n推播未參與社團訊息?", dataGridViewX1.SelectedRows.Count), MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                List<string> StudentIDList = new List<string>();
                StringBuilder sb_log = new StringBuilder();
                sb_log.AppendLine("已發送未參與社團推播");
                sb_log.AppendLine("");
                sb_log.AppendLine("所選學生清單:");
                foreach (DataGridViewRow row in dataGridViewX1.SelectedRows)
                {
                    StudRecord stud = (StudRecord)row.Tag;
                    StudentIDList.Add(stud.id);

                    sb_log.AppendLine("學生「" + stud.name + "」");
                }

                Campus.Message.SendMessage sm = new Campus.Message.SendMessage(StudentIDList, Campus.Message.SendMessage.UserType.StudentAndParent, "SendMessage.K12.Club20.Volunteer.CheckStudentIsNotInClub");
                if (sm.SendNow)
                {
                    sm.Run();

                    FISCA.LogAgent.ApplicationLog.Log("未參與社團學生", "訊息", sb_log.ToString());
                }
                else
                {
                    //不予發送
                }

            }
        }

        private void dataGridViewX1_SelectionChanged(object sender, EventArgs e)
        {
            btnSendMessageStud.Text = string.Format("未選社團推播({0})", dataGridViewX1.SelectedRows.Count);
        }
    }

    public class StudRecord
    {
        public StudRecord(DataRow row)
        {
            //select student.id,class.class_name,student.seat_no,
            //student.student_number,student.name,class.grade_year,student.gender 
            //from student join class on student.ref_class_id=class.id 
            //where student.status=1 or student.status=2  
            //ORDER BY class.grade_year,class.class_name,student.seat_no

            id = "" + row["id"];
            class_name = "" + row["class_name"];
            seat_no = "" + row["seat_no"];
            student_number = "" + row["student_number"];
            name = "" + row["name"];
            grade_year = "" + row["grade_year"];
            dept_name_stud = "" + row["dept_name_stud"];
            dept_name_class = "" + row["dept_name_class"];
            if ("" + row["gender"] == "0")
                gender = "女";
            else if ("" + row["gender"] == "1")
                gender = "男";
            else
                gender = "";

        }
        public string dept_name_stud { get; set; }
        public string dept_name_class { get; set; }
        public string gender { get; set; }
        public string id { get; set; }
        public string class_name { get; set; }
        public string seat_no { get; set; }
        public string student_number { get; set; }
        public string name { get; set; }
        public string grade_year { get; set; }

        public int RandomIndex { get; set; }

        public DataGridViewRow row { get; set; }

    }
}
