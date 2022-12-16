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
using K12.Data;
using System.Xml;
using FISCA.DSAUtil;

namespace K12.Club.Volunteer
{
    public partial class ClubResultsInput : BaseForm
    {

        //1.是否修改資料檢查
        //2.資料儲存時,需要是數字型態
        //3.資料輸入時,必須小於100
        //4.社團修課學生狀態:
        成績取得器 GetPoint { get; set; }

        BackgroundWorker Save_BGW = new BackgroundWorker();
        BackgroundWorker BGW_FormLoad = new BackgroundWorker();

        private AccessHelper _AccessHelper = new AccessHelper();

        List<SCJoinRow> RowList = new List<SCJoinRow>();

        Campus.Windows.ChangeListener _ChangeListener = new Campus.Windows.ChangeListener();
        //記錄位置
        Dictionary<string, int> ColumnDic = new Dictionary<string, int>();
        //位置反推成績名稱
        Dictionary<int, string> _ColumnDic = new Dictionary<int, string>();

        //評語碼表
        Dictionary<string, string> CommentDic = new Dictionary<string, string>();

        //記錄比例
        Dictionary<string, int> ProportionDic = new Dictionary<string, int>();

        Dictionary<string, Log_Result> _logDic = new Dictionary<string, Log_Result>();

        Dictionary<string, ClassRecord> ClassDic { get; set; }

        int colSResults { get; set; }
        int colClearing { get; set; }

        bool IsChangeNow = false;

        public ClubResultsInput()
        {
            InitializeComponent();
        }

        private void ClubResultsInput_Load(object sender, EventArgs e)
        {
            //取得學生社團參與記錄
            BGW_FormLoad.DoWork += new DoWorkEventHandler(BGW_FormLoad_DoWork);
            BGW_FormLoad.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_FormLoad_RunWorkerCompleted);

            //儲存資料(更新社團參與記錄)
            Save_BGW.DoWork += new DoWorkEventHandler(Save_BGW_DoWork);
            Save_BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Save_BGW_RunWorkerCompleted);

            //dataGridViewX1.DataError += new DataGridViewDataErrorEventHandler(dataGridViewX1_DataError);

            _ChangeListener.StatusChanged += new EventHandler<Campus.Windows.ChangeEventArgs>(_ChangeListener_StatusChanged);
            _ChangeListener.Add(new Campus.Windows.DataGridViewSource(dataGridViewX1));

            if (ClubAdmin.Instance.SelectedSource.Count == 0)
                return;

            btnReport.Enabled = false;
            btnSave.Enabled = false;
            this.Text = "成績輸入(資料讀取中..)";

            //取得成績計算比例原則
            GetPoint = new 成績取得器();
            GetPoint.SetWeightProportion();

            AddColumns();

