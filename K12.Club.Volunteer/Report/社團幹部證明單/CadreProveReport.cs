﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using Aspose.Words;
using K12.Data;
using FISCA.Presentation;
using System.Diagnostics;
using Aspose.Words.Drawing;
using Aspose.Words.Tables;
using Aspose.Words.Reporting;
using Campus.Report2014;

namespace K12.Club.Volunteer
{
    public partial class CadreProveReport : BaseForm
    {
        private string CadreConfig = "K12.Club.Config.General.CadreProveReport.cs";

        private BackgroundWorker BGW = new BackgroundWorker();

        //主文件
        private Document _doc;
        //單頁範本
        private Document _template;
        

        // 入學照片
        Dictionary<string, string> _PhotoPDict = new Dictionary<string, string>();

        // 畢業照片
        Dictionary<string, string> _PhotoGDict = new Dictionary<string, string>();

        Dictionary<string, List<ResultScoreRecord>> _StudentResultDic = new Dictionary<string, List<ResultScoreRecord>>();

        public CadreProveReport()
        {
            InitializeComponent();
        }

        private void CadreProveReport_Load(object sender, EventArgs e)
        {
            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //取得設定檔
            ReportConfiguration ConfigurationInCadre = new ReportConfiguration(CadreConfig);
            //畫面內容(範本內容,預設樣式
            TemplateSettingForm TemplateForm;
            if (ConfigurationInCadre.Template != null)
            {
                TemplateForm = new TemplateSettingForm(ConfigurationInCadre.Template, new ReportTemplate(Properties.Resources.社團幹部證明單1, TemplateType.docx));
            }
            else
            {
                ConfigurationInCadre.Template = new ReportTemplate(Properties.Resources.社團幹部證明單1, TemplateType.docx);
                TemplateForm = new TemplateSettingForm(ConfigurationInCadre.Template, new ReportTemplate(Properties.Resources.社團幹部證明單1, TemplateType.docx));
            }

            //預設名稱
            TemplateForm.DefaultFileName = "社團幹部證明單(範本)";
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
            List<string> StudentIDList = K12.Presentation.NLDPanels.Student.SelectedSource;

            _doc = new Document();
            _doc.Sections.Clear(); //清空此Document

            //取得設定檔
            ReportConfiguration ConfigurationInCadre = new ReportConfiguration(CadreConfig);
            if (ConfigurationInCadre.Template == null)
            {
                //如果範本為空,則建立一個預設範本
                ReportConfiguration ConfigurationInCadre_1 = new ReportConfiguration(CadreConfig);
                ConfigurationInCadre_1.Template = new ReportTemplate(Properties.Resources.社團幹部證明單1, TemplateType.docx);
                _template = new Document(ConfigurationInCadre_1.Template.GetStream());
            }
            else
            {
                //如果已有範本,則取得樣板
                //_template = ConfigurationInCadre.Template.ToDocument();
                _template = new Document(ConfigurationInCadre.Template.GetStream());
            }

            List<StudentRecord> StudList = Student.SelectByIDs(StudentIDList);

            //取得學生的 社團學期成績 , 內包含社長副社長,與其它資料
            //$K12.ResultScore.Shinmin
            //結算後,相關記錄會放在學生的社團學期成績

            #region 社團學期成績

            _StudentResultDic.Clear();

            List<ResultScoreRecord> ResultList = tool._A.Select<ResultScoreRecord>(string.Format("ref_student_id in ('{0}')", string.Join("','", StudentIDList)));
            List<string> CLUBIDList = new List<string>();

            FISCA.Data.QueryHelper _Q = new FISCA.Data.QueryHelper();
            DataTable dt = _Q.Select("select now()");
            string stringNow = "" + dt.Rows[0][0];
            DateTime Now = DateTime.Parse(stringNow);

            List<string> StudentOkList = new List<string>();

            foreach (ResultScoreRecord each in ResultList)
            {
                if (string.IsNullOrEmpty(each.CadreName))
                    continue;

                if (!CLUBIDList.Contains(each.RefClubID))
                {
                    if (each.RefClubID != "")
                    {
                        CLUBIDList.Add(each.RefClubID);
                    }
                }

                //當勾選畫面是社長/副社長時
                if (cbPrintByPresident.Checked)
                {
                    if (each.CadreName == "社長" || each.CadreName == "副社長")
                    {
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
                }
                else
                {
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
                    bool check = true;
                    foreach (ResultScoreRecord each in _StudentResultDic[student.ID])
                    {
                        if (!string.IsNullOrEmpty(each.CadreName))
                        {
                            check = false;
                            break;
                        }
                    }

                    if (check)
                        continue;
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
          FormatChineseDate("" + Now.Month),
          FormatChineseDate("" + Now.Day));
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

                PageOne.MailMerge.FieldMergingCallback= new MailMerge_MergeField();
                PageOne.MailMerge.Execute(name.ToArray(), value.ToArray());

                _doc.Sections.Add(_doc.ImportNode(PageOne.FirstSection, true));

            }

            e.Result = _doc;

        }

        static string FormatChineseDate(string dateName)
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

        class MailMerge_MergeField : Aspose.Words.Reporting.IFieldMergingCallback
        {
            //移動使用
            private Run _run;
            public void ImageFieldMerging(ImageFieldMergingArgs args)
            {
                throw new NotImplementedException();
            }

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
                    ////取得目前Cell
                    Cell cell = (Cell)builder.CurrentParagraph.ParentNode;
                    ////取得目前Row
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
                else
                {
                    //...
                }

            }
            /// <summary>
            /// 寫入資料
            /// </summary>
            private void Write(Cell cell, string text)
            {
                if (cell.FirstParagraph == null)
                    cell.Paragraphs.Add(new Paragraph(cell.Document));
                cell.FirstParagraph.Runs.Clear();
                _run.Text = text;
                _run.Font.Size = 12;
                _run.Font.Name = "標楷體";
                cell.FirstParagraph.Runs.Add(_run.Clone(true));
            }
            private int SortResultScore(ResultScoreRecord r1, ResultScoreRecord r2)
            {
                string school_1 = r1.SchoolYear.ToString().PadLeft(3, '0');
                school_1 += r1.Semester.ToString().PadLeft(1, '0');

                string school_2 = r2.SchoolYear.ToString().PadLeft(3, '0');
                school_2 += r2.Semester.ToString().PadLeft(1, '0');

                return school_1.CompareTo(school_2);
            }


        }

