using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using B1.CacheManagement;

namespace B1.Utility.TestConsoleApp
{
    public class CachingTest
    {
        public static void RunTest()
        {
            CacheMgr<int> cacheMgr = new CacheMgr<int>();
            cacheMgr.Add("Testing", getTestingValue, 18);
            while (Console.Read() != 'x')
            {
                Console.WriteLine(cacheMgr.Get("Testing"));
            }
        }

        static int _number = 1;
        public static int getTestingValue(string key)
        {
            return _number++;
        }
    }
}
