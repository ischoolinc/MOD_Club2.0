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
        /// <param name="typeName"></param>
        /// <param name="validatorDescription"></param>
        /// <returns></returns>
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
                default:
                    return null;
            }
        }

        #endregion
    }
}