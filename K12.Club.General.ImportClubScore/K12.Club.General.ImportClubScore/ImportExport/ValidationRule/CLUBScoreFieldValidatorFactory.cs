using Campus.DocumentValidator;
using K12.Club.General.ImportClubScore.ImportExport.ValidationRule.FieldValidator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.General.ImportClubScore.ImportExport.ValidationRule
{
    class CLUBScoreFieldValidatorFactory : IFieldValidatorFactory
    {
        #region IRowValidatorFactory 成員

        IFieldValidator IFieldValidatorFactory.CreateFieldValidator(string typeName, System.Xml.XmlElement validatorDescription)
        {
            switch (typeName.ToUpper())
            {
                case "K12CLUBGENERALCLUBSCORECHECKSTUDENTNUMBER":
                    return new StudentInischoolCheck();
                default:
                    return null;
            }
        }

        #endregion
    }
}
