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

        public CheckStudentINCadres()
        {
            this._listStudentNumber = new List<string>();

            string sql = @"
SELECT student.student_number,student.name,cadresrecord.cadre_name,
cadresrecord.ref_club_id,clubrecord.school_year,clubrecord.semester
    FROM $k12.cadresrecord.universal cadresrecord
join student on student.id=(cadresrecord.ref_student_id)::int
join $k12.clubrecord.universal clubrecord on clubrecord.uid=(cadresrecord.ref_club_id)::int
";

            QueryHelper qh = new QueryHelper();
            DataTable dt = qh.Select(sql);

            foreach (DataRow row in dt.Rows)
            {
                this._listStudentNumber.Add("" + row["school_year"] + row["semester"] + row["cadre_name"] + row["student_number"]);
            }
        }

        #region IRowVaildator 成員

        public bool Validate(IRowStream Value)
        {
            bool check = true;

            if (Value.Contains("學年度") && Value.Contains("學期") && Value.Contains("社團名稱") && Value.Contains("學號"))
            {
                string SchoolYear = Value.GetValue("學年度");
                string Semester = Value.GetValue("學期");
                string CourseName = Value.GetValue("社團名稱");
                string StudentNumber = Value.GetValue("學號");
                if (!string.IsNullOrEmpty(CourseName))
                {
                    string CourseKey = SchoolYear + "," + Semester + "," + CourseName + "," + StudentNumber;

                    if (!_listStudentNumber.Contains(CourseKey))
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
