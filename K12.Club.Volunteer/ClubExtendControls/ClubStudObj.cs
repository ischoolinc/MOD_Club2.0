using K12.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 處理修課學生的OBJ
    /// </summary>
    class ClubStudObj
    {
        /// <summary>
        /// 已在目前社團之學生ID
        /// </summary>
        public List<string> ReMoveTemp = new List<string>();

        /// <summary>
        /// 重覆參與其它社團參與記錄
        /// </summary>
        public List<SCJoin> ReDoubleTemp = new List<SCJoin>();

        /// <summary>
        /// 由待處理可新增的學生
        /// </summary>
        public List<string> InsertList = new List<string>();

        /// <summary>
        /// LogStudent
        /// </summary>
        public Dictionary<string, StudentRecord> LogStudentList = new Dictionary<string, StudentRecord>();

        /// <summary>
        /// 取得新增學生的記錄
        /// </summary>
        public void GetLogStudent()
        {
            if (InsertList.Count != 0)
            {
                LogStudentList = new Dictionary<string, StudentRecord>();
                List<StudentRecord> studList = Student.SelectByIDs(InsertList);
                foreach (StudentRecord sr in studList)
                {
                    if (!LogStudentList.ContainsKey(sr.ID))
                    {
                        LogStudentList.Add(sr.ID, sr);
                    }
                }
            }
        }
        /// <summary>
        /// 給我 重覆參與之社團ID
        /// </summary>
        public List<string> GetClubID()
        {
            return ReDoubleTemp.Select(x => x.RefClubID).ToList();
        }

        /// <summary>
        /// 給我 重覆參與社團之學生ID
        /// </summary>
        /// <returns></returns>
        public List<string> GetStudentID()
        {
            return ReDoubleTemp.Select(x => x.RefStudentID).ToList();
        }

        /// <summary>
        /// 排除已存在於本社團之學生
        /// </summary>
        public void CheckTempStudentInCourse(List<string> IsSaft, Dictionary<string, List<SCJoin>> SCJoin_Dic, CLUBRecord _CLUBRecord)
        {
            //取得學生的社團參與記錄(所有學期)
            List<SCJoin> scjList = tool._A.Select<SCJoin>("ref_student_id in ('" + string.Join("','", IsSaft) + "')");

            if (scjList.Count != 0)
            {
                Dictionary<string, CLUBRecord> clubDic = GetDistinctClub(scjList);

                foreach (SCJoin each in scjList)
                {    //增加判斷已不存在的社團
                    if (clubDic.ContainsKey(each.RefClubID))
                    {
                        //不同社團,學年度學期卻相同
                        if (each.RefClubID != _CLUBRecord.UID && clubDic[each.RefClubID].SchoolYear == _CLUBRecord.SchoolYear && clubDic[each.RefClubID].Semester == _CLUBRecord.Semester)
                        {
                            //加入社團參與記錄
                            ReDoubleTemp.Add(each);

                            ////判斷待處理學生,是否已經有社團參與記錄(SCJoin)
                            //if (IsSaft.Contains(each.RefStudentID))
                            //{
                            //    //有記錄則先剔除
                            //    IsSaft.Remove(each.RefStudentID);
                            //}
                        }
                    }
                }
            }

            foreach (string each in IsSaft)
            {
                if (!SCJoin_Dic.ContainsKey(each))
                {
                    //可加入社團之學生
                    InsertList.Add(each);
                }
                else
                {
                    //重覆加入社團之學生
                    ReMoveTemp.Add(each);
                }
            }
        }

        /// <summary>
        /// 由社團參與記錄,取得社團對應資料
        /// </summary>
        private Dictionary<string, CLUBRecord> GetDistinctClub(List<SCJoin> scjList)
        {
            //Distinct - 移除重覆
            List<string> clublilst = scjList.Select(x => x.RefClubID).ToList().Distinct().ToList();

            //取得社團資料
            List<CLUBRecord> clublist = tool._A.Select<CLUBRecord>("uid in ('" + string.Join("','", clublilst) + "')");

            Dictionary<string, CLUBRecord> dic = new Dictionary<string, CLUBRecord>();
            foreach (CLUBRecord each in clublist)
            {
                if (!dic.ContainsKey(each.UID))
                {
                    dic.Add(each.UID, each);
                }
            }

            return dic;
        }
    }
}
