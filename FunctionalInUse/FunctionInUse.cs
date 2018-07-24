using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalInUse
{
    public class FunctionInUse
    {
        public Result<string> GetAppCode() => Result.Ok("App Code");
        public Result<string> GetApiKey(string appCode) => Result.Ok("API Key");
        public Result<Guid> StartPayment(string appKey) => Result.Ok(PaymentId);

        Guid PaymentId = Guid.Parse("ab8084ca-ae51-4f59-a766-63a02309d016");

        public Result<Guid> TestStartPayment()
        {
            return GetAppCode()
                .OnSuccess(GetApiKey)
                .OnSuccess(StartPayment);

            // Równoważne:
            //return GetAppCode()
            //    .OnSuccess((appCode) => GetApiKey(appCode))
            //    .OnSuccess((appKey) => StartPayment(appKey));
        }

    }
}
