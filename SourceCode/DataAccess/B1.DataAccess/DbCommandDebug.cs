using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace B1.DataAccess
{
    /// <summary>
    /// This class will not have any runtime overhead and is used only when you are debugging.
    /// It has a single constructor accepting a DbCommand object and a DebugScriptHandler.
    /// <para>
    /// Then it exposes a single property with calls the handler and passes in the DbCommand.
    /// The handler returns a debug string version of the DbCommand object (with parameter declarations)
    /// for the specific back-end database.
    /// </para>
    /// <para>
    ///  DbCommandDebug dbCmdDebug = new DbCommandDebug(dbCommand, _dbProviderLib.GetCommandDebugScript);
    /// </para>
    /// </summary>
    public class DbCommandDebug
    {
        /// <summary>
        /// A delegate that will accept the DbCommand and return a formatted representation
        /// of the CommandText SQL with parameter declarations
        /// </summary>
        /// <param name="dbCmd">The DbCommand object</param>
        /// <returns>Returns the formatted representation of the DbCommand's CommandText SQL with parameter declarataions</returns>
        public delegate string DebugScriptHandler(DbCommand dbCmd);
        DbCommand _dbCommand = null;
        DebugScriptHandler _handler = null;

        /// <summary>
        /// Returns the formatted representation of the DbCommand's CommandText SQL with parameter declarataions
        /// (except binary objects unfortunately).
        /// </summary>
        public string DbCommandWithDebug
        {
            get { return _handler(_dbCommand); }
        }

        /// <summary>
        /// The only constructor which accepts the DbCommand object and a delegate
        /// that will format the DbCommand's CommandText (when this class's property is referenced).
        /// </summary>
        /// <param name="dbCmd">The DbCommand object</param>
        /// <param name="handler">The delegate that will accept the DbCommand and return a formatted representation
        /// of the CommandText SQL with parameter declarations</param>
        public DbCommandDebug(DbCommand dbCmd, DebugScriptHandler handler)
        {
            _dbCommand = dbCmd;
            _handler = handler;
        }
    }
}
