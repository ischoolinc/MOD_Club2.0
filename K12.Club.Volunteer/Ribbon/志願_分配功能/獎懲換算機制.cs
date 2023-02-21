using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Data;

namespace K12.Club.Volunteer
{
    //獎懲換算機制
    //大功小功嘉獎,大過小過警告
    public class 獎懲換算機制
    {
        Dictionary<string, int> StudentDic { get; set; }

        public MeritDemeritReduceRecord _Reduce { get; set; }

        public Dictionary<string, int> GetMerit(List<一名學生> StudentList)
        {
            //取得學生ID
            StudentDic = GetStudentIDList(StudentList);

            //取得獎懲資料
            List<MeritRecord> MeritList = Merit.SelectByStudentIDs(StudentDic.Keys);
            List<DemeritRecord> DemeritList = Demerit.SelectByStudentIDs(StudentDic.Keys);

            //取得功過相抵換算值
            _Reduce = K12.Data.MeritDemeritReduce.Select();


            foreach (MeritRecord mr in MeritList)
            {
                if (StudentDic.ContainsKey(mr.RefStudentID))
                {
                    int studINT = 0;

                    int a = mr.MeritA.HasValue ? mr.MeritA.Value : 0;
                    int b = mr.MeritB.HasValue ? mr.MeritB.Value : 0;
                    int c = mr.MeritC.HasValue ? mr.MeritC.Value : 0;

                    if (_Reduce.MeritAToMeritB.HasValue)
                    {
                        int aa = a * _Reduce.MeritAToMeritB.Value;
                        studINT += aa * _Reduce.MeritBToMeritC.Value;
                    }

                    if (_Reduce.MeritBToMeritC.HasValue)
                    {
                        studINT += b * _Reduce.MeritBToMeritC.Value;

                    }

                    studINT += c;

                    StudentDic[mr.RefStudentID] += studINT;
                }
            }

            foreach (DemeritRecord mr in DemeritList)
            {
                if (mr.Cleared == "是") //銷過資料則離開
                    continue;

                if (mr.MeritFlag == "2") //留查資料則離開
                    continue;

                if (StudentDic.ContainsKey(mr.RefStudentID))
                {
                    int studINT = 0;

                    int a = mr.DemeritA.HasValue ? mr.DemeritA.Value : 0;
                    int b = mr.DemeritB.HasValue ? mr.DemeritB.Value : 0;
                    int c = mr.DemeritC.HasValue ? mr.DemeritC.Value : 0;

                    if (_Reduce.MeritAToMeritB.HasValue)
                    {
                        int aa = a * _Reduce.MeritAToMeritB.Value;
                        studINT += aa * _Reduce.MeritBToMeritC.Value;
                    }

                    if (_Reduce.MeritBToMeritC.HasValue)
                    {
                        studINT += b * _Reduce.MeritBToMeritC.Value;

                    }

                    studINT += c;

                    StudentDic[mr.RefStudentID] -= studINT;
                }




            }

            return StudentDic;
        }

        private Dictionary<string, int> GetStudentIDList(List<一名學生> StudentList)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();

            foreach (一名學生 each in StudentList)
            {
                if (!dic.ContainsKey(each.student_id))
                {
                    dic.Add(each.student_id, 0);
                }
            }
            return dic;
        }
    }
}
