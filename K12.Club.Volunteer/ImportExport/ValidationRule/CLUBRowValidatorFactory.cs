using Campus.DocumentValidator;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 用來產生排課系統所需的自訂驗證規則
    /// </summary>
    public class CLUBRowValidatorFactory : IRowValidatorFactory
    {
        #region IRowValidatorFactory 成員

        /// <summary>
        /// 根據typeName建立對應的RowValidator
        /// </summary>
        public IRowVaildator CreateRowValidator(string typeName, System.Xml.XmlElement validatorDescription)
        {
            switch (typeName.ToUpper())
            {
                case "CLUBNOTBEREPEATEDFILL":
                    return new CLUBNotBeRepeatedFill();
                case "CLUBVOLUNTEERNAMECHECK":
                    return new CLUBVolunteerNameCheck();
                case "STUDENTINCLUBISTRUE":
                    return new CLUBCadresCheck();
                case "CLUBTEACHERDOUBLE":
                    return new CLUBTeacherCheck();
                case "STUDENTINCLUBCADRE":
                    return new CheckStudentINCadres(); // 檢查學號是否存在系統中
                case "STUDENTCADREPRESIDENT":
                    return new CheckPresidentDouble(); // 檢查是否重複擔任社長或副社長
                default:
                    return null;
            }
        }

        #endregion
    }
}