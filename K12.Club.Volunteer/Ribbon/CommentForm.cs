using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Aspose.Cells;
using FISCA.DSAUtil;
using Framework.Feature;
using FISCA.Presentation.Controls;
using FISCA.LogAgent;

namespace K12.Club.Volunteer
{
    public partial class CommentForm : FISCA.Presentation.Controls.BaseForm
    {
        private Dictionary<string, ClubComment> _commentDic = new Dictionary<string, ClubComment>();

        List<ClubComment> _commentList { get; set; }

        /// <summary>
        /// 可以儲存,表示
        /// </summary>
        bool _commentCanSave = true;

        public CommentForm()
        {
            InitializeComponent();

            Campus.Windows.DataGridViewImeDecorator dec = new Campus.Windows.DataGridViewImeDecorator(this.dataGridViewX1);
            _commentList = tool._A.Select<ClubComment>();
            foreach (ClubComment each in _commentList)
            {
                if (!_commentDic.ContainsKey(each.code))
                {
                    _commentDic.Add(each.code, each);
                }
            }
            _commentDic = new Dictionary<string, ClubComment>();
        }

        /// <summary>
        /// 更新DataGridView
        /// </summary>
        private void FillGrid(List<ClubComment> list)
        {
            dataGridViewX1.Rows.Clear();
            foreach (ClubComment code in list)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.Cells[colCode.Index].Value = code.code;
                row.Cells[colComment.Index].Value = code.Comment;
                dataGridViewX1.Rows.Add(row);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //如果可以儲存
            if (_commentCanSave)
            {
                List<ClubComment> InsertCommentList = new List<ClubComment>();
                foreach (DataGridViewRow row in dataGridViewX1.Rows)
                {
                    ClubComment comment = new ClubComment();
                    comment.code = "" + row.Cells[colCode.Index].Value;
                    comment.Comment = "" + row.Cells[colComment.Index].Value;
                    InsertCommentList.Add(comment);
                }

                if (InsertCommentList.Count > 0)
                {
                    try
                    {
                        //刪除資料

                        //新增資料

                        //新增Log
                        ApplicationLog.Log("社團評語代碼表", "修改", "「社團評語代碼表」已被修改。");
                    }
                    catch (Exception ex)
                    {
                        MsgBox.Show("儲存失敗。\n" + ex.Message);
                    }

                    MsgBox.Show("儲存成功。");
                    this.DialogResult = DialogResult.OK;
                }
            }
        }

