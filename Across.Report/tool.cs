using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Across.Report
{
    static public class tool
    {

        static public FISCA.UDT.AccessHelper _A = new FISCA.UDT.AccessHelper();

        /// <summary>
        /// 取得社團中英文對照名稱
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetEngList()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            List<EnglishTable> EngList = tool._A.Select<EnglishTable>();
            foreach (EnglishTable each in EngList)
            {
                if (!dic.ContainsKey(each.ClubName))
                {
                    dic.Add(each.ClubName, each.English);
                }
            }
            return dic;
        }

        public static string GetDay(int n)
        {
            string EngMonth = "";
            switch (n.ToString())
            {
                case "1":
                    EngMonth = "first";
                    break;
                case "2":
                    EngMonth = "second";
                    break;
                case "3":
                    EngMonth = "third";
                    break;
                case "4":
                    EngMonth = "fourth";
                    break;
                case "5":
                    EngMonth = "fifth";
                    break;
                case "6":
                    EngMonth = "sixth";
                    break;
                case "7":
                    EngMonth = "seventh";
                    break;
                case "8":
                    EngMonth = "eighth";
                    break;
                case "9":
                    EngMonth = "ninth";
                    break;
                case "10":
                    EngMonth = "tenth";
                    break;
                case "11":
                    EngMonth = "eleventh";
                    break;
                case "12":
                    EngMonth = "twelfth";
                    break;
                case "13":
                    EngMonth = "thirteenth";
                    break;
                case "14":
                    EngMonth = "fourteenth";
                    break;
                case "15":
                    EngMonth = "fifteenth";
                    break;
                case "16":
                    EngMonth = "sixteenth";
                    break;
                case "17":
                    EngMonth = "seventeenth";
                    break;
                case "18":
                    EngMonth = "eighteenth";
                    break;
                case "19":
                    EngMonth = "nineteenth";
                    break;
                case "20":
                    EngMonth = "twentieth";
                    break;
                case "21":
                    EngMonth = "twenty-first";
                    break;
                case "22":
                    EngMonth = "twenty-second";
                    break;
                case "23":
                    EngMonth = "twenty-third";
                    break;
                case "24":
                    EngMonth = "twenty-fourth";
                    break;
                case "25":
                    EngMonth = "twenty-fifth";
                    break;
                case "26":
                    EngMonth = "twenty-sixth";
                    break;
                case "27":
                    EngMonth = "twenty-seventh";
                    break;
                case "28":
                    EngMonth = "twenty-eighth";
                    break;
                case "29":
                    EngMonth = "twenty-ninth";
                    break;
                case "30":
                    EngMonth = "thirtieth";
                    break;
                case "31":
                    EngMonth = "thirty-first";
                    break;
                default:
                    EngMonth = "";
                    break;
            }
            return EngMonth;

        }




        /// <summary>
        /// 取得月份英文名稱
        /// </summary>
        public static string GetMonth(int n)
        {
            string EngMonth = "";
            switch (n.ToString())
            {
                case "1":
                    EngMonth = "January";
                    break;
                case "2":
                    EngMonth = "February";
                    break;
                case "3":
                    EngMonth = "March";
                    break;
                case "4":
                    EngMonth = "April";
                    break;
                case "5":
                    EngMonth = "May";
                    break;
                case "6":
                    EngMonth = "June";
                    break;
                case "7":
                    EngMonth = "July";
                    break;
                case "8":
                    EngMonth = "August";
                    break;
                case "9":
                    EngMonth = "September";
                    break;
                case "10":
                    EngMonth = "October";
                    break;
                case "11":
                    EngMonth = "November";
                    break;
                case "12":
                    EngMonth = "December";
                    break;
                default:
                    EngMonth = "";
                    break;
            }
            return EngMonth;
        }
    }
}
