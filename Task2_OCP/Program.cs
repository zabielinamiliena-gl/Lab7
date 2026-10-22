using System;

namespace Variant1_PaymentProcessing
{
    public class PaymentRequest
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PayerId { get; set; }
    }

    public interface IPaymentStrategy
    {
        string SystemName { get; }
        decimal CalculateProcessingFee(decimal amount);
        bool ProcessPayment(PaymentRequest request);
    }

    public class CreditCardPaymentStrategy : IPaymentStrategy
    {
        public string SystemName => "CreditCard";

        public decimal CalculateProcessingFee(decimal amount)
        {
            return (amount * 0.025m) + 0.30m;
        }

        public bool ProcessPayment(PaymentRequest request)
        {
            decimal fee = CalculateProcessingFee(request.Amount);
            decimal totalAmount = request.Amount + fee;
            Console.WriteLine($"[{SystemName}] Авторизація картки для користувача {request.PayerId}. Списано: ${totalAmount}");
            return true;
        }
    }

    public class PayPalPaymentStrategy : IPaymentStrategy
    {
        public string SystemName => "PayPal";

        public decimal CalculateProcessingFee(decimal amount)
        {
            return amount * 0.034m;
        }

        public bool ProcessPayment(PaymentRequest request)
        {
            decimal fee = CalculateProcessingFee(request.Amount);
            Console.WriteLine($"[{SystemName}] Перенаправлення на шлюз PayPal для {request.PayerId}. Комісія системи: ${fee}");
            return true;
        }
    }

    public class BankTransferPaymentStrategy : IPaymentStrategy
    {
        public string SystemName => "BankTransfer";

        public decimal CalculateProcessingFee(decimal amount)
        {
            return 5.00m;
        }

        public bool ProcessPayment(PaymentRequest request)
        {
            decimal fee = CalculateProcessingFee(request.Amount);
            decimal totalAmount = request.Amount + fee;
            Console.WriteLine($"[{SystemName}] Формування платіжного доручення SWIFT/IBAN для {request.PayerId}. Сума: ${totalAmount}");
            return true;
        }
    }

    public class CryptoPaymentStrategy : IPaymentStrategy
    {
        public string SystemName => "Crypto";

        public decimal CalculateProcessingFee(decimal amount)
        {
            decimal fee = amount * 0.008m;
            if (fee < 1.00m) return 1.00m;
            if (fee > 20.00m) return 20.00m;
            return fee;
        }

        public bool ProcessPayment(PaymentRequest request)
        {
            Console.WriteLine($"[{SystemName}] Очікування підтвердження транзакції в мережі Blockchain для {request.PayerId}.");
            return true;
        }
    }

    public class ApplePayPaymentStrategy : IPaymentStrategy
    {
        public string SystemName => "ApplePay";

        public decimal CalculateProcessingFee(decimal amount)
        {
            return amount * 0.015m;
        }

        public bool ProcessPayment(PaymentRequest request)
        {
            decimal fee = CalculateProcessingFee(request.Amount);
            decimal totalAmount = request.Amount + fee;
            Console.WriteLine($"[{SystemName}] Біометрична підтверджена транзакція Apple Pay для {request.PayerId}. До сплати: ${totalAmount}");
            return true;
        }
    }

    public class PaymentProcessor
    {
        public bool Process(PaymentRequest request, IPaymentStrategy paymentStrategy)
        {
            Console.WriteLine($" Початок обробки транзакції {request.TransactionId} на суму {request.Amount} {request.Currency}");

            return paymentStrategy.ProcessPayment(request);
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var processor = new PaymentProcessor();

            var request = new PaymentRequest
            {
                TransactionId = "TXN-10092",
                Amount = 150.00m,
                Currency = "USD",
                PayerId = "USR-404"
            };

            IPaymentStrategy creditCard = new CreditCardPaymentStrategy();
            processor.Process(request, creditCard);

            Console.WriteLine();

            IPaymentStrategy crypto = new CryptoPaymentStrategy();
            processor.Process(request, crypto);

            Console.WriteLine();

            IPaymentStrategy applePay = new ApplePayPaymentStrategy();
            processor.Process(request, applePay);
        }
    }
}