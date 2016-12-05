 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;
using System.Xml;

namespace K12.Club.Volunteer
{
    //社團成績計算比例原則
    [TableName("K12.WeightProportion.Universal")]
    public class WeightProportion : ActiveRecord
    {
        /// <summary>
        /// 成績比例原則
        /// </summary>
        [Field(Field = "proportion", Indexed = false)]
        public string Proportion { get; set; }


        ///// <summary>
        ///// 平時活動比例
        ///// </summary>
        //[Field(Field = "pa_weight", Indexed = false)]
        //public int PA_Weight { get; set; }

        ///// <summary>
        ///// 出缺率比例
        ///// </summary>
        //[Field(Field = "ar_weight", Indexed = false)]
        //public int AR_Weight { get; set; }

        ///// <summary>
        ///// 活動力及服務比例
        ///// </summary>
        //[Field(Field = "aas_weight", Indexed = false)]
        //public int AAS_Weight { get; set; }

        ///// <summary>
        ///// 成品成果考驗比例
        ///// </summary>
        //[Field(Field = "far_weight", Indexed = false)]
        //public int FAR_Weight { get; set; }
    }
}
