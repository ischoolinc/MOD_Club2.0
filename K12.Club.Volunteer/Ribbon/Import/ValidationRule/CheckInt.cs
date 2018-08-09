using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.DocumentValidator;

namespace K12.Club.Volunteer.Ribbon.Import.ValidationRule
{
    class CheckInt : IFieldValidator
    {
        public string Correct(string Value)
        {
            return string.Empty;
        }

        public string ToString(string template)
        {
            return template;
        }

        public bool Validate(string Value)
        {
            int n = 0;
            bool result = int.TryParse(Value, out n);

            return result;
        }
    }
}
