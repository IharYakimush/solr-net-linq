using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SolrNet.Impl;
using SolrNet.Impl.FieldParsers;
using SolrNet.Impl.FieldSerializers;
using SolrNet.Mapping;

namespace SolrNet.Linq.Expressions.Context
{
    public abstract class MemberContext
    {
        private static readonly ConcurrentDictionary<MemberInfo, string> MemberNames = new ConcurrentDictionary<MemberInfo, string>();

        private static readonly DefaultFieldSerializer DefaultFieldSerializer = new DefaultFieldSerializer();
        private static IReadOnlyMappingManager DefaultMappingManager { get; } = new AttributesMappingManager();

        private ISolrFieldSerializer _fieldSerializer;
        private IReadOnlyMappingManager _mappingManager;
        public abstract bool HasMemberAccess(Expression expression);

        public abstract string GetSolrMemberProduct(Expression expression, bool disableFunctions = false);

        public abstract bool IsAccessToMember(MemberExpression expression);

        public virtual string GetMemberSolrName(MemberInfo info)
        {
            return MemberNames.GetOrAdd(info, m =>
            {
                var att = this.MappingManager.GetFields(info.DeclaringType);

                SolrFieldModel value = att.Values.FirstOrDefault(f => f.Property == info as PropertyInfo);
                if (value != null)
                {
                    return value.FieldName;
                }

                throw new InvalidOperationException(
                    $"Unable to get solr name for {m.DeclaringType}.{m.Name}. Mapping manager has mappings only for {string.Join(", ", att.Values.Select(f => f.Property.Name))}");
            });
        }

        public string TrueStringSerialized => this.FieldSerializer.Serialize(true).Single().FieldValue;
            
        public static MemberContext ForType<T>()
        {
            return new TypeContext(typeof(T));
        }

        public static MemberContext ForType(Type type)
        {
            return new TypeContext(type);
        }

        public static MemberContext ForLambda(MemberContext parent, LambdaExpression lambdaExpression, string fieldName)
        {
            ParamContext context = new ParamContext(lambdaExpression, fieldName);
            context.FieldSerializer = parent.FieldSerializer;

            return context;
        }

        public ISolrFieldSerializer FieldSerializer
        {
            get => _fieldSerializer ?? DefaultFieldSerializer;
            set => _fieldSerializer = value;
        }

        public IReadOnlyMappingManager MappingManager
        {
            get => _mappingManager ?? DefaultMappingManager;
            set => _mappingManager = value;
        }        
    }
}