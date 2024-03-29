﻿using Aspose.Words;
using Aspose.Words.Drawing;
using AllProveReport.UDT;
using FISCA.Presentation;
using FISCA.Presentation.Controls;
using K12.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aspose.Words.Tables;
using Aspose.Words.Reporting;

namespace AllProveReport.Report
{
    public partial class ProveReport : BaseForm
    {
        private string CadreConfig = "AllProveReport.cs.Aki";

        private string 顯示成績Config = "AllProveReport.ShowScore";

        private BackgroundWorker BGW = new BackgroundWorker();

        //主文件
        private Document _doc;
        //單頁範本
        private Document _template;
        //移動使用
        private static Run _run;

        private static string 顯示成績 = "False";


        // 入學照片
        Dictionary<string, string> _PhotoPDict = new Dictionary<string, string>();

        // 畢業照片
        Dictionary<string, string> _PhotoGDict = new Dictionary<string, string>();

        Dictionary<string, List<ResultScoreRecord>> _StudentResultDic = new Dictionary<string, List<ResultScoreRecord>>();



        public ProveReport()
        {
            InitializeComponent();

            K12.Data.Configuration.ConfigData conf = K12.Data.School.Configuration[顯示成績Config];
            if (conf["顯示成績"] == "")
            {
                顯示成績 = "False";
                checkBoxX1.Checked = false;
            }
            else
            {
                顯示成績 = "True";
                checkBoxX1.Checked = true;
            }

        }

