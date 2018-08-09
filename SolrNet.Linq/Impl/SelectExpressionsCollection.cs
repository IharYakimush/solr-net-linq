using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SolrNet.Commands.Parameters;

namespace SolrNet.Linq.Impl
{
    public class SelectExpressionsCollection
    {
        public QueryOptions QueryOptions { get; } = new QueryOptions();

        private int _identity = 0;       

        public ICollection<string> Fields => this.QueryOptions.Fields;

        public Dictionary<MethodCallExpression, string> Computed { get; } =
            new Dictionary<MethodCallExpression, string>();

        public void AddComputed(MethodCallExpression expression, string value)
        {                        
            string existing =
                this.QueryOptions.Fields.FirstOrDefault(s => s.Contains(":") && s.Split(':').Last() == value);

            string alias;
            if (existing == null)
            {
                alias = $"v{this._identity}";
                this.QueryOptions.Fields.Add($"{alias}:{value}");                
                this._identity++;
            }
            else
            {
                alias = existing.Split(':').First();
            }
           
            this.Computed.Add(expression, alias);
        }
    }
}