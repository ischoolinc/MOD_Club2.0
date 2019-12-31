using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using Aspose.Words;
using FISCA.UDT;
using FISCA.Data;
using K12.Data;
using FISCA.Presentation;
using System.Diagnostics;
using System.IO;
using Aspose.Words.Tables;
using K12.Club.Volunteer;

namespace MOD_Club20.Report.ForHCH
{
    public partial class ElectionForm : BaseForm
    {
        private BackgroundWorker BGW = new BackgroundWorker();
        //主文件
        private Document _doc = new Document();

        //移動使用
        private Run _run;
        Document _template;

        AccessHelper _AccessHelper = new AccessHelper();
        QueryHelper _QueryHelper = new QueryHelper();

        int _SchoolYear = 90;
        int _Semester = 1;

        //學生ID:特殊物件
        Dictionary<string, StudentSCjoinObj> SCjoinObjDic = new Dictionary<string, StudentSCjoinObj>();
        //學生ID:班級Record
        Dictionary<string, ClassRecord> ClassDic = new Dictionary<string, ClassRecord>();
        //學生ID:學生List
        Dictionary<string, List<StudentRecord>> ClassByStudentDic = new Dictionary<string, List<StudentRecord>>();

        string RePortName = "班級社團參與學生清單";

        /// <summary>
        /// 列印模式預設為所有
        /// </summary>
        NowPrint np = NowPrint.all;

        public ElectionForm()
        {
            InitializeComponent();
        }

        private void ElectionForm_Load(object sender, EventArgs e)
        {
            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);

            int.TryParse(School.DefaultSchoolYear, out _SchoolYear);
            int.TryParse(School.DefaultSemester, out _Semester);

            intSchoolYear.Value = _SchoolYear;
            intSemester.Value = _Semester;
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (BGW.IsBusy)
            {
                MsgBox.Show("系統忙碌中");
            }
            else
            {
                btnPrint.Enabled = false;
                _SchoolYear = intSchoolYear.Value;
                _Semester = intSemester.Value;
                BGW.RunWorkerAsync();
            }
        }

        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            //取得學生資料
            GetAndSortStudent();

            _template = new Document(new MemoryStream(Properties.Resources.班級學生選社_範本));

            _doc.Sections.Clear();

            foreach (string each in ClassByStudentDic.Keys)
            {
                Document PageOne = SetDocument(each);
                if (PageOne != null)
                {
                    _doc.Sections.Add(_doc.ImportNode(PageOne.FirstSection, true));
                }
            }

            e.Result = _doc;
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

                        SaveFileDialog1.Filter = "Word (*.docx)|*.docx|所有檔案 (*.*)|*.*";
                        SaveFileDialog1.FileName = RePortName;

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

