using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Data;

namespace K12.Club.Volunteer
{
    class ClubTraObj
    {
        public SCJoin SCJ { get; set; }

        public ResultScoreRecord RSR { get; set; }

        public StudentRecord student { get; set; }
    }
}
