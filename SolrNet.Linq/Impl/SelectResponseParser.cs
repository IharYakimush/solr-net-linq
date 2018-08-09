﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using SolrNet.Impl;
using SolrNet.Mapping;

namespace SolrNet.Linq.Impl
{
    public class SelectResponseParser<T> : ISolrDocumentResponseParser<T>
    {
        private readonly ConstructorInfo CtorInfo = typeof(T).GetConstructors().Single();
        private readonly ISolrFieldParser parser ;

        public SelectResponseParser(ISolrFieldParser parser)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public IList<T> ParseResults(XElement parentNode)
        {
            List<T> objList = new List<T>();
            if (parentNode == null)
                return (IList<T>)objList;
            foreach (XElement element in parentNode.Elements((XName)"doc"))
                objList.Add(this.ParseDocument(element));
            return (IList<T>)objList;
        }

        public T ParseDocument(XElement node)
        {
            Dictionary<string, XElement> fields = node.Elements().ToDictionary(element => element.Attribute((XName) "name").Value);
           
            List<object> args = new List<object>(fields.Count);
            foreach (ParameterInfo p in CtorInfo.GetParameters())
            {
                object obj = p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
                if (fields.ContainsKey(p.Name))
                {
                    if (this.parser.CanHandleSolrType(fields[p.Name].Name.LocalName) &&
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

            return (T) CtorInfo.Invoke(args.ToArray());
        }
    }
}