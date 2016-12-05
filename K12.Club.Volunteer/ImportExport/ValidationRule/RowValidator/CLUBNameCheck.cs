using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Campus.DocumentValidator;
using FISCA.Data;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 檢查系統中是否社團名稱/學年度/學期重覆
    /// </summary>
    public class CLUBNameCheck : IRowVaildator
    {
        private List<string> mCourseNames;
        private Task mTask;

        public CLUBNameCheck()
        {
            mTask = Task.Factory.StartNew(() =>
            {
                mCourseNames = new List<string>();
                List<CLUBRecord> CLUBList = tool._A.Select<CLUBRecord>();
                foreach (CLUBRecord each in CLUBList)
                {
                    string CourseKey = each.SchoolYear + "," + each.Semester + "," + each.ClubName;
                    if (!mCourseNames.Contains(CourseKey))
                    {
                        mCourseNames.Add(CourseKey);
                    }
                }
            });
        }

        #region IRowVaildator 成員

        public bool Validate(IRowStream Value)
        {
            if (Value.Contains("學年度") && Value.Contains("學期") && Value.Contains("社團名稱"))
            {
                string CourseName = Value.GetValue("學年度");
                string SchoolYear = Value.GetValue("學期");
                string Semester = Value.GetValue("社團名稱");
                string CourseKey = CourseName + "," + SchoolYear + "," + Semester;

                mTask.Wait();

                return !mCourseNames.Contains(CourseKey);
            }

            return false;
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