using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

using B1.Core;
using B1.CacheManagement;

#pragma warning disable 1591 // disable the xmlComments warning   
namespace B1.DataAccess
{
    internal class LinqTableMgr
    {
        //?? private ContextCache _contextCache;
        private string _defaultSchema;

        internal TypeVisitor _knownTypes = new TypeVisitor();

        internal List<DbPredicateParameter> _parameters = new List<DbPredicateParameter>();

        internal List<string> _aliasStack = new List<string>();

        /// <summary>
        ///  Types to member name to table alias. Most useful for anonymous types that contain members that are tables/entities
        /// </summary>
        internal Dictionary<Type, Dictionary<string, string>> _typeToAlias = new Dictionary<Type,Dictionary<string, string>>();

        /// <summary>
        /// Schema to table alias to table name
        /// </summary>
        internal Dictionary<string, Dictionary<string, string>> _schemaTables =
                new Dictionary<string, Dictionary<string, string>>();

        internal LinqTableMgr(ContextVisitor contextCache, string defaultSchema = null)
        {
            this.ContextCache = contextCache;
            _defaultSchema = defaultSchema;

            if(_defaultSchema != null)
                _schemaTables.Add(_defaultSchema, new Dictionary<string, string>());
        }

        public ContextVisitor ContextCache { get; private set; }

        public int TableCount
        {
            get 
            {
                return _schemaTables.Aggregate(0, (count, kvp) => count + kvp.Value.Count);
            }
        }

        public string GetTableName(Type type)
        {
            return this.ContextCache.GetTableName(type);
            //Type entityType = ObjectContext.GetObjectType(type);

            //if (_contextVisitor.DefaultEntityContainer != null && entityType != null)
            //{
            //    EntitySetBase entitySet = _contextVisitor.DefaultEntityContainer.BaseEntitySets.First(es => es.ElementType.Name == entityType.Name);
            //    QualifiedEntity entity = StorageMetaData.GetQualifiedEntity(entitySet.ElementType.FullName);
            //    return entity.EntityName;
            //}
            //else
            //    return entityType.Name;

        }

        public string GetSchemaQualifiedTableName(Type type)
        {
            return this.ContextCache.GetSchemaQualifiedTableName(type);
            //Type entityType = ObjectContext.GetObjectType(type);

            //string schemaName = _defaultSchema;
            //string tableName = entityType.Name;

            //if (_contextVisitor.DefaultEntityContainer != null && entityType != null)
            //{
            //    EntitySetBase entitySet = _contextVisitor.DefaultEntityContainer.BaseEntitySets.FirstOrDefault(
            //            es => es.ElementType.Name == entityType.Name);
                
            //    if(entitySet != null)
            //    {
            //        QualifiedEntity entity = StorageMetaData.GetQualifiedEntity(entitySet.ElementType.FullName);
            //        if (entity != null)
            //        {
            //            schemaName = entity.SchemaName;
            //            tableName = entity.EntityName;
            //        }
            //        else
            //        {
            //            tableName = entitySet.Name;
            //        }
            //    }
            //}
            
            //return string.Format("{0}.{1}", schemaName, tableName);
        }

        public QualifiedEntity GetQualifiedEntity(Type type)
        {
            return this.ContextCache.GetQualifiedEntity(type);
            //Type entityType = ObjectContext.GetObjectType(type);

            //EntitySetBase entitySet = _contextVisitor.DefaultEntityContainer.BaseEntitySets.FirstOrDefault(
            //            es => es.ElementType.Name == entityType.Name);

            //return StorageMetaData.GetQualifiedEntity(entitySet.ElementType.FullName);
        }

        public QualifiedEntity GetQualifiedEntityFromAlias(string alias)
        {
            string schemaName = null;
            string tableName = null;

            foreach(var kvp in _schemaTables)
            {
                if(kvp.Value.ContainsKey(alias))
                {
                    tableName = kvp.Value[alias];
                    schemaName = kvp.Key;
                    break;
                }
            }

            return EntityContextCache.GetQualifiedEntity(schemaName, tableName);
        }

        public string GetSchema(Type type)
        {
            return this.ContextCache.GetSchemaName(type);
            //Type entityType = ObjectContext.GetObjectType(type);

            //string schemaName = _defaultSchema;

            //if (_contextVisitor.DefaultEntityContainer != null && entityType != null)
            //{
            //    EntitySetBase entitySet = _contextVisitor.DefaultEntityContainer.BaseEntitySets.FirstOrDefault( 
            //            es => es.ElementType.Name == entityType.Name);

            //    if(entitySet != null)
            //    {
            //        QualifiedEntity entity = StorageMetaData.GetQualifiedEntity(entitySet.ElementType.FullName);
            //        if(entity != null)
            //            return entity.SchemaName;
            //    }
            //}

            //return schemaName;
        }

        public string Add(Type type)
        {
            return Add(GetSchema(type), GetTableName(type));
        }

        public string Add(string schemaName, Type type)
        {
            return Add(schemaName, GetTableName(type));
        }

        public string Add(string tableName)
        {
            return Add(_defaultSchema, tableName);
        }

        public string Add(string schemaName, string tableName)
        {
            if(!_schemaTables.ContainsKey(schemaName))
                _schemaTables.Add(schemaName, new Dictionary<string, string>());

            string nextAlias = GetNextTableAlias();

            _schemaTables[schemaName].Add(nextAlias, tableName);

            _aliasStack.Add(nextAlias);

            return nextAlias;
        }

        public void AddTypedAlias(Type containingType, string methodName, string alias)
        {
            if(!_typeToAlias.ContainsKey(containingType))
                _typeToAlias.Add(containingType, new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase));

            if(!_typeToAlias[containingType].ContainsKey(methodName))
                _typeToAlias[containingType].Add(methodName, alias);
        }


        internal Dictionary<string, string> GetColumnDictionary(Type type, string alias)
        {
            QualifiedEntity entity = this.ContextCache.GetQualifiedEntity(type);

            Dictionary<string, string> columns = new Dictionary<string,string>();

            foreach(PropertyInfo p in  type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public 
                    | BindingFlags.Instance))
            {
                string columnName = entity.GetColumnName(p.Name);

                if(columnName != null)
                    columns.Add(columnName, string.Format("{0}.{1}", alias, columnName));
            }

