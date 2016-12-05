using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.DocumentValidator;
using K12.Data;
using System.Data;

namespace K12.Club.Volunteer
{
    class StudentNumberRepeatValidator : IFieldValidator
    {
        Dictionary<string, List<string>> _StudentDic { get; set; }
        public StudentNumberRepeatValidator()
        {
            _StudentDic = GetStudent();
        }

        #region IFieldValidator 成員

        public string Correct(string Value)
        {
            return Value;
        }

        public string ToString(string template)
        {
            return template;
        }

        public bool Validate(string Value)
        {
            if (_StudentDic.ContainsKey(Value)) //包含此學號
            {
                if (_StudentDic[Value].Count > 1) //多名學生為錯誤
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 取得學生學號 vs 系統編號
        /// </summary>
        private Dictionary<string, List<string>> GetStudent()
        {
            FISCA.Data.QueryHelper _queryHelper = new FISCA.Data.QueryHelper();

            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            //取得比對序

            DataTable dt = _queryHelper.Select("select id,student_number from student");
            foreach (DataRow row in dt.Rows)
            {
                string StudentID = "" + row[0];
                string Student_Number = "" + row[1];

                if (string.IsNullOrEmpty(Student_Number))
                    continue;

                if (!dic.ContainsKey(Student_Number))
                {
                    dic.Add(Student_Number, new List<string>());
                }
                dic[Student_Number].Add(StudentID);
            }
            return dic;

        }

        #endregion
    }
}
