﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Club.Volunteer;
using K12.Data;

namespace MOD_Club20.Report.ForHCH
{
    class StudentSCjoinObj
    {
        public StudentSCjoinObj(StudentRecord sr)
        {
            _StudentRecord = sr;
            CLUBRecord = new List<CLUBRecord>();
        }


        /// <summary>
        /// 學生資料
        /// </summary>
        public StudentRecord _StudentRecord { get; set; }

        /// <summary>
        /// 學生目前選擇之社團
        /// 若大於1則代表重覆修課
        /// 若是0則代表沒有參與社團
        /// </summary>
        public List<CLUBRecord> CLUBRecord { get; set; }

        /// <summary>
        /// 取得社團名稱
        /// </summary>
        //public string GetClubName
        //{
        //    get
        //    {
        //        if (CLUBRecord.Count == 0)
        //        {
        //            return "";
        //        }
        //        else if (CLUBRecord.Count > 1)
        //        {
        //            StringBuilder sb = new StringBuilder();
        //            //sb.Append("(★)");
        //            sb.Append(string.Join(",", CLUBRecord.Select(x => x.ClubName).ToList()));
        //            return sb.ToString();
        //        }
        //        else
        //        {
        //            return CLUBRecord[0].ClubName;
        //        }
        //    }
        //}

        /// <summary>
        /// 取得社團類型
        /// </summary>
        //public string GetClubCategory
        //{
        //    get
        //    {
        //        if (CLUBRecord.Count == 0)
        //        {
        //            return "";
        //        }
        //        else if (CLUBRecord.Count > 1)
        //        {
        //            StringBuilder sb = new StringBuilder();
        //            //sb.Append("(★)");
        //            sb.Append(string.Join(",", CLUBRecord.Select(x => x.ClubCategory).ToList()));
        //            return sb.ToString();
        //        }
        //        else
        //        {
        //            return CLUBRecord[0].ClubCategory;
        //        }
        //    }
        //}

        /// <summary>
        /// 取得社團幹部
        /// </summary>
        public string GetClubPresident
        {
            get
            {
                if (CLUBRecord.Count == 0)
                {
                    return "";
                }
                else
                {
                    //處理當社團有多個的時候
                    //社長/副社長/其他幹部都要加入
                    List<string> list = new List<string>();
                    if (CLUBRecord.Select(x => x.President).ToList().Contains(_StudentRecord.ID))
                        list.Add("社長"); //社長

                    if (CLUBRecord.Select(x => x.VicePresident).ToList().Contains(_StudentRecord.ID))
                        list.Add("副社長");

                    List<CadresRecord> CadreList = tool._A.Select<CadresRecord>(CLUBRecord.Select(x => x.UID).ToList());
                    foreach (CadresRecord each in CadreList)
                    {
                        if (each.RefStudentID == _StudentRecord.ID)
                        {
                            list.Add(each.CadreName);
                        }
                    }

                    if (list.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        //sb.Append("(★)");
                        sb.Append(string.Join(",", list));
                        return sb.ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get { return _StudentRecord.Name; } }

        /// <summary>
        /// 性別
        /// </summary>
        public string Gender { get { return _StudentRecord.Gender; } }

        /// <summary>
        /// 學號
        /// </summary>
        public string StudentNumber { get { return _StudentRecord.StudentNumber; } }

        /// <summary>
        /// 座號
        /// </summary>
        public string SeatNo { get { return _StudentRecord.SeatNo.HasValue ? _StudentRecord.SeatNo.Value.ToString() : ""; } }

        /// <summary>
        /// 班級
        /// </summary>
        public string ClassName
        {
            get { return string.IsNullOrEmpty(_StudentRecord.RefClassID) ? "" : Class.SelectByID(_StudentRecord.RefClassID).Name; }
        }
    }
}
