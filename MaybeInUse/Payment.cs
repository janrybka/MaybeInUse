using CSharpFunctionalExtensions;
using System;

namespace MaybeInUse
{
    public class Payment
    {
        public Payment(string correlationId, string appCode)
        {
            CorrelationId = correlationId;
            AppCode = appCode;
        }

        public Maybe<string> CorrelationId { get; private set; }
        public string AppCode { get; private set; }

        public void ChangeCorrelationId(string correlationId)
        {
            this.CorrelationId = correlationId;
        }

        public void MaybeChangeCorrelationId(Maybe<string> correlationId)
        {
            this.CorrelationId = correlationId;
        }

        public void SetNullOnCorrelationIdOnPurpose()
        {
            this.ChangeCorrelationId(null);
        }

        public void SetNullOnMaybeCorrelationIdOnPurpose()
        {
            this.MaybeChangeCorrelationId(null);
        }
    }
}
