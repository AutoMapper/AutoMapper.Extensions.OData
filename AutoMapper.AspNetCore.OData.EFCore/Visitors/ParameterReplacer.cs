using System.Linq.Expressions;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly Expression _target;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="source"><see cref="ParameterExpression"/>.</param>
        /// <param name="target"><see cref="Expression"/>.</param>
        public ParameterReplacer(ParameterExpression source, Expression target)
        {
            _source = source;
            _target = target;
        }

        /// <inheritdoc cref="VisitParameter(ParameterExpression)"/>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _source ? _target : base.VisitParameter(node);
        }
    }    
}
