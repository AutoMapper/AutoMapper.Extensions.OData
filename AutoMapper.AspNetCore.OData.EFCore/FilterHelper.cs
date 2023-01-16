using LogicBuilder.Expressions.Utils;
using LogicBuilder.Expressions.Utils.ExpressionBuilder;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Arithmetic;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Cacnonical;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Conversions;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.DateTimeOperators;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Lambda;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Logical;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Operand;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.StringOperators;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace AutoMapper.AspNet.OData
{
    public class FilterHelper
    {
        public FilterHelper(IDictionary<string, ParameterExpression> parameters, Type underlyingElementType, ODataQueryContext context)
        {
            this.parameters = parameters;
            this.edmModel = context.Model;
        }

        private const string DollarThis = "$this";
        private const string DollarIt = "$it";

        private readonly IDictionary<string, ParameterExpression> parameters;
        private static readonly IDictionary<EdmTypeStructure, Type> typesCache = TypeExtensions.GetEdmToClrTypeMappings();
        private readonly IEdmModel edmModel;

        public IExpressionPart GetFilterPart(QueryNode queryNode)
            => queryNode switch
            {
                SingleValueNode singleValueNode => GetFilterPart(singleValueNode),
                CollectionNode collectionNode => GetFilterPart(collectionNode),
                _ => throw new ArgumentException(nameof(queryNode)),
            };

        public IExpressionPart GetFilterPart(SingleValueNode singleValueNode)
        {
            return singleValueNode.Kind switch
            {
                QueryNodeKind.All => GetAllNodeFilterPart((AllNode)singleValueNode),
                QueryNodeKind.Any => GetAnyNodeFilterPart((AnyNode)singleValueNode),
                QueryNodeKind.BinaryOperator => GetBinaryOperatorFilterPart((BinaryOperatorNode)singleValueNode),
                QueryNodeKind.Constant => GetConstantOperandFilterPart((ConstantNode)singleValueNode),
                QueryNodeKind.Convert => GetConvertOperandFilterPart((ConvertNode)singleValueNode),
                QueryNodeKind.In => GetInFilterPart((InNode)singleValueNode),
                QueryNodeKind.NonResourceRangeVariableReference => GetNonResourceRangeVariableReferenceNodeFilterPart((NonResourceRangeVariableReferenceNode)singleValueNode),
                QueryNodeKind.ResourceRangeVariableReference => GetResourceRangeVariableReferenceNodeFilterPart((ResourceRangeVariableReferenceNode)singleValueNode),
                QueryNodeKind.SingleComplexNode => GetSingleComplexNodeFilterPart((SingleComplexNode)singleValueNode),
                QueryNodeKind.SingleNavigationNode => GetSingleNavigationNodeFilterPart((SingleNavigationNode)singleValueNode),
                QueryNodeKind.SingleResourceCast => GetSingleResourceCastFilterPart((SingleResourceCastNode)singleValueNode),
                QueryNodeKind.SingleResourceFunctionCall => GetSingleResourceFunctionCallNodeFilterPart((SingleResourceFunctionCallNode)singleValueNode),
                QueryNodeKind.SingleValueFunctionCall => GetSingleValueFunctionCallNodeFilterPart((SingleValueFunctionCallNode)singleValueNode),
                QueryNodeKind.SingleValueOpenPropertyAccess => GetSingleValueOpenPropertyAccessFilterPart((SingleValueOpenPropertyAccessNode)singleValueNode),
                QueryNodeKind.SingleValuePropertyAccess => GetSingleValuePropertyAccessFilterPart((SingleValuePropertyAccessNode)singleValueNode),
                QueryNodeKind.UnaryOperator => GetUnaryOperatorNodeFilterPart((UnaryOperatorNode)singleValueNode),
                _ => throw new ArgumentException($"Unsupported {singleValueNode.Kind.GetType().Name} value: {singleValueNode.Kind}"),
            };
        }

        private IExpressionPart GetSingleValueOpenPropertyAccessFilterPart(SingleValueOpenPropertyAccessNode singleValueNode)
        {
            throw new NotImplementedException();
        }

        private IExpressionPart GetSingleComplexNodeFilterPart(SingleComplexNode singleComplexNode)
        {
            string propertyName = singleComplexNode.Property.Name;

            return DoGet
            (
                GetClrType(singleComplexNode.Source.TypeReference).GetMemberInfo(propertyName).GetMemberType(),
                GetClrType(singleComplexNode.TypeReference)
            );

            IExpressionPart DoGet(Type memberType, Type fromEdmType)
            {
                if (ShouldConvertTypes(memberType, fromEdmType, singleComplexNode))
                {
                    return ConvertNonStandardTypes
                    (
                        memberType,
                        fromEdmType,
                        new MemberSelectorOperator
                        (
                            propertyName,
                            GetFilterPart(singleComplexNode.Source)
                        )
                    );
                }

                return new MemberSelectorOperator
                (
                    propertyName,
                    GetFilterPart(singleComplexNode.Source)
                );
            }
        }

        private IExpressionPart GetSingleResourceCastFilterPart(SingleResourceCastNode singleResourceCastNode)
            => new CastOperator
            (
                GetFilterPart(singleResourceCastNode.Source),
                GetClrType(singleResourceCastNode.TypeReference)
            );

        private IExpressionPart GetNonResourceRangeVariableReferenceNodeFilterPart(NonResourceRangeVariableReferenceNode nonResourceRangeVariableReferenceNode) 
            => new ParameterOperator
            (
                parameters,
                ReplaceDollarThisParameter(nonResourceRangeVariableReferenceNode.RangeVariable.Name)
            );

        private IExpressionPart GetResourceRangeVariableReferenceNodeFilterPart(ResourceRangeVariableReferenceNode resourceRangeVariableReferenceNode) 
            => new ParameterOperator
            (
                parameters,
                ReplaceDollarThisParameter(resourceRangeVariableReferenceNode.RangeVariable.Name)
            );

        private bool IsTrueConstantExpression(SingleValueNode node)
        {
            if (node.Kind != QueryNodeKind.Constant)
                return false;

            return ValueIsTrue((ConstantNode)node);

            bool ValueIsTrue(ConstantNode constantNode)
            {
                if (constantNode.Value == null)
                    return false;

                Type constantType = GetClrType(constantNode.TypeReference).ToNullableUnderlyingType();
                if (constantType != typeof(bool) && constantType != typeof(bool?))
                    return false;

                return (bool)constantNode.Value;
            }
        }

        private IExpressionPart GetAnyNodeFilterPart(AnyNode anyNode)
        {
            if (anyNode.Body == null || IsTrueConstantExpression(anyNode.Body))
                return new AnyOperator(GetFilterPart(anyNode.Source));

            //Creating filter part for method call expression with a filter
            //e.g. $it.Property.ChildCollection.Any(c => c.Active);
            return new AnyOperator
            (
                parameters,
                GetFilterPart(anyNode.Source), //source =$it.Property.ChildCollection
                GetFilterPart(anyNode.Body), //body = c.Active.
                anyNode.CurrentRangeVariable.Name//current range variable name is c
            );
        }

        private IExpressionPart GetAllNodeFilterPart(AllNode allNode)
        {
            if (allNode.Body == null || IsTrueConstantExpression(allNode.Body))
                return new AllOperator(GetFilterPart(allNode.Source));

            //Creating filter part for method call expression with a filter
            //e.g. $it.Property.ChildCollection.Any(c => c.Active);
            return new AllOperator
            (
                parameters,
                GetFilterPart(allNode.Source), //source =$it.Property.ChildCollection
                GetFilterPart(allNode.Body), //body = c.Active.
                allNode.CurrentRangeVariable.Name//current range variable name is c
            );
        }

        private IExpressionPart GetSingleResourceFunctionCallNodeFilterPart(SingleResourceFunctionCallNode singleResourceFunctionCallNode)
        {
            return GetFunctionCallFilterPart(singleResourceFunctionCallNode.Parameters.ToList());

            IExpressionPart GetFunctionCallFilterPart(List<QueryNode> arguments)
            {
                return singleResourceFunctionCallNode.Name switch
                {
                    "cast" => GetCastResourceFilterPart(arguments),
                    _ => throw new ArgumentException($"Unsupported SingleResourceFunctionCall value: {singleResourceFunctionCallNode.Name}"),
                };
            }
        }

        private IExpressionPart GetSingleValueFunctionCallNodeFilterPart(SingleValueFunctionCallNode singleValueFunctionCallNode)
        {
            return GetFunctionCallFilterPart(singleValueFunctionCallNode.Parameters.ToList());

            IExpressionPart GetFunctionCallFilterPart(List<QueryNode> arguments)
                => singleValueFunctionCallNode.Name switch
                {
                    "cast" => GetCastFilterPart(arguments),
                    "ceiling" => new CeilingOperator(GetFilterPart(arguments[0])),
                    "concat" => new ConcatOperator(GetFilterPart(arguments[0]), GetFilterPart(arguments[1])),
                    "contains" => new ContainsOperator(GetFilterPart(arguments[0]), GetFilterPart(arguments[1])),
                    "date" => GetFilterPart(arguments[0]), //new DateOperator(parameters, GetFilterPart(arguments[0])),
                                                           //EF does not support Date/TimeOfDay selectors
                    "day" => new DayOperator(GetFilterPart(arguments[0])),
                    "endswith" => new EndsWithOperator(GetFilterPart(arguments[0]), GetFilterPart(arguments[1])),
                    "floor" => new FloorOperator(GetFilterPart(arguments[0])),
                    "fractionalseconds" => new FractionalSecondsOperator(GetFilterPart(arguments[0])),
                    "hour" => new HourOperator(GetFilterPart(arguments[0])),
                    "indexof" => new IndexOfOperator(GetFilterPart(arguments[0]), GetFilterPart(arguments[1])),
                    "isof" => GetIsOdFilterPart(arguments),
                    "length" => new LengthOperator(GetFilterPart(arguments[0])),
                    "minute" => new MinuteOperator(GetFilterPart(arguments[0])),
                    "month" => new MonthOperator(GetFilterPart(arguments[0])),
                    "now" => new NowDateTimeOperator(),
                    "round" => new RoundOperator(GetFilterPart(arguments[0])),
                    "second" => new SecondOperator(GetFilterPart(arguments[0])),
                    "startswith" => new StartsWithOperator(GetFilterPart(arguments[0]), GetFilterPart(arguments[1])),
                    "substring" => new SubstringOperator
                    (
                        GetFilterPart(arguments[0]),//initial string
                        arguments.Skip(1).Select(arg => GetFilterPart(arg)).ToArray()//starting index or (starting and ending indexes)
                    ),
                    "time" => GetFilterPart(arguments[0]), //new TimeOperator(parameters, GetFilterPart(arguments[0])),
                                                           //EF does not support Date/TimeOfDay selectors
                    "tolower" => new ToLowerOperator(GetFilterPart(arguments[0])),
                    "toupper" => new ToUpperOperator(GetFilterPart(arguments[0])),
                    "trim" => new TrimOperator(GetFilterPart(arguments[0])),
                    "year" => new YearOperator(GetFilterPart(arguments[0])),
                    _ => GetCustomMehodFilterPart(singleValueFunctionCallNode.Name, arguments.OfType<SingleValueNode>().ToArray()),
                };
        }

        private IExpressionPart GetIsOdFilterPart(List<QueryNode> arguments)
        {
            if (!(arguments[0] is SingleValueNode sourceNode))
                throw new ArgumentException("Expected SingleValueNode for source node.");

            if (!(arguments[1] is ConstantNode typeNode))
                throw new ArgumentException("Expected ConstantNode for type node.");

            return IsOf(GetCastType(typeNode));

            IExpressionPart IsOf(Type conversionType)
                => new IsOfOperator(GetFilterPart(sourceNode), conversionType);
        }

        private IExpressionPart GetCastResourceFilterPart(List<QueryNode> arguments)
        {
            if (!(arguments[0] is SingleValueNode sourceNode))
                throw new ArgumentException("Expected SingleValueNode for source node.");

            if (!(arguments[1] is ConstantNode typeNode))
                throw new ArgumentException("Expected ConstantNode for type node.");

            return Convert
            (
                GetClrType(sourceNode.TypeReference),
                GetCastType(typeNode)
            );

            IExpressionPart Convert(Type operandType, Type conversionType)
            {
                if (OperandIsNullConstant(sourceNode) || operandType == conversionType)
                    return GetFilterPart(sourceNode);

                if (!(operandType.IsAssignableFrom(conversionType) || conversionType.IsAssignableFrom(operandType)))
                    return new ConstantOperator(null);

                if (ShouldConvertTypes(operandType, conversionType, sourceNode))
                {

                    return new CastOperator
                    (
                        GetFilterPart(sourceNode),
                        conversionType
                    );
                }

                return GetFilterPart(sourceNode);
            }
        }

        private IExpressionPart GetCastFilterPart(List<QueryNode> arguments)
        {
            if (!(arguments[0] is SingleValueNode sourceNode))
                throw new ArgumentException("Expected SingleValueNode for source node.");

            if (!(arguments[1] is ConstantNode typeNode))
                throw new ArgumentException("Expected ConstantNode for type node.");

            return Convert
            (
                GetClrType(sourceNode.TypeReference),
                GetCastType(typeNode)
            );

            IExpressionPart Convert(Type operandType, Type conversionType)
            {
                if (OperandIsNullConstant(sourceNode) || operandType == conversionType)
                    return GetFilterPart(sourceNode);

                if (ShouldConvertTypes(operandType, conversionType, sourceNode))
                {
                    if ((!typeNode.TypeReference.IsPrimitive() && !typeNode.TypeReference.IsEnum())
                        || (!operandType.IsLiteralType() && !operandType.ToNullableUnderlyingType().IsEnum))
                        return new ConstantOperator(null);

                    if (conversionType == typeof(string))
                        return new ConvertToStringOperator(GetFilterPart(sourceNode));

                    if (conversionType.IsEnum)
                    {
                        if (!(sourceNode is ConstantNode enumSourceNode))
                        {
                            if (GetClrType(sourceNode.TypeReference) == typeof(string))
                                return new ConstantOperator(null);

                            throw new ArgumentException("Expected ConstantNode for enum source node.");
                        }

                        return new ConvertToEnumOperator
                        (
                            GetConstantNodeValue(enumSourceNode, conversionType),
                            conversionType
                        );
                    }

                    return new ConvertOperator
                    (
                        GetFilterPart(sourceNode),
                        conversionType
                    );
                }

                return GetFilterPart(sourceNode);
            }
        }

        private Type GetCastType(ConstantNode constantNode)
            => TypeExtensions.GetClrType((string)constantNode.Value, false, typesCache);

        private IExpressionPart GetCustomMehodFilterPart(string functionName, SingleValueNode[] arguments)
        {
            MethodInfo methodInfo = CustomMethodCache.GetCachedCustomMethod(functionName, arguments.Select(p => GetClrType(p.TypeReference)));
            if (methodInfo == null)
                throw new NotImplementedException($"Unsupported SingleValueFunctionCall name - value: {functionName}");

            return new CustomMethodOperator
            (
                methodInfo,
                arguments.Select(arg => GetFilterPart(arg)).ToArray()
            );
        }

        private IExpressionPart GetUnaryOperatorNodeFilterPart(UnaryOperatorNode unaryOperatorNode)
            => unaryOperatorNode.OperatorKind switch
            {
                UnaryOperatorKind.Negate => new NegateOperator(GetFilterPart(unaryOperatorNode.Operand)),
                UnaryOperatorKind.Not => new NotOperator(GetFilterPart(unaryOperatorNode.Operand)),
                _ => throw new ArgumentException($"Unsupported {unaryOperatorNode.OperatorKind.GetType().Name} value: {unaryOperatorNode.OperatorKind}"),
            };

        private IExpressionPart GetConvertOperandFilterPart(ConvertNode covertNode)
        {
            return Convert
            (
                covertNode.Source.TypeReference == null
                ? typeof(object)
                : GetClrType(covertNode.Source.TypeReference),
                GetClrType(covertNode.TypeReference)
            );


            IExpressionPart Convert(Type operandType, Type conversionType)
            {
                if (ShouldConvertTypes(operandType, conversionType, covertNode.Source))
                {
                    return new ConvertOperator
                    (
                        GetFilterPart(covertNode.Source),
                        conversionType
                    );
                }

                return GetFilterPart(covertNode.Source);
            }
        }

        private static bool ShouldConvertTypes(Type original, Type conversion, SingleValueNode operand)
                => original != conversion
                    && !ConvertingUnderlyingTypeToNullable(original, conversion)
                    && !BothTypesDateTimeRelated(original, conversion)
                    && !OperandIsNullConstant(operand);

        private static bool ConvertingUnderlyingTypeToNullable(Type original, Type conversion)
            => conversion.IsNullableType() && Nullable.GetUnderlyingType(conversion) == original;

        private static bool BothTypesDateRelated(Type leftType, Type rightType)
        {
            leftType = leftType.ToNullableUnderlyingType();
            rightType = rightType.ToNullableUnderlyingType();

            return Constants.DateRelatedTypes.Contains(leftType) && Constants.DateRelatedTypes.Contains(rightType);
        }

        private static bool BothTypesDateTimeRelated(Type original, Type conversion)
        {
            original = original.ToNullableUnderlyingType();
            conversion = conversion.ToNullableUnderlyingType();

            return Constants.DateTimeRelatedTypes.Contains(original) && Constants.DateTimeRelatedTypes.Contains(conversion);
        }

        private static bool OperandIsNullConstant(QueryNode operand)
        {
            if (operand is ConvertNode converMode)
                return OperandIsNullConstant(converMode.Source);

            if (operand is SingleValueFunctionCallNode singleValueFunctionCallNode
                && singleValueFunctionCallNode.Name == "cast")
                return OperandIsNullConstant(singleValueFunctionCallNode.Parameters.First());

            return operand.Kind == QueryNodeKind.Constant && ((ConstantNode)operand).Value == null;
        }

        public IExpressionPart GetFilterPart(CollectionNode collectionNode)
        {
            return collectionNode.Kind switch
            {
                QueryNodeKind.CollectionConstant => GetCollectionConstantFilterPart((CollectionConstantNode)collectionNode),
                QueryNodeKind.CollectionNavigationNode => GetCollectionNavigationNodeFilterPart((CollectionNavigationNode)collectionNode),
                QueryNodeKind.CollectionPropertyAccess => GetCollectionPropertyAccessNodeFilterPart((CollectionPropertyAccessNode)collectionNode),
                QueryNodeKind.CollectionComplexNode => GetCollectionComplexNodeFilterPart((CollectionComplexNode)collectionNode),
                QueryNodeKind.CollectionResourceCast => GetCollectionResourceCastFilterPart((CollectionResourceCastNode)collectionNode),
                _ => throw new ArgumentException($"Unsupported {collectionNode.Kind.GetType().Name} value: {collectionNode.Kind}"),
            };
        }

        private IExpressionPart GetCollectionResourceCastFilterPart(CollectionResourceCastNode collectionResourceCastNode)
            => new CollectionCastOperator
            (
                GetFilterPart(collectionResourceCastNode.Source),
                GetClrType(collectionResourceCastNode.ItemType)
            );

        private IExpressionPart GetCollectionComplexNodeFilterPart(CollectionComplexNode collectionComplexNode)
            => new MemberSelectorOperator
            (
                collectionComplexNode.Property.Name,
                GetFilterPart(collectionComplexNode.Source)
            );

        private IExpressionPart GetCollectionPropertyAccessNodeFilterPart(CollectionPropertyAccessNode collectionPropertyAccessNode)
            => new MemberSelectorOperator
            (
                collectionPropertyAccessNode.Property.Name,
                GetFilterPart(collectionPropertyAccessNode.Source)
            );

        private IExpressionPart GetCollectionNavigationNodeFilterPart(CollectionNavigationNode collectionNavigationNode)
            => new MemberSelectorOperator
            (
                collectionNavigationNode.NavigationProperty.Name,
                GetFilterPart(collectionNavigationNode.Source)
            );

        private IExpressionPart GetCollectionConstantFilterPart(CollectionConstantNode collectionNode)
        {
            Type elemenType = GetClrType(collectionNode.ItemType);

            return new CollectionConstantOperator
            (
                GetCollectionParameter(elemenType.ToNullableUnderlyingType()),
                elemenType
            );

            ICollection<object> GetCollectionParameter(Type underlyingType)
            {
                if (!underlyingType.IsEnum)
                    return collectionNode.Collection.Select(item => item.Value).ToList();

                return collectionNode.Collection.Select
                (
                    item => GetConstantNodeValue(item, underlyingType)
                ).ToList();
            }
        }

        private static object GetConstantNodeValue(ConstantNode constantNode, Type enumType)
            => constantNode.Value is ODataEnumValue oDataEnum
                ? GetEnumValue(oDataEnum, enumType)
                : constantNode.Value;

        private static object GetEnumValue(ODataEnumValue oDataEnum, Type enumType)
                => !(oDataEnum.Value ?? "").TryParseEnum(enumType, out object result) ? null : result;

        private Type GetClrType(IEdmTypeReference typeReference)
            => this.edmModel == null
                ? TypeExtensions.GetClrType(typeReference, typesCache)
                : TypeExtensions.GetClrType(typeReference, edmModel, typesCache);

        private IExpressionPart GetSingleValuePropertyAccessFilterPart(SingleValuePropertyAccessNode singleValuePropertyAccesNode)
        {
            Type parentType = GetClrType(singleValuePropertyAccesNode.Source.TypeReference);
            string propertyName = singleValuePropertyAccesNode.Property.Name;

            return DoGet
            (
                parentType.GetMemberInfo(propertyName).GetMemberType(),
                GetClrType(singleValuePropertyAccesNode.TypeReference)
            );

            IExpressionPart DoGet(Type memberType, Type fromEdmType)
            {
                if (ShouldConvertTypes(memberType, fromEdmType, singleValuePropertyAccesNode))
                {
                    return ConvertNonStandardTypes
                    (
                        memberType,
                        fromEdmType,
                        new MemberSelectorOperator
                        (
                            propertyName,
                            GetFilterPart(singleValuePropertyAccesNode.Source)
                        )
                    );
                }

                return new MemberSelectorOperator
                (
                    propertyName,
                    GetFilterPart(singleValuePropertyAccesNode.Source)
                );
            }
        }

        private IExpressionPart GetSingleNavigationNodeFilterPart(SingleNavigationNode singleNavigationNode)
            => new MemberSelectorOperator
            (
                singleNavigationNode.NavigationProperty.Name,
                GetFilterPart(singleNavigationNode.Source)
            );

        private IExpressionPart ConvertNonStandardTypes(Type sourceType, Type fromEdmType, IExpressionPart sourceFilterPart)
        {
            return DoConvert(sourceType.ToNullableUnderlyingType());
            IExpressionPart DoConvert(Type sourceUnderlyingType)
            {
                if (fromEdmType == typeof(string))
                {
                    return sourceUnderlyingType.FullName switch
                    {
                        "System.Char[]" => new ConvertCharArrayToStringOperator(GetSourceFilterPart(sourceUnderlyingType)),
                        _ => new ConvertToStringOperator(GetSourceFilterPart(sourceUnderlyingType)),
                    };
                }

                return new ConvertOperator
                (
                    GetSourceFilterPart(sourceUnderlyingType),
                    fromEdmType
                );
            }

            IExpressionPart GetSourceFilterPart(Type sourceUnderlyingType)
            {
                switch (sourceUnderlyingType.FullName)
                {
                    case "System.UInt16":
                    case "System.UInt32":
                    case "System.UInt64":
                    case "System.Char":
                        return sourceUnderlyingType == sourceType
                                ? sourceFilterPart
                                : new ConvertToNullableUnderlyingValueOperator(sourceFilterPart);
                    default:
                        return sourceFilterPart;
                }
            }
        }

        public IExpressionPart GetBinaryOperatorFilterPart(BinaryOperatorNode binaryOperatorNode)
        {
            var left = GetFilterPart(binaryOperatorNode.Left);
            var right = GetFilterPart(binaryOperatorNode.Right);

            if (ShouldConvertToNumericDate(binaryOperatorNode))
            {
                left = new ConvertToNumericDateOperator(left);
                right = new ConvertToNumericDateOperator(right);
            }
            else if (ShouldConvertToNumericTime(binaryOperatorNode))
            {
                left = new ConvertToNumericTimeOperator(left);
                right = new ConvertToNumericTimeOperator(right);
            }

            switch (binaryOperatorNode.OperatorKind)
            {
                case BinaryOperatorKind.Or:
                    return new OrBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.And:
                    return new AndBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.Equal:
                    return new EqualsBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.NotEqual:
                    return new NotEqualsBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.GreaterThan:
                    return new GreaterThanBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.GreaterThanOrEqual:
                    return new GreaterThanOrEqualsBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.LessThan:
                    return new LessThanBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.LessThanOrEqual:
                    return new LessThanOrEqualsBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.Add:
                    return new AddBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.Subtract:
                    return new SubtractBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.Multiply:
                    return new MultiplyBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.Divide:
                    return new DivideBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.Modulo:
                    return new ModuloBinaryOperator
                    (
                        left,
                        right
                    );
                case BinaryOperatorKind.Has:
                    if (binaryOperatorNode.Right is ConstantNode constantNode
                        && constantNode.Value is Microsoft.OData.ODataEnumValue oDataEnum)
                    {
                        return new HasOperator
                        (
                            left,
                            new ConstantOperator
                            (
                                oDataEnum.Value,
                                typeof(string)
                            )
                        );
                    }

                    throw new ArgumentException($"Unsupported RHS {binaryOperatorNode.Right.Kind.GetType().Name} operand type for  BinaryOperatorKind.Has. Value: {binaryOperatorNode.Right.Kind}");
                default:
                    throw new ArgumentException($"Unsupported {binaryOperatorNode.OperatorKind.GetType().Name} value: {binaryOperatorNode.OperatorKind}");
            }
        }

        private bool ShouldConvertToNumericDate(BinaryOperatorNode binaryOperatorNode)
        {
            if (OperandIsNullConstant(binaryOperatorNode.Left) || OperandIsNullConstant(binaryOperatorNode.Right))
                return false;

            return ShouldConvert
            (
                GetClrType(binaryOperatorNode.Left.TypeReference).ToNullableUnderlyingType(),
                GetClrType(binaryOperatorNode.Right.TypeReference).ToNullableUnderlyingType()
            );

            static bool ShouldConvert(Type leftType, Type rightType)
                => BothTypesDateRelated(leftType, rightType)
#if NET6_0
                    && (
                            leftType == typeof(Date)
                            || rightType == typeof(Date)
                            || leftType == typeof(DateOnly)
                            || rightType == typeof(DateOnly)
                       );
#else
                    && (leftType == typeof(Date) || rightType == typeof(Date));
#endif
        }

        private bool ShouldConvertToNumericTime(BinaryOperatorNode binaryOperatorNode)
        {
            if (OperandIsNullConstant(binaryOperatorNode.Left) || OperandIsNullConstant(binaryOperatorNode.Right))
                return false;

            return ShouldConvert
            (
                GetClrType(binaryOperatorNode.Left.TypeReference).ToNullableUnderlyingType(),
                GetClrType(binaryOperatorNode.Right.TypeReference).ToNullableUnderlyingType()
            );

            static bool ShouldConvert(Type leftType, Type rightType)
                => BothTypesDateTimeRelated(leftType, rightType)
#if NET6_0
                    && (
                            leftType == typeof(TimeOfDay)
                            || rightType == typeof(TimeOfDay)
                            || leftType == typeof(TimeOnly)
                            || rightType == typeof(TimeOnly)
                       );
#else
                    && (leftType == typeof(TimeOfDay) || rightType == typeof(TimeOfDay));
#endif
        }

        public IExpressionPart GetConstantOperandFilterPart(ConstantNode constantNode)
        {
            return GetFilterPart(constantNode.Value == null ? typeof(object) : GetClrType(constantNode.TypeReference));

            IExpressionPart GetFilterPart(Type constantType)
                => new ConstantOperator
                (
                    GetConstantNodeValue(constantNode, constantType),
                    constantType
                );
        }

        public IExpressionPart GetInFilterPart(InNode inNode)
            => new InOperator
            (
                GetFilterPart(inNode.Left),
                GetFilterPart(inNode.Right)
            );

        private static string ReplaceDollarThisParameter(string rangeVariableName) =>
           rangeVariableName == DollarThis ? DollarIt : rangeVariableName;
    }
}
