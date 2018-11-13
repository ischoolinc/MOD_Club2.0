using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using FISCA.Presentation;
using FISCA.Data;
using Aspose.Cells;

namespace K12.Club.Volunteer.Ribbon.Export
{
    public partial class frmExportSCJoin : BaseForm
    {
        private List<string> _listExportField;

        public frmExportSCJoin()
        {
            InitializeComponent();
        }

        private void wizard1_FinishButtonClick(object sender, CancelEventArgs e)
        {
            _listExportField = new List<string>();

            // 取得勾選欄位
            foreach (ListViewItem item in listViewEx1.Items)
            {
                if (item.Checked)
                {
                    _listExportField.Add(item.Text.Trim());
                }
            }

            // 取得使用者選取的社團
            List<string> listClubID = ClubAdmin.Instance.SelectedSource;
            if (listClubID.Count == 0)
            {
                MsgBox.Show("請先選取社團!");
                return;
            }
            
            // 取得資料庫 社團參與學生資料
            DataTable dt = getSCJoin(string.Join("','", listClubID));

            // 寫入資料表
            Workbook wb = new Workbook();
            wb.Worksheets[0].Name = "社團參與學生資料";
            fillData(wb,dt);

            // 儲存資料表
            save(wb);
        }

        /// <summary>
        /// 取得社團參與學生資料
        /// </summary>
        /// <param name="clubIDs"></param>
        /// <returns></returns>
        private DataTable getSCJoin(string clubIDs)
        {
            string sql = string.Format(@"
SELECT
    club.school_year
    , club.semester
    , club.club_name
    , student.name
    , student.student_number
    , class.class_name
    , student.seat_no
FROM
    $k12.scjoin.universal AS scjoin
    LEFT OUTER JOIN $k12.clubrecord.universal AS club
        ON club.uid = scjoin.ref_club_id::BIGINT
    LEFT OUTER JOIN student
        ON student.id = scjoin.ref_student_id::BIGINT
    LEFT OUTER JOIN class
        ON class.id = student.ref_class_id
WHERE
    scjoin.ref_club_id IN('{0}')
    AND student.id IS NOT NULL
    AND student.status IN (1,2)
            ", clubIDs);

            QueryHelper qh = new QueryHelper();
            DataTable dt = qh.Select(sql);

            return dt;
        }

        /// <summary>
        /// 將資料寫入資料表
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="dt"></param>
        private void fillData(Workbook wb,DataTable dt)
        {
            // HeaderText
            for (int i = 0; i < _listExportField.Count(); i++)
            {
                wb.Worksheets[0].Cells[0, i].PutValue(_listExportField[i]);
            }
            // Content
            int rowIndex = 1;
            foreach (DataRow row in dt.Rows)
            {
                if (rowIndex < 65536)
                {
                    for (int col = 0; col < _listExportField.Count(); col++)
                    {
                        switch (_listExportField[col])
                        {
                            case "學年度":
                                wb.Worksheets[0].Cells[rowIndex, col].PutValue("" + row["school_year"]);
                                break;
                            case "學期":
                                wb.Worksheets[0].Cells[rowIndex, col].PutValue("" + row["semester"]);
                                break;
                            case "社團名稱":
                                wb.Worksheets[0].Cells[rowIndex, col].PutValue("" + row["club_name"]);
                                break;
                            case "姓名":
                                wb.Worksheets[0].Cells[rowIndex, col].PutValue("" + row["name"]);
                                break;
                            case "學號":
                                wb.Worksheets[0].Cells[rowIndex, col].PutValue("" + row["student_number"]);
                                break;
                            case "班級":
                                wb.Worksheets[0].Cells[rowIndex, col].PutValue("" + row["class_name"]);
                                break;
                            case "座號":
                                wb.Worksheets[0].Cells[rowIndex, col].PutValue("" + row["seat_no"]);
                                break;
                        }
                    }
                }
                rowIndex++;
            }
        }

        /// <summary>
        /// 儲存資料表
        /// </summary>
        /// <param name="wb"></param>
        private void save(Workbook wb)
        {
            #region 儲存資料

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "匯出社團參與學生";
            saveFileDialog.FileName = "匯出社團參與學生.xls";
            saveFileDialog.Filter = "Excel (*.xls)|*.xls|所有檔案 (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                DialogResult result = new DialogResult();
                try
                {
                    wb.Save(saveFileDialog.FileName);
                    result = MsgBox.Show("檔案儲存完成，是否開啟檔案?", "是否開啟", MessageBoxButtons.YesNo);
                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                    return;
                }

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(saveFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MsgBox.Show("開啟檔案發生失敗:" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                this.Close();
            }

            #endregion
        }

        private void ckbxALL_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbxALL.Checked)
            {
                foreach (ListViewItem item in listViewEx1.Items)
                {
                    item.Checked = true;
                }
            }
            else
            {
                foreach (ListViewItem item in listViewEx1.Items)
                {
                    item.Checked = false;
                }
            }
        }
    }
}
