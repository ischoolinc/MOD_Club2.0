using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aspose.Cells;
using System.IO;
using FISCA.UDT;
using K12.Data;
using FISCA.Data;
using System.Data;
using System.Windows.Forms;
using FISCA.Presentation;
using System.Diagnostics;
using System.Xml.Linq;

namespace K12.Club.Volunteer.Report.匯出選社結果
{
    class ExportStudentClub
    {
        // DIC
        Dictionary<string, string> clubDic = new Dictionary<string, string>();
        public ExportStudentClub()
        {
            AccessHelper access = new AccessHelper();
            List<CLUBRecord> _clubList = access.Select<CLUBRecord>(/*"school_year = "+School.DefaultSchoolYear+" AND semester = "+ School.DefaultSemester*/).ToList();
            foreach (CLUBRecord club in _clubList)
            {
                clubDic.Add(club.UID, club.ClubName);
            }
            //建立Excel範本
            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.匯出選社結果_範本), FileFormatType.Excel2007Xlsx);

            //每一張
            Workbook prototype = new Workbook();
            prototype.Copy(template);
            Worksheet ptws = prototype.Worksheets[0];

            //範圍
            Range ptHeader = ptws.Cells.CreateRange(0, 4, false);
            Range ptEachRow = ptws.Cells.CreateRange(4, 1, false);

            //儲存資料
            Workbook wb = new Workbook();
            wb.Copy(prototype);
            //取得Sheet
            Worksheet ws = wb.Worksheets[0];

            //--
            QueryHelper qh = new QueryHelper();
            string selectSQL = string.Format(@"
                SELECT 
	                 student.id ,class.class_name,class.grade_year,seat_no,name,student_number,student.ref_class_id,
	                 scjoin.ref_club_id,clubrecord.club_name,clubrecord.school_year,clubrecord.semester,
                     volunteer.content
                FROM student 
                LEFT OUTER JOIN
                (
	                SELECT id,class_name,grade_year
	                FROM class
	                WHERE status = 1
                )class on class.id = student.ref_class_id
                LEFT OUTER JOIN
                (
	                SELECT ref_club_id,ref_student_id 
	                FROM $k12.scjoin.universal
                )scjoin on CAST(scjoin.ref_student_id AS integer) = student.id
                LEFT OUTER JOIN
                (
	                SELECT uid,club_name,school_year,semester
	                FROM $k12.clubrecord.universal
                )clubrecord on clubrecord.uid = CAST(scjoin.ref_club_id AS integer)
                LEFT OUTER JOIN
                (
	                SELECT ref_student_id,content
	                FROM $k12.volunteer.universal
                )volunteer on CAST(volunteer.ref_student_id AS integer)= student.id 
                WHERE 
                    status = 1 AND school_year = {0} AND semester = {1}
                ORDER BY grade_year,ref_class_id ,seat_no"
                , School.DefaultSchoolYear,School.DefaultSemester);
            // 取得在校學生與社團資料
            DataTable dt = qh.Select(selectSQL);
            int index = 1;
            foreach (DataRow dr in dt.Rows)
            {
                ws.Cells[index, 0].PutValue("" + dr["class_name"]);
                ws.Cells[index, 0].Style.Copy(ws.Cells[0, 0].Style);

                ws.Cells[index, 1].PutValue("" + dr["seat_no"]);
                ws.Cells[index, 1].Style.Copy(ws.Cells[0, 0].Style);

                ws.Cells[index, 2].PutValue("" + dr["name"]);
                ws.Cells[index, 2].Style.Copy(ws.Cells[0, 0].Style);

                ws.Cells[index, 3].PutValue("" + dr["student_number"]);
                ws.Cells[index, 3].Style.Copy(ws.Cells[0, 0].Style);

                ws.Cells[index, 4].PutValue("" + dr["club_name"]);
                ws.Cells[index, 4].Style.Copy(ws.Cells[0, 0].Style);

                for (int i = 0; i < 8;i++)
                {
                    ws.Cells[index, 5 + i].Style.Copy(ws.Cells[0, 0].Style);
                }

                if (dr["content"] != null && "" + dr["content"] != "")
                {
                    XDocument content = XDocument.Parse("" + dr["content"]);
                    List<XElement> clubList = content.Element("xml").Elements("Club").ToList();
                    int count = 5;
                    foreach (XElement club in clubList)
                    {
                        ws.Cells[index, count].PutValue(clubDic[club.Attribute("Ref_Club_ID").Value]);
                        
                        count++;
                    }
                }

                index++;
            }

            SaveFileDialog SaveFileDialog1 = new SaveFileDialog();
            SaveFileDialog1.Filter = "Excel (*.xls)|*.xls|所有檔案 (*.*)|*.*";
            SaveFileDialog1.FileName = "匯出選社結果";
            try
            {
                if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    wb.Save(SaveFileDialog1.FileName);
                    Process.Start(SaveFileDialog1.FileName);
                    MotherForm.SetStatusBarMessage("社團點名單,列印完成!!");
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
    }
}
