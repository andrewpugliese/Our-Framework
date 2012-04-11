using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using B1.Cryptography;

namespace B1.WebSite.Admin
{
    public class Constants
    {
        public const string UIControlCodeTag = "_UIControlCode_";
        public const int UIControlCode_AdminUser_Code = 5;
        public static readonly string UIControlCode_AdminUser = UIControlCodeTag + UIControlCode_AdminUser_Code.ToString();

        public const string _Page_SignonPartial = "_SignOnPartial";
        public const string About = "About";
        public const string Admin = "Admin";
        public const string Grid = "Grid";
        public const string Home = "Home";
        public const string Index = "Index";

        public const string CookieName = "B1COOKIE";
        public const string CookieContent_UserCode = "USERCODE";
        public const string CookieContent_AccessControlGroup = "ACG";

        public const SymmetricAlgorithmTypeEnum Cookie_SymmetricAlgorithm = SymmetricAlgorithmTypeEnum.Rijndael;
        public const string Cookie_SymmetricKey = "321tnirps321TNIRPS____321tnirps321TNIRPS!@#$%^&*(";
    }
}