using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.Volunteer
{
    //排序依班級序號/班級名稱/學生座號/學生姓名

    class StudentSortObj_K12Data
    {
        public K12.Data.StudentRecord _StudentRecord { get; set; }
        public K12.Data.ClassRecord _ClassRecord { get; set; }
        public string _SortString { get; set; }

        public StudentSortObj_K12Data(K12.Data.ClassRecord classRecord,K12.Data.StudentRecord student)
        {
            string ClassYear = ""; //年級
            string ClassIndex = ""; //班級序號
            string ClassName = ""; //班級名稱
            string StudentSeatNo = ""; //學生座號
            string StudentName = ""; //學生姓名

            _StudentRecord = student;

            #region ClassIndex & ClassName
            if (classRecord != null) //如果有班級
            {
                _ClassRecord = classRecord;
                ClassYear = SortClassIndex.Year(_ClassRecord);
                ClassIndex = SortClassIndex.Index(_ClassRecord);
                ClassName = _ClassRecord.Name.PadLeft(10, '0');
            }
            else //如果沒有班級
            {
                ClassYear = ClassYear.PadLeft(10, '9'); 
                ClassIndex = ClassIndex.PadLeft(10, '9');
                ClassName = ClassName.PadLeft(10, '9');
            }
            #endregion

            StudentName = _StudentRecord.Name.PadLeft(10, '0');

            StudentSeatNo = _StudentRecord.SeatNo.HasValue ? 
                _StudentRecord.SeatNo.Value.ToString().PadLeft(10, '0') :
                StudentSeatNo.PadLeft(10, '9');

            _SortString = ClassYear + ClassIndex + ClassName + StudentSeatNo + StudentName;
        }
    }
}
