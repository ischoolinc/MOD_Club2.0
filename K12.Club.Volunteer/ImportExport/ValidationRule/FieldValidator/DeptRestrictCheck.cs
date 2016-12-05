using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Campus.DocumentValidator;
using FISCA.Data;
using System.Xml;
using FISCA.DSAUtil;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 授課教師檢查
    /// </summary>
    public class DeptRestrictCheck : IFieldValidator
    {
        private List<string> mDeptRestrictNames;

        /// <summary>
        /// 取得ischool系統內的所有老師
        /// </summary>
        public DeptRestrictCheck()
        {
            mDeptRestrictNames = tool.GetQueryDeptList();
        }

        #region IFieldValidator 成員

        /// <summary>
        /// 自動修正
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public string Correct(string Value)
        {
            return string.Empty;
        }

        /// <summary>
        /// 回傳訊息
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public string ToString(string template)
        {
            return template;
        }

        /// <summary>
        /// 驗證
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool Validate(string Value)
        {
            string[] list = Value.Split('/');
            if (list.Length > 1)
            {
                bool check = true;
                foreach (string each in list)
                {
                    if (!mDeptRestrictNames.Contains(each))
                    {
                        check = false;
                    }
                }
                return check;
            }
            else if (list.Length == 1)
            {
                return mDeptRestrictNames.Contains(Value);
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
