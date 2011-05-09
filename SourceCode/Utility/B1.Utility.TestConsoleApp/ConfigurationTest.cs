using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using B1.DataAccess;
using B1.Configuration;

namespace B1.Utility.TestConsoleApp
{
    public class ConfigurationTest
    {

        public static void RunTest()
        {
            ////string configConnStr = ConfigurationManager.AppSettings[ "ConfigConnStr" ];
            ////string appConfigSetName = ConfigurationManager.AppSettings[ "AppConfigSetName" ];
            ////string sysConfigSetName = ConfigurationManager.AppSettings[ "SysConfigSetName" ];

            ////AppConfigMgr.Initialize( configConnStr, appConfigSetName, sysConfigSetName );

            //?? AppConfigMgr.ConfigChanging += new AppConfigMgr.ConfigChangingCallback( AppConfigMgr_ConfigChanging );

            // Read configuration
            string vehicleSourceBook = AppConfigMgr.GetValue( "VehicleSourceBook" );

            // Example for IP address object returned by AppConfigMgr class
            System.Net.IPAddress ipAddress =
                    AppConfigMgr.GetValue( "VehicleServer_IPAddress", val => System.Net.IPAddress.Parse( val ) );
        }

        static bool AppConfigMgr_ConfigChanging( string configKeyName, string oldValue, string newValue )
        {
            if (configKeyName == "Chase_TcpServer")
                return false;

            return true;
        }
    }
}
