using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Utility.TestConsoleApp
{
    class Program
    {
        static void Main( string[ ] args )
        {
            //CachingTest.RunTest();

            

            // Test the configuration API
            //?? ConfigurationTest.RunTest();

            if(args.Count() > 0 && args.Contains("-t"))
                LoggingTest.TestTrace();
        }

    }
}