        private int SortResultScore(ResultScoreRecord r1, ResultScoreRecord r2)
        {
            string school_1 = r1.SchoolYear.ToString().PadLeft(3, '0');
            school_1 += r1.Semester.ToString().PadLeft(1, '0');

            string school_2 = r2.SchoolYear.ToString().PadLeft(3, '0');
            school_2 += r2.Semester.ToString().PadLeft(1, '0');

            return school_1.CompareTo(school_2);
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            object testJean=e.Result;
            Document inResult = (Document)e.Result;
            btnPrint.Enabled = true;

            try
            {
                SaveFileDialog SaveFileDialog1 = new SaveFileDialog();

                SaveFileDialog1.Filter = "Word (*.docx)|*.docx|所有檔案 (*.*)|*.*";
                SaveFileDialog1.FileName = "社團幹部證明單";

                if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    inResult.Save(SaveFileDialog1.FileName);
                    Process.Start(SaveFileDialog1.FileName);
                    MotherForm.SetStatusBarMessage("社團幹部證明單,列印完成!!");
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
        //private void Write(Cell cell, string text)
        //{
        //    if (cell.FirstParagraph == null)
        //        cell.Paragraphs.Add(new Paragraph(cell.Document));
        //    cell.FirstParagraph.Runs.Clear();
        //    _run.Text = text;
        //    _run.Font.Size = 12;
        //    _run.Font.Name = "標楷體";
        //    cell.FirstParagraph.Runs.Add(_run.Clone(true));
        //}

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

           
        }
}
