﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using TaskAwaiter = System.Runtime.CompilerServices.TaskAwaiter;

namespace DotNext.Metaprogramming
{
    using Runtime.CompilerServices;

    /// <summary>
    /// Represents <see langword="await"/> expression.
    /// </summary>
    public sealed class AwaitExpression : Expression
    {
        private static readonly UserDataSlot<bool> IsAwaiterVarSlot = UserDataSlot<bool>.Allocate();

        public AwaitExpression(Expression expression)
        {
            //expression type must have type with GetAwaiter() method
            const BindingFlags PublicInstanceMethod = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            var getAwaiter = expression.Type.GetMethod(nameof(Task.GetAwaiter), PublicInstanceMethod, Type.DefaultBinder, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
            GetAwaiter = expression.Call(getAwaiter ?? throw new ArgumentException(ExceptionMessages.MissingGetAwaiterMethod(expression.Type)));
            GetResultMethod = GetAwaiter.Type.GetMethod(nameof(TaskAwaiter.GetResult), PublicInstanceMethod, Type.DefaultBinder, Array.Empty<Type>(), Array.Empty<ParameterModifier>());
            if (GetResultMethod is null)
                throw new ArgumentException(ExceptionMessages.MissingGetResultMethod(GetAwaiter.Type));
        }

        internal ParameterExpression NewAwaiterHolder()
        {
            var result = Variable(AwaiterType);
            result.GetUserData().Set(IsAwaiterVarSlot, true);
            return result;
        }

        internal static bool IsAwaiterHolder(ParameterExpression variable)
            => variable.GetUserData().Get(IsAwaiterVarSlot);

        internal MethodCallExpression GetAwaiter { get; }

        internal Type AwaiterType => GetAwaiter.Type;

        internal MethodInfo GetResultMethod { get; }

        /// <summary>
        /// Gets result type of asynchronous operation.
        /// </summary>
        public override Type Type => GetResultMethod.ReturnType;

        /// <summary>
        /// Always return <see langword="true"/>.
        /// </summary>
        public override bool CanReduce => true;

        /// <summary>
        /// Gets expression node type.
        /// </summary>
        /// <see cref="ExpressionType.Extension"/>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        /// Produces call of GetResult method which allows to obtain
        /// result in synchronous manner.
        /// </summary>
        /// <returns>Method call expression.</returns>
        public override Expression Reduce() => GetAwaiter.Call(GetResultMethod);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(GetAwaiter.Object);
            return ReferenceEquals(expression, GetAwaiter.Object) ? this : new AwaitExpression(expression);
        }

        internal MethodCallExpression Reduce(ParameterExpression awaiterHolder, uint state, LabelTarget stateLabel, LabelTarget returnLabel, CodeInsertionPoint prologue)
        {
            prologue(Assign(awaiterHolder, GetAwaiter));
            prologue(new MoveNextExpression(awaiterHolder, state));
            prologue(Return(returnLabel));
            prologue(stateLabel.LandingSite());
            return awaiterHolder.Call(GetResultMethod);
        }
    }
}