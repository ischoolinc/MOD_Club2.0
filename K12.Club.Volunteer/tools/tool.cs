using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar;
using FISCA.Data;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using K12.Data;
using Aspose.Cells;
using FISCA.UDT;
using System.Xml;

namespace K12.Club.Volunteer
{
    static class tool
    {
        static public AccessHelper _A = new AccessHelper();
        static public QueryHelper _Q = new QueryHelper();

        /// <summary>
        /// 檢查傳入的文字是否為數字型態,
        /// 回傳Parse後的數字,
        /// 當文字不是數字型態時,
        /// 會回傳預設數字為0,
        /// </summary>
        static public int StringIsInt_DefIsZero(string p)
        {
            int k = 0;
            int.TryParse(p, out k);
            return k;
        }

        /// <summary>
        /// 取得傳入的社團ID清單
        /// (含依據社團序號/社團名稱排序)
        /// </summary>
        static public Dictionary<string, CLUBRecord> GetClub(List<string> ClubIDList)
        {
            Dictionary<string, CLUBRecord> dic = new Dictionary<string, CLUBRecord>();
            List<CLUBRecord> ClubList = tool._A.Select<CLUBRecord>(ClubIDList);
            ClubList.Sort(SortClub);
            foreach (CLUBRecord club in ClubList)
            {
                if (!dic.ContainsKey(club.UID))
                {
                    dic.Add(club.UID, club);
                }
            }
            return dic;
        }

        /// <summary>
        /// 排序社團依據:代碼/名稱排序
        /// </summary>
        static private int SortClub(CLUBRecord cr1, CLUBRecord cr2)
        {
            string Comp1 = cr1.ClubNumber.PadLeft(5, '0');
            Comp1 += cr1.ClubName.PadLeft(20, '0');

            string Comp2 = cr2.ClubNumber.PadLeft(5, '0');
            Comp2 += cr2.ClubName.PadLeft(20, '0');

            return Comp1.CompareTo(Comp2);
        }

        /// <summary>
        /// 檢查傳入的文字是否為數字型態,
        /// 回傳Parse後的布林值,
        /// 當文字不是數字型態時,
        /// 會回傳預設布林值為false,
        /// </summary>
        static public bool StringIsInt_Bool(string p)
        {
            int k;
            if (int.TryParse(p, out k))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 傳入ComboBox,
        /// 檢查所輸入的文字,
        /// 是否為ItemList內的值,
        /// 當ComboBox Text為空時
        /// 視為資料正確
        /// </summary>
        static public bool ComboBoxValueInItemList(ComboBoxEx p)
        {
            bool b = false;

            if (!string.IsNullOrEmpty(p.Text.Trim()))
            {
                foreach (TeacherObj each in p.Items)
                {
                    if (each.TeacherFullName == p.Text.Trim())
                    {
                        b = true;
                        break;
                    }
                }
            }
            else
            {
                b = true;
            }

            return b;
        }


        /// <summary>
        /// 取得科別對照表文字清單
        /// (具排序效果)
        /// </summary>
        static public List<string> GetQueryDeptList()
        {

            //舊寫法
            //List<string> list = new List<string>();
            //DSResponse des = FISCA.Authentication.DSAServices.CallService("SmartSchool.Department.GetAbstractList", new DSRequest());
            //XmlElement elem = des.GetContent().BaseElement;
            //foreach (XmlElement var in elem.SelectNodes("Department/Name"))
            //{
            //    if (!list.Contains(var.InnerText))
            //    {
            //        list.Add(var.InnerText);
            //    }
            //}

            QueryHelper _QueryHelper = new QueryHelper();
            DataTable dtable = _QueryHelper.Select("select name from dept");
            List<string> list = new List<string>();
            foreach (DataRow row in dtable.Rows)
            {
                string name = "" + row[0];
                if (!string.IsNullOrEmpty(name))
                {
                    string[] namelist = name.Split(':');
                    string name1 = "" + namelist.GetValue(0);
                    if (!list.Contains(name1))
                    {
                        list.Add(name1);
                    }
                }
            }
            list.Sort();
            return list;
        }

        public static void SetCellBro(Worksheet ws, int a, int b, int c, int d)
        {
            ws.Cells.CreateRange(a, b, c, d).SetOutlineBorder(BorderType.TopBorder, CellBorderType.Thin, Color.Black);
            ws.Cells.CreateRange(a, b, c, d).SetOutlineBorder(BorderType.LeftBorder, CellBorderType.Thin, Color.Black);
            ws.Cells.CreateRange(a, b, c, d).SetOutlineBorder(BorderType.RightBorder, CellBorderType.Thin, Color.Black);
            ws.Cells.CreateRange(a, b, c, d).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Thin, Color.Black);
        }

        static public string GetDecimalValue(decimal DecValue, int IntValue)
        {
            string StringValue = "";

            StringValue = (DecValue * IntValue / 100).ToString();
            return StringValue;
        }

        /// <summary>
        /// 確認學生狀態是否正確,
        /// True:一般或延修生
        /// </summary>
        static public bool CheckStatus(StudentRecord student)
        {
            if (student.Status == StudentRecord.StudentStatus.一般 || student.Status == StudentRecord.StudentStatus.延修)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 傳入學生RecordList 取得班級清單
        /// </summary>
        static public Dictionary<string, ClassRecord> GetClassDic(List<StudentRecord> studlist)
        {
            Dictionary<string, ClassRecord> dic = new Dictionary<string, ClassRecord>();
            List<string> list = new List<string>();
            foreach (StudentRecord srud in studlist)
            {
                if (!string.IsNullOrEmpty(srud.RefClassID))
                {
                    if (!list.Contains(srud.RefClassID))
                    {
                        list.Add(srud.RefClassID);
                    }
                }
            }
            List<ClassRecord> classlist = Class.SelectByIDs(list);
            foreach (ClassRecord each in classlist)
            {
                if (!dic.ContainsKey(each.ID))
                {
                    dic.Add(each.ID, each);
                }
            }
            return dic;
        }

        static public int SortClass(社團志願分配的Row row1, 社團志願分配的Row row2)
        {
            string Grat1 = row1._GradeYear.PadLeft(1, '0');
            Grat1 += row1._Class_display_order.PadLeft(3, '9');
            Grat1 += row1._Class.PadLeft(10, '0');

            string Grat2 = row2._GradeYear.PadLeft(1, '0');
            Grat2 += row2._Class_display_order.PadLeft(3, '9');
            Grat2 += row2._Class.PadLeft(10, '0');

            return Grat1.CompareTo(Grat2);
        }

        static public int SortMergeList(一名學生 s1, 一名學生 s2)
        {
            string ss1 = s1.seat_no.HasValue ? s1.seat_no.Value.ToString().PadLeft(3, '0') : "999";
            string ss2 = s2.seat_no.HasValue ? s2.seat_no.Value.ToString().PadLeft(3, '0') : "999";
            return ss1.CompareTo(ss2);
        }
    }
}
