using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SolrNet.Impl;

namespace SolrNet.Linq.Impl
{
    public class AnonymousTypeActivator<T> : ISolrDocumentActivator<T>
    {
        public T Create()
        {
            Type type = typeof(T);

            ConstructorInfo[] ctors = type.GetConstructors();

            // Has parameter less ctor, so use activator
            if (ctors.Any(info => !info.GetParameters().Any()))
            {
                return Activator.CreateInstance<T>();
            }

            // try to invoke ctor with minimum args, providing default values

            ConstructorInfo ctor = ctors.OrderBy(info => info.GetParameters().Length).First();
            List<object> args = new List<object>();
            foreach (var p in ctor.GetParameters())
            {
                args.Add(p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null);
            }

            return (T)Activator.CreateInstance(type, args.ToArray());
        }
    }
}