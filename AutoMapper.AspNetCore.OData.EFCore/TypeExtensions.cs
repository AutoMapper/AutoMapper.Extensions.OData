using LogicBuilder.Expressions.Utils;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AutoMapper.AspNet.OData
{
    internal class OrderBySetting
    {
        public string Name { get; set; }
        public OrderBySetting ThenBy { get; set; }
    }

    internal static class TypeExtensions
    {
        public static OrderBySetting FindSortableProperties(this Type type, ODataQueryContext context)
        {
            if (context.ElementType is IEdmEntityType parent)
            {                
                if (parent.FullName().Equals(type.FullName, StringComparison.Ordinal))                
                    return FindProperties(parent);
                
                var child = FindEntity(type, parent);
                if (child is not null)           
                    return FindProperties(child);                           
            }
            return null;

            static IEdmEntityType FindEntity(Type type, IEdmEntityType declaringType)
            {
                var props = declaringType.DeclaredProperties
                    .Where(p => p.Type.Definition is IEdmCollectionType)
                    .Select(p => (IEdmCollectionType)p.Type.Definition)
                    .Where(p => p.ElementType.Definition is IEdmEntityType)
                    .Select(p => (IEdmEntityType)p.ElementType.Definition)
                    .Distinct();

                if (props.Any())
                {
                    var found = props.FirstOrDefault(p => 
                        p.FullName().Equals(type.FullName, StringComparison.Ordinal));

                    if (found is not null)
                        return found;
                    
                    foreach (var prop in props)
                    {
                        return FindEntity(type, prop);
                    }
                }
                return null;
            }

            static OrderBySetting FindProperties(IEdmEntityType entity)
            {
                var properties = entity.Key().Any() switch
                {
                    true => entity.Key().Select(k => k.Name),
                    false => entity.StructuralProperties()
                        .Where(p => p.Type.IsPrimitive() && !p.Type.IsStream())
                        .Select(p => p.Name)
                        .OrderBy(n => n)
                        .Take(1)
                };
                var orderBySettings = new OrderBySetting();
                properties.Aggregate(orderBySettings, (settings, prop) =>
                {
                    if (settings.Name is null)
                    {
                        settings.Name = prop;
                        return settings;
                    }
                    settings.ThenBy = new() { Name = prop };
                    return settings.ThenBy;
                });
                return orderBySettings.Name is null ? null : orderBySettings;
            }            
                            
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

        private static IList<Type> _loadedTypes = null;
        public static IList<Type> LoadedTypes
        {
            get
            {
                if (_loadedTypes == null)
                    _loadedTypes = GetAllTypes(AppDomain.CurrentDomain.GetAssemblies().Distinct().ToList());

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

    }
}
