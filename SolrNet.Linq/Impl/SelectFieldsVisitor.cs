using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq.Impl
{
    public class SelectFieldsVisitor : ExpressionVisitor
    {
        private readonly MemberContext _context;
        private readonly SelectExpressionsCollection _selectContext;


        public SelectFieldsVisitor(MemberContext context, SelectExpressionsCollection selectContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _selectContext = selectContext ?? throw new ArgumentNullException(nameof(selectContext));
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.MemberAccess)
            {
                if (this._context.IsAccessToMember(node))
                {
                    this._selectContext.Fields.Add(this._context.GetSolrMemberProduct(node));
                }
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(SolrExpr.Transformers) ||
                node.Method.DeclaringType == typeof(SolrExpr.Fields))
            {
                this._selectContext.AddComputed(node, this._context.GetSolrMemberProduct(node, false));
            }

            return base.VisitMethodCall(node);
        }
    }
}