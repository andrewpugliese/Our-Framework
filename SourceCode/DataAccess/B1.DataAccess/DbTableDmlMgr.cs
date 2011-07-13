using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Data;
using System.Data.Common;

using B1.Core;
using B1.ILoggingManagement;
using B1.CacheManagement;

namespace B1.DataAccess
{
    #pragma warning disable 1591 // disable the xmlComments warning    
    /// <summary>
    /// Enumeration of join types
    /// </summary>
    public enum DbTableJoinType
    {
        None,
        Inner,
        LeftOuter,
        RightOuter,
        Cross,
    }

    /// <summary>
    /// Enumeration of column options for Merge DbCommand
    /// </summary>
    public enum DbTableColumnType
    {
        None,
        ForInsertOrUpdate,
        ForInsertOnly,
        ForUpdateOnly
    }

#pragma warning restore 1591 // disable the xmlComments warning

     /// <summary>
    /// Contains the information for joining to a table.
    /// </summary>
    public class DbTableJoin 
    {
        /// <summary>
        /// Table name to join to
        /// </summary>
        public string TableName;
        /// <summary>
        /// Table alias
        /// </summary>
        public string TableAlias { get; set; }
        /// <summary>
        /// Schema for table. 
        /// </summary>
        public string SchemaName;
        /// <summary>
        /// When the join is an inline view expression, this returns the reference to it
        /// </summary>
        public DbTableDmlMgr InlineView = null;
        /// <summary>
        /// Type of join
        /// </summary>
        public DbTableJoinType JoinType;
        /// <summary>
        /// List of the "ON" conditions. Can be column to column or column to value.
        /// </summary>
        public DbPredicate JoinPredicate;
        /// <summary>
        /// Contains the set of columns that will either be inserted or updated.
        /// Delete operations do not use columns.
        /// The values of the collection can contain a value or DBNull or 
        /// a DbFunctionStructure for autogeneratd fields or inline functions
        /// </summary>
        public Dictionary<string, object> Columns;

        /// <summary>
        /// SchemaName + "." TableName
        /// </summary>
        public string FullyQualifiedName { get { return SchemaName + "." + TableName; } }

        /// <summary>
        /// Columns to retrieve as part of the select clause.
        /// </summary>
        public Dictionary<string, object> SelectColumns;

