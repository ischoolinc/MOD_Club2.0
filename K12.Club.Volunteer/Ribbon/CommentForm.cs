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
        /// �i�H�x�s,���
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
        /// ��sDataGridView
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
            //�p�G�i�H�x�s
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
                        //�R�����

                        //�s�W���

                        //�s�WLog
                        ApplicationLog.Log("���ε��y�N�X��", "�ק�", "�u���ε��y�N�X��v�w�Q�ק�C");
                    }
                    catch (Exception ex)
                    {
                        MsgBox.Show("�x�s���ѡC\n" + ex.Message);
                    }

                    MsgBox.Show("�x�s���\�C");
                    this.DialogResult = DialogResult.OK;
                }
            }
        }

        private void DisciplineForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_commentCanSave)
            {
                if (FISCA.Presentation.Controls.MsgBox.Show("��Ʃ|���x�s�A�z�T�w�n���}�H", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    e.Cancel = true;
            }
        }

        /// <summary>
        /// �ץX�N�X��
        /// </summary>
        private void btnExport_Click(object sender, EventArgs e)
        {
            #region �ץX
            Workbook wb = new Workbook();
            wb.Worksheets.Clear();
            Worksheet ws = wb.Worksheets[wb.Worksheets.Add()];
            ws.Name = "���ε��y�N�X��";

            ws.Cells.CreateRange(0, 1, true).ColumnWidth = 10;
            ws.Cells.CreateRange(1, 1, true).ColumnWidth = 8;
            ws.Cells.CreateRange(2, 1, true).ColumnWidth = 40;

            ws.Cells[0, 0].PutValue("�N�X");
            ws.Cells[0, 1].PutValue("���y");

            int rowIndex = 1;

            foreach (ClubComment each in _commentList)
            {
                ws.Cells[rowIndex, 0].PutValue(each.code);
                ws.Cells[rowIndex, 1].PutValue(each.Comment);
                rowIndex++;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "�t�s�s��";
            sfd.FileName = "���ε��y�N�X��.xlsx";
            sfd.Filter = "Excel�ɮ� (*.xlsx)|*.xlsx|�Ҧ��ɮ� (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    wb.Save(sfd.FileName);
                    FISCA.Presentation.Controls.MsgBox.Show("�ץX�����C");
                }
                catch
                {
                    FISCA.Presentation.Controls.MsgBox.Show("���w���|�L�k�s���C", "�t�s�ɮץ���", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                ApplicationLog.Log("���ε��y�N�X��", "�ץX", "�u���ε��y�N�X��v�w�Q�ץX�C");
            }
            #endregion
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            #region �פJ
            Workbook wb = new Workbook();
            Dictionary<string, string> importMeritList = new Dictionary<string, string>();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "��ܭn�פJ���ƥѥN�X��";
            ofd.Filter = "Excel�ɮ� (*.xlsx)|*.xlsx";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    wb = new Workbook(ofd.FileName);
                }
                catch
                {
                    FISCA.Presentation.Controls.MsgBox.Show("���w���|�L�k�s���C", "�}���ɮץ���", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
                return;

            List<string> requiredHeaders = new List<string>(new string[] { "�N�X", "���y" });
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
                builder.AppendLine("�פJ�榡���ŦX�C");
                builder.AppendLine("�פJ��Ƽ��D�����]�t�G");
                builder.AppendLine(string.Join(",", requiredHeaders.ToArray()));
                FISCA.Presentation.Controls.MsgBox.Show(builder.ToString());
                return;
            }

            int rowIndex = 1;

            List<ClubComment> ClubCommentList = new List<ClubComment>();
            while (!string.IsNullOrEmpty(ws.Cells[rowIndex, 0].StringValue))
            {
                string codeString = ws.Cells[rowIndex, headerIndexes["�N�X"]].StringValue;
                string commentString = ws.Cells[rowIndex, headerIndexes["���y"]].StringValue;

                ClubComment commentRecord = new ClubComment();
                commentRecord.code = codeString;
                commentRecord.Comment = commentString;
                ClubCommentList.Add(commentRecord);
                rowIndex++;
            }

            FillGrid(ClubCommentList);

            ApplicationLog.Log("���ε��y�N�X��", "�פJ", "�u���ε��y�N�X��v�w�Q�פJ�÷s�W�C");
            #endregion

            FISCA.Presentation.Controls.MsgBox.Show("�w�פJ����!\n���I���x�s�����}�C");
        }

        /// <summary>
        /// ����ܧ�����
        /// </summary>
        private void dataGridViewX1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            ValidateList();
        }

        /// <summary>
        /// �ק��ƧY����
        /// </summary>
        private void dataGridViewX1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ValidateList();
        }

        /// <summary>
        /// ���Ҹ�ƥ��T��
        /// </summary>
        private bool ValidateList()
        {
            dataGridViewX1.EndEdit();
            bool isValid = true;

            //�ק�L���
            _commentCanSave = false;

            List<string> codeList = new List<string>(); //�N�X
            List<string> commentList = new List<string>(); //�ƥ�

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) continue;

                string code = "" + row.Cells[colCode.Index].Value.ToString();
                if (string.IsNullOrEmpty(code))
                {
                    row.Cells[colCode.Index].ErrorText = "�N�X���ର�ť�";
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
                    row.Cells[colCode.Name].ErrorText = "�N�X����";
                    isValid = false;
                    break;
                }

                string comment = "" + row.Cells[colComment.Index].Value.ToString();
                if (string.IsNullOrEmpty(code))
                {
                    row.Cells[colComment.Index].ErrorText = "�ƥѤ��ର�ť�";
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
                    row.Cells[colComment.Name].ErrorText = "�ƥѭ���";
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