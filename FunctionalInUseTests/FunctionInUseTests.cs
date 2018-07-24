using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Xunit;

namespace FunctionalInUseTests
{
    public static class MyExt
    {
        public static async Task<Result<K>> OnSuccessAsync<T, K>(this Result<T> result, Func<T, Task<Result<K>>> func, bool continueOnCapturedContext = true)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            return await func(result.Value);//.ConfigureAwait(continueOnCapturedContext);
        }

        public static async Task<Result<K>> OnSuccessAsync<T, K>(this Task<Result<T>> resultTask, Func<T, Task<Result<K>>> func, bool continueOnCapturedContext = true)
        {
            Result<T> result = await resultTask.ConfigureAwait(continueOnCapturedContext);

            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            return await func(result.Value);
        }
    }

    public class FunctionInUseTests
    {
        Guid PaymentId = Guid.Parse("ab8084ca-ae51-4f59-a766-63a02309d016");

        [Fact]
        public void TestStartPayment()
        {
            Result<string> GetAppCode() => Result.Ok("App Code");
            Result<string> GetApiKey(string appCode) => Result.Ok("API Key");
            Result<Guid> StartPayment(string appKey) => Result.Ok(PaymentId);

            var result = GetAppCode()
                .OnSuccess(GetApiKey)
                .OnSuccess(StartPayment);

            // Równoważne:
            //var result = GetAppCode()
            //    .OnSuccess((appCode) => GetApiKey(appCode))
            //    .OnSuccess((appKey) => StartPayment(appKey));

            result.Should().NotBeNull().And.Match((Result<Guid> res) => res.IsSuccess && res.Value == PaymentId);
        }

        [Fact]
        public void TestFailPayment()
        {
            Result<string> GetAppCode() => Result.Ok("App Code");
            Result<string> GetApiKey(string appCode) => Result.Fail<string>("API Key is invalid");
            Result<Guid> StartPayment(string appKey) => Result.Ok(PaymentId);

            var result = GetAppCode()
                .OnSuccess(GetApiKey)
                .OnSuccess(StartPayment);

            result.Should().NotBeNull().And.Match((Result<Guid> res) => res.IsFailure && res.Error == "API Key is invalid");
        }

        [Fact]
        public async Task TestAsyncPaymentAsync()
        {
            Result<string> GetAppCode() => Result.Ok("App Code");
            async Task<Result<string>> GetApiKeyAsync(string ac) => await Task.Delay(100).ContinueWith((t) => Result.Ok("API Key"));
            async Task<Result<Guid>> StartPaymentAsync(string appKey) => await Task.Delay(100).ContinueWith((t) => Result.Ok(PaymentId));

            var result = await GetAppCode()
                .OnSuccessAsync(GetApiKeyAsync)
                .OnSuccessAsync(StartPaymentAsync);

            //var appCodeResult = GetAppCode();
            //if (appCodeResult.IsFailure)
            //{
            //    throw new Exception("No app code set");
            //}

            //var appCode = appCodeResult.Value;

            //var result =  await GetApiKeyAsync(appCode)
            //        .OnSuccess(async (appKey) => await StartPaymentAsync(appKey));

            //var result = GetAppCode()
            //    .OnSuccess(async (appCode) =>
            //    {
            //        return await GetApiKeyAsync(appCode)
            //                    .OnSuccess((appKey) => StartPaymentAsync(appKey));
            //    });
            //await result.OnSuccess(async t => await t);

            result.Should().NotBeNull().And.Match((Result<Guid> res) => res.IsSuccess && res.Value == PaymentId);
        }

        [Fact]
        public async Task TestAsyncPaymentAllIsAsync()
        {
            Task<Result<string>> GetAppCodeAsync() => Task.FromResult(Result.Ok("App Code"));
            async Task<Result<string>> GetApiKeyAsync(string appCode) => await Task.Delay(100).ContinueWith((t) => Result.Ok("API Key"));
            Task<Result<Guid>> StartPaymentAsync(string appKey) => Task.FromResult(Result.Ok(PaymentId));

            var result = await GetAppCodeAsync()
                .OnSuccess((appCode) => GetApiKeyAsync(appCode))
                .OnSuccess((appKey) => StartPaymentAsync(appKey));

            //var result = await GetApiKeyAsync("test")
            //    .OnSuccess((appKey) => StartPaymentAsync(appKey));

            result.Should().NotBeNull().And.Match((Result<Guid> res) => res.IsSuccess && res.Value == PaymentId);
        }
    }
}
