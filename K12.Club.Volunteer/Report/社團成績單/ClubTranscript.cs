using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using FISCA.UDT;
using K12.Data;
using Aspose.Cells;
using System.IO;
using FISCA.Presentation.Controls;
using System.Windows.Forms;
using System.Diagnostics;
using FISCA.Presentation;
using System.Drawing;
using System.Xml;
using FISCA.DSAUtil;

namespace K12.Club.Volunteer
{
    class ClubTranscript
    {
        private BackgroundWorker BGW = new BackgroundWorker();

        ClubTraMag GetPoint { get; set; }

        WeightProportion _wp { get; set; }

        List<string> ColumnNameList { get; set; }

        public ClubTranscript()
        {
            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);
            BGW.RunWorkerAsync();
        }

        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            //取得相關的學生資料
            //1.基本資料
            //2.社團結算成績
            GetPoint = new ClubTraMag();

            //取得範本
            #region 建立範本

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.社團成績單_範本));


            //Style sy = template.Worksheets[0].Cells[3, 0].Style;
            //每一張
            Workbook prototype = new Workbook();
            prototype.Copy(template);
            prototype.CopyTheme(template);
            Worksheet ptws = prototype.Worksheets[0];

            #region 建立標頭Column

            評量比例 評 = new 評量比例();
            ColumnNameList = new List<string>();
            ColumnNameList.Add("班級名稱");
            ColumnNameList.Add("座號");
            ColumnNameList.Add("姓名");
            ColumnNameList.Add("學號");
            ColumnNameList.Add("性別");
            foreach (string each in 評.ColumnDic.Keys)
            {
                ColumnNameList.Add(each + "(" + 評.ProportionDic[each] + "%)");
            }
            ColumnNameList.Add("學期成績");

            ColumnNameList.Add("社團幹部");

            int ColumnNameIndex = 0;
            //Jean 更新Aspose
            Style style = prototype.Worksheets[0].Cells[0, 2].GetStyle();
            style.IsTextWrapped = true;

            foreach (string each in ColumnNameList)
            {
                ptws.Cells[2, ColumnNameIndex].SetStyle(style);
                ptws.Cells[2, ColumnNameIndex].PutValue(each);

                ptws.Cells.SetColumnWidth(ColumnNameIndex, 8);
                tool.SetCellBro(ptws, 2, ColumnNameIndex, 1, 1);

                //設定Style
                //SetCellStyle(ptws, 2, ColumnNameIndex);
                ColumnNameIndex++;
            }

            #endregion

            Range ptHeader = ptws.Cells.CreateRange(0, 3, false);
            Range ptEachRow = ptws.Cells.CreateRange(3, 1, false);

            //建立Excel檔案
            Workbook wb = new Workbook();
            wb.Copy(prototype);
            wb.CopyTheme(prototype);

            //取得第一張
            Worksheet ws = wb.Worksheets[0];

            int dataIndex = 0;
            int CountPage = 1;

            int DetalIndex = 5;

            #endregion

            #region 填資料

            foreach (string clubID in GetPoint.TraDic.Keys)
            {
                //每一個社團
                ws.Cells.CreateRange(dataIndex, 3, false).Copy(ptHeader);
                ws.Cells.CreateRange(dataIndex, 3, false).CopyStyle(ptHeader);
                ws.Cells.CreateRange(dataIndex, 3, false).CopyValue(ptHeader);
                ws.Cells.CreateRange(dataIndex, 3, false).CopyData(ptHeader);
                CLUBRecord cr = GetPoint.CLUBDic[clubID];

                //第一行 - 建立標頭內容
                ws.Cells.Merge(dataIndex, 0, 1, ColumnNameList.Count);
                string TitleName = string.Format("{0}學年度　第{1}學期　{2}　社團成績單", cr.SchoolYear.ToString(), cr.Semester.ToString(), cr.ClubName);
                ws.Cells[dataIndex, 0].PutValue(TitleName);
                dataIndex++;

                //第二行 - 代碼
                ws.Cells.Merge(dataIndex, 0, 1, 3);
                ws.Cells[dataIndex, 0].PutValue("代碼：" + cr.ClubNumber);

                //第二行 - 老師
                //教師 ColumnDic大小後的一格
                ws.Cells.Merge(dataIndex, ColumnNameList.Count - 3, 1, 3);
                string TeacherString = "教師：" + GetTeacherName(cr);
                ws.Cells[dataIndex, ColumnNameList.Count - 3].PutValue(TeacherString);
                //SetCellBro(ws, dataIndex, 0, 1, ColumnNameList.Count);
                dataIndex += 2;

                GetPoint.TraDic[clubID].Sort(SortTraDic);

                foreach (ClubTraObj each in GetPoint.TraDic[clubID])
                {
                    ws.Cells.CreateRange(dataIndex, 1, false).CopyStyle(ptEachRow);
                    ws.Cells.CreateRange(dataIndex, 1, false).CopyValue(ptEachRow);
                    ws.Cells.CreateRange(dataIndex, 1, false).CopyData(ptEachRow);

                    //基本資料
                    tool.SetCellBro(ws, dataIndex, 0, 1, 1);
                    ws.Cells[dataIndex, 0].PutValue(each.student.Class != null ? each.student.Class.Name : "");
                    tool.SetCellBro(ws, dataIndex, 1, 1, 1);
                    ws.Cells[dataIndex, 1].PutValue(each.student.SeatNo.HasValue ? each.student.SeatNo.Value.ToString() : "");
                    tool.SetCellBro(ws, dataIndex, 2, 1, 1);
                    ws.Cells[dataIndex, 2].PutValue(each.student.Name);
                    tool.SetCellBro(ws, dataIndex, 3, 1, 1);
                    ws.Cells[dataIndex, 3].PutValue(each.student.StudentNumber);
                    tool.SetCellBro(ws, dataIndex, 4, 1, 1);
                    ws.Cells[dataIndex, 4].PutValue(each.student.Gender);

                    if (!string.IsNullOrEmpty(each.SCJ.Score))
                    {
                        XmlElement xml = DSXmlHelper.LoadXml(each.SCJ.Score);

                        foreach (XmlElement each1 in xml.SelectNodes("Item"))
                        {
                            string name = each1.GetAttribute("Name");
                            if (評.ColumnDic.ContainsKey(name))
                            {
                                tool.SetCellBro(ws, dataIndex, DetalIndex + 評.ColumnDic[name], 1, 1);
                                ws.Cells[dataIndex, DetalIndex + 評.ColumnDic[name]].PutValue(each1.GetAttribute("Score"));
                            }
                        }
                    }
                    else
                    {
                        for (int x = 4; x < ColumnNameList.Count; x++)
                        {
                            tool.SetCellBro(ws, dataIndex, x, 1, 1);
                        }
                    }

                    //學期成績
                    if (each.RSR != null)
                    {
                        ws.Cells.SetColumnWidth(ColumnNameList.Count - 2, 8);
                        tool.SetCellBro(ws, dataIndex, ColumnNameList.Count - 2, 1, 1);
                        string Score = each.RSR.ResultScore.HasValue ? each.RSR.ResultScore.Value.ToString() : "";
                        ws.Cells[dataIndex, ColumnNameList.Count - 2].PutValue(Score);
                        //ws.Cells[dataIndex, ColumnNameList.Count - 2].SetStyle(style);
                    }
                    else
                    {
                        ws.Cells.SetColumnWidth(ColumnNameList.Count - 2, 8);
                        tool.SetCellBro(ws, dataIndex, ColumnNameList.Count - 2, 1, 1);
                    }

                    //社團幹部
                    if (each.RSR != null)
                    {
                        ws.Cells.SetColumnWidth(ColumnNameList.Count - 1, 8);
                        tool.SetCellBro(ws, dataIndex, ColumnNameList.Count - 1, 1, 1);
                        string CardreName = each.RSR.CadreName;
                        ws.Cells[dataIndex, ColumnNameList.Count - 1].PutValue(CardreName);
                        //ws.Cells[dataIndex, ColumnNameList.Count - 1].SetStyle(style);
                    }
                    else
                    {
                        ws.Cells.SetColumnWidth(ColumnNameList.Count - 1, 8);
                        tool.SetCellBro(ws, dataIndex, ColumnNameList.Count - 1, 1, 1);
                    }

                    dataIndex++;
                }



                //頁數
                string DateName = "日期：" + DateTime.Now.ToString("yyyy/MM/dd HH:mm") + "　頁數:" + CountPage.ToString();
                CountPage++;
                ws.Cells.Merge(dataIndex, 0, 1, 5);
                //SetCellBro(ws, dataIndex, 0, 1, ColumnNameList.Count);
                ws.Cells[dataIndex, 0].PutValue(DateName);
                ws.Cells[dataIndex, 0].SetStyle(style);
                ws.HPageBreaks.Add(dataIndex + 1, ColumnNameList.Count);
                dataIndex++;
            }

            #endregion

            e.Result = wb;

        }

        public int SortTraDic(ClubTraObj a1, ClubTraObj b1)
        {
            string StudentA = "";
            if (a1.student.Class != null)
            {
                StudentA += a1.student.Class.DisplayOrder.PadLeft(10, '0');
                StudentA += a1.student.Class.Name.PadLeft(10, '0');
            }
            else
            {
                StudentA += "00000000000000000000";
            }

            StudentA += a1.student.SeatNo.HasValue ? a1.student.SeatNo.Value.ToString().PadLeft(10, '0') : "0000000000";
            StudentA += a1.student.Name.PadLeft(10, '0');

            string StudentB = "";
            if (b1.student.Class != null)
            {
                StudentB += b1.student.Class.DisplayOrder.PadLeft(10, '0');
                StudentB += b1.student.Class.Name.PadLeft(10, '0');
            }
            else
            {
                StudentB += "00000000000000000000";
            }
            StudentB += b1.student.SeatNo.HasValue ? b1.student.SeatNo.Value.ToString().PadLeft(10, '0') : "0000000000";
            StudentB += b1.student.Name.PadLeft(10, '0');

            return StudentA.CompareTo(StudentB);
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MsgBox.Show("作業已被中止!!");
            }
            else
            {
                if (e.Error == null)
                {
                    SaveFileDialog SaveFileDialog1 = new SaveFileDialog();
                    SaveFileDialog1.Filter = "Excel (*.xlsx)|*.xlsx|所有檔案 (*.*)|*.*";
                    SaveFileDialog1.FileName = "社團成績單";

                    //資料
                    try
                    {
                        if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            Workbook inResult = (Workbook)e.Result;
                            inResult.Save(SaveFileDialog1.FileName);
                            Process.Start(SaveFileDialog1.FileName);
                            MotherForm.SetStatusBarMessage("社團成績單,列印完成!!");
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

        /// <summary>
        /// 取得老師名稱
        /// </summary>
        private string GetTeacherName(CLUBRecord cr)
        {
            string name = "";
            //老師1

            if (GetPoint.TeacherDic.ContainsKey(cr.RefTeacherID))
            {
                TeacherRecord tr = GetPoint.TeacherDic[cr.RefTeacherID];
                if (string.IsNullOrEmpty(tr.Nickname))
                {
                    name += tr.Name;
                }
                else
                {
                    name += tr.Name + "(" + tr.Nickname + ")";
                }
            }
            //老師2
            if (GetPoint.TeacherDic.ContainsKey(cr.RefTeacherID2))
            {
                TeacherRecord tr = GetPoint.TeacherDic[cr.RefTeacherID2];
                if (string.IsNullOrEmpty(tr.Nickname))
                {
                    name += "／" + tr.Name;
                }
                else
                {
                    name += "／" + tr.Name + "(" + tr.Nickname + ")";
                }
            }
            //老師3
            if (GetPoint.TeacherDic.ContainsKey(cr.RefTeacherID3))
            {
                TeacherRecord tr = GetPoint.TeacherDic[cr.RefTeacherID3];
                if (string.IsNullOrEmpty(tr.Nickname))
                {
                    name += "／" + tr.Name;
                }
                else
                {
                    name += "／" + tr.Name + "(" + tr.Nickname + ")";
                }
            }
            return name;
        }
    }
}
