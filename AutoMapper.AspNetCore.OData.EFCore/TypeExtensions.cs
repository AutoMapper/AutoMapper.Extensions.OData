﻿using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.AspNet.OData
{
    internal static class TypeExtensions
    {


        public static Type GetClrType(string fullName, bool isNullable, IDictionary<EdmTypeStructure, Type> typesCache)
            => GetClrType(new EdmTypeStructure(fullName, isNullable), typesCache);

        public static Type GetClrType(IEdmTypeReference edmTypeReference, IDictionary<EdmTypeStructure, Type> typesCache)
            => edmTypeReference == null
                ? typeof(object)
                : GetClrType(new EdmTypeStructure(edmTypeReference), typesCache);

        private static Type GetClrType(EdmTypeStructure edmTypeStructure, IDictionary<EdmTypeStructure, Type> typesCache)
        {
            if (typesCache.TryGetValue(edmTypeStructure, out Type type))
                return type;

            type = LoadedTypes.SingleOrDefault
            (
                item => edmTypeStructure.FullName == item.FullName
            );

            if (type != null)
            {
                if (type.IsValueType && !type.IsNullableType() && edmTypeStructure.IsNullable)
                {
                    type = type.ToNullable();
                    typesCache.Add(edmTypeStructure, type);
                }

                return type;
            }

            throw new ArgumentException($"Cannot find CLT type for EDM type {edmTypeStructure.FullName}");
        }

        public static Type GetClrType(IEdmTypeReference edmTypeReference, IEdmModel edmModel, IDictionary<EdmTypeStructure, Type> typesCache)
        {
            if (edmTypeReference == null)
                return typeof(object);

            return edmModel.GetTypeMapper().GetClrType(edmModel, edmTypeReference, _AssemblyResolver);
        }

        private static IAssemblyResolver _assemblyResolver;
        private static IAssemblyResolver _AssemblyResolver
        {
            get
            {
                _assemblyResolver ??= new AssemblyResolver();

                return _assemblyResolver;
            }
        }

        private static IList<Type> _loadedTypes = null;
        public static IList<Type> LoadedTypes
        {
            get
            {
                _loadedTypes ??= GetAllTypes(AppDomain.CurrentDomain.GetAssemblies().Distinct().ToList());

                return _loadedTypes;
            }
        }

        public static IList<Type> GetAllTypes(List<Assembly> assemblies)
        {
            return DoLoad(new List<Type>());

            List<Type> DoLoad(List<Type> allTypes)
            {
                assemblies.ForEach(assembly =>
                {
                    try
                    {
                        allTypes.AddRange(assembly.GetTypes().Where(type => type.IsPublic && type.IsVisible));
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        allTypes.AddRange
                        (
                            ex.Types.Where(type => type != null && type.IsPublic && type.IsVisible)
                        );
                    }
                });

                return allTypes;
            }
        }

        public static Dictionary<EdmTypeStructure, Type> GetEdmToClrTypeMappings() => Constants.EdmToClrTypeMappings;

        public static Type GetUnderlyingElementType(this Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            if (typeInfo.IsArray)
                return typeInfo.GetElementType();

            if (!type.IsGenericType)
                throw new ArgumentException(nameof(type));

            Type[] genericArguments = type.GetGenericArguments();
            Type genericTypeDefinition = type.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(IGrouping<,>))
                return genericArguments[1];
            else if (IsGenericDictionaryType())
                return typeof(KeyValuePair<,>).MakeGenericType(genericArguments[0], genericArguments[1]);
            else if (genericArguments.Length == 1)
                return genericArguments[0];
            else
                throw new ArgumentException(nameof(type));

            bool IsGenericDictionaryType()
            {
                return (typeof(IDictionary<,>).IsAssignableFrom(genericTypeDefinition))
                    || (type.GetInterface(typeof(System.Collections.IDictionary).FullName ?? "") != null && genericArguments.Length == 2);
            }
        }

        public static Type GetUnderlyingElementType(this Expression expression)
           => GetUnderlyingElementType(expression.Type);

        private class AssemblyResolver : IAssemblyResolver
        {
            private List<Assembly> _assemblides;
            public IEnumerable<Assembly> Assemblies
            {
                get
                {
                    if (_assemblides == null)
                        _assemblides = AppDomain.CurrentDomain.GetAssemblies().Distinct().ToList();

                    return _assemblides;
                }
            }
        }

    }
}
