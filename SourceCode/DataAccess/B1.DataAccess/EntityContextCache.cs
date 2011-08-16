using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Reflection;
using System.Linq.Expressions;
using System.Data.Metadata.Edm;
using System.Xml.Linq;

using B1.Core;
using B1.CacheManagement;

namespace B1.DataAccess
{
    /// <summary>
    /// This class caches the entity logical and physical storage information and mappings. Also caches the object
    /// context for all the entity types encounter.
    /// </summary>
    internal class EntityContextCache
    {
        // Context cache - key is the name of the .NET fully qualified class name for the ObjectContext
        private static CacheMgr<ObjectContext> _contextCache = new CacheMgr<ObjectContext>();
        public static CacheMgr<ObjectContext> Contexts { get { return _contextCache; } }

        // Types to context look up cache
        private static CacheMgr<ObjectContext> _typesToContextCache = new CacheMgr<ObjectContext>();
        public static CacheMgr<ObjectContext> TypesToContexts { get { return _typesToContextCache; } }

        // .NET Entity type name to mapping entity type name (from MSL file)
        // "B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter"
        //      =>     "B1SampleModel.AppConfigParameter"
        private static CacheMgr<string> _typeNamesMapping = new CacheMgr<string>();

        public static bool ExistsMappingTypeName(Type dotNetEntityType, ObjectContext defaultContext)
        {
            // dotNetEntityType can be a POCO object with no ObjectContext. We are getting proxy for the POCO
            // object which will have the context needed for LINQ stuff
            Type objectType = ObjectContext.GetObjectType(dotNetEntityType);

            ObjectContext context = TypesToContexts.GetOrDefault(objectType.FullName, defaultContext);
            EntityContainer entityContainer = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
            EntitySetBase entitySet = entityContainer.BaseEntitySets.FirstOrDefault(es => es.ElementType.Name == objectType.Name);

            if(entitySet != null)
            {
                _typeNamesMapping.GetOrAdd(objectType.FullName,
                   () =>
                   {
                       return entitySet.ElementType.FullName;
                   });
                return true;
            }
            else return false;
        }

        public static string GetMappingTypeName(Type dotNetEntityType, ObjectContext defaultContext)
        {
            // dotNetEntityType can be a POCO object with no ObjectContext. We are getting proxy for the POCO
            // object which will have the context needed for LINQ stuff
            Type objectType = ObjectContext.GetObjectType(dotNetEntityType);
            return _typeNamesMapping.GetOrAdd(objectType.FullName,
                () =>
                {
                    ObjectContext context = TypesToContexts.GetOrDefault(objectType.FullName, defaultContext);
                    EntityContainer entityContainer = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);
                    EntitySetBase entitySet = entityContainer.BaseEntitySets.FirstOrDefault(es => es.ElementType.Name == objectType.Name);
                    return entitySet.ElementType.FullName;
                });
        }

        // Mapping Entity Type Name to Storage meta data
        // keyed using "B1SampleModel.AppConfigParameter" to QualifiedEntity
        private static CacheMgr<QualifiedEntity> _storageMetaData = new CacheMgr<QualifiedEntity>();
        private static CacheMgr<Assembly> _assembliesLoaded = new CacheMgr<Assembly>();

        public static CacheMgr<QualifiedEntity> StorageMetaData { get { return _storageMetaData; } }

        public static QualifiedEntity GetQualifiedEntity(string schemaName, string entityName)
        {
            return _storageMetaData.FirstOrDefault(
                qe => qe.SchemaName.ToLower() == schemaName.ToLower() && qe.EntityName.ToLower() == entityName.ToLower(),
                null);
        }

        public QualifiedEntity GetQualifiedEntity(Type dotNetEntityType, ObjectContext defaultContext)
        {
            return StorageMetaData.Get(GetMappingTypeName(dotNetEntityType, defaultContext));
        }

        /// <summary>
        /// This function checks if the entity meta data is already cached for the given context. If it is not
        /// then it does it by looking into the assembly file.
        /// </summary>
        /// <param name="context">Object context</param>
        public static void EnsureStorageMetaData(ObjectContext context)
        {
            Assembly assembly = Assembly.GetAssembly(context.GetType());
            _assembliesLoaded.GetOrAdd(assembly.FullName, () => LoadMetaDataFromAssembly(assembly));
        }

