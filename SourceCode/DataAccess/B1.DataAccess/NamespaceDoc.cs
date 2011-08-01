namespace B1.DataAccess
{
    #pragma warning disable 1574
    /// <summary>
    /// This namespace contains classes related to database access.
    /// <para>
    /// Currently, the supported platforms and their minimum version numbers are:
    /// </para>
    /// <list type="bullet">
    /// <item>Microsoft Sql Server: Version 10.50.1600</item>
    /// <item>Oracle: Version 11.2.0.1.0</item>
    /// <item>IBM DB2 UDB: Version 9.7.0002</item>
    /// </list>
    /// <para>
    /// The main assembly of the data access layer is B1.DataAccess.  The assembly does not contain any references
    /// to specific back-end database platforms; instead it references the .Net generic classes of System.Data and System.Data.Common. 
    /// </para>
    /// <para>All database specific code is referenced in DbProvider helper assembly files:</para>
    /// <list type="bullet">
    /// <item>B1.DataAccess.DB2.Db2Helper (DB2/UDB)</item>
    /// <item>B1.DataAccess.OracleDb.OracleHelper (Oracle)</item>
    /// <item>B1.DataAccess.SqlServer.SqlServerHelper (SqlServer)</item>
    /// </list>
    /// Any functions are defined by interface <seealso cref="B1.IDataAccess.IDataAccessProvider"/> in assembly: 
    /// <para>B1.DataAccessProvider.IDataAccess</para>
    /// <para>The interface is implemented by abstract class: <seealso cref="B1.IDataAccess.IDataAccessProvider"/></para>
    /// <para>The dbProvider helper assemblies contain classes that derive from the abstract class.</para>
    /// <para>
    /// The DbAccessManager provides a database independant interface to build and execute parameterized database commands.
    /// </para>
    /// <see cref="DealerTrack.DAL.Common.Data.DbAccessManager"/>
    /// <para>It is the starting point and the main class for your application to use for all its database operations.</para>
    /// </summary>
    #pragma warning restore 1574
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
}