        /// <summary>
        /// Instantiate a DbTableJoin class for defining a join to an inline view or query.
        /// </summary>
        /// <param name="inlineView">DbTableDmlMgr instance for a inline view or query</param>
        /// <param name="joinType">Type of join</param>
        /// <param name="joinPredicate">Join predicate, i.e, SQL ON conditions</param>
        /// <param name="selectColumns">Columns for select.</param>
        public DbTableJoin(DbTableDmlMgr inlineView, string alias, DbTableJoinType joinType,
                DbPredicate joinPredicate, params object[] selectColumns)
        {
            SchemaName = inlineView.MainTable.SchemaName;
            TableName = inlineView.MainTable.TableName;
            TableAlias = alias;
            JoinType = joinType;
            JoinPredicate = joinPredicate;
            if(JoinPredicate != null)
                JoinPredicate._newJoinTable = this;
            InlineView = new DbTableDmlMgr(inlineView);
            Columns = new Dictionary<string,object>(StringComparer.CurrentCultureIgnoreCase);
            if (selectColumns == null)
            {
                selectColumns = new object[inlineView.QualifiedColumns.Count()];
                int i = 0;
                foreach (string qualifiedColumn in inlineView.QualifiedColumns.ToList<string>())
                    selectColumns[i++] = qualifiedColumn.Split(new char[] { '.' })[1];
            }
            SelectColumns = new Dictionary<string,object>(StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Instantiate a DbTableJoin class for defining a join to a another table.
        /// </summary>
        /// <param name="schemaName">Schema of table to join</param>
        /// <param name="tableName">Table to join to</param>
        /// <param name="alias">Table alias</param>
        /// <param name="joinType">Type of join</param>
        /// <param name="joinPredicate">Join predicate, i.e, SQL ON conditions</param>
        /// <param name="selectColumns">Columns for select.</param>
        public DbTableJoin(string schemaName, string tableName, string alias, DbTableJoinType joinType,
                DbPredicate joinPredicate, params object[] selectColumns)
        {
            TableName = tableName;
            TableAlias = alias;
            SchemaName = schemaName;
            JoinType = joinType;
            JoinPredicate = joinPredicate;
            if (JoinPredicate != null)
                JoinPredicate._newJoinTable = this;

            Columns = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            SelectColumns = selectColumns.ToDictionary(item => item.ToString(), item => item
                    , StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Instantiate a DbTableJoin class for defining a join to a another table.
        /// </summary>
        /// <param name="tableStruct">Structure that will be used to get Table Info</param>
        /// <param name="alias">Table alias</param>
        /// <param name="joinType">Type of join</param>
        /// <param name="joinPredicate">Join predicate, i.e, SQL ON conditions</param>
        /// <param name="selectColumns">Columns for select. If none are included, all we be used for select.</param>
        public DbTableJoin(DbTableStructure tableStruct, string alias, DbTableJoinType joinType, 
                DbPredicate joinPredicate, params object[] selectColumns )
        {
            TableAlias = alias;
            TableName = tableStruct.TableName;
            SchemaName = tableStruct.SchemaName;
            Columns = tableStruct.Columns.ToDictionary(kvp => kvp.Key
                    , new Func<KeyValuePair<string, short>, object>(kvp => null));
            JoinType = joinType;
            JoinPredicate = joinPredicate;
            if(JoinPredicate != null)
                JoinPredicate._newJoinTable = this;

            if (selectColumns != null & selectColumns.Count() > 0)
                SelectColumns = selectColumns.ToDictionary(item => item.ToString(), item => item, StringComparer.CurrentCultureIgnoreCase);
            else
                SelectColumns = tableStruct.Row.ToDictionary(kvp => kvp.Value
                        , kvp => (object)kvp.Value, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Gets the join text for a particular join type.
        /// </summary>
        /// <param name="joinType">Join type</param>
        /// <returns></returns>
        public static string GetJoinStringFromType(DbTableJoinType joinType)
        {
            switch(joinType)
            {
                case DbTableJoinType.None:
                    return "";
                case DbTableJoinType.Inner:
                    return "INNER JOIN";
                case DbTableJoinType.LeftOuter:
                    return "LEFT OUTER JOIN";
                case DbTableJoinType.RightOuter:
                    return "RIGHT OUTER JOIN";
                case DbTableJoinType.Cross:
                    return "CROSS JOIN";
                default:
                    throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue, "Unknown joinType: " 
                            + joinType.ToString());
            }
        }
    }

    /// <summary>
    /// Class for creating SQL statements (Select/Update/Delete/Insert)
    /// 
    /// A DbTableJoinMgr can be used with or without catalog information. Usage depends on which overload of the 
    /// Constructor and AddJoin() methods you use. Some methods allow you to pass in all the table info, 
    /// others allow you to pass in catalog classes/structures.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class DbTableDmlMgr
    { 
        /// <summary>
        /// Schemas to Tables to DbTableJoin
        /// </summary>
        internal Dictionary<string, List<DbTableJoin>> Tables = 
                new Dictionary<string, List<DbTableJoin>>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Alias to InLineView
        /// </summary>
        internal Dictionary<string, DbTableDmlMgr> InLineViews =
                new Dictionary<string, DbTableDmlMgr>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Dictionary of Qualified Column/ column value
        /// </summary>
        internal Dictionary<DbQualifiedObject<string>, object> ColumnsForUpdateOrInsert = 
                new Dictionary<DbQualifiedObject<string>,object>();

        /// <summary>
        /// Dictionary of Qualified Column/ column value used only in INSERT statement
        /// </summary>
        internal Dictionary<DbQualifiedObject<string>, object> ColumnsForInsert =
                new Dictionary<DbQualifiedObject<string>, object>();

        /// <summary>
        /// Dictionary of Qualified Column/ column value used only in UPDATE statement
        /// </summary>
        internal Dictionary<DbQualifiedObject<string>, object> ColumnsForUpdate =
                new Dictionary<DbQualifiedObject<string>, object>();

        private int TableCount
        {
            get 
            {
                return Tables.Aggregate(0, (count, kvp) => count + kvp.Value.Count);
            }

        }

        private void Add(string schemaName, string tableName, DbTableJoin join)
        {
            if(!Tables.ContainsKey(schemaName))
                Tables.Add(schemaName, new List<DbTableJoin>());

            Tables[schemaName].Add(join);
        }
     
        internal DbPredicate _whereCondition;
        Int16 _orderByColumnOffset = 0;
        Int16 _groupByColumnOffset = 0;
        DataAccessMgr _daMgr = null;
        string _joinAlias = null;
                      
        /// <summary>
        /// The Main table of the DML
        /// </summary>
        public DbTableJoin MainTable
        {
            get { return Tables.First().Value.First(); }
        }

        /// <summary>
        /// Returns the alias assign to the current join
        /// </summary>
        public DbPredicateString JoinAlias
        {
            get { return _joinAlias; }
        }

        /// <summary>
        /// Returns all qualified columns; columns can be constants, and fully qualified table column names
        /// </summary>
        public IEnumerable<string> QualifiedColumns
        {
            get
            {
                foreach(var tableList in Tables.Values)
                {
                    foreach(DbTableJoin table in tableList)
                    {
                        foreach(object column in table.SelectColumns.Values)
                            if(!(column is string))
                                yield return column.ToString();
                            else
                                yield return String.Format("{0}.{1}", table.TableAlias, column );
                    }
                }
            }
        }

        /// <summary>
        /// Sets or gets boolean indicating whether to add the "DISTINCT" keyword to select statements. 
        /// </summary>
        public bool SelectDistinct { get; set; }
                
        /// <summary>
        /// Used in select operations define a specific group by clause
        /// Items will be listed in sorted key order. 
        /// </summary>
        public SortedDictionary<Int16, DbQualifiedObject<string>> GroupByColumns = 
                new SortedDictionary<short,DbQualifiedObject<string>>();

        /// <summary>
        /// Used in select operations define a specific order by clause.
        /// DbIndexColumnStructure will hold the column name and
        /// (asc or desc) for ascending (default) or descending.
        /// Items will be listed in sorted key order.
        /// </summary>
        public readonly SortedDictionary<Int16, DbQualifiedObject<DbIndexColumnStructure>> OrderByColumns = 
                new SortedDictionary<short,DbQualifiedObject<DbIndexColumnStructure>>();


        /// <summary>
        /// List of case statements to include in select.
        /// </summary>
        public readonly List<DbCase> CaseColumns = new List<DbCase>();

        /// <summary>
        /// List of inline view statements to include in select.
        /// </summary>
        public readonly List<InlineViewColumn> InlineViewColumns = new List<InlineViewColumn>();

        /// <summary>
        /// Get a DbTableJoin instance based on schema name and table name.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DbTableJoin GetTable(string schemaName, string tableName)
        {
            return Tables[schemaName].Find( 
                            t => t.TableName.ToLower() == tableName.ToLower());
        }

        /// <summary>
        /// Get a DbTableJoin instance based on table name. If there is more than one (in different schemas), 
        /// returns first instance of table with tableName.
        /// </summary>
        /// <param name="tableName">Table name to return.</param>
        /// <returns></returns>
        public DbTableJoin GetTable(string tableName)
        {
            foreach(var tableList in Tables)
            {
                DbTableJoin table = tableList.Value.Find(
                            t => t.TableName.ToLower() == tableName.ToLower());

                if(table != null)
                    return table;
            }

            throw new KeyNotFoundException("Could not find table " + tableName);
        }

        /// <summary>
        /// Creates instance of DbTableDmlMgr
        /// </summary>
        /// <param name="daMgr">DataAccessMgr Object</param>
        /// <param name="schemaName">Schema Name of Main table</param>
        /// <param name="tableName">Main table name</param>
        /// <param name="selectColumns">Columns for select. </param>
        public DbTableDmlMgr(DataAccessMgr daMgr
                , string schemaName
                , string tableName
                , params object[] selectColumns)
            : this(schemaName, tableName, null, daMgr, selectColumns)
        {
        }


        /// <summary>
        /// Creates instance of DbTableDmlMgr for the given table, alias and columns for select
        /// </summary>
        /// <param name="schemaName">Schema table belongs to</param>
        /// <param name="tableName">Table name</param>
        /// <param name="alias">An optional alias for the join; if null one will be generated</param>
        /// <param name="daMgr">DataAccessMgr Object</param>
        /// <param name="selectColumns">Columns for select. If none are included, all we be used for select.</param> 
        public DbTableDmlMgr(string schemaName
                   , string tableName
                   , string alias
                   , DataAccessMgr daMgr
                   , params object[] selectColumns)
        {
            _daMgr = daMgr;
            AddJoin(schemaName, tableName, alias, DbTableJoinType.None, null, selectColumns);
        }

        /// <summary>
        /// Creates instance of DbTableDmlMgr with the given table structure and columns for select
        /// </summary>
        /// <param name="daMgr">DataAccessMgr Object</param>
        /// <param name="table">Main DbTableStructure</param>
        /// <param name="selectColumns">Columns for select. If none are included, all we be used for select.</param> 
        public DbTableDmlMgr(DataAccessMgr daMgr
                , DbTableStructure table
                , params object[] selectColumns)
            : this(daMgr, null, table, selectColumns)
        {
        }

        /// <summary>
        /// Creates instance of DbTableDmlMgr with the given table structure, alias and columns for select
        /// </summary>
        /// <param name="daMgr">DataAccessMgr Object</param>
        /// <param name="alias">An optional alias for the join; if null one will be generated</param>
        /// <param name="table">Main DbTableStructure</param>
        /// <param name="selectColumns">Columns for select. If none are included, all we be used for select.</param> 
        public DbTableDmlMgr(DataAccessMgr daMgr
                , string alias
                , DbTableStructure table
                , params object[] selectColumns)
        {
            _daMgr = daMgr;
            AddJoin(table, alias, DbTableJoinType.None, null, selectColumns);
        }


        /// <summary>
        /// Creates instance of DbTableDmlMgr by performing a deep copy on passed in instance.
        /// </summary>
        /// <param name="dmlMgr"></param>
        public DbTableDmlMgr(DbTableDmlMgr dmlMgr)
        {
            foreach(var tableList in dmlMgr.Tables)
            {
                List<DbTableJoin> newTableList = new List<DbTableJoin>();

                Tables.Add(tableList.Key, newTableList);

                foreach(DbTableJoin table in tableList.Value)
                {
                    object[] selectColumns = table.SelectColumns.Values.ToArray<object>();

                    newTableList.Add(new DbTableJoin(table.SchemaName
                            , table.TableName
                            , table.TableAlias
                            , table.JoinType
                            , table.JoinPredicate == null ? null 
                                : new DbPredicate(table.JoinPredicate._predicate, this), 
                            selectColumns));
                }
            }
            _daMgr = dmlMgr._daMgr;
            CaseColumns = dmlMgr.CaseColumns.Select(c => new DbCase(c)).ToList();
            ColumnsForUpdateOrInsert 
                    = new Dictionary<DbQualifiedObject<string>,object>(dmlMgr.ColumnsForUpdateOrInsert);
            GroupByColumns = new SortedDictionary<short,DbQualifiedObject<string>>(dmlMgr.GroupByColumns);
            OrderByColumns 
                    = new SortedDictionary<short,DbQualifiedObject<DbIndexColumnStructure>>(dmlMgr.OrderByColumns);
            SelectDistinct = dmlMgr.SelectDistinct;
            _whereCondition = dmlMgr._whereCondition == null ? null : 
                    new DbPredicate(dmlMgr._whereCondition._predicate, this);
        }

        /// <summary>
        /// Returns a string for use in select where column is aliased
        /// </summary>
        /// <param name="columnName">column name</param>
        /// <param name="columnAlias">column alias</param>
        /// <returns>column as alias</returns>
        public string ColumnsAs(string columnName, string columnAlias)
        {
            return SelectColumnsAs(columnName, columnAlias);
        }

        /// <summary>
        /// Returns a string for use in select where column is aliased
        /// </summary>
        /// <param name="columnName">column name</param>
        /// <param name="columnAlias">column alias</param>
        /// <returns>column as alias</returns>
        public static string SelectColumnsAs(string columnName, string columnAlias)
        {
            return columnName + " as " + columnAlias;
        }

        /// <summary>
        /// Adds the given table and join expression to the current expression
        /// </summary>
        /// <param name="schemaName">Schema for the given table</param>
        /// <param name="tableName">Table name</param>
        /// <param name="type">Join type (inner, outer, cross)</param>
        /// <param name="predicate">Join expression</param>
        /// <param name="selectColumns">Columns for select</param>
        /// <returns>Newly joined table alias.</returns>
        public string AddJoin(string schemaName
                , string tableName
                , DbTableJoinType type
                , Expression<Func<DbTableDmlMgr, bool>> predicate
                , params object[] selectColumns)
        {
            return AddJoin(schemaName, tableName, null, type, predicate, selectColumns);
        }

        /// <summary>
        /// Adds the given table and join expression to the current expression
        /// </summary>
        /// <param name="schemaName">Schema for the given table</param>
        /// <param name="tableName">Table name</param>
        /// <param name="alias">An optional alias for the join; if null one will be generated</param>
        /// <param name="type">Join type (inner, outer, cross)</param>
        /// <param name="predicate">Join expression</param>
        /// <param name="selectColumns">Columns for select</param>
        /// <returns>Newly joined table alias.</returns>
        public string AddJoin(string schemaName
                , string tableName
                , string alias
                , DbTableJoinType type
                , Expression<Func<DbTableDmlMgr, bool>> predicate
                , params object[] selectColumns)
        {
            _joinAlias = string.IsNullOrEmpty(alias) ? "T" + (TableCount + 1).ToString() : alias;
            Add(schemaName, tableName,
                    new DbTableJoin(schemaName, tableName, _joinAlias, type,
                    predicate == null ? null : new DbPredicate(predicate, this), selectColumns));

            return _joinAlias;
        }

        /// <summary>
        /// Adds the given table and join expression to the current expression
        /// </summary>
        /// <param name="table">Table meta data</param>
        /// <param name="type">Join type (inner, outer, cross)</param>
        /// <param name="predicate">Join expression</param>
        /// <param name="selectColumns">Columns for select; If none provided, all will be selected</param>
        /// <returns>Newly joined table alias.</returns>
        public string AddJoin(DbTableStructure table
                , DbTableJoinType type
                , Expression<Func<DbTableDmlMgr, bool>> predicate
                , params object[] selectColumns)
        {
            return AddJoin(table, null, type, predicate, selectColumns);
        }

        /// <summary>
        /// Adds the given table and join expression to the current expression
        /// </summary>
        /// <param name="table">Table meta data</param>
        /// <param name="alias">An optional alias for the join; if null one will be generated</param>
        /// <param name="type">Join type (inner, outer, cross)</param>
        /// <param name="predicate">Join expression</param>
        /// <param name="selectColumns">Columns for select</param>
        /// <returns>Newly joined table alias.</returns>
        public string AddJoin(DbTableStructure table
                , string alias
                , DbTableJoinType type
                , Expression<Func<DbTableDmlMgr, bool>> predicate
                , params object[] selectColumns)
        {
            _joinAlias = string.IsNullOrEmpty(alias) ? "T" + (TableCount + 1).ToString() : alias;
            Add(table.SchemaName, table.TableName,
                    new DbTableJoin(table, _joinAlias, type,
                    predicate == null ? null : new DbPredicate(predicate, this), selectColumns));
            return _joinAlias;
        }

        /// <summary>
        /// Adds the given reference to DbTableDmlMgr and join expression to the current expression
        /// </summary>
        /// <param name="inlineView">A reference to DbTableDmlMgr object with describes the inline view</param>
        /// <param name="type">Join type (inner, outer, cross)</param>
        /// <param name="predicate">Join expression</param>
        /// <returns></returns>
        public string AddJoin(DbTableDmlMgr inlineView
                , DbTableJoinType type
                , Expression<Func<DbTableDmlMgr, bool>> predicate
                , params object[] selectColumns)
        {
            return AddJoin(inlineView, null, type, predicate, selectColumns);
        }

        /// <summary>
        /// Adds the given reference to DbTableDmlMgr and join expression to the current expression
        /// </summary>
        /// <param name="inlineView">A reference to DbTableDmlMgr object with describes the inline view</param>
        /// <param name="alias">An optional alias for the join; if null one will be generated</param>
        /// <param name="type">Join Type (e.g. inner, outer, cross)</param>
        /// <param name="predicate">Join expression</param>
        /// <returns>Newly joined inLineView alias.</returns>
        public string AddJoin(DbTableDmlMgr inlineView
                , string alias
                , DbTableJoinType type
                 , Expression<Func<DbTableDmlMgr, bool>> predicate
                , params object[] selectColumns)
        {
            _joinAlias = string.IsNullOrEmpty(alias) ? "V" + (TableCount + 1).ToString() : alias;
            Add(inlineView.MainTable.SchemaName, inlineView.MainTable.TableName,
                    new DbTableJoin(inlineView
                        , _joinAlias
                        , type
                        , predicate == null ? null : new DbPredicate(predicate, this)
                        , selectColumns));
            return _joinAlias;
        }

        /// <summary>
        /// Adds a column for updating or inserting. Creates a parameter (from columnName) using the catalog.
        /// </summary>
        /// <param name="columnName">Unqalified column name</param>
        public void AddColumn(string columnName)
        {
            DbTableJoin table = MainTable;
            ColumnsForUpdateOrInsert.Add(new DbQualifiedObject<string>(table.SchemaName
                    , table.TableName, columnName)
                    , _daMgr.BuildParamName(columnName));
            if (!table.SelectColumns.ContainsKey(columnName))
              table.SelectColumns.Add(columnName, columnName);
        }

        /// <summary>
        /// Adds a column for updating or inserting. Creates a parameter (parameterName) using the catalog.
        /// </summary>
        /// <param name="columnName">Column name from main table</param>
        /// <param name="parameterName">The name of the parameter that will be created by the DataAccessMgr.</param>
        public void AddColumn(string columnName, string parameterName)
        {
            DbTableJoin table = MainTable;
            ColumnsForUpdateOrInsert.Add(new DbQualifiedObject<string>(table.SchemaName
                    , table.TableName, columnName), parameterName);
            if (!table.SelectColumns.ContainsKey(columnName))
              table.SelectColumns.Add(columnName, columnName);
        }

        /// <summary>
        /// Adds a column for updating or inserting. Uses function as parameter
        /// </summary>
        /// <param name="columnName">Column name from main table</param>
        /// <param name="function"></param>
        public void AddColumn(string columnName, DbFunctionStructure function)
        {
            DbTableJoin table = MainTable;
            ColumnsForUpdateOrInsert.Add(new DbQualifiedObject<string>(
                    table.SchemaName, table.TableName, columnName), function);
            if(!table.SelectColumns.ContainsKey(columnName))
              table.SelectColumns.Add(columnName, columnName);
        }

        /// <summary>
        /// Adds a column for updating or instering. Uses passed in DbParameter
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="parameter"></param>
        public void AddColumn(string columnName, DbParameter parameter)
        {
            DbTableJoin table = MainTable;
            ColumnsForUpdateOrInsert.Add(new DbQualifiedObject<string>(
                    table.SchemaName, table.TableName, columnName), parameter);
            if(!table.SelectColumns.ContainsKey(columnName))
              table.SelectColumns.Add(columnName, columnName);
        }

                /// <summary>
        /// Adds a column for updating or instering.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        public void AddColumn(string columnName, DbConstValue value)
        {
            DbTableJoin table = MainTable;
            ColumnsForUpdateOrInsert.Add(new DbQualifiedObject<string>(
                    table.SchemaName, table.TableName, columnName), value);
            if(!table.SelectColumns.ContainsKey(columnName))
              table.SelectColumns.Add(columnName, columnName);
        }

        /// <summary>
        /// Adds a column for updating or instering.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="dateFunction"></param>
        public void AddColumn(string columnName, EnumDateTimeLocale dateFunction)
        {
            DbTableJoin table = Tables.First().Value.First();
            ColumnsForUpdateOrInsert.Add(new DbQualifiedObject<string>(
                    table.SchemaName, table.TableName, columnName), dateFunction);
            if(!table.SelectColumns.ContainsKey(columnName))
              table.SelectColumns.Add(columnName, columnName);
        }

        /// <summary>
        /// Adds a column for inserting only or updating only or both. Creates a parameter (parameterName) using the catalog.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="parameterName">The name of the parameter that will be created by the DataAccessMgr.</param>
        /// <param name="columnType">Specified whether insert only or update only or both</param>
        public void AddColumn( string columnName, string parameterName, DbTableColumnType columnType )
        {
            DbTableJoin table = MainTable;

            if (columnType != DbTableColumnType.ForInsertOnly)
                ColumnsForUpdate.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), parameterName );
            else if (columnType != DbTableColumnType.ForUpdateOnly)
                ColumnsForInsert.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), parameterName );
            else
                ColumnsForUpdateOrInsert.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), parameterName );

