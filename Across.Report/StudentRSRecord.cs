using K12.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Across.Report
{
    class StudentRSRecord
    {
        /// <summary>
        /// 學生本人
        /// </summary>
        public StudentRecord _student { get; set; }

        /// <summary>
        /// 學生社團參與記錄
        /// </summary>
        public List<ResultScoreRecord> _ResultList { get; set; }

        public string 學生入學照片 { get; set; }

        public string 學生畢業照片 { get; set; }

        public StudentRSRecord(StudentRecord student)
        {
            _student = student;
            _ResultList = new List<ResultScoreRecord>();
        }

        /// <summary>
        /// 設定學生社團參與記錄
        /// </summary>
        public void SetRSR(ResultScoreRecord rsr)
        {
            _ResultList.Add(rsr);
        }
    }
}