        /// <summary>
        /// 每一名學生的報表資料列印
        /// </summary>
        /// <returns></returns>
        private Document SetDocument(string classID)
        {
            List<StudentRecord> CheckList = ClassByStudentDic[classID];

            List<StudentRecord> StudentList = new List<StudentRecord>();

            if (np == NowPrint.empty) //未選
            {
                foreach (StudentRecord each in CheckList)
                {
                    StudentSCjoinObj scj = SCjoinObjDic[each.ID];
                    if (scj.CLUBRecord.Count == 0)
                    {
                        StudentList.Add(each);
                    }
                }
            }
            else if (np == NowPrint.General) //已選
            {
                foreach (StudentRecord each in CheckList)
                {
                    StudentSCjoinObj scj = SCjoinObjDic[each.ID];
                    if (scj.CLUBRecord.Count == 1)
                    {
                        StudentList.Add(each);
                    }
                }
            }
            else if (np == NowPrint.Repeat) //重覆
            {
                foreach (StudentRecord each in CheckList)
                {
                    StudentSCjoinObj scj = SCjoinObjDic[each.ID];
                    if (scj.CLUBRecord.Count > 1)
                    {
                        StudentList.Add(each);
                    }
                }
            }
            else
            {
                StudentList.AddRange(CheckList);
            }

            //如果沒資料回傳null
            if (StudentList.Count == 0)
                return null;

            //取得範本樣式
            Document PageOne = (Document)_template.Clone(true);

            #region MailMerge
            List<string> name = new List<string>();
            List<string> value = new List<string>();

            name.Add("名稱");
            value.Add(RePortName);


            name.Add("班級");
            value.Add(Class.SelectByID(classID).Name);

            name.Add("學年度");
            value.Add(School.DefaultSchoolYear);

            name.Add("學期");
            value.Add(School.DefaultSemester);

            PageOne.MailMerge.Execute(name.ToArray(), value.ToArray());
            #endregion

            //???
            _run = new Run(PageOne);
            //可建構的...
            DocumentBuilder builder = new DocumentBuilder(PageOne);

            builder.MoveToMergeField("資料");
            Cell cell = (Cell)builder.CurrentParagraph.ParentNode;

            //取得目前Row
            Row 日3row = (Row)cell.ParentRow;

            int addcount = 0;
            foreach (StudentRecord each in StudentList)
            {
                StudentSCjoinObj scj = SCjoinObjDic[each.ID];
                foreach (CLUBRecord club in scj.CLUBRecord)
                {
                    addcount++;
                }
            }

            //除了原來的Row-1,高於1就多建立幾行
            for (int x = 1; x < addcount; x++)
            {
                cell.ParentRow.ParentTable.InsertAfter(日3row.Clone(true), cell.ParentNode);
            }

            foreach (StudentRecord each in StudentList)
            {
                StudentSCjoinObj scj = SCjoinObjDic[each.ID];
                foreach (CLUBRecord club in scj.CLUBRecord)
                {
                    //座號
                    Write(cell, scj.SeatNo);
                    cell = GetMoveRightCell(cell, 1);

                    Write(cell, scj.Name);
                    cell = GetMoveRightCell(cell, 1);

                    Write(cell, scj.StudentNumber);
                    cell = GetMoveRightCell(cell, 1);

                    Write(cell, scj.Gender);
                    cell = GetMoveRightCell(cell, 1);

                    Write(cell, club.ClubName);
                    cell = GetMoveRightCell(cell, 1);

                    //社團類型
                    Write(cell, club.ClubCategory);
                    cell = GetMoveRightCell(cell, 1);

                    //學生擔任幹部
                    List<string> list = new List<string>();
                    if (club.President == each.ID)
                        list.Add("社長"); //社長

                    if (club.VicePresident == each.ID)
                        list.Add("副社長");

                    List<CadresRecord> CadreList = tool._A.Select<CadresRecord>("ref_club_id='" + club.UID + "'");
                    foreach (CadresRecord cadre in CadreList)
                    {
                        if (cadre.RefStudentID == each.ID)
                        {
                            list.Add(cadre.CadreName);
                        }
                    }

                    if (list.Count > 0)
                    {
                        Write(cell, string.Join(",", list));
                    }

                    Row Nextrow = cell.ParentRow.NextSibling as Row; //取得下一行
                    if (Nextrow == null)
                    {
                        break;
                    }
                    cell = Nextrow.FirstCell; //第一格


                }
            }

            return PageOne;
        }

