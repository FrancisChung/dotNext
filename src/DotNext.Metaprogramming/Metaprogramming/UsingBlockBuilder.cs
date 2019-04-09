﻿using System;
using MethodInfo = System.Reflection.MethodInfo;
using System.Linq;
using System.Linq.Expressions;

namespace DotNext.Metaprogramming
{
    using static Reflection.DisposableType;

    /// <summary>
    /// Represents <see langword="using"/> statement builder.
    /// </summary>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement">USING statement</see>
    public sealed class UsingBlockBuilder: ScopeBuilder, IExpressionBuilder<Expression>
    {
        private readonly MethodInfo disposeMethod;
        private readonly ParameterExpression disposableVar;
        private readonly BinaryExpression assignment;

        internal UsingBlockBuilder(Expression expression, CompoundStatementBuilder parent)
            : base(parent)
        {
            disposeMethod = expression.Type.GetDisposeMethod();
            if (disposeMethod is null)
                throw new ArgumentNullException(ExceptionMessages.DisposePatternExpected(expression.Type));
            else if (expression is ParameterExpression variable)
                disposableVar = variable;
            else
            {
                disposableVar = Expression.Variable(expression.Type, NextName("disposable_"));
                assignment = disposableVar.Assign(expression);
            }
        }

        /// <summary>
        /// Gets disposable resource attached to this statement.
        /// </summary>
        public UniversalExpression DisposableVar => disposableVar;

        internal override Expression Build()
        {
            Expression @finally = disposableVar.Call(disposeMethod);
            @finally = Expression.Block(typeof(void), @finally, disposableVar.AssignDefault());
            @finally = base.Build().Finally(@finally);
            return assignment is null ?
                @finally :
                Expression.Block(typeof(void), Sequence.Singleton(disposableVar), assignment, @finally);
        }

        Expression IExpressionBuilder<Expression>.Build() => Build();
    }
}