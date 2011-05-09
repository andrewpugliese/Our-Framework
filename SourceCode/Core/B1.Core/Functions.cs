using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace B1.Core
{
    /// <summary>
    /// Static functions to help with various tasks
    /// </summary>
    public class Functions
    {
        /// <summary>
        /// Retuns a unique number that can sequences across multiple threads and processses as well as 
        /// time synchronized physical servers.
        /// <para>It can be used to coordinate and order events without relying on a database Identity or Sequence object</para>
        /// <para>The returned number will have the following format: YYdddHHmmssfffnnnnn (e.g. 1111714372358100001) where:</para>
        /// <para>YY -> year (e.g. 11) </para>
        /// <para>ddd -> julian day of year (e.g. 1 New Years Day, 365 New Years Eve)</para>
        /// <para>HH -> Military Format Time Hour (e.g. 01, 14, 23) (Universal Time Constant: UTC)</para>
        /// <para>mm -> minute (e.g. 01, 10, 59) (UTC)</para>
        /// <para>ss -> second (e.g. 01, 10, 59) (UTC)</para>
        /// <para>fff -> millisecond (e.g. 001, 053, 255)</para>
        /// <para>nnnnn -> a 5 digit numeric value PROVIDED BY CALLER to be the tie-breaker
        /// in the event that two threads generate a sequence at exactly the same 
        /// millisecond.</para>
        /// <para>To generate a unique number, you can use method: B1.DataAccess.DataAccessMgr.GetNextUniqueId</para>
        /// </summary>
        /// <param name="synchronizedTime">The time that is synchronized between servers</param>
        /// <param name="uniqueTieBreakerNumber">A number that needs to be unique and less than 99999</param>
        /// <returns>Unique Sequence number that will correlate between threads and time synchronized machines</returns>
        /// <remarks>
        /// While the database objects like Identities and Sequences work well, they can be a performance
        /// bottleneck.  Especially when multiple threads access them.
        /// <para>This method attempts to offload the need to use the database object for every number generated.</para>
        /// <para>Instead, we offload the gnerating of the sequence numbers to the application servers, which 
        /// will have synchronized clocks.</para>
        /// <para>By utilizing Universal Time to avoid timezone adjustments and military time format, we can leverage
        /// the fact that the collisions can be minimized and the tie breaker number can address those instances.</para>
        /// </remarks>
        public static Int64 GetSequenceNumber(DateTime synchronizedTime, Int64 uniqueTieBreakerNumber)
        {
            if (uniqueTieBreakerNumber > 99999)
                throw new ArgumentOutOfRangeException("uniqueTieBreakerNumber"
                    , string.Format("Value cannot be larger than 99999; value was: {0}", uniqueTieBreakerNumber));

            // format the number as: YYdddHHmmssfffnnnnn
            string sequence = string.Format("{0}{1:000}{2:00}{3:00}{4:00}{5:000}{6:00000}"
                , synchronizedTime.ToString("yy")
                , synchronizedTime.DayOfYear
                , synchronizedTime.ToString("HH")
                , synchronizedTime.ToString("mm")
                , synchronizedTime.ToString("ss")
                , synchronizedTime.ToString("fff")
                , uniqueTieBreakerNumber);
            Int64 sequenceNumber = Convert.ToInt64(sequence);
            return sequenceNumber;
        }

        /// <summary>
        /// Returns a formatted number that can sequences across multiple threads on the same machine.
        /// Unlike, its overloaded method, it will use DateTime.UtcNow as the time.  If you will be
        /// generating numbers on different servers, you should use the overloaded method.
        /// </summary>
        /// <param name="uniqueTieBreakerNumber">A number that needs to be unique and less than 99999</param>
        /// <returns>Unique Sequence number that will correlate between threads</returns>
        public static Int64 GetSequenceNumber(Int64 uniqueTieBreakerNumber)
        {
            return GetSequenceNumber(DateTime.UtcNow, uniqueTieBreakerNumber);
        }

        /// <summary>
        /// Returns a boolean indicating whether or not the last character of the string
        /// text is the given character.
        /// </summary>
        /// <param name="text">Input string to search</param>
        /// <param name="chr">The character to search for.</param>
        /// <returns>Boolean</returns>
        public static bool IsLastCharInText(string text, char chr)
        {
            int idx = text.LastIndexOf(chr);
            if (idx < 0)
                return false;
            int len = text.Length;
            for (int i = idx + 1; i < len; i++)
                if (char.IsLetterOrDigit(text[i]))
                    return false;
            return true;
        }
    }
}
