using Microsoft.OData.Edm;
using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutoMapper.AspNet.OData
{
    internal static class Constants
    {
        public static HashSet<Type> DateRelatedTypes = new()
        {
            typeof(DateTimeOffset),
            typeof(DateTime),
            typeof(Date),
#if NET6_0
            typeof(DateOnly)
#endif
        };

        public static HashSet<Type> DateTimeRelatedTypes = new()
        {
            typeof(DateTimeOffset),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(TimeOfDay),
#if NET6_0
            typeof(TimeOnly),
            typeof(DateOnly),
#endif
            typeof(Date)
        };

        public static readonly Dictionary<EdmTypeStructure, Type> EdmToClrTypeMappings = new Dictionary<EdmTypeStructure, Type>
        {
            [new EdmTypeStructure("Edm.String", true)] = typeof(string),
            [new EdmTypeStructure("Edm.String", false)] = typeof(string),
            [new EdmTypeStructure("Edm.Boolean", false)] = typeof(bool),
            [new EdmTypeStructure("Edm.Boolean", true)] = typeof(bool?),
            [new EdmTypeStructure("Edm.Byte", false)] = typeof(byte),
            [new EdmTypeStructure("Edm.Byte", true)] = typeof(byte?),
            [new EdmTypeStructure("Edm.Decimal", false)] = typeof(decimal),
            [new EdmTypeStructure("Edm.Decimal", true)] = typeof(decimal?),
            [new EdmTypeStructure("Edm.Double", false)] = typeof(double),
            [new EdmTypeStructure("Edm.Double", true)] = typeof(double?),
            [new EdmTypeStructure("Edm.Guid", false)] = typeof(Guid),
            [new EdmTypeStructure("Edm.Guid", true)] = typeof(Guid?),
            [new EdmTypeStructure("Edm.Int16", false)] = typeof(short),
            [new EdmTypeStructure("Edm.Int16", true)] = typeof(short?),
            [new EdmTypeStructure("Edm.Int32", false)] = typeof(int),
            [new EdmTypeStructure("Edm.Int32", true)] = typeof(int?),
            [new EdmTypeStructure("Edm.Int64", false)] = typeof(long),
            [new EdmTypeStructure("Edm.Int64", true)] = typeof(long?),
            [new EdmTypeStructure("Edm.SByte", false)] = typeof(sbyte),
            [new EdmTypeStructure("Edm.SByte", true)] = typeof(sbyte?),
            [new EdmTypeStructure("Edm.Single", false)] = typeof(float),
            [new EdmTypeStructure("Edm.Single", true)] = typeof(float?),
            [new EdmTypeStructure("Edm.Binary", true)] = typeof(byte[]),
            [new EdmTypeStructure("Edm.Binary", false)] = typeof(byte[]),
            [new EdmTypeStructure("Edm.Stream", true)] = typeof(Stream),
            [new EdmTypeStructure("Edm.Stream", false)] = typeof(Stream),
            [new EdmTypeStructure("Edm.Geography", true)] = typeof(Geography),
            [new EdmTypeStructure("Edm.GeographyPoint", true)] = typeof(GeographyPoint),
            [new EdmTypeStructure("Edm.GeographyLineString", true)] = typeof(GeographyLineString),
            [new EdmTypeStructure("Edm.GeographyPolygon", true)] = typeof(GeographyPolygon),
            [new EdmTypeStructure("Edm.GeographyCollection", true)] = typeof(GeographyCollection),
            [new EdmTypeStructure("Edm.GeographyMultiLineString", true)] = typeof(GeographyMultiLineString),
            [new EdmTypeStructure("Edm.GeographyMultiPoint", true)] = typeof(GeographyMultiPoint),
            [new EdmTypeStructure("Edm.GeographyMultiPolygon", true)] = typeof(GeographyMultiPolygon),
            [new EdmTypeStructure("Edm.Geometry", true)] = typeof(Geometry),
            [new EdmTypeStructure("Edm.GeometryPoint", true)] = typeof(GeometryPoint),
            [new EdmTypeStructure("Edm.GeometryLineString", true)] = typeof(GeometryLineString),
            [new EdmTypeStructure("Edm.GeometryPolygon", true)] = typeof(GeometryPolygon),
            [new EdmTypeStructure("Edm.GeometryCollection", true)] = typeof(GeometryCollection),
            [new EdmTypeStructure("Edm.GeometryMultiLineString", true)] = typeof(GeometryMultiLineString),
            [new EdmTypeStructure("Edm.GeometryMultiPoint", true)] = typeof(GeometryMultiPoint),
            [new EdmTypeStructure("Edm.GeometryMultiPolygon", true)] = typeof(GeometryMultiPolygon),
            [new EdmTypeStructure("Edm.DateTimeOffset", false)] = typeof(DateTimeOffset),
            [new EdmTypeStructure("Edm.DateTimeOffset", true)] = typeof(DateTimeOffset?),
            [new EdmTypeStructure("Edm.Duration", false)] = typeof(TimeSpan),
            [new EdmTypeStructure("Edm.Duration", true)] = typeof(TimeSpan?),
            [new EdmTypeStructure("Edm.Date", false)] = typeof(Date),
            [new EdmTypeStructure("Edm.Date", true)] = typeof(Date?),
            [new EdmTypeStructure("Edm.TimeOfDay", false)] = typeof(TimeOfDay),
            [new EdmTypeStructure("Edm.TimeOfDay", true)] = typeof(TimeOfDay?),
        };
    }
}