        private void CadreProveReport_Load(object sender, EventArgs e)
        {
            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //取得設定檔
            Campus.Report.ReportConfiguration ConfigurationInCadre = new Campus.Report.ReportConfiguration(CadreConfig);
            //畫面內容(範本內容,預設樣式
            Campus.Report.TemplateSettingForm TemplateForm;

            if (顯示成績 == "True")
            {
                if (ConfigurationInCadre.Template != null)
                {
                    TemplateForm = new Campus.Report.TemplateSettingForm(ConfigurationInCadre.Template, new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單_score, Campus.Report.TemplateType.Word));
                }
                else
                {
                    ConfigurationInCadre.Template = new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單_score, Campus.Report.TemplateType.Word);
                    TemplateForm = new Campus.Report.TemplateSettingForm(ConfigurationInCadre.Template, new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單_score, Campus.Report.TemplateType.Word));
                }
            }
            else
            {
                if (ConfigurationInCadre.Template != null)
                {
                    TemplateForm = new Campus.Report.TemplateSettingForm(ConfigurationInCadre.Template, new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單, Campus.Report.TemplateType.Word));
                }
                else
                {
                    ConfigurationInCadre.Template = new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單, Campus.Report.TemplateType.Word);
                    TemplateForm = new Campus.Report.TemplateSettingForm(ConfigurationInCadre.Template, new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單, Campus.Report.TemplateType.Word));
                }
            }

            //預設名稱
            TemplateForm.DefaultFileName = "社團參與證明單(範本)";
            //如果回傳為OK
            if (TemplateForm.ShowDialog() == DialogResult.OK)
            {
                //設定後樣試,回傳
                ConfigurationInCadre.Template = TemplateForm.Template;
                //儲存
                ConfigurationInCadre.Save();
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            btnPrint.Enabled = false;
            BGW.RunWorkerAsync();
        }

        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            K12.Data.Configuration.ConfigData conf = K12.Data.School.Configuration[顯示成績Config];
            conf["顯示成績"] = 顯示成績;
            conf.Save();

            List<string> StudentIDList = K12.Presentation.NLDPanels.Student.SelectedSource;

            _doc = new Document();
            _doc.Sections.Clear(); //清空此Document

            //取得設定檔
            Campus.Report.ReportConfiguration ConfigurationInCadre = new Campus.Report.ReportConfiguration(CadreConfig);
            if (ConfigurationInCadre.Template == null)
            {
                //如果範本為空,則建立一個預設範本
                Campus.Report.ReportConfiguration ConfigurationInCadre_1 = new Campus.Report.ReportConfiguration(CadreConfig);
                if (顯示成績 == "True")
                    ConfigurationInCadre_1.Template = new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單_score, Campus.Report.TemplateType.Word);
                else
                    ConfigurationInCadre_1.Template = new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單, Campus.Report.TemplateType.Word);

                _template = new Document(ConfigurationInCadre_1.Template.GetStream());

            }
            else
            {
                //如果已有範本,則取得樣板
                _template = new Document(ConfigurationInCadre.Template.GetStream());
            }

            List<StudentRecord> StudList = Student.SelectByIDs(StudentIDList);

            //取得學生的 社團學期成績,與其它資料
            //$K12.ResultScore.Shinmin
            //結算後,相關記錄會放在學生的社團學期成績

            #region 社團學期成績

            _StudentResultDic.Clear();
            FISCA.UDT.AccessHelper _A = new FISCA.UDT.AccessHelper();
            List<ResultScoreRecord> ResultList = _A.Select<ResultScoreRecord>(string.Format("ref_student_id in ('{0}')", string.Join("','", StudentIDList)));
            List<string> CLUBIDList = new List<string>();

            FISCA.Data.QueryHelper _Q = new FISCA.Data.QueryHelper();
            DataTable dt = _Q.Select("select now()");
            string stringNow = "" + dt.Rows[0][0];
            DateTime Now = DateTime.Parse(stringNow);

            List<string> StudentOkList = new List<string>();

            foreach (ResultScoreRecord each in ResultList)
            {

                //if (string.IsNullOrEmpty(each.CadreName))
                //    continue;

                //if (!CLUBIDList.Contains(each.RefClubID))
                //{
                //    if (each.RefClubID != "")
                //    {
                //        CLUBIDList.Add(each.RefClubID);
                //    }
                //}

                if (!_StudentResultDic.ContainsKey(each.RefStudentID))
                {
                    _StudentResultDic.Add(each.RefStudentID, new List<ResultScoreRecord>());
                }

                _StudentResultDic[each.RefStudentID].Add(each);

                if (!StudentOkList.Contains(each.RefStudentID))
                {
                    StudentOkList.Add(each.RefStudentID);
                }
            }

            #endregion

            if (StudentOkList.Count != 0)
            {
                // 入學照片
                _PhotoPDict.Clear();
                _PhotoPDict = K12.Data.Photo.SelectFreshmanPhoto(StudentOkList);

                // 畢業照片
                _PhotoGDict.Clear();
                _PhotoGDict = K12.Data.Photo.SelectGraduatePhoto(StudentOkList);
            }

            foreach (StudentRecord student in StudList)
            {
                List<string> name = new List<string>();
                List<object> value = new List<object>();

                if (_StudentResultDic.ContainsKey(student.ID))
                {
                }
                else
                    continue;

                name.Add("資料");
                if (_StudentResultDic.ContainsKey(student.ID))
                    value.Add(_StudentResultDic[student.ID]); //重點欄位
                else
                    value.Add(new List<ResultScoreRecord>());

                #region MailMerge

                name.Add("學校名稱");
                value.Add(School.ChineseName);

                if (student.Class != null)
                {
                    name.Add("班級");
                    value.Add(student.Class.Name);
                }
                else
                {
                    name.Add("班級");
                    value.Add("");
                }

                name.Add("座號");
                value.Add(student.SeatNo.HasValue ? student.SeatNo.Value.ToString() : "");

                name.Add("姓名");
                value.Add(student.Name);

                name.Add("日期");
                string salesDate = String.Format("中華民國 {0}年{1}月{2}日",
                    FormatChineseDate((Now.Year - 1911).ToString()), 
                    FormatChineseDate(""+Now.Month), 
                    FormatChineseDate(""+Now.Day));
                value.Add(salesDate);

                name.Add("學號");
                value.Add(student.StudentNumber);

                name.Add("校長");
                if (K12.Data.School.Configuration["學校資訊"].PreviousData != null)
                {
                    if (K12.Data.School.Configuration["學校資訊"].PreviousData.SelectSingleNode("ChancellorChineseName") != null)
                    {
                        value.Add(K12.Data.School.Configuration["學校資訊"].PreviousData.SelectSingleNode("ChancellorChineseName").InnerText);
                    }
                    else
                    {
                        value.Add("");
                    }
                }
                else
                {
                    value.Add("");
                }

                if (_PhotoPDict.ContainsKey(student.ID))
                {
                    name.Add("新生照片1");
                    value.Add(_PhotoPDict[student.ID]);

                    name.Add("新生照片2");
                    value.Add(_PhotoPDict[student.ID]);
                }

                if (_PhotoGDict.ContainsKey(student.ID))
                {
                    name.Add("畢業照片1");
                    value.Add(_PhotoGDict[student.ID]);

                    name.Add("畢業照片2");
                    value.Add(_PhotoGDict[student.ID]);
                }

                #endregion

                //取得範本樣式
                Document PageOne = (Document)_template.Clone(true);

                //PageOne.MailMerge.MergeField += new Aspose.Words.Reporting.MergeFieldEventHandler(MailMerge_MergeField);
                PageOne.MailMerge.FieldMergingCallback = new MailMerge_MergeField();

                PageOne.MailMerge.Execute(name.ToArray(), value.ToArray());

                _doc.Sections.Add(_doc.ImportNode(PageOne.FirstSection, true));

            }

            e.Result = _doc;

        }

        string FormatChineseDate(string dateName)
        {
            string[] chineseNumerals = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };

            string yearStr = "";
            foreach (char digit in dateName)
            {
                if (char.IsDigit(digit))
                {
                    int digitValue = int.Parse(digit.ToString());
                    yearStr += chineseNumerals[digitValue];
                }
                else
                {
                    yearStr += digit;
                }
            }

            return yearStr;
        }

        #region Aspose 修改更新

        class MailMerge_MergeField : Aspose.Words.Reporting.IFieldMergingCallback
        {

            void IFieldMergingCallback.FieldMerging(FieldMergingArgs e)

            {
                if (e.FieldName == "新生照片1" || e.FieldName == "新生照片2")
                {
                    #region 新生照片
                    if (!string.IsNullOrEmpty(e.FieldValue.ToString()))
                    {
                        byte[] photo = Convert.FromBase64String(e.FieldValue.ToString()); //e.FieldValue as byte[];

                        if (photo != null && photo.Length > 0)
                        {
                            DocumentBuilder photoBuilder = new DocumentBuilder(e.Document);
                            photoBuilder.MoveToField(e.Field, true);
                            e.Field.Remove();
                            //Paragraph paragraph = photoBuilder.InsertParagraph();// new Paragraph(e.Document);
                            Shape photoShape = new Shape(e.Document, ShapeType.Image);
                            photoShape.ImageData.SetImage(photo);
                            photoShape.WrapType = WrapType.Inline;
                            //Cell cell = photoBuilder.CurrentParagraph.ParentNode as Cell;
                            //cell.CellFormat.LeftPadding = 0;
                            //cell.CellFormat.RightPadding = 0;
                            if (e.FieldName == "新生照片1")
                            {
                                // 1吋
                                photoShape.Width = ConvertUtil.MillimeterToPoint(25);
                                photoShape.Height = ConvertUtil.MillimeterToPoint(35);
                            }
                            else
                            {
                                //2吋
                                photoShape.Width = ConvertUtil.MillimeterToPoint(35);
                                photoShape.Height = ConvertUtil.MillimeterToPoint(45);
                            }
                            //paragraph.AppendChild(photoShape);
                            photoBuilder.InsertNode(photoShape);
                        }
                    }
                    #endregion
                }
                else if (e.FieldName == "畢業照片1" || e.FieldName == "畢業照片2")
                {
                    #region 畢業照片
                    if (!string.IsNullOrEmpty(e.FieldValue.ToString()))
                    {
                        byte[] photo = Convert.FromBase64String(e.FieldValue.ToString()); //e.FieldValue as byte[];

                        if (photo != null && photo.Length > 0)
                        {
                            DocumentBuilder photoBuilder = new DocumentBuilder(e.Document);
                            photoBuilder.MoveToField(e.Field, true);
                            e.Field.Remove();
                            //Paragraph paragraph = photoBuilder.InsertParagraph();// new Paragraph(e.Document);
                            Shape photoShape = new Shape(e.Document, ShapeType.Image);
                            photoShape.ImageData.SetImage(photo);
                            photoShape.WrapType = WrapType.Inline;
                            //Cell cell = photoBuilder.CurrentParagraph.ParentNode as Cell;
                            //cell.CellFormat.LeftPadding = 0;
                            //cell.CellFormat.RightPadding = 0;
                            if (e.FieldName == "畢業照片1")
                            {
                                // 1吋
                                photoShape.Width = ConvertUtil.MillimeterToPoint(25);
                                photoShape.Height = ConvertUtil.MillimeterToPoint(35);
                            }
                            else
                            {
                                //2吋
                                photoShape.Width = ConvertUtil.MillimeterToPoint(35);
                                photoShape.Height = ConvertUtil.MillimeterToPoint(45);
                            }
                            //paragraph.AppendChild(photoShape);
                            photoBuilder.InsertNode(photoShape);
                        }
                    }
                    #endregion
                }
                else if (e.FieldName == "資料")
                {
                    List<ResultScoreRecord> records = (List<ResultScoreRecord>)e.FieldValue;
                    records.Sort(SortResultScore);

                    Document PageOne = e.Document; // (Document)_template.Clone(true);
                    _run = new Run(PageOne);
                    DocumentBuilder builder = new DocumentBuilder(PageOne);
                    builder.MoveToMergeField("資料");
                    //取得目前Cell
                    Cell cell = (Cell)builder.CurrentParagraph.ParentNode;
                    //取得目前Row
                    Row row = (Row)builder.CurrentParagraph.ParentNode.ParentNode;

                    //建立新行
                    for (int x = 1; x < records.Count; x++)
                    {
                        (cell.ParentNode.ParentNode as Table).InsertAfter(row.Clone(true), cell.ParentNode);
                    }

                    foreach (ResultScoreRecord obj in records)
                    {
                        List<string> list = new List<string>();
                        list.Add(obj.SchoolYear.ToString());
                        list.Add(obj.Semester.ToString());
                        list.Add(obj.ClubName);
                        list.Add(obj.CadreName);

                        list.Add(obj.ClubLevel);

                        if (顯示成績 == "True")
                        {
                            //2020/12/29
                            //考量宏文高中康組長客服內容(#9480)
                            //轉學生會帶成績離開
                            //
                            list.Add(obj.ResultScore.HasValue ? obj.ResultScore.Value.ToString() : ""); //社團學期成績
                        }

                        // 當樣板使用顯示成績樣板，但是畫面有勾不顯示成績，需要補一格
                        if (顯示成績 == "False" && row.Cells.Count == 7)
                        {
                            list.Add("");
                        }

                        // 評語
                        list.Add(obj.Comment);

                        foreach (string listEach in list)
                        {
                            Write(cell, listEach);

                            if (cell.NextSibling != null) //是否最後一格
                                cell = cell.NextSibling as Cell; //下一格
                        }

                        Row Nextrow = cell.ParentRow.NextSibling as Row; //取得下一個Row
                        if (Nextrow == null)
                            break;
                        cell = Nextrow.FirstCell; //第一格Cell 
                    }
                }
            }
            void IFieldMergingCallback.ImageFieldMerging(ImageFieldMergingArgs args)
            {
            }

        }
        #endregion



        private static int SortResultScore(ResultScoreRecord r1, ResultScoreRecord r2)
        {
            string school_1 = r1.SchoolYear.ToString().PadLeft(3, '0');
            school_1 += r1.Semester.ToString().PadLeft(1, '0');

            string school_2 = r2.SchoolYear.ToString().PadLeft(3, '0');
            school_2 += r2.Semester.ToString().PadLeft(1, '0');

            return school_1.CompareTo(school_2);
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Document inResult = (Document)e.Result;
            btnPrint.Enabled = true;

            try
            {
                SaveFileDialog SaveFileDialog1 = new SaveFileDialog();

                SaveFileDialog1.Filter = "Word (*.docx)|*.docx|所有檔案 (*.*)|*.*";
                SaveFileDialog1.FileName = "社團參與證明單";

                if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    inResult.Save(SaveFileDialog1.FileName);
                    Process.Start(SaveFileDialog1.FileName);
                    MotherForm.SetStatusBarMessage("社團參與證明單,列印完成!!");
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

        /// <summary>
        /// 寫入資料
        /// </summary>
        private static void Write(Cell cell, string text)
        {
            if (cell.FirstParagraph == null)
                cell.Paragraphs.Add(new Paragraph(cell.Document));
            cell.FirstParagraph.Runs.Clear();
            _run.Text = text;
            _run.Font.Size = 12;
            _run.Font.Name = "標楷體";
            cell.FirstParagraph.Runs.Add(_run.Clone(true));
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBoxX1_CheckedChanged(object sender, EventArgs e)
        {
            顯示成績 = checkBoxX1.Checked.ToString();
        }
    }
}
