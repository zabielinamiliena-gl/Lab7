using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace Variant1_OrderProcessing
{
    // --- 1. Сутності доменної моделі ---

    public class OrderItem
    {
        public string ItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderCalculationResult
    {
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public interface IOrderValidator
    {
        void Validate(string customerEmail, List<OrderItem> items);
    }

    public class OrderValidator : IOrderValidator
    {
        public void Validate(string customerEmail, List<OrderItem> items)
        {
            if (string.IsNullOrWhiteSpace(customerEmail) || !customerEmail.Contains("@"))
            {
                throw new ArgumentException("Некоректна електронна адреса покупця.");
            }

            if (items == null || items.Count == 0)
            {
                throw new InvalidOperationException("Кошик порожній.");
            }

            foreach (var item in items)
            {
                if (item.Quantity <= 0 || item.Price <= 0)
                {
                    throw new ArgumentException($"Некоректний товар {item.Name}: ціна та кількість мають бути більше 0.");
                }
            }
        }
    }

    public interface IPricingCalculator
    {
        OrderCalculationResult Calculate(List<OrderItem> items, string customerCategory);
    }

    public class PricingCalculator : IPricingCalculator
    {
        public OrderCalculationResult Calculate(List<OrderItem> items, string customerCategory)
        {
            decimal subtotal = 0;                              
            foreach (var item in items)
            {
                subtotal += item.Price * item.Quantity;
            }

            decimal discountRate = customerCategory switch
            {
                "VIP" => 0.15m,
                "Regular" => 0.05m,
                _ => 0m
            };

            decimal discountAmount = subtotal * discountRate;           
            decimal taxAmount = (subtotal - discountAmount) * 0.20m;
            decimal totalAmount = subtotal - discountAmount + taxAmount;

            return new OrderCalculationResult
            {
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount
            };
        }
    }

    public interface IInvoiceGenerator
    {
        string GenerateHtmlInvoice(string customerEmail, string deliveryAddress, List<OrderItem> items, string customerCategory, OrderCalculationResult calculation);
    }

    public class HtmlInvoiceGenerator : IInvoiceGenerator
    {
        public string GenerateHtmlInvoice(string customerEmail, string deliveryAddress, List<OrderItem> items, string customerCategory, OrderCalculationResult calculation)
        {
            var sb = new StringBuilder();               //інструмент для швидкого збирання великих текстів.
            sb.AppendLine("<html><body>");         
            sb.AppendLine($"<h1>Рахунок на замовлення для {customerEmail}</h1>");   
            sb.AppendLine($"<p>Адреса доставки: {deliveryAddress}</p>");
            sb.AppendLine("<table border='1'><tr><th>Товар</th><th>Кількість</th><th>Ціна</th></tr>");

            foreach (var item in items)
            {
                sb.AppendLine($"<tr><td>{item.Name}</td><td>{item.Quantity}</td><td>{item.Price} грн</td></tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine($"<p>Проміжна сума: {calculation.Subtotal} грн</p>");
            sb.AppendLine($"<p>Знижка ({customerCategory}): -{calculation.DiscountAmount} грн</p>");
            sb.AppendLine($"<p>ПДВ (20%): {calculation.TaxAmount} грн</p>");
            sb.AppendLine($"<h3>Загалом до сплати: {calculation.TotalAmount} грн</h3>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }
    }

    // 2.4. Сховище даних (База даних / Файл)
    public interface IOrderRepository
    {
        void SaveOrder(string customerEmail, decimal totalAmount, string deliveryAddress);
    }

    public class FileOrderRepository : IOrderRepository
    {
        private readonly string _filePath;   

        public FileOrderRepository(string filePath = "orders_db.txt")
        {
            _filePath = filePath;
        }

        public void SaveOrder(string customerEmail, decimal totalAmount, string deliveryAddress)
        {
            string record = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{customerEmail}|{totalAmount}|{deliveryAddress}{Environment.NewLine}";
            File.AppendAllText(_filePath, record);
        }
    }

    public interface INotificationService
    {
        void SendNotification(string recipientEmail, string subject, string bodyHtml);
    }

    public class EmailNotificationService : INotificationService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;

        public EmailNotificationService(string smtpHost = "smtp.store.local", int smtpPort = 587)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
        }

        public void SendNotification(string recipientEmail, string subject, string bodyHtml)
        {
            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort);                    
                var mail = new MailMessage("noreply@store.local", recipientEmail)
                {
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true
                };
                client.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Service Warning] Помилка відправки email: {ex.Message}");
            }
        }
    }

    // --- 3. Клас-оркестратор ---

    public class OrderProcessor
    {
        private readonly IOrderValidator _validator;                                                               
        private readonly IPricingCalculator _calculator;
        private readonly IInvoiceGenerator _invoiceGenerator;
        private readonly IOrderRepository _repository;
        private readonly INotificationService _notificationService;

        public OrderProcessor(
            IOrderValidator validator,
            IPricingCalculator calculator,
            IInvoiceGenerator invoiceGenerator,
            IOrderRepository repository,
            INotificationService notificationService)
        {
            _validator = validator;
            _calculator = calculator;
            _invoiceGenerator = invoiceGenerator;
            _repository = repository;
            _notificationService = notificationService;
        }

        public void ProcessOrder(string customerEmail, string customerCategory, List<OrderItem> items, string deliveryAddress)
        {
            _validator.Validate(customerEmail, items);

            var calculation = _calculator.Calculate(items, customerCategory);

            string htmlInvoice = _invoiceGenerator.GenerateHtmlInvoice(customerEmail, deliveryAddress, items, customerCategory, calculation);

            _repository.SaveOrder(customerEmail, calculation.TotalAmount, deliveryAddress);

            _notificationService.SendNotification(customerEmail, "Ваше замовлення успішно оформлено", htmlInvoice);
        }
    }


    internal class Program
    {
     
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            IOrderValidator validator = new OrderValidator();           //ств сервісів
            IPricingCalculator calculator = new PricingCalculator();
            IInvoiceGenerator invoiceGenerator = new HtmlInvoiceGenerator();
            IOrderRepository repository = new FileOrderRepository();
            INotificationService notificationService = new EmailNotificationService();

            var processor = new OrderProcessor(validator, calculator, invoiceGenerator, repository, notificationService);

            var items = new List<OrderItem>
            {
                new OrderItem { ItemId = "A101", Name = "Смартфон", Price = 12000m, Quantity = 1 },
                new OrderItem { ItemId = "B202", Name = "Бездротові навушники", Price = 2500m, Quantity = 2 }
            };

            Console.WriteLine("=== Початок обробки замовлення ===");
            try
            {
                processor.ProcessOrder("client@example.com", "VIP", items, "м. Київ, вул. Хрещатик, 1");
                Console.WriteLine("Замовлення успішно оброблено та збережено в orders_db.txt!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка виконання: {ex.Message}");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для завершення");
            Console.ReadKey();
        }
    }
}
