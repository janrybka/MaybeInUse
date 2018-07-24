using System;

namespace MaybeInUseWithoutFody
{
    public class Payment
    {
        public Payment(string correlationId, string appCode)
        {
            CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
            AppCode = appCode ?? throw new ArgumentNullException(nameof(correlationId));
        }

        public Maybe<string> CorrelationId { get; private set; }
        public string AppCode { get; private set; }

        public void ChangeCorrelationId(string correlationId)
        {
            this.CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
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
