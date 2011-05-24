﻿using System;
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

        public static void EnsureStorageMetaData(ObjectContext context)
        {
            Assembly assembly = Assembly.GetAssembly(context.GetType());
            _assembliesLoaded.GetOrAdd(assembly.FullName, () => LoadMetaDataFromAssembly(assembly));
        }

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
                    string schema = set.Attribute("Schema").Value;

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

    internal class ContextVisitor : ExpressionVisitor
    {
        ObjectContext _defaultContext = null;

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

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Type.IsGenericType && (node.Type.GetGenericTypeDefinition() == typeof(ObjectQuery<>)
                    || node.Type.GetGenericTypeDefinition() == typeof(ObjectSet<>)))
            {
                // Example: "System.Data.Objects.ObjectSet`1[[B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter, B1.Utility.DatabaseSetup, Version=1.13.1.0, Culture=neutral, PublicKeyToken=null]]"
                string objectSetTypeName = node.Type.FullName;
                
                // Example: "B1.Utility.DatabaseSetup.ModelsSecond.AppConfigParameter"
                string entityTypeName = ObjectContext.GetObjectType(node.Type.GetGenericArguments()[0]).FullName;

                // Example: 
                string contextClassName = node.Member.DeclaringType.FullName;

                // Ensure that the Context is stored
                ObjectContext entityContext = EntityContextCache.Contexts.GetOrAdd(contextClassName,
                    () => { return ((ObjectQuery)Expression.Lambda(node).Compile().DynamicInvoke()).Context ?? _defaultContext; });

                // Ensure that the Storage meta data is loaded into cache
                EntityContextCache.EnsureStorageMetaData(entityContext);

                // Map the type names for the collection of entities and the entity to the context so that
                // subsequent encounter of these types results in context look up
                EntityContextCache.TypesToContexts.GetOrAdd(objectSetTypeName, () => { return entityContext; });
                EntityContextCache.TypesToContexts.GetOrAdd(entityTypeName, () => { return entityContext; });
            }

            return node;
        }

        public QualifiedEntity GetQualifiedEntity(Type dotNetEntityType)
        {
            return EntityContextCache.StorageMetaData.Get(
                EntityContextCache.GetMappingTypeName(dotNetEntityType, _defaultContext));
        }

        public string GetSchemaName(Type type)
        {
            return GetQualifiedEntity(type).SchemaName;
        }

        public string GetTableName(Type type)
        {
            return GetQualifiedEntity(type).EntityName;
        }

        public string GetSchemaQualifiedTableName(Type type)
        {
            QualifiedEntity entity = GetQualifiedEntity(type);
            return string.Format("{0}.{1}", entity.SchemaName, entity.EntityName);
        }
    }

    internal class QualifiedEntity
    {
        public string SchemaName { get; set; }
        public string EntityName { get; set; }

        public Dictionary<string, string> _propertyToColumnMap = new Dictionary<string, string>();

        public QualifiedEntity() { }
        public QualifiedEntity(string schemaName, string entityName)
        {
            SchemaName = schemaName;
            EntityName = entityName;
        }

        public string GetColumnName(string propertyName)
        {
            return _propertyToColumnMap.ContainsKey(propertyName) ? _propertyToColumnMap[propertyName]
                : null;
        }

    }
}