using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.DocumentValidator;
using FISCA.Data;
using System.Data;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 檢查學年度、學期、社團資料是否存在系統中
    /// </summary>
    class CheckStudentINCadres : IRowVaildator
    {
        private List<string> _listStudentNumber;

        QueryHelper qh = new QueryHelper();

        public CheckStudentINCadres()
        {
            _listStudentNumber = new List<string>();

            //所有預設幹部
            string sql = @"
SELECT student.student_number,student.name,cadresrecord.cadre_name,
cadresrecord.ref_club_id,clubrecord.school_year,clubrecord.semester,clubrecord.club_name
    FROM $k12.cadresrecord.universal cadresrecord
join student on student.id=(cadresrecord.ref_student_id)::int
join $k12.clubrecord.universal clubrecord on clubrecord.uid=(cadresrecord.ref_club_id)::int
";
            DataTable dt = qh.Select(sql);

            foreach (DataRow row in dt.Rows)
            {
                _listStudentNumber.Add(row["school_year"] + "," + row["semester"] + "," + row["club_name"] + "," + row["cadre_name"] + "," + row["student_number"]);
            }

            //社長
            string sql2 = @"
select clubrecord.school_year,clubrecord.semester,
clubrecord.club_name,clubrecord.president,clubrecord.vice_president,
student.name,student.student_number
from $k12.clubrecord.universal clubrecord
join student on student.id=(clubrecord.president)::int
";

            DataTable dt2 = qh.Select(sql2);
            foreach (DataRow row in dt2.Rows)
            {
                _listStudentNumber.Add(row["school_year"] + "," + row["semester"] + "," + row["club_name"] + "," +"社長" + "," + row["student_number"]);
            }

            //副社長
            string sql3 = @"
select clubrecord.school_year,clubrecord.semester,
clubrecord.club_name,clubrecord.president,clubrecord.vice_president,
student.name,student.student_number
from $k12.clubrecord.universal clubrecord
join student on student.id=(clubrecord.vice_president)::int
";

            DataTable dt3 = qh.Select(sql3);
            foreach (DataRow row in dt3.Rows)
            {
                _listStudentNumber.Add(row["school_year"] + "," + row["semester"] + "," + row["club_name"] + "," + "副社長" + "," + row["student_number"]);
            }

        }

        #region IRowVaildator 成員

        public bool Validate(IRowStream Value)
        {
            bool check = true;

            if (Value.Contains("學年度") && Value.Contains("學期") && Value.Contains("社團名稱") && Value.Contains("幹部名稱") && Value.Contains("學號"))
            {
                string SchoolYear = Value.GetValue("學年度");
                string Semester = Value.GetValue("學期");
                string CourseName = Value.GetValue("社團名稱");
                string CadreStudent = Value.GetValue("幹部名稱");
                string StudentNumber = Value.GetValue("學號");
                if (!string.IsNullOrEmpty(CourseName))
                {
                    string CourseKey = SchoolYear + "," + Semester + "," + CourseName + "," + CadreStudent + "," + StudentNumber;

                    if (_listStudentNumber.Contains(CourseKey))
                    {
                        check = false;
                    }
                }
            }

            return check;
        }

        /// <summary>
        /// 沒有提供自動修正
        /// </summary>
        public string Correct(IRowStream Value)
        {
            return string.Empty;
        }

        /// <summary>
        /// 傳回預設樣版
        /// </summary>
        public string ToString(string template)
        {
            return template;
        }

        #endregion
    }
}
