using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.AspNet.OData
{
    public static class CustomMethodCache
    {
        private static readonly Dictionary<string, MethodInfo> customMethods = new Dictionary<string, MethodInfo>();

        public static void CacheCustomMethod(string edmFunctionName, MethodInfo methodInfo)
            => customMethods.Add
            (
                GetMethodKey
                (
                    edmFunctionName,
                    GetArguments(methodInfo)
                ),
                methodInfo
            );

        public static MethodInfo GetCachedCustomMethod(string edmFunctionName, IEnumerable<Type> argumentTypes)
        {
            if (customMethods.TryGetValue(GetMethodKey(edmFunctionName, argumentTypes), out MethodInfo methodInfo))
                return methodInfo;

            return null;
        }

        public static bool RemoveCachedCustomMethod(string edmFunctionName, MethodInfo methodInfo)
        {
            return Remove(GetMethodKey(edmFunctionName, GetArguments(methodInfo)));

            static bool Remove(string key)
            {
                if (customMethods.ContainsKey(key))
                {
                    customMethods.Remove(key);
                    return true;
                }

                return false;
            }
        }

        private static string GetMethodKey(string edmFunctionName, IEnumerable<Type> argumentTypes)
            => string.Concat
            (
                edmFunctionName,
                ":",
                string.Join
                (
                    ",",
                    argumentTypes.Select(type => type.FullName)
                )
            );

        private static IEnumerable<Type> GetArguments(MethodInfo methodInfo)
                => methodInfo.IsStatic
                    ? methodInfo.GetParameters().Select(p => p.ParameterType)
                    : new Type[] { methodInfo.DeclaringType }
                        .Concat(methodInfo.GetParameters()
                        .Select(p => p.ParameterType));
    }
}
