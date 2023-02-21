using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.Volunteer
{
    class 學生選社亂數檔
    {
        /// <summary>
        /// 亂數字
        /// </summary>
        public int _Number { get; set; }

        ///<summary>
        /// 序號
        /// </summary>
        public int _Index { get; set; }

        /// <summary>
        /// 選社記錄
        /// </summary>
        public VolunteerRecord _record { get; set; }

        /// <summary>
        /// 是否選社成功
        /// </summary>
        public bool AllocationSucceeds { get; set; }

        public 學生選社亂數檔(VolunteerRecord record, int Number)
        {
            _Number = Number;
            _record = record;
            AllocationSucceeds = false;
        }
    }
}
