using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Data;

namespace K12.Club.Volunteer
{
    public class 社團志願分配的Row
    {
        /// <summary>
        /// 年級
        /// </summary>
        public string _GradeYear { get; set; }

        /// <summary>
        /// 班級人數
        /// </summary>
        public string _Class { get; set; }

        /// <summary>
        /// 班級序號
        /// </summary>
        public string _Class_display_order { get; set; }

        /// <summary>
        /// 老師人數
        /// </summary>
        public string _teacher { get; set; }

        /// <summary>
        /// 學生人數
        /// </summary>
        public int _StudentCount
        {
            get
            {
                return _StudentDic.Count;
            }
        }

        /// <summary>
        /// 學生系統編號:學生基本資料物件
        /// </summary>
        public Dictionary<string, 一名學生> _StudentDic { get; set; }

        /// <summary>
        /// 學生選社數
        /// </summary>
        public int _VolunteerCount
        {
            get
            {
                return _Volunteer.Count;
            }
        }

        /// <summary>
        /// 學生系統編號:學生選社物件
        /// </summary>
        public Dictionary<string, VolunteerRecord> _Volunteer { get; set; }

        public int StudentSCJoinCount
        {
            get
            {
                return _SCJDic.Count;
            }
        }

        public int LockStudentLockCount
        {
            get
            {
                int count = 0;
                foreach (SCJoin each in _SCJDic.Values)
                {
                    if (each.Lock)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// 學生的社團記錄
        /// </summary>
        public Dictionary<string, SCJoin> _SCJDic { get; set; }

        public Dictionary<string, CLUBRecord> _ClubDic { get; set; }

        public 社團志願分配的Row()
        {
            _StudentDic = new Dictionary<string, 一名學生>();
            _Volunteer = new Dictionary<string, VolunteerRecord>();
            _SCJDic = new Dictionary<string, SCJoin>();
            _ClubDic = new Dictionary<string, CLUBRecord>();
        }


    }
}
