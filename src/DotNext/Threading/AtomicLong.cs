﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotNext.Threading
{
	using Generic;

	/// <summary>
	/// Various atomic operations for long data type
	/// accessible as extension methods.
	/// </summary>
	/// <remarks>
	/// Methods exposed by this class provide volatile read/write
	/// of the field even if it is not declared as volatile field.
	/// </remarks>
	/// <see cref="Interlocked"/>
	public static class AtomicLong
	{
		private sealed class CASProvider : Constant<CAS<long>>
		{
			public CASProvider()
				: base(CompareAndSet)
			{
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long VolatileGet(ref this long value)
			=> Volatile.Read(ref value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void VolatileSet(ref this long value, long newValue)
			=> Volatile.Write(ref value, newValue);

		/// <summary>
		/// Atomically increments by one referenced value.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <returns>Incremented value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long IncrementAndGet(ref this long value)
			=> Interlocked.Increment(ref value);

		/// <summary>
		/// Atomically decrements by one the current value.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <returns>Decremented value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long DecrementAndGet(ref this long value)
			=> Interlocked.Decrement(ref value);

		/// <summary>
		/// Atomically sets referenced value to the given updated value if the current value == the expected value.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="expected">The expected value.</param>
		/// <param name="update">The new value.</param>
		/// <returns>true if successful. False return indicates that the actual value was not equal to the expected value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CompareAndSet(ref this long value, long expected, long update)
			=> Interlocked.CompareExchange(ref value, update, expected) == expected;

		/// <summary>
		/// Adds two 64-bit integers and replaces referenced integer with the sum, 
		/// as an atomic operation.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="operand">The value to be added to the currently stored integer.</param>
		/// <returns>Result of sum operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Add(ref this long value, long operand)
			=> Interlocked.Add(ref value, operand);

		/// <summary>
		/// Modifies referenced value of the container atomically.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="update">A new value to be stored inside of container.</param>
		/// <returns>Original value before modification.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long GetAndSet(ref this long value, long update)
			=> Interlocked.Exchange(ref value, update);

		/// <summary>
		/// Modifies value of the container atomically.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="update">A new value to be stored inside of container.</param>
		/// <returns>A new value passed as argument.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long SetAndGet(ref this long value, long update)
		{
			VolatileSet(ref value, update);
			return update;
		}

		/// <summary>
		/// Atomically updates the current value with the results of applying the given function 
		/// to the current and given values, returning the updated value.
		/// </summary>
		/// <remarks>
		/// The function is applied with the current value as its first argument, and the given update as the second argument.
		/// </remarks>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="x">Accumulator operand.</param>
		/// <param name="accumulator">A side-effect-free function of two arguments</param>
		/// <returns>The updated value.</returns>
		public static long AccumulateAndGet(ref this long value, long x, Func<long, long, long> accumulator)
			=> Atomic<long, CASProvider>.Accumulute(ref value, x, accumulator).NewValue;

		/// <summary>
		/// Atomically updates the current value with the results of applying the given function 
		/// to the current and given values, returning the original value.
		/// </summary>
		/// <remarks>
		/// The function is applied with the current value as its first argument, and the given update as the second argument.
		/// </remarks>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="x">Accumulator operand.</param>
		/// <param name="accumulator">A side-effect-free function of two arguments</param>
		/// <returns>The original value.</returns>
		public static long GetAndAccumulate(ref this long value, long x, Func<long, long, long> accumulator)
			=> Atomic<long, CASProvider>.Accumulute(ref value, x, accumulator).OldValue;

		/// <summary>
		/// Atomically updates the stored value with the results 
		/// of applying the given function, returning the updated value.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="updater">A side-effect-free function</param>
		/// <returns>The updated value.</returns>
		public static long UpdateAndGet(ref this long value, Func<long, long> updater)
			=> Atomic<long, CASProvider>.Update(ref value, updater).NewValue;

		/// <summary>
		/// Atomically updates the stored value with the results 
		/// of applying the given function, returning the original value.
		/// </summary>
		/// <param name="value">Reference to a value to be modified.</param>
		/// <param name="updater">A side-effect-free function</param>
		/// <returns>The original value.</returns>
		public static long GetAndUpdate(ref this long value, Func<long, long> updater)
			=> Atomic<long, CASProvider>.Update(ref value, updater).OldValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long VolatileGet(this long[] array, long index)
            => VolatileGet(ref array[index]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VolatileSet(this long[] array, long index, long value)
            => VolatileSet(ref array[index], value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long IncrementAndGet(this long[] array, long index)
            => IncrementAndGet(ref array[index]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long DecrementAndGet(this long[] array, long index)
            => DecrementAndGet(ref array[index]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CompareExchange(this long[] array, long index, long value, long comparand)
            => Interlocked.CompareExchange(ref array[index], value, comparand);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareAndSet(this long[] array, long index, long expected, long update)
            => CompareAndSet(ref array[index], expected, update);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Add(this long[] array, long index, long operand)
            => Add(ref array[index], operand);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetAndSet(this long[] array, long index, long update)
            => GetAndSet(ref array[index], update);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SetAndGet(this long[] array, long index, long update)
        {
            VolatileSet(array, index, update);
            return update;
        }

        public static long AccumulateAndGet(this long[] array, long index, long x, Func<long, long, long> accumulator)
            => AccumulateAndGet(ref array[index], x, accumulator);

        public static long GetAndAccumulate(this long[] array, long index, long x, Func<long, long, long> accumulator)
            => GetAndAccumulate(ref array[index], x, accumulator);

        public static long UpdateAndGet(this long[] array, long index, Func<long, long> updater)
            => UpdateAndGet(ref array[index], updater);

        public static long GetAndUpdate(this long[] array, long index, Func<long, long> updater)
            => GetAndUpdate(ref array[index], updater);
    }
}