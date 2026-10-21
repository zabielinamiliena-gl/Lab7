using System;
using System.Collections.Generic;

namespace Variant1_OrderFulfillment
{
    public class Order
    {
        public string OrderId { get; set; }
        public string CustomerEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public List<string> Items { get; set; } = new List<string>();
    }

    public interface IOrderRepository
    {
        void SaveOrder(Order order);
    }

    public interface IEmailService
    {
        void SendOrderConfirmation(string email, string orderId);
    }

    public interface IAuditLogger
    {
        void LogTransaction(string message);
    }

    public class MySqlDatabaseRepository : IOrderRepository
    {
        public void SaveOrder(Order order)
        {
            Console.WriteLine($"[MySQL DB] Замовлення {order.OrderId} на суму {order.TotalAmount} грн збережено в таблицю 'orders'.");
        }
    }

    public class SmtpEmailService : IEmailService
    {
        public void SendOrderConfirmation(string email, string orderId)
        {
            Console.WriteLine($"[SMTP Email] Лист з підтвердженням замовлення {orderId} надіслано на {email}.");
        }
    }

    public class FileAuditLogger : IAuditLogger
    {
        public void LogTransaction(string message)
        {
            Console.WriteLine($"[File Logger] Запис до файлу audit.log: {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }
    }

    public class MockOrderRepository : IOrderRepository
    {
        public void SaveOrder(Order order)
        {
            Console.WriteLine($"[Mock DB] Замовлення {order.OrderId} збережено в пам'яті.");
        }
    }

    public class MockEmailService : IEmailService
    {
        public void SendOrderConfirmation(string email, string orderId)
        {
            Console.WriteLine($"[Mock Email] Лист для {email} щодо замовлення {orderId} не надсилався.");
        }
    }

    public class MockAuditLogger : IAuditLogger
    {
        public void LogTransaction(string message)
        {
            Console.WriteLine($"[Mock Logger] {message}");
        }
    }

    public class OrderFulfillmentService
    {
        private readonly IOrderRepository _databaseRepository;
        private readonly IEmailService _emailService;
        private readonly IAuditLogger _fileLogger;

        public OrderFulfillmentService(IOrderRepository databaseRepository, IEmailService emailService, IAuditLogger fileLogger)
        {
            _databaseRepository = databaseRepository;
            _emailService = emailService;
            _fileLogger = fileLogger;
        }

        public void ProcessOrder(Order order)
        {
            if (order == null || order.Items.Count == 0)
            {
                throw new ArgumentException("Замовлення не може бути порожнім.");
            }

            _databaseRepository.SaveOrder(order);
            _fileLogger.LogTransaction($"Успішно оброблено замовлення {order.OrderId}");
            _emailService.SendOrderConfirmation(order.CustomerEmail, order.OrderId);
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Order order = new Order
            {
                OrderId = "A-100",
                CustomerEmail = "buyer@mail.com",
                TotalAmount = 1250,
                Items = new List<string> { "Навушники" }
            };

            OrderFulfillmentService production = new OrderFulfillmentService(
                new MySqlDatabaseRepository(),
                new SmtpEmailService(),
                new FileAuditLogger());
            production.ProcessOrder(order);

            Console.WriteLine();

            OrderFulfillmentService mock = new OrderFulfillmentService(
                new MockOrderRepository(),
                new MockEmailService(),
                new MockAuditLogger());
            mock.ProcessOrder(order);
        }
    }
}
