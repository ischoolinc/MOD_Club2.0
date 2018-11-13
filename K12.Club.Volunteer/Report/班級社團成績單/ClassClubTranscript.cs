﻿using System;
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
using Aspose.Cells;
using System.Diagnostics;
using FISCA.Presentation;
using System.IO;
using FISCA.DSAUtil;
using System.Xml;

namespace K12.Club.Volunteer
{
    public partial class ClassClubTranscript : BaseForm
    {
        private BackgroundWorker BGW = new BackgroundWorker();

        /// <summary>
        /// 資料學期
        /// </summary>
        private int _SchoolYear { get; set; }

        /// <summary>
        /// 資料學年度
        /// </summary>
        private int _Semester { get; set; }

        /// <summary>
        /// 是否只列印不及格生
        /// </summary>
        private bool PrintLost { get; set; }

        /// <summary>
        /// UDT資料取得器
        /// </summary>
        private AccessHelper _AccessHelper = new AccessHelper();

        string PriontName = "班級社團成績單";

        List<string> ColumnNameList { get; set; }

        public ClassClubTranscript()
        {
            InitializeComponent();
        }

        private void ClassClubTranscript_Load(object sender, EventArgs e)
        {
            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);

            intSchoolYear.Value = int.Parse(School.DefaultSchoolYear);
            intSemester.Value = int.Parse(School.DefaultSemester);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (K12.Presentation.NLDPanels.Class.SelectedSource.Count == 0)
            {
                MsgBox.Show("請選擇班級");
                return;
            }

            btnSave.Enabled = false;
            this.Text = "班級社團成績單(列印中...)";

            _SchoolYear = intSchoolYear.Value;
            _Semester = intSemester.Value;
            PrintLost = checkBoxX1.Checked;
            BGW.RunWorkerAsync();
        }

        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            ClassClubTraMag mag = new ClassClubTraMag(_SchoolYear, _Semester, PrintLost);

            //依據使用者所選擇的學年期
            //取得相關學生之社團結算成績

            //列印不及格學生清單時,排除所有學期成績60分(含)以上之學生
            //取得範本

