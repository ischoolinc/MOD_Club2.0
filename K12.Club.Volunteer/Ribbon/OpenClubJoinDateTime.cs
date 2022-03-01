using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FISCA.LogAgent;
using K12.Data;
using FISCA.Data;
using FISCA.UDT;
using FISCA.Presentation.Controls;


namespace K12.Club.Volunteer
{
    public partial class OpenClubJoinDateTime : FISCA.Presentation.Controls.BaseForm
    {
        //2012/7/23
        //group by出目前系統的年級資料
        //取得 DTClub ,並且判斷其開始與結束時間
        private const string DateTimeFormat = "yyyy/MM/dd HH:mm";

        AccessHelper _AccessHelper = new AccessHelper();
        QueryHelper _QueryHelper = new QueryHelper();
        List<DTClub> Low_DTClubList = new List<DTClub>();
        List<UDT.OpenSchoolYearSemester> opensemester = new List<UDT.OpenSchoolYearSemester>();


        public OpenClubJoinDateTime()
        {
            InitializeComponent();
            List<int> cols = new List<int>() { 2, 3, 5, 6 };
            Campus.Windows.DataGridViewImeDecorator dec = new Campus.Windows.DataGridViewImeDecorator(this.dgvTimes, cols);
        }

        private void DailyLifeInputControl_Load(object sender, EventArgs e)
        {
            // 2017/3/16 穎驊註解，因應 社團2.0 要支援跨學期選社，故將原顯示目前系統學年學期的Lable註解
            //lblSemester.Text = string.Format("{0}學年度　第{1}學期", School.DefaultSchoolYear, School.DefaultSemester);

            //先將 Grid 填入此學校有的年級。
            FillGridViewGradeYear();
            
            // 學年為 上下加減一學年
            comboBoxEx1.Items.Add("" + (int.Parse(School.DefaultSchoolYear) - 1));
            comboBoxEx1.Items.Add(School.DefaultSchoolYear);
            comboBoxEx1.Items.Add("" + (int.Parse(School.DefaultSchoolYear) + 1));

            // 學期為1、2
            comboBoxEx2.Items.Add("1");
            comboBoxEx2.Items.Add("2");

            //預設為當下學年度、學期
            comboBoxEx1.Text = School.DefaultSchoolYear;
            comboBoxEx2.Text = School.DefaultSemester;

            opensemester = _AccessHelper.Select<UDT.OpenSchoolYearSemester>();

            //填入之前的紀錄
            if(opensemester.Count>0)
            {
                comboBoxEx1.Text = opensemester[0].SchoolYear;
                comboBoxEx2.Text = opensemester[0].Semester;
            }

            //第一階段無 "不開放" 選項
            chStage1_Mode.Items.Add("先搶先贏");
            chStage1_Mode.Items.Add("志願序");
            //chStage1_Mode.Items.Add("不開放");

            chStage2_Mode.Items.Add("先搶先贏");
            chStage2_Mode.Items.Add("志願序");
            chStage2_Mode.Items.Add("不開放");

            //將對應年級的時間填入。
            FillTimes();


        }

        private void FillTimes()
        {
            Low_DTClubList = _AccessHelper.Select<DTClub>();

            //沒有資料就不顯示資料。
            if (Low_DTClubList.Count == 0) return;

            foreach (DTClub each in Low_DTClubList)
            {
                string grade = each.GradeYear.ToString();

                string startTime1 = each.Start1.HasValue ? each.Start1.Value.ToString(DateTimeFormat) : "";
                string endTime1 = each.End1.HasValue ? each.End1.Value.ToString(DateTimeFormat) : "";

                string startTime2 = each.Start2.HasValue ? each.Start2.Value.ToString(DateTimeFormat) : "";
                string endTime2 = each.End2.HasValue ? each.End2.Value.ToString(DateTimeFormat) : "";

                string stage1_Mode = each.Stage1_Mode;
                string stage2_Mode = each.Stage2_Mode;




                foreach (DataGridViewRow eachRow in dgvTimes.Rows)
                {
                    string rowgrade = eachRow.Cells[chGradeYear.Index].Value + "";

                    if (rowgrade == grade)
                    {
                        eachRow.Cells[chStartTime1.Index].Value = startTime1;
                        eachRow.Cells[chEndTime1.Index].Value = endTime1;

                        eachRow.Cells[chStartTime2.Index].Value = startTime2;
                        eachRow.Cells[chEndTime2.Index].Value = endTime2;

                        eachRow.Cells[chStage1_Mode.Index].Value = stage1_Mode;
                        eachRow.Cells[chStage2_Mode.Index].Value = stage2_Mode;


                        break;
                    }
                }
            }
        }