        private void DisciplineForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_commentCanSave)
            {
                if (FISCA.Presentation.Controls.MsgBox.Show("資料尚未儲存，您確定要離開？", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    e.Cancel = true;
            }
        }

        /// <summary>
        /// 匯出代碼表
        /// </summary>
        private void btnExport_Click(object sender, EventArgs e)
        {
            #region 匯出
            Workbook wb = new Workbook();
            wb.Worksheets.Clear();
            Worksheet ws = wb.Worksheets[wb.Worksheets.Add()];
            ws.Name = "社團評語代碼表";

            ws.Cells.CreateRange(0, 1, true).ColumnWidth = 10;
            ws.Cells.CreateRange(1, 1, true).ColumnWidth = 8;
            ws.Cells.CreateRange(2, 1, true).ColumnWidth = 40;

            ws.Cells[0, 0].PutValue("代碼");
            ws.Cells[0, 1].PutValue("評語");

            int rowIndex = 1;

            foreach (ClubComment each in _commentList)
            {
                ws.Cells[rowIndex, 0].PutValue(each.code);
                ws.Cells[rowIndex, 1].PutValue(each.Comment);
                rowIndex++;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存新檔";
            sfd.FileName = "社團評語代碼表.xlsx";
            sfd.Filter = "Excel檔案 (*.xlsx)|*.xlsx|所有檔案 (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    wb.Save(sfd.FileName);
                    FISCA.Presentation.Controls.MsgBox.Show("匯出完成。");
                }
                catch
                {
                    FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "另存檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                ApplicationLog.Log("社團評語代碼表", "匯出", "「社團評語代碼表」已被匯出。");
            }
            #endregion
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            #region 匯入
            Workbook wb = new Workbook();
            Dictionary<string, string> importMeritList = new Dictionary<string, string>();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "選擇要匯入的事由代碼表";
            ofd.Filter = "Excel檔案 (*.xlsx)|*.xlsx";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    wb = new Workbook(ofd.FileName);
                }
                catch
                {
                    FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "開啟檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
                return;

            List<string> requiredHeaders = new List<string>(new string[] { "代碼", "評語" });
            Dictionary<string, int> headerIndexes = new Dictionary<string, int>();
            Worksheet ws = wb.Worksheets[0];
            for (int i = 0; i <= ws.Cells.MaxDataColumn; i++)
            {
                string header = ws.Cells[0, i].StringValue;
                if (requiredHeaders.Contains(header))
                    headerIndexes.Add(header, i);
            }
            if (headerIndexes.Count != requiredHeaders.Count)
            {
                StringBuilder builder = new StringBuilder("");
                builder.AppendLine("匯入格式不符合。");
                builder.AppendLine("匯入資料標題必須包含：");
                builder.AppendLine(string.Join(",", requiredHeaders.ToArray()));
                FISCA.Presentation.Controls.MsgBox.Show(builder.ToString());
                return;
            }

            int rowIndex = 1;

            List<ClubComment> ClubCommentList = new List<ClubComment>();
            while (!string.IsNullOrEmpty(ws.Cells[rowIndex, 0].StringValue))
            {
                string codeString = ws.Cells[rowIndex, headerIndexes["代碼"]].StringValue;
                string commentString = ws.Cells[rowIndex, headerIndexes["評語"]].StringValue;

                ClubComment commentRecord = new ClubComment();
                commentRecord.code = codeString;
                commentRecord.Comment = commentString;
                ClubCommentList.Add(commentRecord);
                rowIndex++;
            }

            FillGrid(ClubCommentList);

            ApplicationLog.Log("社團評語代碼表", "匯入", "「社團評語代碼表」已被匯入並新增。");
            #endregion

            FISCA.Presentation.Controls.MsgBox.Show("已匯入完成!\n請點選儲存後離開。");
        }

        /// <summary>
        /// 資料變更驗證
        /// </summary>
        private void dataGridViewX1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            ValidateList();
        }

        /// <summary>
        /// 修改資料即驗證
        /// </summary>
        private void dataGridViewX1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ValidateList();
        }

        /// <summary>
        /// 驗證資料正確性
        /// </summary>
        private bool ValidateList()
        {
            dataGridViewX1.EndEdit();
            bool isValid = true;

            //修改過資料
            _commentCanSave = false;

            List<string> codeList = new List<string>(); //代碼
            List<string> commentList = new List<string>(); //事由

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) continue;

                string code = "" + row.Cells[colCode.Index].Value.ToString();
                if (string.IsNullOrEmpty(code))
                {
                    row.Cells[colCode.Index].ErrorText = "代碼不能為空白";
                    isValid = false;
                    break;
                }
                else
                    row.Cells[colCode.Index].ErrorText = "";

                if (!codeList.Contains(code))
                {
                    codeList.Add(code);
                    row.Cells[colCode.Index].ErrorText = "";
                }
                else
                {
                    row.Cells[colCode.Name].ErrorText = "代碼重複";
                    isValid = false;
                    break;
                }

                string comment = "" + row.Cells[colComment.Index].Value.ToString();
                if (string.IsNullOrEmpty(code))
                {
                    row.Cells[colComment.Index].ErrorText = "事由不能為空白";
                    isValid = false;
                    break;
                }
                else
                    row.Cells[colComment.Index].ErrorText = "";

                if (!commentList.Contains(code))
                {
                    commentList.Add(code);
                    row.Cells[colComment.Index].ErrorText = "";
                }
                else
                {
                    row.Cells[colComment.Name].ErrorText = "事由重複";
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }

        private void dataGridViewX1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}