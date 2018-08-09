using System;
using System.Xml.Linq;
using SolrNet.Linq.Expressions;
using SolrNet.Linq.Expressions.Context;

namespace SolrNet.Linq
{
    public static class SolrExpr
    {
        private static T Throw<T>(T value)
        {
            throw new InvalidOperationException("This method not intended to be invoked. Use it to build expressions only");

        }

        public static class Fields
        {
            public static double Score()
            {
                return Throw(0);
            }
        }
        public static class Transformers
        {
            public static string Value(string value)
            {
                return Throw(value);               
            }

            public static int Value(int value)
            {
                return Throw(value);
            }

            public static float Value(float value)
            {
                return Throw(value);
            }

            public static double Value(double value)
            {
                return Throw(value);
            }

            public static DateTime Value(DateTime value)
            {
                return Throw(value);
            }

            public static XElement ExplainNl()
            {
                return Throw<XElement>(null);
            }

            public static string ExplainText()
            {
                return Throw(string.Empty);
            }

            public static string ExplainHtml()
            {
                return Throw(string.Empty);
            }

            public static int DocId()
            {
                return Throw(0);
            }
        }
    }
}