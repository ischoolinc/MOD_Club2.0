using Campus.DocumentValidator;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 用來產生排課系統所需的自訂驗證規則
    /// </summary>
    public class CLUBFieldValidatorFactory : IFieldValidatorFactory
    {
        #region IFieldValidatorFactory 成員

        /// <summary>
        /// 根據typeName建立對應的FieldValidator
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="validatorDescription"></param>
        /// <returns></returns>
        public IFieldValidator CreateFieldValidator(string typeName, System.Xml.XmlElement validatorDescription)
        {
            switch (typeName.ToUpper())
            {
                case "TEACHERINISCHOOLCHECK":
                    return new TeacherInischoolCheck(); //取得ischool系統內的所有老師
                case "DEPTRESTRICTCHECK":
                    return new DeptRestrictCheck(); //取得ischool系統內的所有科別清單
                case "STUDENTNUMBEREXISTENCE":
                    return new StudentNumberExistenceValidator(); //
                case "STUDENTNUMBERREPEAT":
                    return new StudentNumberRepeatValidator(); //
                case "STUDENTNUMBERSTATUS":
                    return new StudentNumberStatusValidator(); //學生狀況是一般或延修生
                case "CHECKSTUDENTNUMBERINISCHOOL":
                    return new Ribbon.Import.ValidationRule.CheckStudentNumberInIschool(); // 檢查學號是否存在系統中
                case "INTPARSE":
                    return new Ribbon.Import.ValidationRule.CheckInt();
                default:
                    return null;
            }
        }

        #endregion
    }
}