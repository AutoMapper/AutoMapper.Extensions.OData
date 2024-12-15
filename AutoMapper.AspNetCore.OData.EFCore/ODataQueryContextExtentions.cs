using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.UriParser;

namespace AutoMapper.AspNet.OData
{
    internal static class ODataQueryContextExtentions
    {
        public static OrderBySetting FindSortableProperties(this ODataQueryContext context, Type type,
            OrderByDirection orderByDirection = OrderByDirection.Ascending)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            var entity = GetEntity();
            return entity is not null 
                ? FindProperties(entity, orderByDirection) 
                : throw new InvalidOperationException($"The type '{type.FullName}' has not been declared in the entity data model.");

            IEdmEntityType GetEntity()
            {
                List<IEdmEntityType> entities = context.Model.SchemaElements.OfType<IEdmEntityType>().Where(e => e.Name == type.Name).ToList();
                if (entities.Count == 1)
                    return entities[0];

                if (entities.Count > 1)
                {
                    return entities.SingleOrDefault(e => GetClrType(e, context.Model).FullName == type.FullName);
                }

                return null;
            }

            static Type GetClrType(IEdmEntityType entityType, IEdmModel edmModel)
                => TypeExtensions.GetClrType(new EdmEntityTypeReference(entityType, true), edmModel, TypeExtensions.GetEdmToClrTypeMappings());

            static OrderBySetting FindProperties(IEdmEntityType entity, OrderByDirection orderByDirection)
            {
                var propertyNames = entity.Key().Any() switch
                {
                    true => entity.Key().Select(k => k.Name),
                    false => entity.StructuralProperties()
                        .Where(p => p.Type.IsPrimitive() && !p.Type.IsStream())
                        .Select(p => p.Name)
                        .OrderBy(n => n)
                        .Take(1)
                };
                var orderBySettings = new OrderBySetting();
                propertyNames.Aggregate(orderBySettings, (settings, name) =>
                {
                    if (settings.Name is null)
                    {
                        settings.Name = name;
                        settings.Direction = orderByDirection;
                        return settings;
                    }
                    settings.ThenBy = new() { Name = name, Direction = orderByDirection };
                    return settings.ThenBy;
                });
                return orderBySettings.Name is null ? null : orderBySettings;
            }

        }
    }
}
