using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 專門用來檢查目前社團狀況的
    /// </summary>
    class 一個社團檢查
    {
        public 一個社團檢查(CLUBRecord ClubObj)
        {
            _ClubObj = ClubObj;
            _SCJDic = new Dictionary<string, SCJoin>();

            _Now_ClubStudentCount = 0;
            _Now_GradeYear1 = 0;
            _Now_GradeYear2 = 0;
            _Now_GradeYear3 = 0;
            DeptList = new List<string>();

            //選社之科系限制
            GetDept();
        }

        /// <summary>
        /// 取得本社團之科系限制
        /// </summary>
        private void GetDept()
        {
            if (!string.IsNullOrEmpty(_ClubObj.DeptRestrict))
            {
                FISCA.DSAUtil.DSXmlHelper dsx = new FISCA.DSAUtil.DSXmlHelper();
                dsx.Load(_ClubObj.DeptRestrict);
                foreach (System.Xml.XmlElement xml in dsx.BaseElement.SelectNodes("Dept"))
                {
                    if (!DeptList.Contains(xml.InnerText))
                    {
                        DeptList.Add(xml.InnerText);
                    }
                }
            }
        }

        /// <summary>
        /// 本社團之科別限制清單
        /// </summary>
        public List<string> DeptList { get; set; }

        //一個記錄社團目前社員數與社員上限的資料
        public CLUBRecord _ClubObj { get; set; }

        /// <summary>
        /// 本社團的社團參與記錄
        /// </summary>
        public Dictionary<string, SCJoin> _SCJDic = new Dictionary<string, SCJoin>();

        public string CLUBID
        {
            get
            {
                return _ClubObj.UID;
            }
        }

        /// <summary>
        /// 本社團人數上限
        /// </summary>
        public int _Limit_ClubStudentCount
        {
            get
            {
                if (_ClubObj.Limit.HasValue)
                {
                    return _ClubObj.Limit.Value;
                }
                else
                {
                    return 99999;
                }
            }
        }

        /// <summary>
        /// 一年級上限
        /// </summary>
        public int _Limit_GradeYear1
        {
            get
            {
                if (_ClubObj.Grade1Limit.HasValue)
                {
                    return _ClubObj.Grade1Limit.Value;
                }
                else
                {
                    return 99999;
                }
            }
        }

        /// <summary>
        /// 二年級上限
        /// </summary>
        public int _Limit_GradeYear2
        {
            get
            {
                if (_ClubObj.Grade2Limit.HasValue)
                {
                    return _ClubObj.Grade2Limit.Value;
                }
                else
                {
                    return 99999;
                }
            }
        }

        /// <summary>
        /// 三年級上限
        /// </summary>
        public int _Limit_GradeYear3
        {
            get
            {
                if (_ClubObj.Grade3Limit.HasValue)
                {
                    return _ClubObj.Grade3Limit.Value;
                }
                else
                {
                    return 99999;
                }
            }
        }

        //==========================

        /// <summary>
        /// 本社團目前總人數
        /// </summary>
        public int _Now_ClubStudentCount { get; set; }

        /// <summary>
        /// 一年級目前人數
        /// </summary>
        public int _Now_GradeYear1 { get; set; }

        /// <summary>
        /// 二年級目前人數
        /// </summary>
        public int _Now_GradeYear2 { get; set; }

        /// <summary>
        /// 三年級目前人數
        /// </summary>
        public int _Now_GradeYear3 { get; set; }

        /// <summary>
        /// TRUE為未滿
        /// </summary>
        public bool 人數未滿
        {
            get
            {
                if (_Now_ClubStudentCount < _Limit_ClubStudentCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// TRUE為未滿
        /// </summary>
        public bool 一年級未滿
        {
            get
            {
                if (_Now_GradeYear1 < _Limit_GradeYear1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// TRUE為未滿
        /// </summary>
        public bool 二年級未滿
        {
            get
            {
                if (_Now_GradeYear2 < _Limit_GradeYear2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// TRUE為未滿
        /// </summary>
        public bool 三年級未滿
        {
            get
            {
                if (_Now_GradeYear3 < _Limit_GradeYear3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public GetVolunteerData.男女 男女限制
        {
            get
            {
                if (_ClubObj.GenderRestrict == "男")
                {
                    return GetVolunteerData.男女.男;
                }
                else if (_ClubObj.GenderRestrict == "女")
                {
                    return GetVolunteerData.男女.女;
                }
                else
                {
                    return GetVolunteerData.男女.不限制;
                }

            }
        }
    }
}
