using Microsoft.OData.Edm;
using System;

namespace AutoMapper.AspNet.OData
{
    public struct EdmTypeStructure : IEquatable<EdmTypeStructure>
    {
        public EdmTypeStructure(IEdmTypeReference edmTypeReference)
        {
            FullName = edmTypeReference.FullName();
            IsNullable = edmTypeReference.IsNullable;
        }

        public EdmTypeStructure(string fullName, bool isNullable)
        {
            FullName = fullName;
            IsNullable = isNullable;
        }

        public string FullName { get; }
        public bool IsNullable { get; }

        public bool Equals(EdmTypeStructure other)
            => this.FullName == other.FullName && this.IsNullable == other.IsNullable;

        public override bool Equals(object obj)
        {
            if (!(obj is EdmTypeStructure typeDefinition)) return false;

            return Equals(typeDefinition);
        }

        public override int GetHashCode() => FullName.GetHashCode();
    }
}
