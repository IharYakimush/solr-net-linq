using System;
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
        }
    }
}