            #region 建立範本

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.班級社團成績單_範本), FileFormatType.Excel2003);
            if (PrintLost) //不及格確認單
            {
                PriontName = "班級社團成績不及格(確認單)";
            }
            else
            {
                PriontName = "班級社團成績單";
            }

            Workbook prototype = new Workbook();
            prototype.Copy(template);
            Worksheet ptws = prototype.Worksheets[0];

            #region 建立標頭Column

            評量比例 評 = new 評量比例();
            if (評._wp == null)
            {
                e.Cancel = true;
                return;
            }

            ColumnNameList = new List<string>();
            ColumnNameList.Add("座號");
            ColumnNameList.Add("姓名");
            ColumnNameList.Add("學號");
            ColumnNameList.Add("性別");
            ColumnNameList.Add("社團");
            foreach (string each in 評.ColumnDic.Keys)
            {
                ColumnNameList.Add(each + "(" + 評.ProportionDic[each] + "%)");
            }
            ColumnNameList.Add("學期成績");
            if (PrintLost) //不及格確認單
            {
                ColumnNameList.Add("簽名");
            }

            int ColumnNameIndex = 0;
            foreach (string each in ColumnNameList)
            {
                ptws.Cells[2, ColumnNameIndex].Style.IsTextWrapped = true;
                ptws.Cells[2, ColumnNameIndex].PutValue(each);
                if (ColumnNameIndex >= 5)
                {
                    ptws.Cells.SetColumnWidth(ColumnNameIndex, 10);
                    tool.SetCellBro(ptws, 2, ColumnNameIndex, 1, 1);
                }
                ColumnNameIndex++;
            }

            #endregion

            Range ptHeader = ptws.Cells.CreateRange(0, 3, false);
            Range ptEachRow = ptws.Cells.CreateRange(3, 1, false);

            //建立Excel檔案
            Workbook wb = new Workbook();
            wb.Copy(prototype);

            //取得第一張
            Worksheet ws = wb.Worksheets[0];

            int dataIndex = 0;
            int CountPage = 1;

            int DetalIndex = 5;

            #endregion

            #region 填資料

            foreach (string classID in mag.TraDic.Keys)
            {
                if (mag.TraDic[classID].Count == 0)
                    continue;
                ws.Cells.CreateRange(dataIndex, 3, false).Copy(ptHeader);

                ClassRecord cr = mag.ClassDic[classID];

                ws.Cells.Merge(dataIndex, 0, 1, ColumnNameList.Count);
                string TitleName = string.Format("{0}學年度　第{1}學期　{2}", _SchoolYear.ToString(), _Semester.ToString(), PriontName);
                ws.Cells[dataIndex, 0].PutValue(TitleName);
                dataIndex++;

                //班級
                ws.Cells.Merge(dataIndex, 0, 1, 3);
                ws.Cells[dataIndex, 0].PutValue(string.Format("班級：{0}", cr.Name));

                ws.Cells.Merge(dataIndex, 4, 1, 3);
                //教師
                if (!string.IsNullOrEmpty(cr.RefTeacherID))
                {
                    #region 教師
                    if (mag.TeacherDic.ContainsKey(cr.RefTeacherID))
                    {
                        TeacherRecord tr = mag.TeacherDic[cr.RefTeacherID];
                        //是否有暱稱
                        if (!string.IsNullOrEmpty(tr.Nickname))
                        {
                            string TeacherString = "班導師：" + tr.Name + "(" + tr.Nickname + ")";
                            ws.Cells[dataIndex, 4].PutValue(TeacherString);
                        }
                        else
                        {
                            string TeacherString = "班導師：" + mag.TeacherDic[cr.RefTeacherID].Name;
                            ws.Cells[dataIndex, 4].PutValue(TeacherString);
                        }
                    }
                    #endregion
                }

                //頁數
                ws.Cells.Merge(dataIndex, ColumnNameList.Count - 4, 1, 4);
                ws.Cells[dataIndex, ColumnNameList.Count - 4].PutValue("日期：" + DateTime.Now.ToString("yyyy/MM/dd HH:mm") + "　頁數:" + CountPage.ToString());

                dataIndex += 2;

                mag.TraDic[classID].Sort(SortTraDic);

                foreach (ClassClubTraObj each in mag.TraDic[classID])
                {
                    ws.Cells.CreateRange(dataIndex, 1, false).Copy(ptEachRow);

                    ws.Cells[dataIndex, 0].PutValue(each.studentRecord.SeatNo.HasValue ? each.studentRecord.SeatNo.Value.ToString() : "");
                    ws.Cells[dataIndex, 1].PutValue(each.studentRecord.Name);
                    ws.Cells[dataIndex, 2].PutValue(each.studentRecord.StudentNumber);
                    ws.Cells[dataIndex, 3].PutValue(each.studentRecord.Gender);

                    //社團
                    if (each.club != null)
                    {
                        ws.Cells[dataIndex, 4].PutValue(each.club.ClubName);
                    }

                    if (each.SCJoin != null)
                    {
                        if (!string.IsNullOrEmpty(each.SCJoin.Score))
                        {
                            int x = 0;

                            XmlElement xml = DSXmlHelper.LoadXml(each.SCJoin.Score);

                            foreach (XmlElement each1 in xml.SelectNodes("Item"))
                            {
                                x++;
                                string name = each1.GetAttribute("Name");
                                if (評.ColumnDic.ContainsKey(name))
                                {
                                    ws.Cells[dataIndex, DetalIndex + 評.ColumnDic[name]].PutValue(each1.GetAttribute("Score"));
                                }
                            }

                        }
                    }

                    for (int x = 3; x < ColumnNameList.Count; x++)
                    {
                        tool.SetCellBro(ws, dataIndex, x, 1, 1);
                    }

                    //學期成績
                    if (PrintLost) //不及格確認單
                    {
                        ws.Cells.SetColumnWidth(ColumnNameList.Count - 1, 14);

                        if (each.RSR != null)
                        {
                            ws.Cells.SetColumnWidth(ColumnNameList.Count - 2, 8);
                            string Score = each.RSR.ResultScore.HasValue ? each.RSR.ResultScore.Value.ToString() : "";
                            ws.Cells[dataIndex, ColumnNameList.Count - 2].PutValue(Score);
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        if (each.RSR != null) //有學期成績
                        {

                            ws.Cells.SetColumnWidth(ColumnNameList.Count - 1, 8);
                            string Score = each.RSR.ResultScore.HasValue ? each.RSR.ResultScore.Value.ToString() : "";
                            ws.Cells[dataIndex, ColumnNameList.Count - 1].PutValue(Score);
                        }
                        else
                        {

                        }
                    }

                    dataIndex++;
                }

                CountPage++; //每班增加1頁

                ws.HPageBreaks.Add(dataIndex, ColumnNameList.Count);
            }

            #endregion

            e.Result = wb;
        }

        int SortTraDic(ClassClubTraObj a1, ClassClubTraObj b1)
        {
            int x = a1.studentRecord.SeatNo.HasValue ? a1.studentRecord.SeatNo.Value : 0;
            int y = b1.studentRecord.SeatNo.HasValue ? b1.studentRecord.SeatNo.Value : 0;
            return x.CompareTo(y);
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnSave.Enabled = true;
            this.Text = "班級社團成績單";

            if (e.Cancelled)
            {
                MsgBox.Show("列印失敗!未設定成績評量!!");
            }
            else
            {
                if (e.Error == null)
                {
                    SaveFileDialog SaveFileDialog1 = new SaveFileDialog();
                    SaveFileDialog1.Filter = "Excel (*.xls)|*.xls|所有檔案 (*.*)|*.*";
                    SaveFileDialog1.FileName = PriontName;

                    //資料
                    try
                    {
                        if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            Workbook inResult = (Workbook)e.Result;
                            inResult.Save(SaveFileDialog1.FileName);
                            Process.Start(SaveFileDialog1.FileName);
                            MotherForm.SetStatusBarMessage(PriontName + "列印完成!!");
                        }
                        else
                        {
                            FISCA.Presentation.Controls.MsgBox.Show("檔案未儲存");
                            return;
                        }
                    }
                    catch
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("檔案儲存錯誤,請檢查檔案是否開啟中!!");
                        MotherForm.SetStatusBarMessage("檔案儲存錯誤,請檢查檔案是否開啟中!!");
                    }
                }
                else
                {
                    MsgBox.Show("列印資料發生錯誤\n" + e.Error.Message);
                    SmartSchool.ErrorReporting.ReportingService.ReportException(e.Error);
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBoxX1_CheckedChanged(object sender, EventArgs e)
        {
            labelX1.Visible = checkBoxX1.Checked;
        }
    }
}
