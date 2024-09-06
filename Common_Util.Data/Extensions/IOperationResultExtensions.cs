using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Extensions
{
    public static class IOperationResultExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TResult> AsTask<TResult>(this TResult result) where TResult : IOperationResult
        {
            return Task.FromResult(result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<TResult> AsValueTask<TResult>(this TResult result) where TResult : IOperationResult
        {
            return ValueTask.FromResult(result);
        }

        #region Task<???> => Task<IOperationResult>

        public static async Task<IOperationResult> Convert<TResult>(this Task<TResult> task)
            where TResult : IOperationResult
        {
            return await task;
        }

        public static async ValueTask<IOperationResult> Convert<TResult>(this ValueTask<TResult> task)
            where TResult : IOperationResult
        {
            return await task;
        }

        #endregion

        #region obj => OperationResult<T>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OperationResult<T> AsSuccessOperationResult<T>(this T? obj)
        {
            return OperationResult<T>.Success(obj);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OperationResult<T> AsFailureOperationResult<T>(this T? obj, string failureReason)
        {
            if (obj == null)
            {
                return OperationResult<T>.Failure(failureReason);
            }
            else
            {
                return OperationResult<T>.Failure((failureReason, obj));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<OperationResult<T>> AsSuccessOperationResultAsync<T>(this T? obj)
        {
            return ValueTask.FromResult(OperationResult<T>.Success(obj));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<OperationResult<T>> AsFailureOperationResultAsync<T>(this T? obj, string failureReason)
        {
            if (obj == null)
            {
                return ValueTask.FromResult(OperationResult<T>.Failure(failureReason));
            }
            else
            {
                return ValueTask.FromResult(OperationResult<T>.Failure((failureReason, obj)));
            }
        }

        #endregion

        #region IEnumerable<T> => CollectionResult<T>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CollectionResult<T> AsSuccessCollectionResult<T>(this IEnumerable<T> data)
        {
            return CollectionResult<T>.Success(data);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<CollectionResult<T>> AsSuccessCollectionResultAsync<T>(this IEnumerable<T> data)
        {
            return ValueTask.FromResult(CollectionResult<T>.Success(data));
        }

        #endregion
    }
}
