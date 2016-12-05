using Aspose.Words;
using Aspose.Words.Drawing;
using FISCA.Presentation.Controls;
using K12.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Across.Report
{
    public partial class EnglishSocietiesProveSingle : BaseForm
    {
        private string CadreConfig = "MOD_Club_Acrossdivisions.EnglishSocietiesProveSingle.Eng";

        private BackgroundWorker BGW = new BackgroundWorker();

        int 記錄多少筆 = 15;

        public EnglishSocietiesProveSingle()
        {
            InitializeComponent();
        }

        private void EnglishSocietiesProveSingle_Load(object sender, EventArgs e)
        {
            BGW.DoWork += BGW_DoWork;
            BGW.RunWorkerCompleted += BGW_RunWorkerCompleted;
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (BGW.IsBusy)
            {
                MsgBox.Show("忙碌中,稍後再試!!");
                return;
            }

            btnPrint.Enabled = false;
            BGW.RunWorkerAsync();

        }

        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            #region 範本

            //整理取得報表範本
            Campus.Report.ReportConfiguration ConfigurationInCadre = new Campus.Report.ReportConfiguration(CadreConfig);
            Aspose.Words.Document Template;

            if (ConfigurationInCadre.Template == null)
            {
                //如果範本為空,則建立一個預設範本
                Campus.Report.ReportConfiguration ConfigurationInCadre_1 = new Campus.Report.ReportConfiguration(CadreConfig);
                ConfigurationInCadre_1.Template = new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單_英文_範本, Campus.Report.TemplateType.Word);
                Template = ConfigurationInCadre_1.Template.ToDocument();
            }
            else
            {
                //如果已有範本,則取得樣板
                Template = ConfigurationInCadre.Template.ToDocument();
            }

            #endregion

            //取得社團中英文對照表
            Dictionary<string, string> EngDic = tool.GetEngList();



            List<StudentRecord> StudentList = K12.Data.Student.SelectByIDs(K12.Presentation.NLDPanels.Student.SelectedSource);

            //取得資料
            List<string> StudentIDList = new List<string>();
            foreach (StudentRecord stud in StudentList)
            {
                StudentIDList.Add(stud.ID);
            }

            Dictionary<string, StudentRSRecord> AllSRSRDic = GetRSR(StudentList, StudentIDList);

            //填資料部份
            DataTable table = new DataTable();

            table.Columns.Add("學校名稱");
            table.Columns.Add("學校英文名稱");
            table.Columns.Add("列印日期");
            table.Columns.Add("校長");
            table.Columns.Add("校長英文名稱");

            table.Columns.Add("班級");
            table.Columns.Add("座號");
            table.Columns.Add("學號");
            table.Columns.Add("姓名");
            table.Columns.Add("英文姓名");

            table.Columns.Add("新生照片1");
            table.Columns.Add("新生照片2");
            table.Columns.Add("畢業照片1");
            table.Columns.Add("畢業照片2");

            for (int x = 1; x <= 記錄多少筆; x++)
            {
                table.Columns.Add(string.Format("學年度_{0}", x));
            }

            for (int x = 1; x <= 記錄多少筆; x++)
            {
                table.Columns.Add(string.Format("學期_{0}", x));
            }

            for (int x = 1; x <= 記錄多少筆; x++)
            {
                table.Columns.Add(string.Format("社團_{0}", x));
            }

            for (int x = 1; x <= 記錄多少筆; x++)
            {
                table.Columns.Add(string.Format("英文社團名稱_{0}", x));
            }

            foreach (string studentID in AllSRSRDic.Keys)
            {
                StudentRSRecord student = AllSRSRDic[studentID];

                DataRow row = table.NewRow();
                row["學校名稱"] = K12.Data.School.ChineseName;
                row["學校英文名稱"] = K12.Data.School.EnglishName;

                string PrintDay = string.Format("{0}　{1},{2}", tool.GetMonth(DateTime.Today.Month), tool.GetDay(DateTime.Today.Day), DateTime.Today.Year.ToString());
                row["列印日期"] = PrintDay;

                XmlElement xml = K12.Data.School.Configuration["學校資訊"].PreviousData;

                if (xml.SelectSingleNode("ChancellorEnglishName") != null)
                {
                    row["校長英文名稱"] = xml.SelectSingleNode("ChancellorEnglishName").InnerText;
                    row["校長"] = xml.SelectSingleNode("ChancellorChineseName").InnerText;
                }
                else
                {
                    row["校長英文名稱"] = "";
                    row["校長"] = "";
                }

                row["班級"] = student._student.Class != null ? student._student.Class.Name : "";
                row["座號"] = student._student.SeatNo.HasValue ? student._student.SeatNo.Value.ToString() : "";
                row["學號"] = student._student.StudentNumber;
                row["姓名"] = student._student.Name;
                row["英文姓名"] = student._student.EnglishName;

                row["新生照片1"] = student.學生入學照片;
                row["新生照片2"] = student.學生入學照片;
                row["畢業照片1"] = student.學生畢業照片;
                row["畢業照片2"] = student.學生畢業照片;

                student._ResultList.Sort(SortResultScore);

                int y = 1;
                foreach (ResultScoreRecord ResultRecord in student._ResultList)
                {
                    if (y <= 記錄多少筆)
                    {
                        row[string.Format("學年度_{0}", y)] = ResultRecord.SchoolYear.ToString();
                        row[string.Format("學期_{0}", y)] = ResultRecord.Semester.ToString();
                        row[string.Format("社團_{0}", y)] = ResultRecord.ClubName;
                        if (EngDic.ContainsKey(ResultRecord.ClubName))
                            row[string.Format("英文社團名稱_{0}", y)] = EngDic[ResultRecord.ClubName];
                        else
                            row[string.Format("英文社團名稱_{0}", y)] = ResultRecord.ClubName;
                        y++;
                    }
                }

                table.Rows.Add(row);
            }

            Document PageOne = (Document)Template.Clone(true);
            PageOne.MailMerge.MergeField += new Aspose.Words.Reporting.MergeFieldEventHandler(MailMerge_MergeField);
            PageOne.MailMerge.Execute(table);
            e.Result = PageOne;
        }

        public int SortResultScore(ResultScoreRecord rsr1, ResultScoreRecord rsr2)
        {
            string schoolYear1 = rsr1.SchoolYear.ToString().PadLeft(5, '0');
            schoolYear1 += rsr1.Semester.ToString().PadLeft(2, '0');

            string schoolYear2 = rsr2.SchoolYear.ToString().PadLeft(5, '0');
            schoolYear2 += rsr2.Semester.ToString().PadLeft(2, '0');

            return schoolYear1.CompareTo(schoolYear2);
        }

        /// <summary>
        /// 學生資料整理
        /// </summary>
        private Dictionary<string, StudentRSRecord> GetRSR(List<StudentRecord> StudentList, List<string> StudentIDList)
        {
            Dictionary<string, StudentRSRecord> dic = new Dictionary<string, StudentRSRecord>();

            List<ResultScoreRecord> RSList = tool._A.Select<ResultScoreRecord>(string.Format("ref_student_id in ('{0}')", string.Join("','", StudentIDList)));

            //整理學生基本資料記錄
            foreach (StudentRecord stud in StudentList)
            {
                StudentRSRecord rsr = new StudentRSRecord(stud);
                if (!dic.ContainsKey(stud.ID))
                {
                    dic.Add(stud.ID, rsr);
                }
            }

            //整理學生社團記錄
            foreach (ResultScoreRecord rsr in RSList)
            {
                if (dic.ContainsKey(rsr.RefStudentID))
                {
                    dic[rsr.RefStudentID].SetRSR(rsr);
                }
            }

            // 入學照片
            Dictionary<string, string> _PhotoPDict = new Dictionary<string, string>();

            // 畢業照片
            Dictionary<string, string> _PhotoGDict = new Dictionary<string, string>();

            if (StudentList.Count != 0)
            {
                // 入學照片
                _PhotoPDict = K12.Data.Photo.SelectFreshmanPhoto(StudentIDList);

                // 畢業照片
                _PhotoGDict = K12.Data.Photo.SelectGraduatePhoto(StudentIDList);
            }

            //處理照片
            foreach (string studnetID in dic.Keys)
            {
                if (_PhotoPDict.ContainsKey(studnetID))
                {
                    dic[studnetID].學生入學照片 = _PhotoPDict[studnetID];
                }

                if (_PhotoGDict.ContainsKey(studnetID))
                {
                    dic[studnetID].學生畢業照片 = _PhotoGDict[studnetID];
                }
            }

            return dic;
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            btnPrint.Enabled = true;

            if (e.Cancelled)
            {
                MsgBox.Show("作業已被中止!!");
            }
            else
            {
                if (e.Error == null)
                {
                    Document inResult = (Document)e.Result;

                    try
                    {
                        SaveFileDialog SaveFileDialog1 = new SaveFileDialog();

                        SaveFileDialog1.Filter = "Word (*.doc)|*.doc|所有檔案 (*.*)|*.*";
                        SaveFileDialog1.FileName = "社團參與證明單(英文)";

                        if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            inResult.Save(SaveFileDialog1.FileName);
                            Process.Start(SaveFileDialog1.FileName);
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
                        return;
                    }

                    this.Close();
                }
                else
                {
                    MsgBox.Show("列印資料發生錯誤\n" + e.Error.Message);
                }
            }


        }

        void MailMerge_MergeField(object sender, Aspose.Words.Reporting.MergeFieldEventArgs e)
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
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //取得設定檔
            Campus.Report.ReportConfiguration ConfigurationInCadre = new Campus.Report.ReportConfiguration(CadreConfig);
            //畫面內容(範本內容,預設樣式
            Campus.Report.TemplateSettingForm TemplateForm;
            if (ConfigurationInCadre.Template != null)
            {
                TemplateForm = new Campus.Report.TemplateSettingForm(ConfigurationInCadre.Template, new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單_英文_範本, Campus.Report.TemplateType.Word));
            }
            else
            {
                ConfigurationInCadre.Template = new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單_英文_範本, Campus.Report.TemplateType.Word);
                TemplateForm = new Campus.Report.TemplateSettingForm(ConfigurationInCadre.Template, new Campus.Report.ReportTemplate(Properties.Resources.社團參與證明單_英文_範本, Campus.Report.TemplateType.Word));
            }

            //預設名稱
            TemplateForm.DefaultFileName = "社團參與證明單_英文(範本)";
            //如果回傳為OK
            if (TemplateForm.ShowDialog() == DialogResult.OK)
            {
                //設定後樣試,回傳
                ConfigurationInCadre.Template = TemplateForm.Template;
                //儲存
                ConfigurationInCadre.Save();
            }
        }

        private void lbTempAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存新檔";
            sfd.FileName = "社團參與證明單_英文_合併欄位總表.doc";
            sfd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    fs.Write(Properties.Resources.社團參與證明單_英文_合併欄位總表, 0, Properties.Resources.社團參與證明單_英文_合併欄位總表.Length);
                    fs.Close();
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch
                {
                    FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "另存檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            EnglishTableForm sot = new EnglishTableForm();
            sot.ShowDialog();
        }
    }
}
