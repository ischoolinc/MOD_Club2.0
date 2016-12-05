using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.Volunteer
{
    class LogVolunteer
    {
        public VolunteerRecord lo_Vol { get; set; }
        public VolunteerRecord New_Vol { get; set; }
        public Dictionary<string, CLUBRecord> ClubDic { get; set; }
    }
}
