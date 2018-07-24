using MaybeInUseWithoutFody;
using System;
using Xunit;

namespace MaybeTests
{
    public class PaymentTests
    {
        [Fact]
        public void CheckNulls()
        {
            var payment = new Payment("Order1", "SCHOOLS");

            Assert.Throws<ArgumentNullException>(() => {
                payment.ChangeCorrelationId(null);
            });
        }

        [Fact]
        public void CheckMaybeNulls()
        {
            var payment = new Payment("Order1", "SCHOOLS");

            payment.MaybeChangeCorrelationId(null);
        }

        [Fact]
        public void Call_a_method_test_fails()
        {
            var payment = new Payment("Order1", "SCHOOLS");

            payment.SetNullOnCorrelationIdOnPurpose();
        }

        [Fact]
        public void Call_a_method_with_success()
        {
            var payment = new Payment("Order1", "SCHOOLS");

            payment.SetNullOnMaybeCorrelationIdOnPurpose();
        }
    }
}
