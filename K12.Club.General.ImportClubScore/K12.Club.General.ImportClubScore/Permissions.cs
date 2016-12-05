using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.General.ImportClubScore
{
    public class Permissions
    {
        public const string KeyCLUBScoreImport = "K12.Club.Universal.ImportClubScore.cs";

        public static bool IsEnableCLUBScoreImport
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[KeyCLUBScoreImport].Executable;
            }
        }
    }
}
