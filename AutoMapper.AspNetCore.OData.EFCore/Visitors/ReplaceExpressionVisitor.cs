using System.Linq.Expressions;

namespace AutoMapper.AspNet.OData.Visitors
{
    internal class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldExpression;
        private readonly Expression _newExpression;

        public ReplaceExpressionVisitor(Expression oldExpression, Expression newExpression)
        {
            _oldExpression = oldExpression;
            _newExpression = newExpression;
        }

        public override Expression Visit(Expression node)
        {
            if (_oldExpression == node)
                return _newExpression;

            return base.Visit(node);
        }
    }
}
