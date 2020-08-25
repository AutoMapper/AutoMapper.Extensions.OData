using LogicBuilder.Expressions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal abstract class ProjectionVisitor : ExpressionVisitor
    {
        public ProjectionVisitor(List<Expansion> expansions)
        {
            this.expansions = expansions;
        }

        protected readonly List<Expansion> expansions;
        private readonly List<Expression> foundExpansions = new List<Expression>();

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            Expansion expansion = expansions.First();

            if (node.NewExpression.Type != expansion.ParentType)
                return base.VisitMemberInit(node);

            return Expression.MemberInit
            (
                Expression.New(node.Type),
                node.Bindings.OfType<MemberAssignment>().Aggregate
                (
                    new List<MemberBinding>(),
                    AddBinding
                )
            );

            List<MemberBinding> AddBinding(List<MemberBinding> list, MemberAssignment binding)
            {
                if (ListTypesAreEquivalent(binding.Member.GetMemberType(), expansion.MemberType)
                        && binding.Member.Name == expansion.MemberName)//found the expansion
                {
                    if (foundExpansions.Count > 0)
                        throw new NotSupportedException("Recursive queries not supported");

                    AddBindingExpression(GetBindingExpression(binding, expansion));
                }
                else
                {
                    list.Add(binding);
                }

                return list;

                void AddBindingExpression(Expression bindingExpression)
                {
                    list.Add(Expression.Bind(binding.Member, bindingExpression));
                    foundExpansions.Add(bindingExpression);
                }
            }
        }

        protected abstract Expression GetBindingExpression(MemberAssignment binding, Expansion expansion);

        protected static bool ListTypesAreEquivalent(Type bindingType, Type expansionType)
        {
            if (!bindingType.IsList() || !expansionType.IsList())
                return false;

            return bindingType.GetUnderlyingElementType() == expansionType.GetUnderlyingElementType();
        }
    }
}
