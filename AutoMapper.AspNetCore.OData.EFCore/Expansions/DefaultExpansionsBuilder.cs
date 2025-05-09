using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.AspNet.OData.Expansions
{
    internal class DefaultExpansionsBuilder(ODataQueryContext context)
    {
        private readonly ODataQueryContext context = context;
        const BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public LambdaExpression[] Build(ParameterExpression param, Expression parentBody)
        {
            List<LambdaExpression> memberSelectors = [];
            Build(memberSelectors, param, parentBody);
            return [.. memberSelectors];
        }

        public void Build(List<LambdaExpression> memberSelectors, ParameterExpression param, Expression parentBody)
        {
            HashSet<string> navigationProperties = GetNavigationProperties(parentBody.Type);
            MemberInfo[] infos = GetMemberInfos(parentBody.Type).Where
            (
                info => (info.MemberType == MemberTypes.Field || info.MemberType == MemberTypes.Property)
                    && (!navigationProperties.Contains(info.Name))
            ).ToArray();

            foreach (MemberInfo info in infos)
            {
                Type memberType = LogicBuilder.Expressions.Utils.TypeExtensions.GetMemberType(info);
                Expression selector = Expression.MakeMemberAccess(parentBody, info);
                if (selector.Type.IsValueType)
                    selector = Expression.Convert(selector, typeof(object));

                memberSelectors.Add
                (
                    Expression.Lambda
                    (
                        typeof(Func<,>).MakeGenericType([param.Type, typeof(object)]),
                        selector,
                        param
                    )
                );

                if (LogicBuilder.Expressions.Utils.TypeExtensions.IsLiteralType(memberType))
                    continue;
                
                if (LogicBuilder.Expressions.Utils.TypeExtensions.IsList(memberType)
                    && LogicBuilder.Expressions.Utils.TypeExtensions.IsLiteralType
                        (
                            TypeExtensions.GetUnderlyingElementType(memberType)
                        ) == false)
                {
                    List<LambdaExpression> childMemberSelectors = [];
                    ParameterExpression childParam = Expression.Parameter
                    (
                         TypeExtensions.GetUnderlyingElementType(memberType),
                         GetChildParameterName(param.Name)
                    );

                    Build(childMemberSelectors, childParam, childParam);
                    childMemberSelectors.ForEach(childSelector =>
                    {
                        memberSelectors.Add
                        (
                            Expression.Lambda
                            (
                                typeof(Func<,>).MakeGenericType([param.Type, typeof(object)]),
                                Expression.Call
                                (
                                    typeof(Enumerable),
                                    "Select",
                                    [TypeExtensions.GetUnderlyingElementType(selector), typeof(object)],
                                    selector,
                                    childSelector
                                ),
                                param
                            )
                        );
                    });
                }
                else if (LogicBuilder.Expressions.Utils.TypeExtensions.IsList(memberType) == false)
                {
                    Build(memberSelectors, param, selector);
                }
            }
        }

        private static string GetChildParameterName(string currentParameterName)
        {
            string lastChar = currentParameterName.Substring(currentParameterName.Length - 1);
            if (short.TryParse(lastChar, out short lastCharShort))
            {
                return string.Concat
                (
                    currentParameterName.Substring(0, currentParameterName.Length - 1),
                    (lastCharShort++).ToString(CultureInfo.CurrentCulture)
                );
            }
            else
            {
                return currentParameterName += "0";
            }
        }

        private static MemberInfo[] GetMemberInfos(Type parentType)
               => parentType.GetMembers(instanceBindingFlags).Select
               (
                   member =>
                   {
                       if (member.DeclaringType != parentType)
                           return member.DeclaringType.GetMember(member.Name, instanceBindingFlags).FirstOrDefault();

                       return member;
                   }
               ).ToArray();

        private HashSet<string> GetNavigationProperties(Type type)
        {
            IEdmEntityType entityType = context.Model.SchemaElements.OfType<IEdmEntityType>()
                .SingleOrDefault(e => GetClrTypeFromEntityType(e).FullName == type.FullName);
            if (entityType != null)
                return entityType.NavigationProperties().Select(GetNavigationPropertyName).ToHashSet();

            IEdmComplexType complexType = context.Model.SchemaElements.OfType<IEdmComplexType>()
                .SingleOrDefault(e => GetClrTypeFromComplexType(e).FullName == type.FullName);
            if (complexType != null)
                return complexType.NavigationProperties().Select(GetNavigationPropertyName).ToHashSet();

            return [];

            Type GetClrTypeFromEntityType(IEdmEntityType entityType)
                => TypeExtensions.GetClrType(new EdmEntityTypeReference(entityType, true), context.Model, TypeExtensions.GetEdmToClrTypeMappings());

            Type GetClrTypeFromComplexType(IEdmComplexType complexType)
                => TypeExtensions.GetClrType(new EdmComplexTypeReference(complexType, true), context.Model, TypeExtensions.GetEdmToClrTypeMappings());

            // Look up the name of the corresponding C# property, which may differ from the property name
            // used in the EDM model, e.g. because the EnableLowerCamelCase option is being used.
            string GetNavigationPropertyName(IEdmNavigationProperty prop)
            {
                var annotation = context.Model.GetAnnotationValue<ClrPropertyInfoAnnotation>(prop);
                return annotation?.ClrPropertyInfo?.Name ?? prop.Name;
            }
        }
    }
}
