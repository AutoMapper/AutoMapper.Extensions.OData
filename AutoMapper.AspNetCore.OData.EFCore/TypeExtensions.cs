using AutoMapper.Configuration.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.AspNet.OData
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Get MemberInfo
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static MemberInfo GetMemberInfo(this Type parentType, string memberName)
        {
            MemberInfo mInfo = parentType.GetMember(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase).FirstOrDefault();
            if (mInfo == null)
                throw new ArgumentException(string.Format(Properties.Resources.memberDoesNotExistFormat, memberName, parentType.FullName));

            return mInfo;
        }

        public static MemberInfo[] GetSelectedMembers(this Type parentType, List<string> selects)
        {
            if (selects == null || !selects.Any())
                return parentType.GetValueTypeMembers();

            return selects.Select(select => parentType.GetMemberInfo(select)).ToArray();
        }

        private static MemberInfo[] GetValueTypeMembers(this Type parentType)
        {
            if (parentType.IsList())
                return new MemberInfo[] { };

            return parentType.GetMemberInfos().Where
            (
                info => (info.MemberType == MemberTypes.Field || info.MemberType == MemberTypes.Property)
                && info.GetMemberType().IsLiteralType()
            ).ToArray();
        }

        private static MemberInfo[] GetMemberInfos(this Type parentType) 
            => parentType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);

        public static bool IsList(this Type type)
            => type.IsArray || (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type));

        /// <summary>
        /// Get the member type or its the underlying element type if it is a list
        /// </summary>
        /// <param name="memberType"></param>
        /// <returns></returns>
        public static Type GetCurrentType(this Type memberType)
            //when the member is an IEnumberable<T> we really need T.
            => memberType.IsList()
                ? memberType.GetUnderlyingElementType()
                : memberType;

        public static Type GetUnderlyingElementType(this Type type)
        {
            TypeInfo tInfo = type.GetTypeInfo();
            if (tInfo.IsArray)
                return tInfo.GetElementType();

            Type[] genericArguments;
            if (!tInfo.IsGenericType || (genericArguments = tInfo.GetGenericArguments()).Length != 1)
                throw new ArgumentException("type");

            return genericArguments[0];
        }

        public static Type GetUnderlyingElementType(this Expression expression)
            => expression.Type.GetUnderlyingElementType();

        /// <summary>
        /// Get Member Type
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case MethodInfo mInfo:
                    return mInfo.ReturnType;
                case PropertyInfo pInfo:
                    return pInfo.PropertyType;
                case FieldInfo fInfo:
                    return fInfo.FieldType;
                case null:
                    throw new ArgumentNullException(nameof(memberInfo));
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberInfo));
            }
        }

        public static LambdaExpression GetTypedSelector<TSource>(this string propertyFullName, string parameterName = "a")
            => propertyFullName.GetTypedSelector(typeof(TSource), parameterName);

        public static LambdaExpression GetTypedSelector(this string propertyFullName, Type parentType, string parameterName = "a")
        {
            ParameterExpression param = Expression.Parameter(parentType, parameterName);
            string[] parts = propertyFullName.Split('.');
            Expression parent = parts.Aggregate((Expression)param, (p, next) => Expression.MakeMemberAccess(p, p.Type.GetMemberInfo(next)));

            Type[] typeArgs = new[] { parentType, parent.Type };//Generic arguments e.g. T1 and T2 MethodName<T1, T2>(method arguments)
            Type delegateType = typeof(Func<,>).MakeGenericType(typeArgs);//Delegate type for the selector expression.  It takes a TSource and returns the sort property type
            return Expression.Lambda(delegateType, parent, param);//Resulting lambda expression for the selector.
        }

        public static MemberInfo GetMemberInfoFromFullName(this Type type, string propertyFullName)
        {
            int indexOfSeparator = propertyFullName.IndexOf('.');
            if (indexOfSeparator < 0)
            {
                return type.GetMemberInfo(propertyFullName);
            }

            string propertyName = propertyFullName.Substring(0, indexOfSeparator);
            string childFullName = propertyFullName.Substring(indexOfSeparator + 1);

            return GetMemberInfoFromFullName(type.GetMemberInfo(propertyName).GetMemberType(), childFullName);
        }

        public static bool IsLiteralType(this Type type)
        {
            if (PrimitiveHelper.IsNullableType(type))
                type = Nullable.GetUnderlyingType(type);

            return LiteralTypes.Contains(type);
        }

        private static HashSet<Type> LiteralTypes => new HashSet<Type>(_literalTypes);

        private static Type[] _literalTypes => new Type[] {
                typeof(bool),
                typeof(DateTime),
                typeof(TimeSpan),
                typeof(Guid),
                typeof(decimal),
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(char),
                typeof(sbyte),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
                typeof(string)
            };
    }
}
