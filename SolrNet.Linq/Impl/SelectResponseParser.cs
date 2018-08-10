using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using SolrNet.Impl;
using SolrNet.Mapping;

namespace SolrNet.Linq.Impl
{
    public class SelectResponseParser<TNew,TOld> : ISolrDocumentResponseParser<TNew>
    {
        private readonly ConstructorInfo CtorInfo = typeof(TNew).GetConstructors().Single();
        private readonly ISolrFieldParser parser ;

        public SelectResponseParser(ISolrFieldParser parser)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public IList<TNew> ParseResults(XElement parentNode)
        {
            List<TNew> objList = new List<TNew>();
            if (parentNode == null)
                return (IList<TNew>)objList;
            foreach (XElement element in parentNode.Elements((XName)"doc"))
                objList.Add(this.ParseDocument(element));
            return (IList<TNew>)objList;
        }

        public TNew ParseDocument(XElement node)
        {
            Dictionary<string, XElement> fields = node.Elements().ToDictionary(element => element.Attribute((XName) "name").Value);
           
            List<object> args = new List<object>(fields.Count);
            foreach (ParameterInfo p in CtorInfo.GetParameters())
            {
                object obj = p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
                if (fields.ContainsKey(p.Name))
                {
                    if (p.ParameterType == typeof(XElement))
                    {
                        string text = fields[p.Name].ToString();
                        try
                        {
                            
                            obj = XElement.Parse(text);
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException(
                                $"Unable to set value for {p.Name}. Value {text} can't be parsed to XElement",e);
                        }                        
                    }
                    else if (this.parser.CanHandleSolrType(fields[p.Name].Name.LocalName) &&
                        this.parser.CanHandleType(p.ParameterType))
                    {
                        obj = this.parser.Parse(fields[p.Name], p.ParameterType);

                        if (obj != null)
                        {
                            if (!p.ParameterType.IsAssignableFrom(obj.GetType()))
                            {
                                throw new InvalidOperationException(
                                    $"Unable to set value for {p.Name}. Value {obj} of type {obj.GetType()} not assignable to type {p.ParameterType}");
                            }
                        }
                        else if (p.ParameterType.IsValueType)
                        {
                            throw new InvalidOperationException(
                                $"Unable to set value for {p.Name}. Value null not assignable to type {p.ParameterType}");
                        }
                    }
                }

                args.Add(obj);
            }

            return (TNew) CtorInfo.Invoke(args.ToArray());
        }
    }
}