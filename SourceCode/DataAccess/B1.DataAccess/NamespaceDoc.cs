namespace B1.DataAccess
{
    #pragma warning disable 1574
    /// <summary>
    /// This namespace contains classes related to database access.
    /// <para>
    /// Currently supported DBMS backends and minimum version numbers:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="B1.DataAccess.SqlServer">Microsoft Sql Server Version 10.50.1600</see></item>
    /// <item><see cref="B1.DataAccess.Oracle">Oracle Version 11.2.0.1.0</see></item>
    /// <item><see cref="B1.DataAccess.DB2">IBM DB2 UDB Version 9.7.0002</see></item>
    /// </list>
    /// Some of the supported database operations are:
    /// <list type="bullet">
    /// <item>Merge (aka Upsert)</item>
    /// <item>Update with joins</item>
    /// <item>Delete with joins</item>
    /// <item>Select: inner/outer/cross joins, case columns, group by, order by, in clause, between clause</item>
    /// <item>Insert</item>
    /// </list>
    /// 
    /// <para>
    /// The main class used to access a database is <see cref="B1.DataAccess.DataAccessMgr"/>. This class is used to build and
    /// execute DbCommand objects.
    /// </para>
    /// <para>
    /// <see cref="B1.DataAccess.DataAccessMgr"/> uses the <see cref="B1.DataAccess.DbTableDmlMgr"/> class to build database operations.
    /// </para>
    /// </summary>
    // This class is only needed for SandCastle and its ability to create HTML documenatation for
    // a given namespace.
    #pragma warning restore 1574
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
}