            if (!table.SelectColumns.ContainsKey( columnName ))
                table.SelectColumns.Add(columnName, columnName);
        }

        /// <summary>
        /// Adds a column for inserting only or updating only or both. Uses function as parameter
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="function"></param>
        /// <param name="columnType">Specified whether insert only or update only or both</param>
        public void AddColumn( string columnName, DbFunctionStructure function, DbTableColumnType columnType )
        {
            DbTableJoin table = MainTable;

            if (columnType != DbTableColumnType.ForInsertOnly)
                ColumnsForUpdate.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), function );
            else if (columnType != DbTableColumnType.ForUpdateOnly)
                ColumnsForInsert.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), function );
            else
                ColumnsForUpdateOrInsert.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), function );

            if (!table.SelectColumns.ContainsKey(columnName))
                table.SelectColumns.Add(columnName, columnName);
        }

        /// <summary>
        /// Adds a column for inserting only or updating only or both. Uses passed in DbParameter
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="parameter"></param>
        /// <param name="columnType">Specified whether insert only or update only or both</param>
        public void AddColumn( string columnName, DbParameter parameter, DbTableColumnType columnType )
        {
            DbTableJoin table = MainTable;

            if (columnType != DbTableColumnType.ForInsertOnly)
                ColumnsForUpdate.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), parameter );
            else if (columnType != DbTableColumnType.ForUpdateOnly)
                ColumnsForInsert.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), parameter );
            else
                ColumnsForUpdateOrInsert.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), parameter );

            if (!table.SelectColumns.ContainsKey(columnName))
                table.SelectColumns.Add(columnName, columnName);
        }

        /// <summary>
        /// Adds a column for inserting only or updating only or both.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <param name="columnType">Specified whether insert only or update only or both</param>
        public void AddColumn( string columnName, DbConstValue value, DbTableColumnType columnType )
        {
            DbTableJoin table = MainTable;

            if (columnType != DbTableColumnType.ForInsertOnly)
                ColumnsForUpdate.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), value );
            else if (columnType != DbTableColumnType.ForUpdateOnly)
                ColumnsForInsert.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), value );
            else
                ColumnsForUpdateOrInsert.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), value );

            if (!table.SelectColumns.ContainsKey(columnName))
                table.SelectColumns.Add(columnName, columnName);
        }

        /// <summary>
        /// Adds a column for inserting only or updating only or both.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="dateFunction"></param>
        /// <param name="columnType">Specified whether insert only or update only or both</param>
        public void AddColumn( string columnName, EnumDateTimeLocale dateFunction, DbTableColumnType columnType )
        {
            DbTableJoin table = Tables.First().Value.First();

            if (columnType != DbTableColumnType.ForInsertOnly)
                ColumnsForUpdate.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), dateFunction );
            else if (columnType != DbTableColumnType.ForUpdateOnly)
                ColumnsForInsert.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), dateFunction );
            else
                ColumnsForUpdateOrInsert.Add( new DbQualifiedObject<string>( 
                        table.SchemaName, table.TableName, columnName ), dateFunction );

            if (!table.SelectColumns.ContainsKey(columnName))
                table.SelectColumns.Add( columnName, columnName );
        }

        /// <summary>
        /// Method will add the fully qualified column name to the Order By clause
        /// as an ascending sort order
        /// </summary>
        /// <param name="schemaName">Schema name of the table that owns the column</param>
        /// <param name="tableName">Table name of the table that owns the column</param>
        /// <param name="columnName">Column name to use in the order by</param>
        /// <returns>The offset of the column in the order by container (0, 1, 2)</returns>
        public Int16 AddOrderByColumnAscending(string schemaName, string tableName, string columnName)
        {
            OrderByColumns.Add(_orderByColumnOffset, new DbQualifiedObject<DbIndexColumnStructure>(
                    schemaName
                    , tableName
                    , _daMgr.BuildIndexColumnAscending(columnName)));
            return _orderByColumnOffset++;
        }

        /// <summary>
        /// Method will add the column name to the Order By clause
        /// as an ascending sort order.  It will assume the MainTable Schema and Table.
        /// </summary>
        /// <param name="columnName">Column Name of the Main Table to use in the order by</param>
        /// <returns>The offset of the column in the order by container (0, 1, 2)</returns>
        public Int16 AddOrderByColumnAscending(string columnName)
        {
            return AddOrderByColumnAscending(MainTable.SchemaName, MainTable.TableName, columnName);
        }

        /// <summary>
        /// Method will add the fully qualified column name to the Order By clause
        /// as an ascending sort order
        /// </summary>
        /// <param name="schemaName">Schema name of the table that owns the column</param>
        /// <param name="tableName">Table name of the table that owns the column</param>
        /// <param name="columnName">Column name to use in the order by</param>
        /// <returns>The offset of the column in the order by container (0, 1, 2)</returns>
        public Int16 AddOrderByColumnDescending(string schemaName, string tableName, string columnName)
        {
            OrderByColumns.Add(_orderByColumnOffset, new DbQualifiedObject<DbIndexColumnStructure>(
                    schemaName
                    , tableName
                    , _daMgr.BuildIndexColumnDescending(columnName)));
            return _orderByColumnOffset++;
        }

        /// <summary>
        /// Method will add the column name to the Order By clause
        /// as an descending sort order.  It will assume the MainTable Schema and Table.
        /// </summary>
        /// <param name="columnName">Column Name of the Main Table to use in the order by</param>
        /// <returns>The offset of the column in the order by container (0, 1, 2)</returns>
        public Int16 AddOrderByColumnDescending(string columnName)
        {
            return AddOrderByColumnDescending(MainTable.SchemaName, MainTable.TableName, columnName);
        }

        /// <summary>
        /// Method will add the fully qualified column name to the Group By clause
        /// </summary>
        /// <param name="schemaName">Schema Name of the table containing the column</param>
        /// <param name="tableName">Table Name which column belongs to</param>
        /// <param name="columnName">Column to group by</param>
        /// <returns>The offset of the column in the group by container (0, 1, 2)</returns>
        public Int16 AddGroupByColumn(string schemaName, string tableName, string columnName)
        {
            GroupByColumns.Add(_groupByColumnOffset, new DbQualifiedObject<string>(
                    schemaName
                    , tableName
                    , columnName));
            return _groupByColumnOffset++;
        }

        /// <summary>
        /// Method will add the column name to the Group By clause.  
        /// It will assume the column belongs to the main table.
        /// </summary>
        /// <param name="columnName">Column (of the MainTable) to group by</param>
        /// <returns>The offset of the column in the group by container (0, 1, 2)</returns>
        public Int16 AddGroupByColumn(string columnName)
        {
            return AddGroupByColumn(MainTable.SchemaName, MainTable.TableName, columnName);
        }

        /// <summary>
        /// <para>
        /// Adds a case statement as a column in the select.
        /// </para>
        /// <para>
        /// Example 1:
        /// </para>
        /// 
        /// To generate this SQL:
        /// <![CDATA[
        /// <code>
        ///   case
        ///       when T1.ConfigSetName < 'b' then 'Less than B'
        ///       when T1.ConfigSetName = 'Z' then 'Is Z'
        ///   else T1.ConfigSetName
        ///   end as TestCase 
        /// </code>
        /// ]]>
        /// The following code would be used:
        /// <![CDATA[
        /// <code>
        /// joinSelect.AddCaseColumn(joinSelect.AliasedColumn(joinSelect.MainTable.TableAlias, "ConfigSetName"), "TestCase", 
        ///        joinSelect.When(t => t.AliasedColumn(joinSelect.MainTable.TableAlias, "ConfigSetName") < "b" , "'Less than B'"),
        ///        joinSelect.When(t => t.AliasedColumn(joinSelect.MainTable.TableAlias, "ConfigSetName") == "Z" , "'Is Z'"));
        /// </code>
        /// ]]>   
        /// 
        /// Example 2:
        /// <code>
        /// To generate this SQL:
        ///    case when isc.is_nullable = 'yes' then 1
        ///    else 0 
        ///    end as IsNullable
        /// </code>
        /// 
        /// The following code would be used:
        /// <code>
        ///  joinSelect.AddCaseColumn("0", "IsNullable", 
        ///      joinSelect.When( t => t.AliasedColumn(columnsInformationAlias, "is_nullable") == "yes", "1"));
        /// </code>
        /// 
        /// See the SetWhereCondition method below for different methods that could be used for the else and then statements.
        /// </summary>
        /// <param name="elseStatement"></param>
        /// <param name="caseAlias"></param>
        /// <param name="when"></param>
        public void AddCaseColumn(string elseStatement, string caseAlias, params DbCaseWhen[] when)
        {
            DbCase dbCase = new DbCase() 
                    {
                        ElseStatement = elseStatement,
                        Alias = caseAlias
                    };

            dbCase.Whens.AddRange(when);

            CaseColumns.Add(dbCase);
        }


        /// <summary>
        /// Adds the given reference to DbTableDmlMgr as an inline query
        /// </summary>
        /// <param name="inlineView">DbTableDmlManager instance</param>
        /// <returns>The alias assigned to the inline view</returns>
        public string AddInlineViewColumn(DbTableDmlMgr inlineView)
        {
            return AddInlineViewColumn(inlineView, "I" + InlineViewColumns.Count().ToString());
        }

        /// <summary>
        /// Adds the given reference to DbTableDmlMgr as an inline query and assigns it to the given alias
        /// </summary>
        /// <param name="inlineView">DbTableDmlManager instance</param>
        /// <param name="alias">Alias to reference the Inline view with</param>
        /// <returns>The alias assigned to the inline view</returns>
        public string AddInlineViewColumn(DbTableDmlMgr inlineView, string alias)
        {
            Tuple<string, DbParameterCollection> inlineVw = _daMgr.BuildSelect(inlineView, null, null);
            string inlineViewSelect = inlineVw.Item1;
            InlineViewColumn ivc = new InlineViewColumn();
            ivc.Alias = alias;
            ivc.InlineView = inlineVw.Item1;
            ivc.dbParams = inlineVw.Item2;
            InlineViewColumns.Add(ivc);
            return alias;
        }

        /// <summary>
        /// Used to create WHEN portions of case statement. See AddCaseColumn for examples.
        /// </summary>
        /// <param name="whenPredicate">See SetWhereCondition for usage and examples</param>
        /// <param name="thenStatement">Then statement. See AddCaseColumn for examples. 
        /// See SetWhereCondition for possible helper functions like AliasedColumn </param>
        /// <returns></returns>
        public DbCaseWhen When(Expression<Func<DbTableDmlMgr, bool>> whenPredicate, string thenStatement)
        {
            return new DbCaseWhen(new DbPredicate(whenPredicate, this), thenStatement);
        }

        /// <summary>
        /// <param>
        /// Uses .NET expression to represent where clause.
        /// </param>
        /// </summary>
        /// <remarks>
        /// 
        /// <param>
        /// Example:
        /// </param>
        /// 
        /// <code>
        /// joinSelect.SetWhereCondition( t => 
        ///         (paramSchemaName == null || t.Column("sys", "schemas", "name") == paramSchemaName)
        ///         &amp;&amp; (paramTableName  == null || t.Column("sys", "tables", "name") == paramTableName));
        /// </code>
        /// generates the where clause:
        /// 
        /// <code>
        /// where (@SchemaName is null or s.name = @SchemaName)
        /// and (@TableName is null or t.Name = @TableName)
        /// </code>
        /// 
        /// <list type="bullet">
        /// <item>
        /// For representing columns in a predicate, you use the j.Column() methods. 
        /// </item>
        /// <item>
        /// For representing values in a predicate, you can use the j.Value() methods or just put in the value: 
        /// <list type="bullet">
        /// <item>
        ///     <code>
        ///     j.Column("sys", "schemas", "name") == j.Value("MySchema") 
        ///     </code>
        ///           is the same as 
        ///     <code>
        ///     j.Column("sys", "schemas", "name") == "MySchema"
        ///     </code>
        /// </item>
        /// <item>
        /// Values are quoted correctly based on type: j.Value(32) will not be quoted. 
        ///     If you want it quoted, make the call j.Value("32") or j.Value(someNum.ToString())
        /// </item>
        /// <item>
        ///  nulls are handled appropriately:
        ///     j.Column("sys", "schemas", "name") == null generates SQL:  tablequalifier.schemas is null
        /// </item>
        /// </list>
        /// </item>
        /// <item>
        /// For representing a database function, i.e, getDate(), use the j.Function methods.
        /// </item>
        /// <item>
        /// For representing a parameter the developer has 2 options:  
        /// <list type="bullet">
        /// 
        /// <item>
        ///  Option 1: In the example above, the developer not being able to make use of the catalog, 
        ///    needs to create his own .Net DbParameters (paramSchemaName and paramTableName), when a 
        ///    .Net DbParameter is encountered in the predicate expression, it is handled appropriately.
        /// </item>
        /// <item>
        ///  Option2:  When the catalog is available, use the j.Parameter() methods. This will create 
        ///    a parameter for you based on the catalog information of that column.
        /// </item>
        /// </list>
        /// </item>
        /// </list>
        /// </remarks>
        /// 
        /// <param name="predicate">A lambda which takes a DbTableDmlMgr. The rules for the lambda are described in 
        /// method summary.</param>
        #pragma warning restore 1591 // disable the xmlComments warning    
        public void SetWhereCondition(Expression<Func<DbTableDmlMgr, bool>> predicate)
        {
            _whereCondition = new DbPredicate(predicate, this);
        }

        /// <summary cref="DbTableDmlMgr.SetWhereCondition(Expression{Func{DbTableDmlMgr, bool}})"/>
        /// <param name="predicate"></param>
        public void SetWhereCondition(Expression predicate)
        {
            _whereCondition = new DbPredicate(Expression.Lambda<Func<DbTableDmlMgr, bool>>(predicate, 
                Expression.Parameter(typeof(DbTableDmlMgr), "j")), this);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="combiningOperation"></param>
        /// <param name="predicate"></param>
        public void SetOrAddWhereCondition(ExpressionType combiningOperation, Expression predicate)
        {
            if(_whereCondition == null)
                SetWhereCondition(predicate);
            else
                AddToWhereCondition(combiningOperation, predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="combiningOperation"></param>
        /// <param name="predicate"></param>
        public void AddToWhereCondition(ExpressionType combiningOperation, Expression predicate)
        {
            _whereCondition._predicate = Expression.Lambda<Func<DbTableDmlMgr, bool>>(
                    Expression.MakeBinary(combiningOperation, _whereCondition._predicate.Body,
                        predicate), Expression.Parameter(typeof(DbTableDmlMgr), 
                            _whereCondition._predicate.Parameters.First().Name));
        }

        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// </summary>
        /// <param name="tableAlias"></param>
        /// <param name="columnName"></param>
        /// <returns>tableAlias.columnName;</returns>
        public DbPredicateString AliasedColumn(string tableAlias, string columnName)
        {
            return tableAlias + "." + columnName;
        }

        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// It will use the main table's alias as the qualification of the column
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns>MainTable.Alias.columnName;</returns>
        public DbPredicateString AliasedColumn(string columnName)
        {
            return AliasedColumn(MainTable.TableAlias, columnName);
        }

        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// It will use the alias of the current join predicate as the qualification of the column
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns>joinAlias.columnName;</returns>
        public DbPredicateString JoinAliasedColumn(string columnName)
        {
            return _joinAlias + "." + columnName;
        }

        /// <summary>
        /// Meant for use ONLY in SetWhereCondition or Join predicate lambda.
        /// Can be used for where condition when there is only one table or for join
        /// when refering to columns in newly joined table.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public DbPredicateString Column(string columnName)
        {
            string alias = DbPredicate._currentJoinTableAlias == null ? 
                MainTable.TableAlias : DbPredicate._currentJoinTableAlias;

            return AliasedColumn(alias, columnName);
        }
        
        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public DbPredicateString Column(string tableName, string columnName)
        {
            return GetTable(tableName).TableAlias + "." + columnName;
        }

        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public DbPredicateString Column(string schemaName, string tableName, string columnName)
        {
            return GetTable(schemaName, tableName).TableAlias + "." + columnName;
        }

        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public DbPredicateString Function(string functionName)
        {
            return functionName;
        }

        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public DbPredicateString Value(object value)
        {
            return DbConstValue.GetQuotedValue(value);  
        }

        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// </summary>
        /// <param name="columnName">Unqualified Column Name from the MainTable</param>
        /// <returns>DbPredicateString of the fully qualified column Name</returns>
        public DbPredicateString Parameter(string columnName)
        {
            return new DbPredicateStringParameter(new DbPredicateParameter()
            {
                ParameterName = _daMgr.BuildParamName(columnName),
                TableName = MainTable.TableName,
                ColumnName = columnName,
                SchemaName = MainTable.SchemaName
            });
        }

        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public DbPredicateString Parameter(string tableName, string columnName, string parameterName)
        {
            return new DbPredicateStringParameter(new DbPredicateParameter()
            {
                ParameterName = parameterName,
                TableName = tableName,
                ColumnName = columnName,
                SchemaName = GetTable(tableName).SchemaName
            });
        }

        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public DbPredicateString Parameter(string schemaName, string tableName, string columnName, string parameterName)
        {
            return new DbPredicateStringParameter(new DbPredicateParameter()
                {
                    ParameterName = parameterName,
                    TableName = tableName,
                    ColumnName = columnName,
                    SchemaName = schemaName
                });
        }
        
        /// <summary>
        /// Meant for use in SetWhereCondition expressions, Join predicate expressions, and Case statements
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public DbPredicateString Parameter(DbParameter parameter)
        {
            return new DbPredicateStringParameter(new DbPredicateParameter()
                {
                    ParameterName = parameter.ParameterName,
                    Parameter = parameter
                });
        }
        
        /// <summary>
        /// Creates a SQL IN clause. 
        /// Example: If numParameters is 3 and parameterNamePrefix is "firstName" then in clause will look like:
        /// IN ( @firstname1, @firstname2, @firstname3 )
        /// Meant for use ONLY in SetWhereCondition or Join predicate lambda
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="numParameters">Number of parameters withing the IN clause</param>
        /// <param name="parameterNamePrefix">Parameter name prefix.</param>
        /// <returns></returns>
        public DbPredicateInClause In(string schemaName, string tableName, string columnName, int numParameters, 
                string parameterNamePrefix)
        {
            string tableAlias = GetTable(schemaName, tableName).TableAlias;
            return new DbPredicateInClause(schemaName
                    , tableName
                    , tableAlias
                    , columnName
                    , numParameters
                    , parameterNamePrefix);
        }
   
        /// <summary>
        /// Creates a SQL Between clause.
        /// Meant for use ONLY in SetWhereCondition or Join predicate lambda
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="lowParameterName"></param>
        /// <param name="highParameterName"></param>
        /// <returns></returns>
        public DbPredicateBetweenClause Between(string schemaName, string tableName, string columnName,
                string lowParameterName, string highParameterName)
        {
            string tableAlias = GetTable(schemaName, tableName).TableAlias;
            return new DbPredicateBetweenClause(schemaName, tableName, tableAlias, columnName, lowParameterName, 
                    highParameterName);
        }
    }
    
    #pragma warning disable 1591 // disable the xmlComments warning    
    public class DbPredicateParameter
    {
        public string SchemaName { get; set; }
        public string ParameterName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public DbParameter Parameter { get; set; }
        public object Value { get; set; }

        // Fields used for member access in Linq and Entity Framework DML;
        public string MemberPropertyName { get; set; }
        public Delegate MemberAccess { get; set; }

        public DbPredicateParameter()
        {
        }

        public DbPredicateParameter(DbPredicateParameter param)
        {
            SchemaName = param.SchemaName;
            ParameterName = param.ParameterName;
            TableName = param.TableName;
            ColumnName = param.ColumnName;
            Parameter = param.Parameter;
            Value = param.Value;
            MemberPropertyName = param.MemberPropertyName;
            MemberAccess = param.MemberAccess;
        }
    }

    public class DbPredicateStringParameter : DbPredicateString
    {
        public DbPredicateParameter Parameter;

        public DbPredicateStringParameter(DbPredicateParameter parameter)
        {
            Parameter = parameter;
        }
    }

    public class DbPredicateInClause : DbPredicateString
    {
        public string SchemaName {get; set;}
        public string TableName {get; set;}
        public string TableAlias {get; set;}
        public string ColumnName {get; set;}
        public string ParameterNamePrefix {get; set;}
        public int NumParameters {get; set;}

        public DbPredicateInClause(string schemaName, string tableName, string tableAlias, string columnName, 
                int numParameters, string parameterNamePrefix)
        {
            SchemaName = schemaName;
            TableName = tableName;
            TableAlias = tableAlias;
            ColumnName = columnName;
            NumParameters = numParameters;
            ParameterNamePrefix = parameterNamePrefix;
        }
    }

    public class DbPredicateBetweenClause : DbPredicateString
    {
        public string SchemaName {get; set;}
        public string TableName {get; set;}
        public string TableAlias {get; set;}
        public string ColumnName {get; set;}
        public string LowParameterName {get; set;}
        public string HighParameterName {get; set;}
        
        public DbPredicateBetweenClause(string schemaName, string tableName, string tableAlias, string columnName, 
                string lowParameterName, string highParameterName)
        {
            SchemaName = schemaName;
            TableName = tableName;
            TableAlias = tableAlias;
            ColumnName = columnName;
            LowParameterName = lowParameterName;
            HighParameterName = highParameterName;
        }
    }

    public class DbPredicateString
    {
        private string _value;

        public DbPredicateString()
        {
        }
                
        /// <summary>
        /// Creates a DbPredicateString object.
        /// </summary>
        /// <param name="value">String that this object will hold.</param>
        public DbPredicateString(string value)
        {
            _value = value;
        }

        public static implicit operator string(DbPredicateString dbPredicateString)
        {
            return dbPredicateString.ToString();
        }

        public static implicit operator DbPredicateString(string value)
        {
            return new DbPredicateString(value);
        }

        public static bool operator >(DbPredicateString left, object right)
        {
            throw new NotImplementedException();
        }

        public static bool operator <(DbPredicateString left, object right)
        {
            throw new NotImplementedException();
        }

        public static bool operator <=(DbPredicateString left, object right)
        {
            throw new NotImplementedException();
        }

        public static bool operator >=(DbPredicateString left, object right)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(DbPredicateString left, object right)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(DbPredicateString left, object right)
        {
            throw new NotImplementedException();
        }

        public static implicit operator bool(DbPredicateString obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return _value;
        }

       
    }

    public class DbPredicate
    {   
        /// <summary>
        /// This table alias is used in the buiding of a predicate for Joins.
        /// It will be changed when a predicate is being evaluated to a string for a join. It is
        /// thread specific.
        /// </summary>
        [ThreadStatic]
        internal static string _currentJoinTableAlias;

        private DbTableDmlMgr _joinMgr = null;
        private int _paramCount = 0;
        internal Expression<Func<DbTableDmlMgr, bool>> _predicate;
        // if not null, the table that was joined using this predicate;
        internal DbTableJoin _newJoinTable = null;

        public readonly Dictionary<string, DbPredicateParameter> Parameters = 
                new Dictionary<string, DbPredicateParameter>(StringComparer.CurrentCultureIgnoreCase);

        public DbPredicate(Expression<Func<DbTableDmlMgr, bool>> predicate, DbTableDmlMgr joinMgr)
        {
            _joinMgr = joinMgr;
            _predicate = predicate;
        }

        public string ToString(DataAccessMgr daMgr)
        {
            return ParsePredicate(_predicate.Body, daMgr);
        }

        private string ParsePredicate(Expression body, DataAccessMgr daMgr)
        {
            if(_newJoinTable != null)
                _currentJoinTableAlias = _newJoinTable.TableAlias;
            else
                _currentJoinTableAlias = null;

            StringBuilder sqlPredicate = new StringBuilder();
            if(body is BinaryExpression)
            {
                bool lparens = false;
                bool rparens = false;

                BinaryExpression exp = (BinaryExpression)body;

                if(IsLogicalOp(exp.Left) && exp.Left.NodeType != exp.NodeType)
                    lparens = true;
                else
                    lparens = false;

                if(IsLogicalOp(exp.Right) && exp.Right.NodeType != exp.NodeType)
                    rparens = true;
                else
                    rparens = false;                

                sqlPredicate.AppendFormat("{0}{1}{2} {3} {4}{5}{6}", 
                        lparens ? "(" : "" , ParsePredicate(exp.Left, daMgr), lparens ? ")" : "",
                        GetBinaryOp(exp, exp.Right),
                        rparens ? "(" : "" , ParsePredicate(exp.Right, daMgr), rparens ? ")" : "");
                
            }
            else if(body is MethodCallExpression || 
                    (body is UnaryExpression && body.NodeType == ExpressionType.Convert && 
                        ((UnaryExpression)body).Operand is MethodCallExpression))
            {
                MethodCallExpression exp = (MethodCallExpression) (body is MethodCallExpression ? body :
                         ((UnaryExpression)body).Operand);

                DbPredicateString predicatePart = (DbPredicateString)exp.Method.Invoke(_joinMgr, 
                        exp.Arguments.Select(arg => Expression.Lambda(arg).Compile().DynamicInvoke()).ToArray());

                if(predicatePart is DbPredicateStringParameter)
                {
                    DbPredicateStringParameter paramPart = (DbPredicateStringParameter)predicatePart;
                    _paramCount++;

                    if(!Parameters.ContainsKey(paramPart.Parameter.ParameterName))
                        Parameters.Add(paramPart.Parameter.ParameterName, paramPart.Parameter);

                    return daMgr.BuildBindVariableName(paramPart.Parameter.ParameterName);
                }
                else if(predicatePart is DbPredicateInClause)
                {
                    DbPredicateInClause inClause = (DbPredicateInClause)predicatePart;

                    return BuildInClause(inClause, daMgr);
                }
                else if(predicatePart is DbPredicateBetweenClause)
                {
                    DbPredicateBetweenClause betweenClause = (DbPredicateBetweenClause)predicatePart;

                    return BuildBetweenClause(betweenClause, daMgr);
                }
                else
                    return predicatePart;
            }
            else if(body is UnaryExpression && body.NodeType == ExpressionType.Convert)
            {
                UnaryExpression exp = (UnaryExpression)body;
                
                if(exp.Operand is MemberExpression)
                {
                    object member = Expression.Lambda(exp.Operand).Compile().DynamicInvoke();

                    return _joinMgr.Value(member);
                }
                else if (exp.Operand is ConstantExpression)
                    return _joinMgr.Value(((ConstantExpression)exp.Operand).Value);
                else
                    throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue, 
                            "Unkown expression in DbPredicate: " + body.ToString());
            }
            else if(body is MemberExpression)
            {
                MemberExpression exp = (MemberExpression)body;

                object member = Expression.Lambda(exp).Compile().DynamicInvoke();

                if(member is DbParameter)
                {
                    DbParameter param = (DbParameter)member;
                     if(!Parameters.ContainsKey(param.ParameterName))
                        Parameters.Add(param.ParameterName, 
                                new DbPredicateParameter() 
                                { ParameterName = param.ParameterName, Parameter = param });
                    return daMgr.BuildBindVariableName(((DbParameter)member).ParameterName);
                }
                else
                    throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue, 
                        "Unkown expression in DbPredicate: " + body.ToString());
            }
            else if(body is ConstantExpression )
            {
                if(((ConstantExpression)body).Value == null)
                    return "NULL";
                else
                    return _joinMgr.Value(((ConstantExpression)body).Value);
            }
            else
            {
                throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue, 
                        "Unkown expression in DbPredicate: " + body.ToString());
            }
            
            return sqlPredicate.ToString();
        }

        private bool IsBooleanOp(Expression exp)
        {
            switch(exp.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsLogicalOp(Expression exp)
        {
            switch(exp.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    return true;
                default:
                    return false;
            }
        }

        public static string GetBinaryOp(Expression exp, Expression rightExp)
        {
            bool rightExpressionIsNull = false;

            ConstantExpression rightValue = rightExp as ConstantExpression;

            if(rightValue != null && rightValue.Value == null)
                    rightExpressionIsNull = true;
           
            switch(exp.NodeType)
            {
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Equal:
                    return rightExpressionIsNull ? "is" : "=";
                case ExpressionType.NotEqual:
                    return rightExpressionIsNull ? "is not" : "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                default:
                    throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue, 
                            "Unknown operator in DbPredicate.");
            }   
        }

        private string BuildInClause(DbPredicateInClause inClause, DataAccessMgr daMgr)
        {
            StringBuilder inClauseInterior = new StringBuilder();

            for(int n = 1; n <= inClause.NumParameters; n++)
            {
                string paramName = string.Format("{0}{1}", inClause.ParameterNamePrefix, n);
                inClauseInterior.AppendFormat("{0}{1}", inClauseInterior.Length == 0 ? "" : " ,", 
                        daMgr.BuildBindVariableName(paramName));

                if(!Parameters.ContainsKey(paramName))
                    Parameters.Add(paramName, 
                            new DbPredicateParameter()
                            {
                                SchemaName = inClause.SchemaName,
                                TableName = inClause.TableName,
                                ColumnName = inClause.ColumnName,
                                ParameterName = paramName
                            });
            }
            
            return string.Format("{0}.{1} IN ({2})", inClause.TableAlias, inClause.ColumnName, inClauseInterior);
        }

        private string BuildBetweenClause(DbPredicateBetweenClause between, DataAccessMgr daMgr)
        {
            foreach(string paramName in new string[] { between.LowParameterName, between.HighParameterName })
            {
                if(!Parameters.ContainsKey(paramName))
                        Parameters.Add(paramName, 
                                new DbPredicateParameter()
                                {
                                    SchemaName = between.SchemaName,
                                    TableName = between.TableName,
                                    ColumnName = between.ColumnName,
                                    ParameterName = paramName
                                });
            }

            return string.Format("{0}.{1} BETWEEN {2} AND {3}", between.TableAlias, between.ColumnName,
                    daMgr.BuildBindVariableName(between.LowParameterName), 
                    daMgr.BuildBindVariableName(between.HighParameterName));
        }

        public static Expression CreatePredicatePart(Expression<Func<DbTableDmlMgr, bool>> exp)
        {
            return exp.Body;
        }

        public static Expression CreatePredicatePart(string schemaName, string tableName, string columnName, 
                DbParameter param, ComparisonOperatorEnum comparisonOperator)
        {
            string finalSchemaName = schemaName;
            string finalTableName = tableName;
            string finalColumnName = columnName;
            DbParameter finalParameter = param;
            ComparisonOperatorEnum finalComparisonOperator = comparisonOperator;

            // The equal operator will not be used, This lambda is just a convenient way to get the left and right and side
            // of the expression.
            Expression<Func<DbTableDmlMgr, bool>> exp = (t) => 
                    t.Column(finalSchemaName, finalTableName, finalColumnName) == finalParameter;

            return GetComparisonExpression(((BinaryExpression)exp.Body).Left, ((BinaryExpression)exp.Body).Right, 
                    finalComparisonOperator);
        }

        public static Expression CreatePredicatePart(string tableName, string columnName, DbParameter param,
                ComparisonOperatorEnum comparisonOperator)
        {
            string finalTableName = tableName;
            string finalColumnName = columnName;
            DbParameter finalParameter = param;
            ComparisonOperatorEnum finalComparisonOperator = comparisonOperator;

            // The equal operator will not be used, This lambda is just a convenient way to get the left and right and side
            // of the expression.
            Expression<Func<DbTableDmlMgr, bool>> exp = (t) => t.Column(finalTableName, finalColumnName) == finalParameter;

            return GetComparisonExpression(((BinaryExpression)exp.Body).Left, ((BinaryExpression)exp.Body).Right, 
                    finalComparisonOperator);
        }

        public static Expression CreatePredicatePart(string tableName, string columnName, string parameterName,
                ComparisonOperatorEnum comparisonOperator)
        {
            // The equal operator will not be used, This lambda is just a convenient way to get the left and right and side
            // of the expression.
            Expression<Func<DbTableDmlMgr, bool>> exp = ( t ) => t.Column( tableName, columnName ) ==
                    t.Parameter( tableName, columnName, parameterName );

            return GetComparisonExpression( ((BinaryExpression)exp.Body).Left, ((BinaryExpression)exp.Body).Right, 
                    comparisonOperator);
        }

        public static Expression GetComparisonExpression(Expression left, Expression right, 
                ComparisonOperatorEnum comparisonOperator)
        {
            switch(comparisonOperator)
            {
                case ComparisonOperatorEnum.Greater:
                    return Expression.GreaterThan(left, right);
                case ComparisonOperatorEnum.GreaterEquals:
                    return Expression.GreaterThanOrEqual(left, right);
                case ComparisonOperatorEnum.Less:
                    return Expression.LessThan(left, right);
                case ComparisonOperatorEnum.LessEquals:
                    return Expression.LessThanOrEqual(left, right);
                case ComparisonOperatorEnum.Equals:
                    return Expression.Equal(left, right);
                case ComparisonOperatorEnum.NotEquals:
                    return Expression.NotEqual(left, right);
                default:
                    throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue,
                            "Unsupported comparison operator: " + comparisonOperator.ToString());
            }
        }

    }

    /// <summary>
    /// Class for constructing case statements
    /// </summary>
    public class DbCase
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly List<DbCaseWhen> Whens = new List<DbCaseWhen>();
        /// <summary>
        /// 
        /// </summary>
        public string ElseStatement { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DbCase()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="dbCase"></param>
        public DbCase(DbCase dbCase)
        {
            Whens = dbCase.Whens.Select(w => new DbCaseWhen(w.WhenPredicate, w.ThenStatement)).ToList();
            ElseStatement = dbCase.ElseStatement;
            Alias = dbCase.Alias;
        }

        /// <summary>
        /// Generates the SQL When statement
        /// </summary>
        /// <param name="daMgr">DataAccessMgr instance</param>
        /// <returns></returns>
        public string ToString(DataAccessMgr daMgr)
        {
            StringBuilder sbCase = new StringBuilder("case" + Environment.NewLine);

            foreach(DbCaseWhen when in Whens)
            {
                sbCase.AppendFormat("    when {0} then {1}{2}", when.WhenPredicate.ToString(daMgr), when.ThenStatement, 
                        Environment.NewLine);
            }

            if(!String.IsNullOrWhiteSpace(ElseStatement))
                sbCase.AppendFormat("else {0}{1}", ElseStatement, Environment.NewLine);

            sbCase.Append("end");

            return sbCase.ToString();
        }

    }

    /// <summary>
    /// Class for constructing the inline view sql of a column in a select statement
    /// </summary>
    public class InlineViewColumn
    {
        public string InlineView;
        public string Alias;
        public DbParameterCollection dbParams;
    }

    /// <summary>
    /// Class for constructing WHEN portion of case statment
    /// </summary>
    public class DbCaseWhen
    {
        /// <summary>
        /// 
        /// </summary>
        public DbPredicate WhenPredicate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ThenStatement { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whenPredicate"></param>
        /// <param name="thenStatement"></param>
        public DbCaseWhen(DbPredicate whenPredicate, string thenStatement)
        {
            WhenPredicate = whenPredicate;
            ThenStatement = thenStatement;
        }
    }

    /// <summary>
    /// Class used for representing hard "hard coded" const value
    /// </summary>
    public class DbConstValue
    {
        public readonly object Value;

        /// <summary>
        /// Create a DbConstValue instance
        /// </summary>
        /// <param name="value">Constant value</param>
        public DbConstValue(object value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the correctly quoted value for SQL, i.e., strings are quoted, ints are not
        /// </summary>
        /// <returns>Correctly quoted value.</returns>
        public string GetQuotedValue()
        {
            return DbConstValue.GetQuotedValue(Value);
        }

        /// <summary>
        /// Gets the correctly quoted value for SQL, i.e., strings are quoted, ints are not
        /// </summary>
        /// <param name="value">Value to quote</param>
        /// <returns>Correctly quoted value.</returns>
        public static string GetQuotedValue(object value)
        {
            if(value is string || value is DateTime || value is char)
                return string.Format("{0}{1}{0}", "'", value);
            else
                return value.ToString();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class DbFunction
    {
        public readonly string Value;

        /// <summary>
        /// Create a DbFunction instance
        /// </summary>
        /// <param name="value">Function</param>
        public DbFunction(string value)
        {
            Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }

#pragma warning restore 1591 // disable the xmlComments warning    
}
