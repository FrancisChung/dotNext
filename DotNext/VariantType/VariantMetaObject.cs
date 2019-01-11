﻿using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace DotNext.VariantType
{
	internal abstract class VariantMetaObject : DynamicMetaObject
	{
		private protected VariantMetaObject(Expression parameter, IVariant variant)
			: base(parameter, BindingRestrictions.Empty, variant)
		{
			
		}

		protected static DynamicMetaObject CreateActualMetaObject(Expression parameter, IVariant variant)
			=> Create(variant.Value, Expression.Property(parameter, typeof(IVariant), nameof(IVariant.Value)));

		public new IVariant Value => (IVariant)base.Value;

		protected abstract DynamicMetaObject VariantValue { get; }

		public sealed override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
			=> VariantValue.BindBinaryOperation(binder, arg);

		public sealed override DynamicMetaObject BindConvert(ConvertBinder binder)
			=> VariantValue.BindConvert(binder);

		public sealed override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
			=> VariantValue.BindCreateInstance(binder, args);

		public sealed override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
			=> VariantValue.BindDeleteIndex(binder, indexes);

		public sealed override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
			=> VariantValue.BindDeleteMember(binder);

		public sealed override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
			=> VariantValue.BindGetIndex(binder, indexes);

		public sealed override DynamicMetaObject BindGetMember(GetMemberBinder binder)
			=> VariantValue.BindGetMember(binder);

		public sealed override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
			=> VariantValue.BindInvoke(binder, args);

		public sealed override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
			=> VariantValue.BindInvokeMember(binder, args);

		public sealed override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
			=> VariantValue.BindSetIndex(binder, indexes, value);

		public sealed override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
			=> VariantValue.BindSetMember(binder, value);

		public sealed override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
			=> VariantValue.BindUnaryOperation(binder);

		public sealed override IEnumerable<string> GetDynamicMemberNames()
			=> VariantValue.GetDynamicMemberNames();
	}

	internal sealed class VariantImmutableMetaObject : VariantMetaObject
	{
		internal VariantImmutableMetaObject(Expression parameter, IVariant variant)
			: base(parameter, variant)
		{
			VariantValue = CreateActualMetaObject(parameter, variant);
		}

		protected override DynamicMetaObject VariantValue { get; }
	}

	internal sealed class VariantVolatileMetaObject : VariantMetaObject
	{
		internal VariantVolatileMetaObject(Expression parameter, IVariant variant)
			: base(parameter, variant)
		{
		}

		protected override DynamicMetaObject VariantValue => CreateActualMetaObject(Expression, Value);
	}
}