
using Aspose.Words;
using Campus.ePaperCloud;
using FISCA.Presentation.Controls;
using K12.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace K12.Club.Volunteer
{
    public partial class ReportTeacherForm : BaseForm
    {
        private string ConfigName = "社團選社結果通知_2023_v1";

        ePaperCloud ePaperCloud = new ePaperCloud();
        private BackgroundWorker _BGWDisciplineNotification;


        string schoolyear = K12.Data.School.DefaultSchoolYear;
        string semester = K12.Data.School.DefaultSemester;

        string _defSchoolyear { get; set; }
        string _defSemester { get; set; }

        List<社團志願分配的Row> _ClasssList;
        Setup_ByV _By_V { get; set; }

        public ReportTeacherForm(List<社團志願分配的Row> ClasssList, string defSchoolYear, string defSemester, Setup_ByV By_V)
        {
            InitializeComponent();

            _ClasssList = ClasssList;
            _defSchoolyear = defSchoolYear;
            _defSemester = defSemester;
            _By_V = By_V;

            _BGWDisciplineNotification = new BackgroundWorker();
            _BGWDisciplineNotification.DoWork += _BGWDisciplineNotification_DoWork;
            _BGWDisciplineNotification.RunWorkerCompleted += _BGWDisciplineNotification_RunWorkerCompleted;

        }

        private void _BGWDisciplineNotification_DoWork(object sender, DoWorkEventArgs e)
        {
            #region 產生報表

            Campus.Report2014.ReportConfiguration ConfigurationInCadre = new Campus.Report2014.ReportConfiguration(ConfigName);
            Aspose.Words.Document template;
            if (ConfigurationInCadre.Template == null)
            {
                //如果範本為空,則建立一個預設範本
                Campus.Report2014.ReportConfiguration ConfigurationInCadre_1 = new Campus.Report2014.ReportConfiguration(ConfigName);
                ConfigurationInCadre_1.Template = new Campus.Report2014.ReportTemplate(Properties.Resources.社團選社結果通知範本, Campus.Report2014.TemplateType.docx);
                template = ConfigurationInCadre_1.Template.ToDocument();
            }
            else
            {
                //如果已有範本,則取得樣板
                template = ConfigurationInCadre.Template.ToDocument();
            }

            Aspose.Words.Document doc = new Aspose.Words.Document();
            doc.Sections.Clear();

            //整理社團資資料
            List<CLUBRecord> ClubList = tool._A.Select<CLUBRecord>(string.Format("school_year={0} and semester={1}", _defSchoolyear, _defSemester));
            Dictionary<string, CLUBRecord> ClubDic = new Dictionary<string, CLUBRecord>();
            foreach (CLUBRecord club in ClubList)
            {
                if (!ClubDic.ContainsKey(club.UID))
                    ClubDic.Add(club.UID, club);
            }

            //處理列印資料狀態
            foreach (社團志願分配的Row _VolRow in _ClasssList)
            {
                foreach (一名學生 studPage in _VolRow._StudentDic.Values)
                {
                    //當此學生有社團參與記錄時
                    if (_VolRow._SCJDic.ContainsKey(studPage.student_id))
                    {

                        Aspose.Words.Document eachStudent = new Aspose.Words.Document();
                        eachStudent.Sections.Clear();
                        eachStudent.Sections.Add(eachStudent.ImportNode(template.Sections[0], true));

                        //合併列印的資料
                        Dictionary<string, object> mapping = new Dictionary<string, object>();

                        #region 當此學生有社團參與記錄時

                        mapping.Add("學年度", schoolyear);
                        mapping.Add("學期", semester);
                        mapping.Add("選社學年度", _defSchoolyear);
                        mapping.Add("選社學期", _defSemester);

                        mapping.Add("系統編號", studPage.student_id);
                        mapping.Add("班級", studPage.class_name);
                        mapping.Add("姓名", studPage.student_name);
                        mapping.Add("座號", studPage.seat_no);
                        mapping.Add("學號", studPage.student_number);


                        SCJoin scj = _VolRow._SCJDic[studPage.student_id];
                        if (_VolRow._ClubDic.ContainsKey(scj.RefClubID))
                        {
                            CLUBRecord club = _VolRow._ClubDic[scj.RefClubID];

                            mapping.Add("入選社團", club.ClubName);
                        }

                        #endregion

                        if (_VolRow._Volunteer.ContainsKey(studPage.student_id))
                        {
                            #region 必須有填志願才會被填入社團資料
                            //學生基本資料
                            VolunteerRecord obj = _VolRow._Volunteer[studPage.student_id];

                            //取得單一學生志願序選填狀況
                            if (!string.IsNullOrEmpty(obj.Content))
                            {
                                XmlElement Element = XmlHelper.LoadXml(obj.Content);
                                int 志願數 = 1;
                                foreach (XmlElement xml in Element.SelectNodes("Club"))
                                {
                                    //所選填的必須只有設定之數量
                                    int ClubIndex = 0;
                                    int.TryParse(xml.GetAttribute("Index"), out ClubIndex);
                                    if (ClubIndex <= _By_V.學生選填志願數 && ClubIndex != 0)
                                    {
                                        string clubID = xml.GetAttribute("Ref_Club_ID");
                                        //是否包含此社團
                                        if (ClubDic.ContainsKey(clubID))
                                        {
                                            CLUBRecord cr = ClubDic[clubID];

                                            mapping.Add("志願" + 志願數, cr.ClubName);
                                            志願數++;
                                        }
                                    }
                                }
                            }
                            #endregion
                        }

                        string[] keys = new string[mapping.Count];
                        object[] values = new object[mapping.Count];
                        int i = 0;
                        foreach (string key in mapping.Keys)
                        {
                            keys[i] = key;
                            values[i++] = mapping[key];
                        }

                        eachStudent.MailMerge.CleanupOptions = Aspose.Words.Reporting.MailMergeCleanupOptions.RemoveEmptyParagraphs;
                        //eachDoc.MailMerge.FieldMergingCallback = new HandleMergeImageFieldFromBlob();
                        eachStudent.MailMerge.Execute(keys, values);
                        eachStudent.MailMerge.DeleteFields(); //刪除未合併之內容

                        MemoryStream stream = new MemoryStream();
                        eachStudent.Save(stream, SaveFormat.Docx);


                        //加入文件
                        foreach (Aspose.Words.Section each in eachStudent.Sections)
                        {
                            Aspose.Words.Node eachSectionNode = each.Clone();
                            doc.Sections.Add(doc.ImportNode(eachSectionNode, true));
                        }

                    }

                }
            }

            #endregion


            string ReportName = string.Format("選社結果通知單「{0}」學年度 第「{1}」學期", _defSchoolyear, _defSemester);
            string path = Path.Combine(Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, ReportName + ".docx");

            e.Result = new object[] { ReportName, path, doc };
        }

        private void _BGWDisciplineNotification_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnPort.Enabled = true;
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    #region 列印

                    object[] result = (object[])e.Result;

                    string reportName = (string)result[0];
                    string path = (string)result[1];
                    Aspose.Words.Document doc = (Aspose.Words.Document)result[2];

                    //是否列印電子報表
                    MemoryStream memoryStream = new MemoryStream();
                    doc.Save(memoryStream, Aspose.Words.SaveFormat.Docx);

                    string message = string.Format("【電子報表通知】親愛的同學 您好 「{0} 」學年度 第「 {1}」學期「選社結果通知」已產生,可於電子報表中檢視", _defSchoolyear, _defSemester);

                    ePaperCloud.upload_ePaper(int.Parse(schoolyear), int.Parse(semester), reportName, "", memoryStream, ePaperCloud.ViewerType.Student,
                        ePaperCloud.FormatType.Docx, message);

                    #endregion
                }
                else
                {
                    MsgBox.Show("發生錯誤:\n" + e.Error.Message);
                }
            }
            else
            {
                MsgBox.Show("列印失敗,未取得資料!");
            }
        }

        private void btnPort_Click(object sender, EventArgs e)
        {
            if (!_BGWDisciplineNotification.IsBusy)
            {
                btnPort.Enabled = false;

                //開始列印
                _BGWDisciplineNotification.RunWorkerAsync();
            }
            else
            {
                btnPort.Enabled = true;
                MsgBox.Show("系統忙碌中.");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存新檔";
            sfd.FileName = "社團選設結果通知_功能變數總表.docx";
            sfd.Filter = "Word檔案 (*.docx)|*.docx|所有檔案 (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    fs.Write(Properties.Resources.社團選社結果通知_功能變數總表, 0, Properties.Resources.社團選社結果通知_功能變數總表.Length);
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

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //取得設定檔
            Campus.Report2014.ReportConfiguration ConfigurationInCadre = new Campus.Report2014.ReportConfiguration(ConfigName);
            //畫面內容(範本內容,預設樣式

            Campus.Report2014.ReportTemplate tempRpt = new Campus.Report2014.ReportTemplate(Properties.Resources.社團選社結果通知範本, Campus.Report2014.TemplateType.docx);

            // 假設完全沒有
            if (ConfigurationInCadre.Template == null)
                ConfigurationInCadre.Template = tempRpt;

            Campus.Report2014.TemplateSettingForm TemplateForm = new Campus.Report2014.TemplateSettingForm(ConfigurationInCadre.Template, tempRpt);
            //預設名稱
            TemplateForm.DefaultFileName = "社團選社結果通知範本(樣版)";
            //如果回傳為OK
            if (TemplateForm.ShowDialog() == DialogResult.OK)
            {
                //設定後樣試,回傳
                ConfigurationInCadre.Template = TemplateForm.Template;
                //儲存
                ConfigurationInCadre.Save();
            }
        }

    }
}
