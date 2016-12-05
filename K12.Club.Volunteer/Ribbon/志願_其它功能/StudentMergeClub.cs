using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Data;

namespace K12.Club.Volunteer
{
    //學生基本資料物件
    class StudentMergeClub
    {
        public StudentRecord _Student { get; set; }

        public List<CLUBRecord> _CLUBList { get; set; }


        public StudentMergeClub(StudentRecord stud)
        {
            _Student = stud;
            _CLUBList = new List<CLUBRecord>();
        }


    }
}