        /// <summary>
        /// This function is loading up the meta data information from the assembly about the entities, its storage name
        /// and its logical mapping information. This information is cached so that the subsequent queries do not need to
        /// lookup these meta data. The entity framework adds the following xml meta data resource files into the assembly of
        /// the context class:
        /// 
        ///     .csdl   : Conceptual schema definition language
        ///     .ssdl   : Storage schema definition language
        ///     .msl    : Mapping specification language
        ///     
        /// Respectively these files describes the conceptual model, storage model, and the mapping between these models
        /// 
        /// </summary>
        /// <param name="contextAssembly">Assembly where the context code reside.</param>
        /// <returns></returns>
        private static Assembly LoadMetaDataFromAssembly(Assembly contextAssembly)
        {
            foreach (string ssdlResourceName in contextAssembly.GetManifestResourceNames().Where(
                    r => r.EndsWith(".ssdl", StringComparison.CurrentCultureIgnoreCase)))
            {
                XDocument ssdlDoc = XDocument.Load(contextAssembly.GetManifestResourceStream(ssdlResourceName));

                //Find correct way to do this.
                string mslResourceName = ssdlResourceName.Replace(".ssdl", ".msl");
                XDocument mslDoc = XDocument.Load(contextAssembly.GetManifestResourceStream(
                        mslResourceName));

                XNamespace ns = mslDoc.Elements().First().GetDefaultNamespace();

                //mapping for storage name to EntitySetName
                Dictionary<string, XElement> storageNameToMappingFragment =
                        mslDoc.Descendants(ns + "EntityTypeMapping").ToDictionary(
                            e => e.Element(ns + "MappingFragment").Attribute("StoreEntitySet").Value,
                            e => e);

                ns = ssdlDoc.Elements().First().GetDefaultNamespace();

                foreach (XElement set in ssdlDoc.Descendants(ns + "EntityContainer")
                            .First().Elements(ns + "EntitySet"))
                {
                    string name = set.Attribute("Name").Value;
                    // Sometime in the same SSDL XML - We have both "Schema" or one adorned with namespace
                    string schema = set.Attributes().First(attr => attr.Name.LocalName.ToLower() == "schema").Value;

                    string typeName = storageNameToMappingFragment[name].Attribute("TypeName").Value;


                    QualifiedEntity entity = new QualifiedEntity(schema, name);

                    foreach (XElement prop in storageNameToMappingFragment[name].Descendants().Where(e => e.Name.LocalName == "ScalarProperty"))
                    {
                        entity._propertyToColumnMap.Add(prop.Attribute("Name").Value, prop.Attribute("ColumnName").Value);
                    }

                    _storageMetaData.Add(typeName, entity);
                }
            }

            return contextAssembly;
        }
    }

    /// <summary>
    /// This class visits the query expression to discover the ObjectContexts from the entities. This adds the support for
    /// multi context query. It caches the Entities to ObjectContexts for future query expressions.
    /// </summary>
    internal class ContextVisitor : ExpressionVisitor
    {
        ObjectContext _defaultContext = null;

        /// <summary>
        /// Constructor for ContextVisitor takes the query expression.
        /// </summary>
        /// <param name="queryable">Query expression</param>
        public ContextVisitor(IQueryable queryable)
        {
            if (queryable.GetType().IsGenericType && (queryable.GetType().GetGenericTypeDefinition() == typeof(ObjectQuery<>)
                    || queryable.GetType().GetGenericTypeDefinition() == typeof(ObjectSet<>)))
            {
                _defaultContext = ((ObjectQuery)queryable).Context;
                EntityContextCache.EnsureStorageMetaData(_defaultContext);
            }

            // Visit the expression to discover if any other ObjectContext is used (cross-model query)
            Visit(queryable.Expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            /* // The expression tree - Where clause is argument to Select clause. Join clause is argument to Select clause.
             * // Visit the arguments expression to keep drilling the expression tree.
.Call System.Linq.Queryable.Select(
    .Call System.Linq.Queryable.Where(
        .Call System.Linq.Queryable.Join(
            .Call ((System.Data.Objects.ObjectQuery`1[B1.Utility.DatabaseSetup.Models.AppConfigSetting]).Constant<System.Data.Objects.ObjectSet`1[B1.Utility.DatabaseSetup.Models.AppConfigSetting]>(System.Data.Objects.ObjectSet`1[B1.Utility.DatabaseSetup.Models.AppConfigSetting])).MergeAs(.Constant<System.Data.Objects.MergeOption>(AppendOnly))
            ,
            .Call ((System.Data.Objects.ObjectQuery`1[B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter]).Constant<System.Data.Objects.ObjectSet`1[B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter]>(System.Data.Objects.ObjectSet`1[B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter])).MergeAs(.Constant<System.Data.Objects.MergeOption>(AppendOnly))
            ,
            '(.Lambda #Lambda1<System.Func`2[B1.Utility.DatabaseSetup.Models.AppConfigSetting,System.String]>),
            '(.Lambda #Lambda2<System.Func`2[B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter,System.String]>),
            '(.Lambda #Lambda3<System.Func`3[B1.Utility.DatabaseSetup.Models.AppConfigSetting,B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter,<>f__AnonymousType0`2[B1.Utility.DatabaseSetup.Models.AppConfigSetting,B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter]]>))
        , ......
             */

            // Discover the context from the expression
            DiscoverContext(expression);

            // Visit all the arguments to this method call
            foreach (Expression argument in expression.Arguments)
            {
                Visit(argument);
            }

            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            return DiscoverContext(expression);
        }

        /// <summary>
        /// Find the ObjectContext from the given expression and cache it for future queries.
        /// </summary>
        protected Expression DiscoverContext(Expression expression)
        {
            Type type = expression is MemberExpression ? ((MemberExpression)expression).Type :
                expression is MethodCallExpression && ((MethodCallExpression)expression).Object != null ?
                ((MethodCallExpression)expression).Object.Type : expression.Type;

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ObjectQuery<>)
                    || type.GetGenericTypeDefinition() == typeof(ObjectSet<>)))
            {
                // Example: "System.Data.Objects.ObjectSet`1[[B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter, B1.Utility.DatabaseSetup, Version=1.13.1.0, Culture=neutral, PublicKeyToken=null]]"
                string objectSetTypeName = type.FullName;

                // Example: "B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter"
                string entityTypeName = ObjectContext.GetObjectType(type.GetGenericArguments()[0]).FullName;
                string contextClassName = type.FullName; //?? ((MemberExpression) expression).Member.DeclaringType.FullName;

                // Get the expression which can be evaluated to get the ObjectContext
                Expression expressionToRun = expression is MemberExpression ? (MemberExpression)expression :
                    expression is MethodCallExpression ? ((MethodCallExpression)expression).Object : null;

                // Ensure that the Context is stored
                ObjectContext entityContext = EntityContextCache.Contexts.GetOrAdd(contextClassName,
                    () => { return ((ObjectQuery)Expression.Lambda(expressionToRun).Compile().DynamicInvoke()).Context ?? _defaultContext; });

                // Ensure that the Storage meta data is loaded into cache
                EntityContextCache.EnsureStorageMetaData(entityContext);

                // Map the type names for the collection of entities and the entity to the context so that
                // subsequent encounter of these types results in context look up
                EntityContextCache.TypesToContexts.GetOrAdd(objectSetTypeName, () => { return entityContext; });
                EntityContextCache.TypesToContexts.GetOrAdd(entityTypeName, () => { return entityContext; });
            }

            return expression;
        }

        /// <summary>
        /// Check if the entity already exists in the cache.
        /// </summary>
        /// <param name="dotNetEntityType">Entity type</param>
        /// <returns>True if exists in the cache else false</returns>
        public bool EntityExists(Type dotNetEntityType)
        {
            return EntityContextCache.ExistsMappingTypeName(dotNetEntityType, _defaultContext);
        }

        /// <summary>
        /// Get the QualifiedEntity which has the entity name and the schema name.
        /// </summary>
        /// <param name="dotNetEntityType">Entity type</param>
        /// <returns>QualifiedEntity for the given entity type.</returns>
        public QualifiedEntity GetQualifiedEntity(Type dotNetEntityType)
        {
            return EntityContextCache.StorageMetaData.Get(
                EntityContextCache.GetMappingTypeName(dotNetEntityType, _defaultContext));
        }

        /// <summary>
        /// Get the schema name for the type.
        /// </summary>
        /// <param name="type">Entity type</param>
        /// <returns>Returns the name of the schema</returns>
        public string GetSchemaName(Type type)
        {
            return GetQualifiedEntity(type).SchemaName;
        }

        /// <summary>
        /// Get the table name for the given type.
        /// </summary>
        /// <param name="type">Entity type</param>
        /// <returns>Table name</returns>
        public string GetTableName(Type type)
        {
            return GetQualifiedEntity(type).EntityName;
        }

        /// <summary>
        /// Get the table name adorned with schema - qualified table name.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetSchemaQualifiedTableName(Type type)
        {
            QualifiedEntity entity = GetQualifiedEntity(type);
            return string.Format("{0}.{1}", entity.SchemaName, entity.EntityName);
        }
    }

    /// <summary>
    /// This class has the storage entity name and schema name.
    /// </summary>
    internal class QualifiedEntity
    {
        /// <summary>
        /// Storage schema name
        /// </summary>
        public string SchemaName { get; set; }
        
        /// <summary>
        /// Name of the table
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Property to column mapping
        /// </summary>
        public Dictionary<string, string> _propertyToColumnMap = new Dictionary<string, string>();

        /// <summary>
        /// Empty constructor
        /// </summary>
        public QualifiedEntity() { }

        /// <summary>
        /// Constructor to initialize the entity name and schema name
        /// </summary>
        /// <param name="schemaName">Schema name</param>
        /// <param name="entityName">Entity name</param>
        public QualifiedEntity(string schemaName, string entityName)
        {
            SchemaName = schemaName;
            EntityName = entityName;
        }

        /// <summary>
        /// Get the column name from the property name
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Column name for the given property</returns>
        public string GetColumnName(string propertyName)
        {
            return _propertyToColumnMap.ContainsKey(propertyName) ? _propertyToColumnMap[propertyName]
                : null;
        }

    }
}