        private void FillGridViewGradeYear()
        {
            foreach (int each in GroupGradeYear().Keys)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dgvTimes, each.ToString(), "", "");
                dgvTimes.Rows.Add(row);
            }
        }

        private Dictionary<int, string> GroupGradeYear()
        {
            string gradeyearString = "select grade_year from class group by grade_year ORDER by grade_year";
            DataTable dtTable = _QueryHelper.Select(gradeyearString);

            Dictionary<int, string> years = new Dictionary<int, string>();
            foreach (DataRow row in dtTable.Rows)
            {
                string yearString = "" + row[0];

                int year;

                if (!int.TryParse(yearString, out year))
                    continue;

                if (!years.ContainsKey(year))
                    years.Add(year, yearString);
            }

            return years;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            MsgBox.Show("溫馨提示：開放社團選社前..\n" +
                "請務必確認開放 [學年期] 是否正確\n" +
                "設定錯誤,將會造成學生在 [錯誤的學年期] 加選/退選社團");

            if (IsDataValidity())
            {

                #region 儲存學年、學期紀錄
               
                //填入之前的紀錄
                if (opensemester.Count > 0)
                {
                    opensemester[0].SchoolYear = comboBoxEx1.Text;
                    opensemester[0].Semester = comboBoxEx2.Text;
                    
                    opensemester.SaveAll();
                }
                else 
                {
                    UDT.OpenSchoolYearSemester config = new UDT.OpenSchoolYearSemester();

                    config.SchoolYear = comboBoxEx1.Text;
                    config.Semester = comboBoxEx2.Text;

                    config.Save();                
                }

                
                
                #endregion

                #region 資料正確,進行儲存
                List<DTClub> DTClubList = new List<DTClub>();
                foreach (DataGridViewRow each in dgvTimes.Rows)
                {
                    string Grade = "" + each.Cells[chGradeYear.Index].Value;

                    string Start1 = "" + each.Cells[chStartTime1.Index].Value;
                    string End1 = "" + each.Cells[chEndTime1.Index].Value;

                    string Start2 = "" + each.Cells[chStartTime2.Index].Value;
                    string End2 = "" + each.Cells[chEndTime2.Index].Value;

                    string stage1_Mode = "" + each.Cells[chStage1_Mode.Index].Value;
                    string stage2_Mode = "" + each.Cells[chStage2_Mode.Index].Value;


                    DTClub dt = new DTClub();

                    dt.GradeYear = int.Parse(Grade);

                    if (!string.IsNullOrEmpty(Start1))
                    {
                        dt.Start1 = DateTime.Parse(Start1);
                    }
                    if (!string.IsNullOrEmpty(End1))
                    {
                        dt.End1 = DateTime.Parse(End1);
                    }

                    if (!string.IsNullOrEmpty(Start2))
                    {
                        dt.Start2 = DateTime.Parse(Start2);
                    }
                    if (!string.IsNullOrEmpty(End2))
                    {
                        dt.End2 = DateTime.Parse(End2);
                    }

                    if (!string.IsNullOrEmpty(stage1_Mode))
                    {
                        dt.Stage1_Mode = stage1_Mode;
                    }
                    if (!string.IsNullOrEmpty(stage2_Mode))
                    {
                        dt.Stage2_Mode = stage2_Mode;
                    }





                    DTClubList.Add(dt);
                }

                try
                {
                    _AccessHelper.InsertValues(DTClubList);
                    _AccessHelper.DeletedValues(Low_DTClubList);

                    DTClubList.Sort(SortDTClub);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("已修改開放選社時間");


                    foreach (DTClub each in DTClubList)
                    {
                        if (each.Start1.HasValue && each.End1.HasValue)
                        {
                            string start1 = each.Start1.HasValue ? each.Start1.Value.ToString("yyyy/MM/dd HH:mm") : "";
                            string end1 = each.End1.HasValue ? each.End1.Value.ToString("yyyy/MM/dd HH:mm") : "";
                            sb.AppendLine(string.Format("階段1" + "「{0}」年級：開始時間「{1}」結束時間「{2}」", each.GradeYear, start1, end1));
                        }
                        if (each.Start2.HasValue && each.End2.HasValue)
                        {
                            string start2 = each.Start2.HasValue ? each.Start2.Value.ToString("yyyy/MM/dd HH:mm") : "";
                            string end2 = each.End2.HasValue ? each.End2.Value.ToString("yyyy/MM/dd HH:mm") : "";
                            sb.AppendLine(string.Format("階段" + "「{0}」年級：開始時間「{1}」結束時間「{2}」", each.GradeYear, start2, end2));
                        }

                    }


                    FISCA.LogAgent.ApplicationLog.Log("社團", "修改選社時間", sb.ToString());
                }
                catch (Exception ex)
                {
                    MsgBox.Show("儲存失敗!!\n" + ex.Message);
                    SmartSchool.ErrorReporting.ReportingService.ReportException(ex);
                    return;
                }
                MsgBox.Show("儲存成功!!");
                this.Close();

                #endregion
            }
            else
            {
                MsgBox.Show("畫面中含有不正確資料。");
            }
        }

        /// <summary>
        /// 依年級進行排序
        /// </summary>
        private int SortDTClub(DTClub dt1, DTClub dt2)
        {
            return dt1.GradeYear.CompareTo(dt2.GradeYear);
        }

        private bool IsDataValidity()
        {
            bool valid = true;
            foreach (DataGridViewRow each in dgvTimes.Rows)
            {
                if (!string.IsNullOrEmpty(each.ErrorText))
                {
                    valid = false;
                }

                foreach (DataGridViewCell eachCell in each.Cells)
                {
                    if (!string.IsNullOrEmpty(eachCell.ErrorText))
                    {
                        valid = false;
                    }
                }

                if (!valid) break;
            }

            return valid;
        }

        private void dgvTimes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //dgvTimes.BeginEdit(true);
        }

        /// <summary>
        /// 開始&結束日期是否有錯誤
        /// </summary>
        private void dgvTimes_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridViewRow row = dgvTimes.Rows[e.RowIndex];

            string startTime1 = row.Cells[chStartTime1.Index].Value + "";
            string endTime1 = row.Cells[chEndTime1.Index].Value + "";

            string startTime2 = row.Cells[chStartTime2.Index].Value + "";
            string endTime2 = row.Cells[chEndTime2.Index].Value + "";

            row.ErrorText = "";
            if (string.IsNullOrEmpty(startTime1) && string.IsNullOrEmpty(endTime1))
            {
                //這裡沒有程式。
            }
            else if (!string.IsNullOrEmpty(startTime1) && !string.IsNullOrEmpty(endTime1))
            {
                DateTime? objStart = DateTimeHelper.Parse(startTime1);
                DateTime? objEnd = DateTimeHelper.Parse(endTime1);

                if (objStart.HasValue && objEnd.HasValue)
                {
                    if (objStart.Value >= objEnd.Value)
                        row.ErrorText = "截止時間必須在開始時間之後。";
                }
            }
            else
            {
                row.ErrorText = "請輸入正確的時間限制資料(必需同時有資料或同時沒有資料)。";
            }

            if (string.IsNullOrEmpty(startTime2) && string.IsNullOrEmpty(endTime2))
            {
                //這裡沒有程式。
            }
            else if (!string.IsNullOrEmpty(startTime2) && !string.IsNullOrEmpty(endTime2))
            {
                DateTime? objStart = DateTimeHelper.Parse(startTime2);
                DateTime? objEnd = DateTimeHelper.Parse(endTime2);

                if (objStart.HasValue && objEnd.HasValue)
                {
                    if (objStart.Value >= objEnd.Value)
                        row.ErrorText = "截止時間必須在開始時間之後。";
                }
            }
            else
            {
                row.ErrorText = "請輸入正確的時間限制資料(必需同時有資料或同時沒有資料)。";
            }


            if (!string.IsNullOrEmpty(endTime1) && !string.IsNullOrEmpty(startTime2))
            {
                DateTime? objendTime1 = DateTimeHelper.Parse(endTime1);
                DateTime? objstartTime2 = DateTimeHelper.Parse(startTime2);

                if (objendTime1.HasValue && objstartTime2.HasValue)
                {
                    if (objendTime1.Value > objstartTime2.Value)
                        row.ErrorText = "選社階段1、2時段不得重疊。";
                }
            }



        }

        private void dgvTimes_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //ValidateCellData(e.ColumnIndex, e.RowIndex, e.FormattedValue + "");
        }

        private void dgvTimes_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ValidateCellData(e.ColumnIndex, e.RowIndex, dgvTimes.Rows[e.RowIndex].Cells[e.ColumnIndex].Value + "");
            //TryToCorrectData(e.ColumnIndex, e.RowIndex);
        }

        private void ValidateCellData(int columnIndex, int rowIndex, string value)
        {
            if (columnIndex == chStartTime1.Index || columnIndex == chEndTime1.Index || columnIndex == chStartTime2.Index || columnIndex == chEndTime2.Index)
            {
                DataGridViewCell cell = dgvTimes.Rows[rowIndex].Cells[columnIndex];
                cell.ErrorText = "";
                if (string.IsNullOrEmpty(value)) //沒有資料就不作任何檢查。
                    return;

                DateTime dt;
                if (!DateTime.TryParse(value, out dt))
                {
                    cell.ErrorText = "日期格式錯誤。";
                }
                else
                {
                    cell.Value = dt.ToString(DateTimeFormat);
                }
            }
        }

        private void dgvTimes_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            //TryToCorrectData(e.ColumnIndex, e.RowIndex);
        }

        //private void TryToCorrectData(int columnIndex, int rowIndex)
        //{
        //    if (columnIndex == chStartTime.Index || columnIndex == chEndTime.Index)
        //    {
        //        DataGridViewRow row = dgvTimes.Rows[rowIndex];
        //        row.Cells[columnIndex].ErrorText = string.Empty;
        //        string time = row.Cells[columnIndex].Value + "";

        //        if (string.IsNullOrEmpty(time)) //沒有資料就不作任何檢查。
        //            return;

        //        DateTime? objStart = DateTimeHelper.ParseGregorian(time, PaddingMethod.First);

        //        if (objStart.HasValue)
        //            row.Cells[columnIndex].Value = objStart.Value.ToString(DateTimeFormat);
        //    }
        //}
    }
}
