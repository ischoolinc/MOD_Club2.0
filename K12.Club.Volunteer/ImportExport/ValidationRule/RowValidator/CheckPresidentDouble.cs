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
    /// 檢查學年度、學期、社團 是否重複擔任幹部
    /// </summary>
    class CheckPresidentDouble : IRowVaildator
    {
        private List<string> _listStudentNumber;

        QueryHelper qh = new QueryHelper();

        public CheckPresidentDouble()
        {
            _listStudentNumber = new List<string>();

            //查找社長資料
            string sql = @"
SELECT student.student_number,student.name,'社長' as cadre_name,
clubrecord.uid,clubrecord.school_year,clubrecord.semester,clubrecord.club_name
    FROM $k12.clubrecord.universal clubrecord
join student on student.id=(clubrecord.president)::int
";
            DataTable dt = qh.Select(sql);

            foreach (DataRow row in dt.Rows)
            {
                _listStudentNumber.Add(row["school_year"] + "," + row["semester"] + "," + row["club_name"] + "," + row["cadre_name"]);
            }

            //查找副社長資料
            string sql2 = @"
SELECT student.student_number,student.name,'副社長' as cadre_name,
clubrecord.uid,clubrecord.school_year,clubrecord.semester,clubrecord.club_name
    FROM $k12.clubrecord.universal clubrecord
join student on student.id=(clubrecord.vice_president)::int
";

            DataTable dt2 = qh.Select(sql2);
            foreach (DataRow row in dt2.Rows)
            {
                _listStudentNumber.Add(row["school_year"] + "," + row["semester"] + "," + row["club_name"] + "," + "副社長");
            }
        }

        #region IRowVaildator 成員

        public bool Validate(IRowStream Value)
        {
            bool check = true;

            //查找系統內是不是已經有相關幹部資料

            if (Value.Contains("學年度") && Value.Contains("學期") && Value.Contains("社團名稱") && Value.Contains("幹部名稱"))
            {
                string SchoolYear = Value.GetValue("學年度");
                string Semester = Value.GetValue("學期");
                string CourseName = Value.GetValue("社團名稱");
                string CadreStudent = Value.GetValue("幹部名稱");
                if (!string.IsNullOrEmpty(CourseName))
                {
                    string CourseKey = SchoolYear + "," + Semester + "," + CourseName + "," + CadreStudent;

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

