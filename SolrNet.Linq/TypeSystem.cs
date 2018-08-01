using System;
using System.Collections.Generic;

namespace SolrNet.Linq
{
    public static class TypeSystem
    {
        public static Type GetElementType(Type seqType)
        {
            Type ienum = FindEnumerable(seqType);
            if (ienum == null)
            {
                return seqType;
            }

            return ienum.GetGenericArguments()[0];
        }

        private static Type FindEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }

            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }

            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }

            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindEnumerable(iface);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindEnumerable(seqType.BaseType);
            }

            return null;
        }
    }
}