            return columns;
        }

        internal string GetColumnList(Type type, string alias)
        {
            QualifiedEntity entity = this.ContextCache.GetQualifiedEntity(type);

            StringBuilder sb = new StringBuilder();

            foreach(PropertyInfo p in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public 
                    | BindingFlags.Instance))
            {
                string columnName = entity.GetColumnName(p.Name);

                if(columnName != null)
                    sb.AppendFormat("{0}{1}.{2}", sb.Length == 0 ? "" : ", ",
                        alias, columnName);
            }

            return sb.ToString();
        }

        private string GetNextTableAlias()
        {
            return string.Format("T{0}", TableCount);
        }

        internal static string AdjustParamNameLength(string paramName, List<DbPredicateParameter> parameters, DataAccessMgr daMgr)
        {
            if(paramName.Length <= Constants.ParamNameOracleMaxLength || 
                    daMgr.DatabaseType == DataAccessMgr.EnumDbType.SqlServer)
                return paramName;
            else
            {
                int count = 1;
                string newParamName;
                string suffix;
                do
                {
                    suffix = count.ToString();
                    newParamName = paramName + suffix;
                    if (newParamName.Length > Constants.ParamNameOracleMaxLength)
                        newParamName = paramName.Substring(0, Constants.ParamNameOracleMaxLength - suffix.Length)
                                + suffix;
                }
                while (parameters.Count( p => p.ParameterName.ToLower() == newParamName.ToLower()) > 0
                        && ++count > 0);

                return newParamName;
            }
        }

        internal static string BuildParamName(string paramName
                , List<DbPredicateParameter> parameters
                , DataAccessMgr daMgr)
        {
            return daMgr.BuildParamName(AdjustParamNameLength(paramName, parameters, daMgr), false);
        }

        internal static string BuildParamName(string paramName
                , List<DbPredicateParameter> parameters
                , DataAccessMgr daMgr
                , bool isNewValueParam)
        {
            return daMgr.BuildParamName(AdjustParamNameLength(paramName, parameters, daMgr), isNewValueParam);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class LinqQueryParser : ExpressionVisitor
    {
        internal WhereParser _whereParser;
        internal JoinParser _joinParser;
        internal SelectParser _selectParser;
        internal SelectParser _groupByParser;
        internal SelectParser _orderByParser;
        internal SelectParser _distinctParser;

        private bool _orderByDecending = false;
        
        private MethodCallExpression _whereExpression;
        private MethodCallExpression _outerMostExpression;

        private DataAccessMgr _daMgr;

        private LinqTableMgr _tableMgr;

        public List<DbPredicateParameter> Parameters { get { return _tableMgr._parameters; } }

        public LinqQueryParser(IQueryable queryable, DataAccessMgr daMgr, string defaultSchemaName = null)
        {
            InitTableMgr(queryable, defaultSchemaName);
            Parse(queryable.Expression as MethodCallExpression, daMgr);
        }
       
        internal LinqQueryParser(MethodCallExpression expression, LinqTableMgr tableMgr, DataAccessMgr daMgr, 
                string defaultSchemaName = null)
        {
            _tableMgr = tableMgr;
            Parse(expression, daMgr);
        }

        private void InitTableMgr(IQueryable queryable, string defaultSchemaName = null)
        {
            ContextVisitor visitor = new ContextVisitor(queryable);
            _tableMgr = new LinqTableMgr(visitor, defaultSchemaName);
            ////?? TypeVisitor visitor = new TypeVisitor(queryable.Expression);

            //ObjectContext entityContext = null;
            //EntityContainer cspaceEntityContainer = null;

            //if(!(queryable.Expression is MethodCallExpression))
            //    throw new Exception("Invalid linq expression.");

            //if(queryable.GetType().IsGenericType && (queryable.GetType().GetGenericTypeDefinition() == typeof(ObjectQuery<>)
            //        || queryable.GetType().GetGenericTypeDefinition() == typeof(ObjectSet<>)))
            //{

            //    entityContext = ((ObjectQuery)queryable).Context;

            //    cspaceEntityContainer = entityContext.MetadataWorkspace.GetEntityContainer(
            //            entityContext.DefaultContainerName, DataSpace.CSpace);

            //    //?? Loading all storage meta data from the assembly of this context.
            //    //?? We have to load the storage meta data for every context we encounter
            //    StorageMetaData.EnsureStorageMetaData(entityContext/*, cspaceEntityContainer*/);
            //}

            //_tableMgr = new LinqTableMgr(entityContext, cspaceEntityContainer, defaultSchemaName);
        }

        private void Parse(MethodCallExpression expression, DataAccessMgr daMgr)
        {
            if(expression == null)
                throw new Exception("Invalid linq expression.");

            _daMgr = daMgr;
            _outerMostExpression = expression;

            Visit(expression);
        }

        internal string GetOuterSelect()
        {
            if(_selectParser != null)
                return "SELECT " + _selectParser.GetSelect();
            else if(_distinctParser != null && _distinctParser._parser != null)
                return "SELECT DISTINCT " + _distinctParser.GetSelect();
            else if(_whereParser != null && _whereParser._parser != null)
                return _whereParser._parser.GetOuterSelect();
            else if(_groupByParser != null && _groupByParser._parser != null)
                return _groupByParser._parser.GetOuterSelect();
            else if(_orderByParser != null && _orderByParser._parser != null)
                return _orderByParser._parser.GetOuterSelect();
            else if(_joinParser != null && _joinParser._selectParser != null)
                return "SELECT " + _joinParser._selectParser.GetSelect();
            else 
            {
                Type[] genericTypes = _outerMostExpression.Method.ReturnType.GetGenericArguments();

                string alias = "";

                if(_tableMgr._aliasStack.Count() == 0)
                    alias = _tableMgr.Add(genericTypes[0]);
                else
                    alias = _tableMgr._schemaTables.First().Value.First().Key;

                return string.Format("SELECT {0}",
                        _tableMgr.GetColumnList(genericTypes.First(), alias));

                throw new NotImplementedException();
            }
        }

        internal string GetOuterFrom()
        {
            if(_selectParser != null && _selectParser._from.Length > 0)
            {
                return string.Format("{0}", _selectParser._from.ToString());
            }
            else if(_selectParser != null && _selectParser._parser != null)
                return _selectParser._parser.GetOuterFrom();
            else if(_distinctParser != null && _distinctParser._parser != null)
                return _distinctParser._parser.GetOuterFrom();
            else if(_whereParser != null && _whereParser._parser != null)
                return _whereParser._parser.GetOuterFrom();
            else if(_groupByParser != null && _groupByParser._parser != null)
                return _groupByParser._parser.GetOuterFrom();
            else if(_orderByParser != null && _orderByParser._parser != null)
                return _orderByParser._parser.GetOuterFrom();
            else
                return null;
            
        }

        internal string GetJoinFrom()
        {
            if(_joinParser != null)
            {
                return string.Format("{0}", _joinParser._joins.Aggregate(new StringBuilder(),
                        (sb, j) => sb.AppendFormat(" {0} {1}", sb.Length == 0 ? "" : "INNER JOIN", 
                            j.Value.ToString())).ToString());
            }
            else if(_selectParser != null && _selectParser._parser != null)
                return _selectParser._parser.GetJoinFrom();
            else if(_whereParser != null && _whereParser._parser != null)
                return _whereParser._parser.GetJoinFrom();
            else if(_distinctParser != null && _distinctParser._parser != null)
                return _distinctParser._parser.GetJoinFrom();
            else if(_groupByParser != null && _groupByParser._parser != null)
                return _groupByParser._parser.GetJoinFrom();
            else if(_orderByParser != null && _orderByParser._parser != null)
                return _orderByParser._parser.GetJoinFrom();
            else 
                return null;
        }

        internal string GetDefaultFrom()
        {
            string joinFrom = GetJoinFrom();

            if(joinFrom != null)
                return joinFrom;
            else
            {
                var aliasAndTable = _tableMgr._schemaTables.First().Value.First();

                return string.Format("{0}.{1} {2}", _tableMgr._schemaTables.First().Key, aliasAndTable.Value,
                        aliasAndTable.Key);
            }
        }

        internal string GetOuterWhere()
        {
            if(_whereParser != null && _whereParser._sqlPredicate.Length > 0)
                return "WHERE " + _whereParser._sqlPredicate;
            else if(_selectParser != null && _selectParser._parser != null)
                return _selectParser._parser.GetOuterWhere();
            else if(_distinctParser != null && _distinctParser._parser != null)
                return _distinctParser._parser.GetOuterWhere();
            else if(_groupByParser != null && _groupByParser._parser != null)
                return _groupByParser._parser.GetOuterWhere();
            else if(_orderByParser != null && _orderByParser._parser != null)
                return _orderByParser._parser.GetOuterWhere();
            else
                return null;
        }

        internal string GetOuterOrderBy()
        {
            if(_orderByParser != null)
                return "ORDER BY " + _orderByParser.GetSelect(false) + (_orderByDecending ? " DESC" : " ASC");
            else if(_selectParser != null && _selectParser._parser != null)
                return _selectParser._parser.GetOuterOrderBy();
            else if(_distinctParser != null && _distinctParser._parser != null)
                return _distinctParser._parser.GetOuterOrderBy();
            else if(_whereParser != null && _whereParser._parser != null)
                return _whereParser._parser.GetOuterOrderBy();
            else if(_groupByParser != null && _groupByParser._parser != null)
                return _groupByParser._parser.GetOuterOrderBy();
            else
                return null;
        }

        internal string GetOuterGroupBy()
        {
            if(_groupByParser != null)
                return "GROUP BY " + _groupByParser.GetSelect();
            else if(_selectParser != null && _selectParser._parser != null)
                return _selectParser._parser.GetOuterGroupBy();
            else if(_whereParser != null && _whereParser._parser != null)
                return _whereParser._parser.GetOuterGroupBy();
            else if(_distinctParser != null && _distinctParser._parser != null)
                return _distinctParser._parser.GetOuterGroupBy();
            else if(_orderByParser != null && _orderByParser._parser != null)
                return _orderByParser._parser.GetOuterGroupBy();
            else
                return null;
        }

        internal List<Tuple<string, string>> GetOuterGroupBySelectList()
        {
            if(_groupByParser != null)
                return _groupByParser.GetSelectList();
            else if(_selectParser != null && _selectParser._parser != null)
                return _selectParser._parser.GetOuterGroupBySelectList();
            else if(_whereParser != null && _whereParser._parser != null)
                return _whereParser._parser.GetOuterGroupBySelectList();
            else if(_distinctParser != null && _distinctParser._parser != null)
                return _distinctParser._parser.GetOuterGroupBySelectList();
            else if(_orderByParser != null && _orderByParser._parser != null)
                return _orderByParser._parser.GetOuterGroupBySelectList();
            else
                return null;
        }

        internal Dictionary<string, StringBuilder> GetOuterJoin()
        {
            if(_joinParser != null && _joinParser._joins.Count > 0)
                return _joinParser._joins;
            else if(_selectParser != null && _selectParser._parser != null)
                return _selectParser._parser.GetOuterJoin();
            else if(_distinctParser != null && _distinctParser._parser != null)
                return _distinctParser._parser.GetOuterJoin();
            else if(_groupByParser != null && _groupByParser._parser != null)
                return _groupByParser._parser.GetOuterJoin();
            else if(_orderByParser != null && _orderByParser._parser != null)
                return _orderByParser._parser.GetOuterJoin();
            else
                return new Dictionary<string, StringBuilder>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if(expression.Method.Name == "Where" && expression.Method.DeclaringType == typeof(Queryable))
            {
                _whereExpression = expression;
                _whereParser = new WhereParser(expression, _tableMgr, _daMgr);
            }
            else if(expression.Method.Name == "Select" && expression.Method.DeclaringType == typeof(Queryable))
            {
                _selectParser = new SelectParser(expression, _tableMgr, _daMgr);
            }
            else if(expression.Method.Name == "SelectMany" && expression.Method.DeclaringType == typeof(Queryable))
            {
                _selectParser = new SelectParser(expression, _tableMgr, _daMgr);
            }
            else if((expression.Method.Name == "OrderBy" || expression.Method.Name == "OrderByDescending")
                    && expression.Method.DeclaringType == typeof(Queryable))
            {
                _orderByParser = new SelectParser(expression, _tableMgr, _daMgr);
            }
            else if(expression.Method.Name == "GroupBy" && expression.Method.DeclaringType == typeof(Queryable))
            {
                _groupByParser = new SelectParser(expression, _tableMgr, _daMgr);
            }
            else if(expression.Method.Name == "Distinct" && expression.Method.DeclaringType == typeof(Queryable))
            {
                _distinctParser = new SelectParser(expression, _tableMgr, _daMgr);
            }
            else if((expression.Method.Name == "Join" || expression.Method.Name == "GroupJoin") 
                && expression.Method.DeclaringType == typeof(Queryable))
                _joinParser = new JoinParser(expression, _tableMgr, _daMgr);
            

            return expression;
        }

    }

    internal class SelectParser : ExpressionVisitor
    {
        internal StringBuilder _from = new StringBuilder();
        internal StringBuilder _joinPredicate = new StringBuilder();
        internal LinqQueryParser _parser;
        internal Dictionary<string, string> _parameterToAlias = new Dictionary<string, string>();

        private List<Tuple<string, string>> _select = new List<Tuple<string, string>>();
        private DataAccessMgr _daMgr;
        private LinqTableMgr _tableMgr;
        private string _currentAliasName;
        private bool _inCaseStatement = false;
        private bool _inSelect = false;

        private DbTableJoinType _joinType = DbTableJoinType.Inner;
        private StringBuilder _caseStatement = new StringBuilder();

        private Dictionary<Type, List<Tuple<string, string>>> _typeToSelect = 
                new Dictionary<Type, List<Tuple<string, string>>>();

        public bool HasSelect { get { return _select.Count > 0; } }

        /// <summary>
        /// This constuctor is used for getting a select string out of expressions OTHER than
        /// Select or SelectMany
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="tableMgr"></param>
        /// <param name="daMgr"></param>
        public SelectParser(Expression expression, LinqTableMgr tableMgr, DataAccessMgr daMgr)
        {
            _daMgr = daMgr;
            _tableMgr = tableMgr;

            _inSelect = true;
            Visit(expression);
            _inSelect = false;
        }

        public SelectParser(MethodCallExpression expression, LinqTableMgr tableMgr, DataAccessMgr daMgr)
        {
            _daMgr = daMgr;
            _tableMgr = tableMgr;

            _tableMgr._knownTypes.Visit(expression.Arguments[0]);

            if(expression.Arguments[0] is MethodCallExpression)
            {
                MethodCallExpression method = (MethodCallExpression)expression.Arguments[0];
                _parser = new LinqQueryParser(method, tableMgr, daMgr);
            }

            if(expression.Method.Name == "SelectMany")
            {
                string internalFrom = _parser.GetOuterFrom();

                if(!string.IsNullOrEmpty(internalFrom))
                    _from.Append(internalFrom);
                else
                {
                    if(tableMgr._aliasStack.Count == 0)
                    {
                        tableMgr.Add(GetTableType(expression.Arguments[0]));
                    }

                    _from.Append(GetQualifiedTableString(expression.Arguments[0], tableMgr._aliasStack.First()));
                }

                _tableMgr._knownTypes.Visit(expression.Arguments[1]);

                Type joinedTableType = GetTableType(expression.Arguments[1]);
                string joinedTableAlias = tableMgr.Add(joinedTableType);
                string joinedTable = GetQualifiedTableString(expression.Arguments[1], joinedTableAlias);

                _inSelect = true;
                Visit(expression.Arguments[2]);
                _inSelect = false;

                // Needed in case of where
                ParseSecondTable(expression.Arguments[1]);

                if(IsDefaultIfEmptyMethod(expression.Arguments[0]))
                    _joinType = DbTableJoinType.RightOuter;
                else if(IsDefaultIfEmptyMethod(expression.Arguments[1]))
                    _joinType = DbTableJoinType.LeftOuter;
                else _joinType = DbTableJoinType.Inner;

                Dictionary<string, StringBuilder> joins = _parser.GetOuterJoin();

                _from.Append(Environment.NewLine);

                MemberVisitor memberVisitor = new MemberVisitor(expression.Arguments[1]);
                string memberName = memberVisitor._lastMember;
                Type memberType = memberVisitor._lastDeclaringType;

                if(joins.Count() > 0 && _tableMgr._typeToAlias.ContainsKey(memberType))
                {
                    _from.AppendFormat(" {0} {1}", DbTableJoin.GetJoinStringFromType(_joinType),
                            joins[_tableMgr._typeToAlias[memberType][memberName]].ToString());
                }
                else if(_joinPredicate.Length == 0)
                {
                    _from.AppendFormat(" CROSS JOIN {0}", joinedTable);
                }
                else
                {
                    _from.AppendFormat(" {0} {1} ON {2}", DbTableJoin.GetJoinStringFromType(_joinType),
                        joinedTable, _joinPredicate);
                }
            }
            else
            {
                _inSelect = true;

                Visit(expression.Arguments.Count == 1 ? expression.Arguments[0] : expression.Arguments[1]);
                _inSelect = false;

                if(expression.Method.Name == "GroupBy")
                    Visit(expression.Arguments[2]);
            }

        }

        internal string GetSelect(bool withAlias = true)
        {
            return _select.Aggregate(new StringBuilder(), (sb, s) => sb.AppendFormat("{0}{1}{2}", sb.Length > 0 ? ", " : "",
                    s.Item2, withAlias && s.Item1 != null ? string.Format(" AS {0}",s.Item1) : "")).ToString();
        }

        internal List<Tuple<string, string>> GetSelectList()
        {
            return new List<Tuple<string, string>>(_select);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            int memberIndex = 0;
            foreach(var arg in node.Arguments)
            {
                _currentAliasName = node.Members[memberIndex].Name;

                if(arg is ParameterExpression)
                {
                    ParameterExpression param = (ParameterExpression)arg;
                    string alias = _parameterToAlias[param.Name];

                    Type type = ObjectContext.GetObjectType(TypeVisitor.GetNonGenericTypes(param.Type)[0]);
                    if(type != null)
                    {
                        foreach(var kvp in _tableMgr.GetColumnDictionary(type, alias))
                        {
                            _select.Add(new Tuple<string, string>(null, kvp.Value));
                        }
                    }

                    _tableMgr.AddTypedAlias(node.Type, param.Name, alias);      
                }
                else
                    Visit(arg);

                _currentAliasName = null;
                memberIndex++;
            }

            _typeToSelect.Add(node.Type, _select);


            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if(_inSelect)
            {
                Type type = null;

                if(node.Member.DeclaringType.IsGenericType &&
                        node.Member.DeclaringType.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                {
                    foreach(var col in _parser.GetOuterGroupBySelectList())
                    {
                        _select.Add(new Tuple<string,string>(string.IsNullOrWhiteSpace(_currentAliasName) ? null :
                                _currentAliasName, col.Item2));
                    }
                }
                else
                {
                    type = node.Member.DeclaringType;

                    string alias = "";

                    if(node.Expression is MemberExpression &&
                            _tableMgr._typeToAlias.ContainsKey(((MemberExpression)node.Expression).Member.DeclaringType))
                    {
     
                        MemberInfo member = ((MemberExpression)node.Expression).Member;
                        alias = _tableMgr._typeToAlias[member.DeclaringType][member.Name];
                        QualifiedEntity entity = _tableMgr.GetQualifiedEntityFromAlias(alias);

                        _select.Add(new Tuple<string, string>(string.IsNullOrEmpty(_currentAliasName) ? 
                                null : _currentAliasName, string.Format("{0}.{1}",
                                alias,
                                entity.GetColumnName(node.Member.Name))));
                    }
                    else
                    {
                        if(_tableMgr._aliasStack.Count() == 0)
                            alias = _tableMgr.Add(type);
                        else
                        {
                            ParameterExpression param = TypeVisitor.GetFirstParent<ParameterExpression>(node);
                            if(param != null)
                            {
                                alias = _parameterToAlias[param.Name];
                            }
                            else
                            {
                                // TODO: better error.
                                throw new Exception("Invalid query expression");
                            }
                        }

                        QualifiedEntity entity = _tableMgr.GetQualifiedEntityFromAlias(alias);

                        _select.Add(new Tuple<string, string>(string.IsNullOrEmpty(_currentAliasName) ? 
                                null : _currentAliasName, string.Format("{0}.{1}", alias, 
                                entity.GetColumnName(node.Member.Name))));
                    }
                }
            }

            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            for(int p = 0; p < node.Parameters.Count; p++)
            {
                if(!_parameterToAlias.ContainsKey(node.Parameters[p].Name))
                {
                    if(_tableMgr._aliasStack.Count == 0)
                        _tableMgr.Add(node.Parameters[p].Type);

                    _parameterToAlias.Add(node.Parameters[p].Name,
                            _tableMgr._aliasStack.ElementAt(p));
                }
            }

            if(_inSelect && node.Body is ParameterExpression)
            {
                ParameterExpression param = node.Body as ParameterExpression;
                Type type = TypeVisitor.GetNonGenericTypes(param.Type)[0];

                string alias = "";

                if(_tableMgr._aliasStack.Count() == 0)
                    alias = _tableMgr.Add(type);
                else if(_parameterToAlias.ContainsKey(param.Name))
                    alias = _parameterToAlias[param.Name];
                else
                    alias = _tableMgr._aliasStack.Last();

                foreach(var kvp in _tableMgr.GetColumnDictionary(type, alias))
                {
                    _select.Add(new Tuple<string, string>(null, kvp.Value));
                }
            }
            else
                base.VisitLambda(node);

            return node;
        }


        protected override Expression VisitConditional(ConditionalExpression node)
        {
            if(!_inCaseStatement)
            {
                _inCaseStatement = true;
                _caseStatement.AppendFormat("{0}{1}CASE", _caseStatement.Length == 0 ? "" : ", ", Environment.NewLine);
            }

            WhereParser whenParser = new WhereParser(node.Test, _tableMgr, _daMgr, _parameterToAlias);
            WhereParser thenParser = new WhereParser(node.IfTrue, _tableMgr, _daMgr, _parameterToAlias);

            _caseStatement.AppendFormat("{0}    WHEN {1} THEN {2}", Environment.NewLine, 
                    whenParser._sqlPredicate.ToString(), thenParser._sqlPredicate.ToString());

            if(!(node.IfFalse is ConditionalExpression))
            {
                WhereParser elseParser = new WhereParser(node.IfFalse, _tableMgr, _daMgr, _parameterToAlias);
                _caseStatement.AppendFormat("{0}    ELSE {1}{0}END AS {2} {0}", Environment.NewLine, 
                        elseParser._sqlPredicate.ToString(), _currentAliasName);
                _inCaseStatement = false;
                _select.Add(new Tuple<string, string>(null, _caseStatement.ToString()));
                _caseStatement.Length = 0;
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if(node.Method.DeclaringType == typeof(SqlFunctions))
            {
                _select.Add(new Tuple<string, string>(_currentAliasName, string.Format("{0}", GenerateFunctionSql(node, 
                        _daMgr))));
            }
            else if(node.Method.Name == "Where")
            {
                WhereParser whereParser = new WhereParser(node, _tableMgr, _daMgr, _parameterToAlias);
                _joinPredicate = whereParser._sqlPredicate;
            }
            else
            {
                Visit(node.Object);

                foreach (Expression arg in node.Arguments)
                    Visit(arg);
            }

            return node;
        }

        private void ParseSecondTable(Expression expression)
        {

            if(expression is MethodCallExpression)
            {
                Visit(expression);
            }
            else if(expression is UnaryExpression)
            {
                UnaryExpression unary = (UnaryExpression)expression;

                if(unary.Operand is LambdaExpression)
                {
                    LambdaExpression lambda = (LambdaExpression)unary.Operand;

                    if(lambda.Body is MethodCallExpression)
                        Visit(lambda.Body);
                }
            }
            
        }

        internal string GetQualifiedTableString(Expression expression, string alias)
        {
            Type type = GetTableType(expression);

            return string.Format("{0}.{1} {2}", _tableMgr.GetSchema(type), _tableMgr.GetTableName(type),
                      alias);
        }

        internal Type GetTableType(Expression expression)
        {
            Type type = null;

            if(expression is MethodCallExpression)
            {
                MethodCallExpression method = (MethodCallExpression)expression;

                List<Type> nonGenericTypes = TypeVisitor.GetNonGenericTypes(method.Method.ReturnType);

                type = nonGenericTypes[0];
                
            }
            else if(expression is UnaryExpression)
            {
                UnaryExpression unary = (UnaryExpression)expression;

                if(unary.Operand is LambdaExpression)
                {
                    LambdaExpression lambda = (LambdaExpression)unary.Operand;
                    if(lambda.ReturnType.IsGenericType)
                        type = lambda.ReturnType.GetGenericArguments()[0];
                    else
                        type = lambda.ReturnType;
                }
            }
            else
                type = expression.Type;

            return type;
        }

        private bool IsDefaultIfEmptyMethod(Expression expression)
        {
            MethodCallExpression method = expression as MethodCallExpression;
            if(method == null && expression is UnaryExpression)
            {
                UnaryExpression unary = (UnaryExpression)expression;
                if(unary.Operand is LambdaExpression)
                {
                    LambdaExpression lambda = (LambdaExpression)unary.Operand;
                    if(lambda.Body is MethodCallExpression)
                        method = (MethodCallExpression)lambda.Body;
                }
            }

            if(method != null && method.Method.Name == "DefaultIfEmpty")
                return true;
            else
                return false;
        }

        internal static string GenerateFunctionSql(MethodCallExpression node, DataAccessMgr daMgr)
        {
            switch(node.Method.Name.ToLower())
            {   
                //case "dateadd":
                //    daMgr.FormatDateMathSql(EnumDateDiffInterval.
                //    break;
                default:
                    throw new Exception("Unsupported DB Function: " + node.Method.Name);
            }

        }
    }

    internal class JoinParser : ExpressionVisitor 
    {
        private DataAccessMgr _daMgr;
        private LinqTableMgr _tableMgr;       
        private StringBuilder _join = null;
        
        private string _currentTableAlias = "";
        private string _leftAlias;
        private string _rightAlias;

        internal Dictionary<string, string> _parameterToAlias = new Dictionary<string, string>();
        internal Dictionary<string, StringBuilder> _joins = new Dictionary<string, StringBuilder>(); 
        internal SelectParser _selectParser = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="tableMgr"></param>
        /// <param name="daMgr"></param>
        public JoinParser(MethodCallExpression expression, LinqTableMgr tableMgr, DataAccessMgr daMgr)
        {
            _daMgr = daMgr;
            _tableMgr = tableMgr;

            VisitJoin(expression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if((expression.Method.Name == "Join" || expression.Method.Name == "GroupJoin") 
                    && expression.Method.DeclaringType == typeof(Queryable))
            {
                VisitJoin(expression);
                
            }
            else
            {
                Type tableType = TypeVisitor.GetNonGenericTypes(expression.Method.ReturnType)[0];

                _join = new StringBuilder();

                _currentTableAlias = _tableMgr.Add(tableType);
                _joins.Add(_currentTableAlias, _join);
                
                _join.AppendFormat("{0}{1}.{2} {3}", Environment.NewLine,
                        _tableMgr.GetSchema(tableType), _tableMgr.GetTableName(tableType), _currentTableAlias);
            }

            return expression;
        }

        private void VisitJoin(MethodCallExpression expression)
        {
            Expression outerTable = expression.Arguments[0];
            Expression innerTable = expression.Arguments[1];
            Expression leftCond = expression.Arguments[2];
            Expression rightCond = expression.Arguments[3];
            StringBuilder prevJoin = _join;

            _join = new StringBuilder();

            Visit(outerTable);
            _leftAlias = _leftAlias = _currentTableAlias;

            Visit(innerTable);
            _rightAlias = _currentTableAlias;

            Visit(expression.Arguments[4]);

            OnParser leftOn = new OnParser(leftCond, _tableMgr);
            if(!string.IsNullOrEmpty(leftOn._alias))
                _leftAlias = leftOn._alias;

            OnParser rightOn = new OnParser(rightCond, _tableMgr);

            _selectParser = new SelectParser(expression.Arguments[4], _tableMgr, _daMgr);

            int p = 0;
            _join.AppendFormat(" ON");

            QualifiedEntity leftEntity = _tableMgr.GetQualifiedEntityFromAlias(_leftAlias);
            QualifiedEntity rightEntity = _tableMgr.GetQualifiedEntityFromAlias(_rightAlias);
            foreach(var prop in leftOn._properties)
            {
                _join.AppendFormat(" {0}{1}.{2} = {3}.{4}",
                    p > 0 ? "AND " : " ",
                        _leftAlias, leftEntity.GetColumnName(prop),
                        _rightAlias, rightEntity.GetColumnName(rightOn._properties[p]));
                p++;
            }

            _join = prevJoin;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            for(int p = 0; p < node.Parameters.Count; p++)
            {
                if(!_parameterToAlias.ContainsKey(node.Parameters[p].Name))
                {
                    if(_tableMgr._aliasStack.Count == 0)
                        _tableMgr.Add(node.Parameters[p].Type);

                    _parameterToAlias.Add(node.Parameters[p].Name,
                            _tableMgr._aliasStack.ElementAt(_tableMgr._aliasStack.Count - node.Parameters.Count + p));
                }
            }

            return base.VisitLambda<T>(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            string alias = _leftAlias;
            foreach(Expression arg in node.Arguments)
            {
                if(arg is MemberExpression)
                {
                    MemberExpression member = (MemberExpression)arg;
                    _tableMgr.AddTypedAlias(node.Type, member.Member.Name, alias);
                }
                else if (arg is ParameterExpression)
                {
                    ParameterExpression param = (ParameterExpression)arg;
                    _tableMgr.AddTypedAlias(node.Type, param.Name, alias);
                }
                
                alias = _rightAlias;
            }

            return base.VisitNew(node);
        }
    }

    internal class OnParser : ExpressionVisitor
    {
        internal List<string> _properties = new List<string>();
        internal string _alias = null;
        private LinqTableMgr _tableMgr;

        public OnParser(Expression exp, LinqTableMgr tableMgr)
        {
            _tableMgr = tableMgr;

            LambdaExpression lambda = null;
            if(exp is UnaryExpression)
            {
                UnaryExpression unary = (UnaryExpression)exp;

                if(unary.Operand is LambdaExpression)
                    lambda = (LambdaExpression)unary.Operand;
            }
            else if (exp is LambdaExpression)
                lambda = (LambdaExpression)exp;

            Visit(exp);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _properties.Add(node.Member.Name);

            if(node.Expression is MemberExpression)
            {
                MemberExpression member = (MemberExpression)node.Expression;

                if(_tableMgr._typeToAlias.ContainsKey(member.Member.DeclaringType))
                {
                    _alias = _tableMgr._typeToAlias[member.Member.DeclaringType][member.Member.Name];
                }
            }
            
            return node;
        }
    }


    internal class WhereParser : ExpressionVisitor
    {
        internal StringBuilder _sqlPredicate = new StringBuilder();
        internal LinqQueryParser _parser;

        private DataAccessMgr _daMgr;
        private LinqTableMgr _tableMgr;
        private DbPredicateParameter _currentParameter;
        private Dictionary<string, string> _parameterToAlias = new Dictionary<string,string>();

        private string _currentBinaryOp = "";

        public WhereParser(MethodCallExpression expression, LinqTableMgr tableMgr, DataAccessMgr daMgr,
                Dictionary<string, string> parameterToAlias = null)
        {
            _tableMgr = tableMgr;
            _daMgr = daMgr;

            if (expression.Method.Name == "Join" && expression.Method.DeclaringType == typeof(Queryable))
                return;

            //Get all the known types in the collection we are querying against.
            _tableMgr._knownTypes.Visit(expression.Arguments[0]);

            if(expression.Arguments[0] is MethodCallExpression)
                _parser = new LinqQueryParser((MethodCallExpression)expression.Arguments[0], tableMgr, daMgr);


            if(parameterToAlias != null)
                _parameterToAlias = parameterToAlias;
            else if (_parser._selectParser != null)
                _parameterToAlias = _parser._selectParser._parameterToAlias;
            
            //Visit the filter argument.
            Visit(expression.Arguments[1]);
        }

        public WhereParser(Expression expression, LinqTableMgr tableMgr, DataAccessMgr daMgr, 
                Dictionary<string, string> parameterToAlias = null)
        {
            _tableMgr = tableMgr;
            _daMgr = daMgr;

            if(parameterToAlias != null)
                _parameterToAlias = parameterToAlias;

            if (expression is MethodCallExpression)
            {
                MethodCallExpression method = (MethodCallExpression)expression;
                if(method.Method.Name == "Where")
                    //Visit the filter argument.
                    Visit(method.Arguments[1]);

                return;
            }

            Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if(node.Method.DeclaringType == typeof(SqlFunctions))
            {
                _sqlPredicate.AppendFormat(" {0} {1} ", _currentBinaryOp, SelectParser.GenerateFunctionSql(node, 
                        _daMgr));
            }
            else
            {
                Visit(node.Object);

                _sqlPredicate.AppendFormat(" {0} ", _currentBinaryOp);

                foreach(Expression arg in node.Arguments)
                    Visit(arg);
            }

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if(node.Operand is LambdaExpression || node.Operand is MemberExpression)
                return Visit(node.Operand);
            else
                return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if(_tableMgr._aliasStack.Count == 0)
                _tableMgr.Add(node.Parameters[0].Type);

            if(!_parameterToAlias.ContainsKey(node.Parameters[0].Name) && _tableMgr._aliasStack.Count > 0)
                _parameterToAlias.Add(node.Parameters[0].Name, _tableMgr._aliasStack.Last());

            return Visit(node.Body);
        }

        protected override Expression VisitBinary(BinaryExpression exp)
        {
            bool lparens = false;
            bool rparens = false;
            bool isLeftMethodCall = exp.Left is MethodCallExpression && 
                    (((MethodCallExpression)exp.Left).Method.Name == "CompareTo" || 
                     ((MethodCallExpression)exp.Left).Method.DeclaringType == typeof(SqlFunctions));

            if(DbPredicate.IsLogicalOp(exp.Left) && exp.Left.NodeType != exp.NodeType)
                lparens = true;
            else
                lparens = false;

            if(DbPredicate.IsLogicalOp(exp.Right) && exp.Right.NodeType != exp.NodeType)
                rparens = true;
            else
                rparens = false;       

            if(lparens)
                _sqlPredicate.Append("(");

            _currentBinaryOp = DbPredicate.GetBinaryOp(exp, exp.Right);

            Visit(exp.Left);

            if(!isLeftMethodCall)
            {
                if(lparens)
                   _sqlPredicate.Append(")");

                _sqlPredicate.AppendFormat(" {0} ", DbPredicate.GetBinaryOp(exp, exp.Right));

                if(rparens)
                    _sqlPredicate.Append("(");

                Visit(exp.Right);

                if(rparens)
                    _sqlPredicate.Append(")");
            }
            
            return exp;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // Database column reference
            if(_tableMgr._knownTypes._types.Contains(node.Member.DeclaringType))
            {
                QualifiedEntity entity = _tableMgr.GetQualifiedEntity(node.Member.DeclaringType);

                string columName = entity.GetColumnName(node.Member.Name);

                string alias = "";

                if(_tableMgr._aliasStack.Count() == 0)
                    alias = _tableMgr.Add(node.Member.DeclaringType);

                else if(node.Expression is MemberExpression)
                {   
                    MemberExpression member = ((MemberExpression)node.Expression);
                    int aliasIndex = member.Member.DeclaringType.GetProperties().Select( p => p.Name).ToList()
                            .IndexOf(member.Member.Name);
                            
                    alias = _tableMgr._aliasStack[aliasIndex];
                }       
                else if(node.Expression is ParameterExpression)
                {
                    ParameterExpression param = (ParameterExpression)node.Expression;
                    alias = _parameterToAlias[param.Name];
                }

                _sqlPredicate.AppendFormat("{0}.{1}", alias, 
                        columName);   

                _currentParameter = new DbPredicateParameter()
                        {
                            ColumnName = columName,
                            TableName = _tableMgr.GetTableName(node.Member.DeclaringType),
                            SchemaName = _tableMgr.GetSchema(node.Member.DeclaringType),
                        };

            }
            else // value/parameter reference
            {   
                if(_currentParameter != null)
                {
                    Delegate memberAccess = Expression.Lambda(node).Compile();
                    _currentParameter.MemberPropertyName = node.Member.Name;
                    _currentParameter.MemberAccess = memberAccess;
                    _currentParameter.Value = memberAccess.DynamicInvoke();
                    _currentParameter.ParameterName = LinqTableMgr.BuildParamName(node.Member.Name, _tableMgr._parameters, _daMgr);
                    _sqlPredicate.Append(_daMgr.BuildBindVariableName(_currentParameter.ParameterName));
                    _tableMgr._parameters.Add(_currentParameter);
                    _currentParameter = null;
                }
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression exp)
        {
            if(exp.Value == null)
                _sqlPredicate.Append("NULL");
            else
                _sqlPredicate.Append(DbConstValue.GetQuotedValue(exp.Value));

            return exp;
        }
    }

    internal class TypeVisitor : ExpressionVisitor
    {
        public static TypeComparer Comparer = new TypeComparer();

        internal SortedSet<Type> _types = new SortedSet<Type>(Comparer);

        public TypeVisitor()
        {
        }

        public TypeVisitor(Expression exp)
        {
            Visit(exp);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            foreach(Type typ in GetNonGenericTypes(node.Method.ReturnType))
            {
                if(!_types.Contains(typ))
                    _types.Add(typ);
            }

            foreach(Expression arg in node.Arguments)
                Visit(arg);

            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            foreach(ParameterExpression param in node.Parameters)
                foreach(Type typ in GetNonGenericTypes(param.Type))
                {
                    if(!_types.Contains(typ))
                        _types.Add(typ);
                }

            Visit(node.Body);

            return node;
        }

        public static List<Type> GetNonGenericTypes(Type type, List<Type> nonGenericTypes = null)
        {
            if(type.IsGenericType)
                foreach(Type typ in type.GetGenericArguments())
                    nonGenericTypes = GetNonGenericTypes(typ, nonGenericTypes);
            else
            {
                if(nonGenericTypes == null)
                    nonGenericTypes = new List<Type>();

                if(!type.IsPrimitive)
                    nonGenericTypes.Add(type);
            }

            return nonGenericTypes;
        }

        public static T GetFirstParent<T>(MemberExpression exp) where T : Expression
        {
            while(exp.Expression is MemberExpression)
            {
                exp = (MemberExpression)exp.Expression;
            }

            if(exp.Expression is T)
                return (T)exp.Expression;
            else
                return default(T);
        }
    }

    internal class TypeComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y)
        {
            return x.FullName.CompareTo(y.FullName);
        }
    }

    /// <summary>
    /// Used to get new values of variables that were used as parameters in LINQ queries.
    /// </summary>
    internal class ParameterSite : System.ComponentModel.ISite
    {
        List<DbPredicateParameter> _parameters;

        public ParameterSite(List<DbPredicateParameter> parameters)
        {
            _parameters = parameters;
        }

        public System.ComponentModel.IComponent Component
        {
            get { throw new NotImplementedException(); }
        }

        public System.ComponentModel.IContainer Container
        {
            get { throw new NotImplementedException(); }
        }

        public bool DesignMode
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public object GetService(Type serviceType)
        {
            return _parameters;
        }

    }

    internal class MemberVisitor : ExpressionVisitor
    {
        internal string _lastMember;
        internal Type _lastDeclaringType;

        public MemberVisitor(Expression exp)
        {
            Visit(exp);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _lastMember = node.Member.Name;
            _lastDeclaringType = node.Member.DeclaringType;
            return node;
        }
    }

    internal class ObjectParser
    {
        private QualifiedEntity _qualifiedTable;
        private List<DbPredicateParameter> _parameters = new List<DbPredicateParameter>();

        private Type _entityType;
        private DataAccessMgr _daMgr;
        private object _objRef;
        private ObjectContext _entityContext;

        internal QualifiedEntity QualifiedTable { get { return _qualifiedTable; } }
        internal string EntitySetName { get; set; }

        internal ObjectParser(ObjectContext entityContext, object obj, DataAccessMgr daMgr)
        {
            _daMgr = daMgr;
            _objRef = obj;

            _entityContext = entityContext;
            EntityContainer cspaceEntityContainer = _entityContext.MetadataWorkspace.GetEntityContainer(
                    _entityContext.DefaultContainerName, DataSpace.CSpace);

            //?? 
            EntityContextCache.EnsureStorageMetaData(_entityContext);

            _entityType = ObjectContext.GetObjectType(obj.GetType());

            EntitySetBase entitySet = cspaceEntityContainer.BaseEntitySets.FirstOrDefault( 
                    es => es.ElementType.Name == _entityType.Name);

            EntitySetName = entitySet.Name;
            _qualifiedTable = EntityContextCache.StorageMetaData.Get(entitySet.ElementType.FullName);

            //?? _qualifiedTable = EntityContextCache.GetQualifiedEntity(obj.GetType(), entityContext);


        }

        internal ObjectParser(object obj, DataAccessMgr daMgr, string schemaName = null)
        {
            _daMgr = daMgr;
            Type type = obj.GetType();
        
            _qualifiedTable = new QualifiedEntity() { EntityName = type.Name, SchemaName = schemaName };
            _entityType = type;
        }

        internal Tuple<string, List<DbPredicateParameter>> GetInsertSqlAndParams(bool getRowId = false)
        {
            return GetInsertSqlAndParams(new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase), getRowId);
        }

        internal Tuple<string, List<DbPredicateParameter>> GetInsertSqlAndParams(Dictionary<string, object> propertyDbFunctions, bool getRowId = false)
        {
            StringBuilder sb = new StringBuilder("INSERT INTO");

            sb.AppendFormat(" {0}.{1}{2}", _qualifiedTable.SchemaName
                    , _qualifiedTable.EntityName
                    , Environment.NewLine);

            StringBuilder sbColumns = new StringBuilder("");
            StringBuilder sbValues = new StringBuilder("");

            DbTableStructure table = _daMgr.DbCatalogGetTable(QualifiedTable.SchemaName, QualifiedTable.EntityName);

            List<DbPredicateParameter> parameters = new List<DbPredicateParameter>();
            foreach (PropertyInfo prop in _entityType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public
                    | BindingFlags.Instance))
            {
                string columnName = _qualifiedTable.GetColumnName(prop.Name);

                DbColumnStructure column = _daMgr.DbCatalogGetColumn(_qualifiedTable.SchemaName
                        , _qualifiedTable.EntityName
                        , columnName);

                if (!column.IsAutoGenerated && !column.IsComputed)
                {
                    sbColumns.AppendFormat("{0}{1}", sbColumns.Length == 0 ? "" : ", ",
                            columnName);

                    string value = null;
                    if (!propertyDbFunctions.ContainsKey(prop.Name))
                    {
                        Delegate memberAccess = Expression.Lambda(
                                Expression.Property(Expression.Constant(_objRef), prop)).Compile();

                        DbPredicateParameter param = new DbPredicateParameter()
                        {
                            ColumnName = columnName,
                            TableName = _qualifiedTable.EntityName,
                            SchemaName = _qualifiedTable.SchemaName,
                            ParameterName = LinqTableMgr.BuildParamName(prop.Name, parameters, _daMgr),
                            MemberPropertyName = prop.Name,
                            MemberAccess = memberAccess,
                            Value = memberAccess.DynamicInvoke()
                        };

                        parameters.Add(param);
                        value = _daMgr.BuildBindVariableName(param.ParameterName);
                    }
                    else 
                    {
                        if(propertyDbFunctions[prop.Name] is string)
                            value = (string)propertyDbFunctions[prop.Name];
                        else if (propertyDbFunctions[prop.Name] is DbFunctionStructure)
                        {
                            value = ((DbFunctionStructure)propertyDbFunctions[prop.Name]).FunctionBody;
                        }
                    }

                    sbValues.AppendFormat("{0}{1}", sbValues.Length == 0 ? "" : ", ", value);

                }

            }

            if(sbColumns.Length == 0 && _daMgr.DatabaseType == DataAccessMgr.EnumDbType.SqlServer)
                sb.Append(" default values ");
            else
                sb.AppendFormat("({0}) {2}VALUES ({1}){2}", sbColumns, sbValues, Environment.NewLine);

            return new Tuple<string,List<DbPredicateParameter>>(sb.ToString(), parameters);
        }

        internal Tuple<string, List<DbPredicateParameter>> GetUpdateSqlAndParams(ObjectContext context, object entity)
        {
            return GetUpdateSqlAndParams(context
                    , entity
                    , new Dictionary<PropertyInfo, object>());
        }

        internal Tuple<string, List<DbPredicateParameter>> GetUpdateSqlAndParams(
            ObjectContext context, object entity, Dictionary<PropertyInfo, object> propertyDbFunctions)
        {
            StringBuilder updateSQL = new StringBuilder();

            updateSQL.AppendFormat("UPDATE {0}.{1}{2} SET "
                    , _qualifiedTable.SchemaName
                    , _qualifiedTable.EntityName
                    , Environment.NewLine);

            StringBuilder setColumns = new StringBuilder();

            List<DbPredicateParameter> parameters = new List<DbPredicateParameter>();

            ObjectStateEntry ose = context.ObjectStateManager.GetObjectStateEntry(entity);

            foreach (string propName in ose.GetModifiedProperties())
            {
                string columnName = _qualifiedTable.GetColumnName(propName);

                DbColumnStructure column = _daMgr.DbCatalogGetColumn(_qualifiedTable.SchemaName
                    , _qualifiedTable.EntityName
                    , columnName);

                object value = null;
                if (propertyDbFunctions == null || !propertyDbFunctions.ContainsKey(_entityType.GetProperty(propName)))
                {
                    Delegate memberAccess = Expression.Lambda(
                            Expression.Property(Expression.Constant(_objRef)
                            , _entityType.GetProperty(propName))).Compile();

                    DbPredicateParameter param = new DbPredicateParameter()
                    {
                        ColumnName = columnName,
                        TableName = _qualifiedTable.EntityName,
                        SchemaName = _qualifiedTable.SchemaName,
                        ParameterName = LinqTableMgr.BuildParamName(propName
                                , parameters
                                , _daMgr
                                , ose.EntityKey.EntityKeyValues.Where( j => j.Key == propName).Count() > 0 ? true : false),
                        MemberPropertyName = propName,
                        MemberAccess = memberAccess,
                        Value = memberAccess.DynamicInvoke()
                    };

                    value = _daMgr.BuildBindVariableName(param.ParameterName);

                    parameters.Add(param);
                }
                else
                {
                    if (propertyDbFunctions[_entityType.GetProperty(propName)] is string)
                        value = (string)propertyDbFunctions[_entityType.GetProperty(propName)];
                    else if (propertyDbFunctions[_entityType.GetProperty(propName)] is DbFunctionStructure)
                    {
                        value = ((DbFunctionStructure)propertyDbFunctions[_entityType.GetProperty(propName)]).FunctionBody;
                    }
                }

                setColumns.AppendFormat("{0}{1} = {2}{3}", setColumns.Length == 0 ? "" : ", ",
                         columnName
                         , value
                         , Environment.NewLine);
            }

            foreach (PropertyInfo property in propertyDbFunctions.Keys)
                if (ose.GetModifiedProperties().Where(j => j == property.Name).Count() == 0)
                {
                    string columnName = _qualifiedTable.GetColumnName(property.Name);
                    setColumns.AppendFormat("{0}{1} = {2}{3}", setColumns.Length == 0 ? "" : ", ",
                             columnName
                             , propertyDbFunctions[property]
                             , Environment.NewLine);
                }

            updateSQL.Append(setColumns);

            // build where clause
            // build where clause
            Tuple<string, List<DbPredicateParameter>> whereClause
                    = BuildWhereClause(ose.EntityKey.EntityKeyValues, parameters);

            updateSQL.Append(whereClause.Item1);
            parameters = whereClause.Item2;
            return new Tuple<string,List<DbPredicateParameter>>(updateSQL.ToString(), parameters);
        }


        internal Tuple<string, List<DbPredicateParameter>> GeDeleteSqlAndParams(
            ObjectContext context, object entity)
        {
            StringBuilder deleteSQL = new StringBuilder();

            deleteSQL.AppendFormat("DELETE FROM {0}.{1}{2} "
                    , _qualifiedTable.SchemaName
                    , _qualifiedTable.EntityName
                    , Environment.NewLine);

            List<DbPredicateParameter> parameters = new List<DbPredicateParameter>();

            ObjectStateEntry ose = context.ObjectStateManager.GetObjectStateEntry(entity);

            Tuple<string, List<DbPredicateParameter>> whereClause
                    = BuildWhereClause(ose.EntityKey.EntityKeyValues, parameters);

            deleteSQL.Append(whereClause.Item1);
            parameters = whereClause.Item2;
            return new Tuple<string, List<DbPredicateParameter>>(deleteSQL.ToString(), parameters);
        }

        Tuple<string, List<DbPredicateParameter>> BuildWhereClause(EntityKeyMember[] entityKeyValues
                , List<DbPredicateParameter> parameters)
        {
            // build where clause
            StringBuilder where = new StringBuilder();
            foreach (EntityKeyMember key in entityKeyValues)
            {
                string parameterName = LinqTableMgr.BuildParamName(key.Key, parameters, _daMgr);

                where.AppendFormat("{0}{1} = {2}{3}", where.Length > 0 ? "AND " : "WHERE" + Environment.NewLine
                        , key.Key, _daMgr.BuildBindVariableName(parameterName), Environment.NewLine);

                parameters.Add(new DbPredicateParameter()
                {
                    ColumnName = _qualifiedTable.GetColumnName(_qualifiedTable.GetColumnName(key.Key)),
                    ParameterName = parameterName,
                    TableName = _qualifiedTable.EntityName,
                    SchemaName = _qualifiedTable.SchemaName,
                    MemberPropertyName = key.Key,
                    Value = key.Value,
                    MemberAccess = Expression.Lambda(
                            Expression.Property(Expression.Constant(_objRef)
                            , _entityType.GetProperty(key.Key))).Compile()
                });
            }
            return new Tuple<string, List<DbPredicateParameter>>(where.ToString(), parameters);
        }

        /// <summary>
        /// Points the ParameterSite(Isite) parameters to the new object.
        /// </summary>
        /// <param name="dbCmd"></param>
        /// <param name="obj"></param>
        internal static void RemapDbCommandParameters(DbCommand dbCmd, object obj)
        {
            if(!(dbCmd.Site is ParameterSite))
                return;

            ParameterSite paramSite = (ParameterSite)dbCmd.Site;

            List<DbPredicateParameter> parameters = (List<DbPredicateParameter>)paramSite.GetService(null);

            foreach(var param in parameters)
            {
                PropertyInfo property = obj.GetType().GetProperty(param.MemberPropertyName);

                param.MemberAccess = Expression.Lambda(
                            Expression.Property(Expression.Constant(obj)
                            , obj.GetType().GetProperty(param.MemberPropertyName))).Compile();

                dbCmd.Parameters[param.ParameterName].Value = param.MemberAccess.DynamicInvoke();
            }
        }

        internal static string GetEntitySetName(ObjectContext entityContext, object entity)
        {
            Type entityType = ObjectContext.GetObjectType(entity.GetType());
            return GetEntitySetName(entityContext, entity.GetType());
        }

        internal static string GetEntitySetName(ObjectContext entityContext, Type entityType)
        {
            EntityContainer cspaceEntityContainer = entityContext.MetadataWorkspace.GetEntityContainer(
                    entityContext.DefaultContainerName, DataSpace.CSpace);

            EntityContextCache.EnsureStorageMetaData(entityContext);

            return cspaceEntityContainer.BaseEntitySets.FirstOrDefault(
                    es => es.ElementType.Name == entityType.Name).Name;
        }
    }
}

#pragma warning restore 1591 // disable the xmlComments warning
