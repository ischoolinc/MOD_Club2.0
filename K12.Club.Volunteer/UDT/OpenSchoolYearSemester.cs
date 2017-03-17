using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;
using System.Xml;

namespace K12.Club.Volunteer.UDT
{
    [TableName("openschoolyearsemester")]
        
    public class OpenSchoolYearSemester : ActiveRecord    
    {                      
        /// <summary>    
        /// 學年度        
        /// </summary>        
        [Field(Field = "school_year", Indexed = false)]        
        public string SchoolYear { get; set; }

        
        /// <summary>        
        /// 學期        
        /// </summary>        
        [Field(Field = "semester", Indexed = false)]
        public string Semester { get; set; }
                    
    }    
}
