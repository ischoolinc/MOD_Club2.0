using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.DocumentValidator;
using K12.Data;
using System.Data;

namespace K12.Club.Volunteer
{
    class StudentNumberExistenceValidator : IFieldValidator
    {
        Dictionary<string, List<string>> _StudentDic { get; set; }
        public StudentNumberExistenceValidator()
        {
            _StudentDic = GetStudent();
        }
        #region IFieldValidator 成員

        //自動修正
        public string Correct(string Value)
        {
            return Value;
        }
        //回傳驗證訊息
        public string ToString(string template)
        {
            return template;
        }

        public bool Validate(string Value)
        {
            if (_StudentDic.ContainsKey(Value)) //包含此學號
            {
                return true;
            }
            else //不包含此學號
            {
                return false;
            }
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
                dic[Student_Number].Add(StudentID); //如果重覆也加進去
            }
            return dic;

        }

        #endregion
    }
}