        /// <summary>
        /// 取得學生的社團記錄
        /// </summary>
        private void GetAndSortStudent()
        {
            SCjoinObjDic.Clear();
            ClassByStudentDic.Clear();

            //取得班級清單
            List<string> ClassIDList = K12.Presentation.NLDPanels.Class.SelectedSource;


            foreach (ClassRecord each in Class.SelectByIDs(ClassIDList))
            {
                if (!ClassDic.ContainsKey(each.ID))
                {
                    ClassDic.Add(each.ID, each);
                }
            }

            //取得學生清單
            List<StudentRecord> studentList = K12.Data.Student.SelectByClassIDs(ClassIDList);

            studentList = SortClassIndex.K12Data_StudentRecord(studentList);

            foreach (StudentRecord stud in studentList)
            {
                if (CheckStatus(stud))
                {
                    if (string.IsNullOrEmpty(stud.RefClassID))
                        continue;

                    //班級:學生ID清單:學生Record
                    if (!ClassByStudentDic.ContainsKey(stud.RefClassID))
                    {
                        ClassByStudentDic.Add(stud.RefClassID, new List<StudentRecord>());
                    }

                    if (!ClassByStudentDic[stud.RefClassID].Contains(stud))
                    {
                        ClassByStudentDic[stud.RefClassID].Add(stud);
                    }

                    //學生ID:特殊Obj
                    StudentSCjoinObj sc = new StudentSCjoinObj(stud);
                    if (!SCjoinObjDic.ContainsKey(stud.ID))
                    {
                        SCjoinObjDic.Add(stud.ID, sc);
                    }
                }
            }

            List<string> StudentIDList = new List<string>();
            foreach (StudentRecord sr in studentList)
            {
                if (!StudentIDList.Contains(sr.ID))
                {
                    StudentIDList.Add(sr.ID);
                }
            }

            //由學生ID去比對SCJoin
            List<SCJoin> SCJoinLIst = _AccessHelper.Select<SCJoin>("ref_student_id in ('" + string.Join("','", StudentIDList) + "')");

            List<CLUBRecord> CLUBList = GetCLUB(SCJoinLIst);
            Dictionary<string, CLUBRecord> CLUBDic = new Dictionary<string, CLUBRecord>();
            foreach (CLUBRecord each in CLUBList)
            {
                if (each.SchoolYear == _SchoolYear && each.Semester == _Semester)
                {
                    if (!CLUBDic.ContainsKey(each.UID))
                    {
                        CLUBDic.Add(each.UID, each);
                    }
                }
            }

            //由SCJoin的ref_club_id取得社團資料
            foreach (SCJoin each in SCJoinLIst)
            {
                if (SCjoinObjDic.ContainsKey(each.RefStudentID))
                {
                    if (CLUBDic.ContainsKey(each.RefClubID))
                    {
                        SCjoinObjDic[each.RefStudentID].CLUBRecord.Add(CLUBDic[each.RefClubID]);
                    }
                }
            }
        }

        static public bool CheckStatus(StudentRecord student)
        {
            if (student.Status == StudentRecord.StudentStatus.一般 || student.Status == StudentRecord.StudentStatus.延修)
            {
                return true;
            }
            else
            {
                return false;
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
            _run.Font.Size = 10;
            _run.Font.Name = "標楷體";
            cell.FirstParagraph.Runs.Add(_run.Clone(true));
        }

        /// <summary>
        /// 以Cell為基準,向右移一格
        /// </summary>
        private Cell GetMoveRightCell(Cell cell, int count)
        {
            if (count == 0) return cell;

            Row row = cell.ParentRow;
            int col_index = row.IndexOf(cell);
            Table table = row.ParentTable;
            int row_index = table.Rows.IndexOf(row);

            try
            {
                return table.Rows[row_index].Cells[col_index + count];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<CLUBRecord> GetCLUB(List<SCJoin> SCJoinLIst)
        {
            if (SCJoinLIst.Count != 0)
            {
                List<string> list = new List<string>();
                foreach (SCJoin each in SCJoinLIst)
                {
                    if (!list.Contains(each.RefClubID))
                    {
                        list.Add(each.RefClubID);
                    }
                }
                return _AccessHelper.Select<CLUBRecord>(list);
            }
            else
            {
                return new List<CLUBRecord>();
            }
        }

        //全部,一般,未選,重覆
        enum NowPrint
        {
            all,
            General,
            empty,
            Repeat
        };

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