            BGW_FormLoad.RunWorkerAsync();

        }

        /// <summary>
        /// 加入Column
        /// </summary>
        private void AddColumns()
        {
            DataGridViewTextBoxColumn dgvR = new DataGridViewTextBoxColumn();
            dgvR.HeaderText = "評語";
            dgvR.Name = "colComment";
            int Columnindex = dataGridViewX1.Columns.Add(dgvR);
            //記錄位置
            ColumnDic.Add("評語", Columnindex);

            if (GetPoint._wp != null)
            {
                if (!string.IsNullOrEmpty(GetPoint._wp.Proportion))
                {
                    XmlElement xml = DSXmlHelper.LoadXml(GetPoint._wp.Proportion);
                    foreach (XmlElement xml2 in xml.SelectNodes("Item"))
                    {
                        DataGridViewTextBoxColumn dgvT = new DataGridViewTextBoxColumn();
                        dgvT.HeaderText = xml2.GetAttribute("Name") + "(" + xml2.GetAttribute("Proportion") + "%)";
                        dgvT.Name = xml2.GetAttribute("Name");
                        Columnindex = dataGridViewX1.Columns.Add(dgvT);
                        //記錄位置
                        ColumnDic.Add(xml2.GetAttribute("Name"), Columnindex);
                        _ColumnDic.Add(Columnindex, xml2.GetAttribute("Name"));
                        //記錄比例
                        ProportionDic.Add(xml2.GetAttribute("Name"), int.Parse(xml2.GetAttribute("Proportion")));
                    }

                    //dataGridViewX1.AutoResizeColumns();
                }
            }

            DataGridViewTextBoxColumn dgvK = new DataGridViewTextBoxColumn();
            dgvK.HeaderText = "學期成績試算";
            dgvK.Name = "colSResults";
            dgvK.ReadOnly = true; //不可寫
            dgvK.DefaultCellStyle.BackColor = Color.LightCyan; //不可寫
            colSResults = dataGridViewX1.Columns.Add(dgvK);

            DataGridViewTextBoxColumn dgvJ = new DataGridViewTextBoxColumn();
            dgvJ.HeaderText = "學期成績";
            dgvJ.Name = "colClearing";
            dgvJ.ReadOnly = true; //不可寫
            dgvJ.DefaultCellStyle.BackColor = Color.LightCyan; //不可寫
            colClearing = dataGridViewX1.Columns.Add(dgvJ);
        }

        void _ChangeListener_StatusChanged(object sender, Campus.Windows.ChangeEventArgs e)
        {
            IsChangeNow = (e.Status == Campus.Windows.ValueStatus.Dirty);
        }

        //void dataGridViewX1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        //{
        //    MsgBox.Show("輸入資料錯誤!!");
        //    e.Cancel = false;
        //}

        void BGW_FormLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            StringBuilder sb_3 = new StringBuilder();
            GetPoint = new 成績取得器();
            GetPoint.SetWeightProportion();
            GetPoint.GetSCJoinByClubIDList(ClubAdmin.Instance.SelectedSource);

            //取得評語對照表
            foreach (ClubComment each in tool._A.Select<ClubComment>())
            {
                if (!CommentDic.ContainsKey(each.code))
                {
                    CommentDic.Add(each.code, each.Comment);
                }
            }

            #region 社團老師資訊

            List<string> teacherIDList = new List<string>();
            foreach (CLUBRecord club in GetPoint._ClubDic.Values)
            {
                if (!string.IsNullOrEmpty(club.RefTeacherID))
                {
                    if (!teacherIDList.Contains(club.RefTeacherID))
                    {
                        teacherIDList.Add(club.RefTeacherID);
                    }
                }
            }

            List<TeacherRecord> TeacherList = Teacher.SelectByIDs(teacherIDList);
            Dictionary<string, TeacherRecord> ClubTeacherDic = new Dictionary<string, TeacherRecord>();
            foreach (TeacherRecord each in TeacherList)
            {
                if (!ClubTeacherDic.ContainsKey(each.ID))
                {
                    ClubTeacherDic.Add(each.ID, each);
                }
            }

            #endregion

            #region 取得班級資料

            //從學生Record內取得班級ID,再取得班級Record
            ClassDic = GetClassDic();

            #endregion

            RowList.Clear();


            _logDic = new Dictionary<string, Log_Result>();

            //取得社團參與記錄
            foreach (StudentRecord student in GetPoint._StudentDic.Values)
            {
                if (GetPoint._SCJoinDic.ContainsKey(student.ID))
                {
                    List<SCJoin> each = GetPoint._SCJoinDic[student.ID];
                    if (each.Count == 1)
                    {
                        #region 只有一筆資料
                        SCJoin sch = each[0];

                        SCJoinRow scjRow = new SCJoinRow();
                        scjRow.SCJ = sch;
                        //學生
                        if (GetPoint._StudentDic.ContainsKey(sch.RefStudentID))
                        {
                            scjRow.student = GetPoint._StudentDic[sch.RefStudentID];

                            //社團
                            if (GetPoint._ClubDic.ContainsKey(sch.RefClubID))
                            {
                                scjRow.club = GetPoint._ClubDic[sch.RefClubID];

                                if (ClubTeacherDic.ContainsKey(GetPoint._ClubDic[sch.RefClubID].RefTeacherID))
                                {
                                    scjRow.teacher = ClubTeacherDic[GetPoint._ClubDic[sch.RefClubID].RefTeacherID];
                                }
                            }

                            if (GetPoint._RSRDic.ContainsKey(sch.UID))
                            {
                                scjRow.RSR = GetPoint._RSRDic[sch.UID];
                            }

                            RowList.Add(scjRow);
                        }
                        #endregion
                    }
                    else if (each.Count >= 1)
                    {
                        #region 有兩筆資料
                        //錯誤訊息
                        StudentRecord sr = Student.SelectByID(each[0].RefStudentID);
                        sb_3.AppendLine("學生[" + sr.Name + "]有2筆以上社團記錄");

                        SCJoin sch = each[0];
                        SCJoinRow scjRow = new SCJoinRow();
                        scjRow.SCJ = sch;
                        //學生
                        if (GetPoint._StudentDic.ContainsKey(sch.RefStudentID))
                        {
                            scjRow.student = GetPoint._StudentDic[sch.RefStudentID];

                            //社團
                            if (GetPoint._ClubDic.ContainsKey(sch.RefClubID))
                            {
                                scjRow.club = GetPoint._ClubDic[sch.RefClubID];

                                if (ClubTeacherDic.ContainsKey(GetPoint._ClubDic[sch.RefClubID].RefTeacherID))
                                {
                                    scjRow.teacher = ClubTeacherDic[GetPoint._ClubDic[sch.RefClubID].RefTeacherID];
                                }
                            }

                            if (GetPoint._RSRDic.ContainsKey(sch.UID))
                            {
                                scjRow.RSR = GetPoint._RSRDic[sch.UID];
                            }

                            RowList.Add(scjRow);
                        }
                        #endregion
                    }
                    else
                    {
                        //沒有記錄繼續
                    }




                }
            }

            if (!string.IsNullOrEmpty(sb_3.ToString()))
            {
                MsgBox.Show(sb_3.ToString());
            }
        }

        /// <summary>
        /// 取得班級清單
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, ClassRecord> GetClassDic()
        {
            Dictionary<string, ClassRecord> dic = new Dictionary<string, ClassRecord>();

            List<string> classIDList = new List<string>();
            foreach (StudentRecord sr in GetPoint._StudentDic.Values)
            {
                if (!string.IsNullOrEmpty(sr.RefClassID))
                {
                    classIDList.Add(sr.RefClassID);
                }
            }
            List<ClassRecord> classList = Class.SelectByIDs(classIDList);
            foreach (ClassRecord each in classList)
            {
                if (!dic.ContainsKey(each.ID))
                {
                    dic.Add(each.ID, each);
                }
            }
            return dic;
        }

        //畫面取得
        void BGW_FormLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnReport.Enabled = true;
            btnSave.Enabled = true;
            this.Text = "成績輸入";

            if (e.Cancelled)
            {
                MsgBox.Show("資料取得已被中止");
            }
            else
            {
                if (e.Error == null)
                {
                    dataGridViewX1.AutoGenerateColumns = false;
                    dataGridViewX1.DataSource = RowList;

                    //Log
                    foreach (SCJoinRow each in RowList)
                    {
                        if (!_logDic.ContainsKey(each.SCJoinID))
                        {
                            _logDic.Add(each.SCJoinID, new Log_Result(ColumnDic.Keys.ToList(), each.SCJ));
                            _logDic[each.SCJoinID]._stud = each.student;
                            if (!string.IsNullOrEmpty(each.student.RefClassID))
                            {
                                if (ClassDic.ContainsKey(each.student.RefClassID))
                                {
                                    _logDic[each.SCJoinID]._class = ClassDic[each.student.RefClassID];
                                }
                            }
                        }
                    }

                    //學期成績資料
                    foreach (DataGridViewRow row in dataGridViewX1.Rows)
                    {
                        SCJoinRow scjRow = (SCJoinRow)row.DataBoundItem;

                        row.Cells[ColumnDic["評語"]].Value = scjRow.SCJ.Comment;

                        if (scjRow.RSR != null)
                        {
                            if (scjRow.RSR.ResultScore.HasValue)
                            {
                                row.Cells[colClearing].Value = scjRow.RSR.ResultScore.Value;
                            }
                        }
                    }

                    //分項成績資料與試算
                    if (GetPoint._wp != null)
                    {
                        foreach (DataGridViewRow row in dataGridViewX1.Rows)
                        {
                            //把資料顯示在畫面上
                            SetValueToRow(row);
                            //試算學期成績
                            SetRowResults(row);
                        }
                    }
                    else
                    {
                        MsgBox.Show("尚未設定評量比例\n將無法試算出總成績資料!!");
                    }
                    _ChangeListener.Reset();
                    _ChangeListener.ResumeListen();
                    IsChangeNow = false;
                }
                else
                {
                    MsgBox.Show("發生錯誤:\n" + e.Error.Message);
                }
            }
        }

        /// <summary>
        /// 把成績資料顯示在畫面上
        /// </summary>
        private void SetValueToRow(DataGridViewRow row)
        {
            SCJoinRow scjRow = (SCJoinRow)row.DataBoundItem;

            //如果不是空值
            if (!string.IsNullOrEmpty(scjRow.Score))
            {
                XmlElement xml = DSXmlHelper.LoadXml(scjRow.Score);

                foreach (XmlElement each in xml.SelectNodes("Item"))
                {
                    string name = each.GetAttribute("Name");
                    if (ColumnDic.ContainsKey(name))
                    {
                        row.Cells[ColumnDic[name]].Value = each.GetAttribute("Score");
                    }
                }
            }
        }

        /// <summary>
        /// 試算學期成績
        /// </summary>
        private void SetRowResults(DataGridViewRow row)
        {
            List<decimal?> list = new List<decimal?>();
            foreach (string each in ColumnDic.Keys)
            {
                //2022/12/16 - 新增評語,但是試算不予處理
                if (each == "評語")
                    continue;

                decimal? 成績 = ParseValue(row.Cells[ColumnDic[each]]);
                int 比例 = ProportionDic[each];

                if (成績.HasValue)
                {
                    成績 = 比例 * 成績 / 100;
                }

                list.Add(成績);
            }

            decimal results = 0;
            bool check = true;
            foreach (decimal? each in list)
            {
                if (each.HasValue)
                {
                    results += each.Value;

                }
                else
                {
                    check = false;
                }
            }

            //四欄都有資料,才進行運算
            if (check)
            {
                row.Cells[colSResults].Value = Math.Round(results, MidpointRounding.AwayFromZero);
            }
            else //不是就清空
            {
                row.Cells[colSResults].Value = "";
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (Save_BGW.IsBusy)
                return;

            if (!CheckDataGridValue())
            {
                MsgBox.Show("請修正資料後儲存!");
                return;
            }

            btnSave.Enabled = false;
            Save_BGW.RunWorkerAsync();
        }

        //儲存
        void Save_BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            List<SCJoin> list = new List<SCJoin>();
            if (GetPoint._wp != null)
            {
                foreach (DataGridViewRow row in dataGridViewX1.Rows)
                {
                    SCJoinRow scjRow = (SCJoinRow)row.DataBoundItem;
                    Log_Result Set_Log = _logDic[scjRow.SCJoinID];

                    //Log
                    if (scjRow.SCJ.Comment != "" + row.Cells[ColumnDic["評語"]].Value)
                    {
                        scjRow.SCJ.Comment = "" + row.Cells[ColumnDic["評語"]].Value;
                        if (Set_Log._NewItemDic.ContainsKey("評語"))
                        {
                            Set_Log._NewItemDic["評語"] = scjRow.SCJ.Comment;
                        }
                    }

                    if (scjRow.HasChange)
                    {
                        DSXmlHelper dsx = new DSXmlHelper("Xml");
                        bool IsTrue = false;

                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (!IsScore(cell))
                                continue;
                            //取得
                            if (_ColumnDic.ContainsKey(cell.ColumnIndex))
                            {
                                XmlElement xml = dsx.AddElement("Item");

                                string name = _ColumnDic[cell.ColumnIndex];
                                string value = "" + cell.Value;

                                if (!string.IsNullOrEmpty(value))
                                    IsTrue = true;
                                xml.SetAttribute("Name", name);
                                xml.SetAttribute("Score", value);

                                //Log
                                if (Set_Log._NewItemDic.ContainsKey(name))
                                {
                                    Set_Log._NewItemDic[name] = value;
                                }
                            }
                        }
                        if (IsTrue)
                        {
                            scjRow.SCJ.Score = dsx.BaseElement.OuterXml;
                        }
                        else
                        {
                            scjRow.SCJ.Score = "";
                        }
                        list.Add(scjRow.SCJ);
                    }
                }
                //修改儲存

                if (list.Count > 0)
                    _AccessHelper.UpdateValues(list);

                //Log
                FISCA.LogAgent.ApplicationLog.Log("社團", "成績輸入", GetLostConn());
            }
            else
            {
                //沒有指定成績比例原則
                e.Cancel = true;
            }
        }

        private string GetLostConn()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("修改社團成績資料：");
            foreach (Log_Result each in _logDic.Values)
            {
                if (each.IsChange)
                {
                    sb.AppendLine(each.GetLogString(GetPoint));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 檢查資料
        /// </summary>
        private bool CheckDataGridValue()
        {
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (!IsScore(cell))
                        continue;

                    if (!CheckCellValue(cell))
                        return false;
                }
            }
            return true;

        }

        /// <summary>
        /// 是否為成績的Cell
        /// </summary>
        private bool IsScore(DataGridViewCell cell)
        {
            //至少大於1 ROW
            if (cell.RowIndex < 0)
                return false;
            //如果小於學號欄位,則離開
            if (cell.ColumnIndex <= ColStudentNumber.Index)
                return false;

            if (cell.ColumnIndex == ColumnDic["評語"])
                return true;

            //如果大於/等於學期成績試算欄位
            if (cell.ColumnIndex >= dataGridViewX1.Columns["colSResults"].Index)
                return false;

            return true;


        }

        /// <summary>
        /// 儲存作業完成
        /// </summary>
        void Save_BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnSave.Enabled = true;

            if (e.Cancelled)
            {
                MsgBox.Show("儲存動作已中止!!");
                return;
            }

            if (e.Error != null)
            {
                MsgBox.Show("儲存發生錯誤:\n" + e.Error.Message);
            }

            MsgBox.Show("資料儲存成功");

            if (!BGW_FormLoad.IsBusy)
            {
                BGW_FormLoad.RunWorkerAsync();
            }
        }

        /// <summary>
        /// 排序資料
        /// 社團編號 ->社團名稱 ->班級名稱 ->座號 ->學生姓名
        /// </summary>
        private int SortSCJ(SCJoinRow a, SCJoinRow b)
        {
            string clubNameA = a.club.ClubNumber.PadLeft(3, '0');
            clubNameA += a.ClubName.PadLeft(10, '0');
            clubNameA += a.ClassIndex.PadLeft(3, '0');
            clubNameA += a.ClassName.PadLeft(5, '0');
            clubNameA += a.SeatNo.PadLeft(3, '0');
            clubNameA += a.StudentName.PadLeft(10, '0');

            string clubNameB = b.club.ClubNumber.PadLeft(3, '0');
            clubNameB += b.ClubName.PadLeft(10, '0');
            clubNameB += b.ClassIndex.PadLeft(3, '0');
            clubNameB += b.ClassName.PadLeft(5, '0');
            clubNameB += b.SeatNo.PadLeft(3, '0');
            clubNameB += b.StudentName.PadLeft(10, '0');

            return clubNameA.CompareTo(clubNameB);
        }

        /// <summary>
        /// 將資料 Parse 為 decimal
        /// </summary>
        private bool ParseDec(string dec)
        {
            decimal decTry;
            bool a = decimal.TryParse(dec, out decTry);
            return a;
        }

        /// <summary>
        /// 當成績資料被修改時...
        /// </summary>
        private void dataGridViewX1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            DataGridViewCell cell = dataGridViewX1.CurrentCell;

            if (!IsScore(cell))
                return;

            dataGridViewX1.CurrentCell.ErrorText = "";
            dataGridViewX1.CurrentCell.Style.BackColor = Color.White;

            SCJoinRow scjRow = (SCJoinRow)dataGridViewX1.CurrentRow.DataBoundItem;
            scjRow.HasChange = true;

            if (_logDic.ContainsKey(scjRow.SCJoinID))
            {
                _logDic[scjRow.SCJoinID].IsChange = true;
            }

            //如果是評語欄位,則替換代碼
            if (cell.ColumnIndex == ColumnDic["評語"])
            {
                //以逗號","分割為多組字串
                string cellValue = "" + cell.Value;
                List<string> nameList = cellValue.Split(',').ToList();
                //變更後的內容
                List<string> changeValue = new List<string>();
                foreach (string each in nameList)
                {
                    if (CommentDic.ContainsKey(each))
                        changeValue.Add(CommentDic[each]);
                    else
                        changeValue.Add(each);
                }
                cell.Value = string.Join(",", changeValue);
            }

            //資料錯誤則離開
            if (!CheckCellValue(dataGridViewX1.CurrentCell))
            {
                return;
            }

            //進行成績及時計算
            if (GetPoint._wp != null)
            {
                SetRowResults(dataGridViewX1.CurrentRow);
            }
        }

        /// <summary>
        /// 把Cell Parse 為 decimal
        /// </summary>
        private decimal? ParseValue(DataGridViewCell cell)
        {
            if (!string.IsNullOrEmpty("" + cell.Value))
            {
                decimal dc = 0;
                decimal.TryParse("" + cell.Value, out dc);
                return dc;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 檢查Cell資料是否正確
        /// 1.必須是數字
        /// 2.大於100 則標黃色
        /// 3.輸入小數點 則標黃色
        /// </summary>
        private bool CheckCellValue(DataGridViewCell cell)
        {

            if (string.IsNullOrEmpty("" + cell.Value))
                return true;

            if (cell.ColumnIndex == ColumnDic["評語"])
                return true;


            if (!ParseDec("" + cell.Value))
            {
                cell.ErrorText = "必須是數字";
                cell.Style.BackColor = Color.Red;

                //輸入的是數字如果未繼續編輯將會出現只能輸入單位數的狀況
                //dataGridViewX1.BeginEdit(false);
                return false;
            }

            //大於100黃色字
            decimal de = decimal.Parse("" + cell.Value);
            if (de > 100)
            {
                cell.Style.BackColor = Color.Yellow;
                //dataGridViewX1.BeginEdit(false);
                return true;
            }

            //有小數點黃色字
            if (de.ToString().Contains('.'))
            {
                cell.Style.BackColor = Color.Yellow;
                return true;
            }

            return true;
        }

        /// <summary>
        /// 當畫面關閉
        /// 如果資料有修改 
        /// </summary>
        private void ClubResultsInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsChangeNow)
            {
                DialogResult dr = FISCA.Presentation.Controls.MsgBox.Show("確認放棄?", "尚未儲存資料", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr != System.Windows.Forms.DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// 匯出資料
        /// </summary>
        private void btnReport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = "匯出成績輸入";
            saveFileDialog1.Filter = "Excel (*.xlsx)|*.xlsx";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

            DataGridViewExport export = new DataGridViewExport(dataGridViewX1);
            export.Save(saveFileDialog1.FileName);

            if (new CompleteForm().ShowDialog() == DialogResult.Yes)
                System.Diagnostics.Process.Start(saveFileDialog1.FileName);
        }

        /// <summary>
        /// 關閉畫面
        /// </summary>
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 選擇切換時,開始編輯目前Cell
        /// </summary>
        private void dataGridViewX1_SelectionChanged(object sender, EventArgs e)
        {
            dataGridViewX1.BeginEdit(true);
        }

        private void dataGridViewX1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 5)
            {
                dataGridViewX1.ImeMode = ImeMode.OnHalf;
                dataGridViewX1.ImeMode = ImeMode.Off;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Permissions.社團評語代碼表權限)
            {
                CommentForm form = new CommentForm();
                form.ShowDialog();
            }
            else
            {
                MsgBox.Show("請開啟權限");
            }
        }
    }
}
