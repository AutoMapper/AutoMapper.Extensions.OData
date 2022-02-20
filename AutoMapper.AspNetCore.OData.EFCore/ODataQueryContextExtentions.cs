using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using System;
using System.Linq;

namespace AutoMapper.AspNet.OData
{
    internal static class ODataQueryContextExtentions
    {
        public static OrderBySetting FindSortableProperties(this ODataQueryContext context, Type type)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

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
    }
}
