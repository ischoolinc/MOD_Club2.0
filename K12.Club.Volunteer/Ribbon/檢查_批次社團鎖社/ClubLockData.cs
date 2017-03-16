using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.Volunteer.Ribbon.檢查_批次社團鎖社
{
    public class ClubLockData
    {

        public string ClubID { get; set; }

        public string ClubName { get; set; }

        public int AlreadyLockCount { get; set; }

        public int TotalStudentCount { get; set; }

    }
}
