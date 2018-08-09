using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.DocumentValidator;
using FISCA.Data;
using System.Data;

namespace K12.Club.Volunteer.Ribbon.Import.ValidationRule
{
    /// <summary>
    /// 檢查學年度、學期、社團資料是否存在系統中
    /// </summary>
    class CheckStudentNumberInIschool : IFieldValidator
    {
        private List<string> _listStudentNumber;

        public CheckStudentNumberInIschool()
        {
            this._listStudentNumber = new List<string>();

            string sql = @"
SELECT
    student_number
FROM
    student
WHERE
    status IN(1,2)
    AND student_number IS NOT NULL
";
            QueryHelper qh = new QueryHelper();
            DataTable dt = qh.Select(sql);

            foreach (DataRow row in dt.Rows)
            {
                this._listStudentNumber.Add("" + row["student_number"]);
            }
        }

        public string Correct(string Value)
        {
            return string.Empty;
        }

        public string ToString(string template)
        {
            return template;
        }

        public bool Validate(string Value)
        {
            return this._listStudentNumber.Contains(Value);
        }
    }
}
