using FISCA.Presentation.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Across.Report
{
    public partial class EnglishTableForm : BaseForm
    {
        public EnglishTableForm()
        {
            InitializeComponent();
        }

        private void EnglishTableForm_Load(object sender, EventArgs e)
        {

            List<EnglishTable> EngList = tool._A.Select<EnglishTable>();
            foreach (EnglishTable each in EngList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridViewX1);
                row.Cells[0].Value = each.ClubName;
                row.Cells[1].Value = each.English;
                dataGridViewX1.Rows.Add(row);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            tool._A.DeletedValues(tool._A.Select<EnglishTable>());

            List<EnglishTable> list = new List<EnglishTable>();

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow)
                    continue;

                EnglishTable et = new EnglishTable();
                et.ClubName = "" + row.Cells[0].Value;
                et.English = "" + row.Cells[1].Value;
                list.Add(et);
            }

            tool._A.InsertValues(list);

            MsgBox.Show("儲存完成!!");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
