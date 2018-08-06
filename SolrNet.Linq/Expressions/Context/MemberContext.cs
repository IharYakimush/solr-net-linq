using System;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Impl;
using SolrNet.Impl.FieldParsers;
using SolrNet.Impl.FieldSerializers;
using SolrNet.Mapping;

namespace SolrNet.Linq.Expressions.Context
{
    public abstract class MemberContext
    {        
        private static readonly DefaultFieldSerializer DefaultFieldSerializer = new DefaultFieldSerializer();
        private static IReadOnlyMappingManager DefaultMappingManager { get; } = new AttributesMappingManager();

        private ISolrFieldSerializer _fieldSerializer;
        private IReadOnlyMappingManager _mappingManager;
        public abstract bool HasMemberAccess(Expression expression);

        public abstract string GetSolrMemberProduct(Expression expression, bool disableFunctions = false);

        public abstract bool IsAccessToMember(MemberExpression expression);

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