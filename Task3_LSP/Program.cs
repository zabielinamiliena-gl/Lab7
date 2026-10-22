using System;

namespace Variant1_BankingAccounts
{
    public interface IBankAccount
    {
        string AccountNumber { get; }
        decimal Balance { get; }
        void Deposit(decimal amount);
    }
    public interface IWithdrawableAccount : IBankAccount
    {
        void Withdraw(decimal amount);
    }


    public abstract class BankAccount : IBankAccount
    {
        public string AccountNumber { get; }
        public decimal Balance { get; protected set; }

        protected BankAccount(string accountNumber, decimal initialBalance)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Номер рахунку не може бути порожнім.", nameof(accountNumber));

            if (initialBalance < 0)
                throw new ArgumentException("Початковий баланс не може бути від'ємним.", nameof(initialBalance));

            AccountNumber = accountNumber;
            Balance = initialBalance;
        }

        public virtual void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Сума поповнення повинна бути більшою за нуль.", nameof(amount));

            Balance += amount;
        }
    }


    public class SavingsAccount : BankAccount, IWithdrawableAccount
    {
        public SavingsAccount(string accountNumber, decimal initialBalance)
            : base(accountNumber, initialBalance)
        {
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Сума зняття повинна бути більшою за нуль.", nameof(amount));

            if (amount > Balance)
                throw new InvalidOperationException($"Недостатньо коштів на рахунку {AccountNumber}. Запитано: {amount} $, Доступно: {Balance} $");

            Balance -= amount;
        }
    }

    public class FixedDepositAccount : BankAccount
    {
        public int TermMonths { get; }

        public FixedDepositAccount(string accountNumber, decimal initialBalance, int termMonths)
            : base(accountNumber, initialBalance)
        {
            if (termMonths <= 0)
                throw new ArgumentException("Термін депозиту повинен бути більшим за нуль.", nameof(termMonths));

            TermMonths = termMonths;
        }
    }

    public class BankService
    {
        public void ProcessMonthlyFee(IWithdrawableAccount account, decimal fee)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (fee <= 0)
                throw new ArgumentException("Сума комісії повинна бути більшою за нуль.", nameof(fee));

            account.Withdraw(fee);
            Console.WriteLine($"[Fee] Комісію {fee} $ успішно списано з рахунку {account.AccountNumber}. Новий баланс: {account.Balance} $");
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var bankService = new BankService();
            var savings = new SavingsAccount("ACC-SAV-001", 500.00m);
            Console.WriteLine($"Створено ощадний рахунок {savings.AccountNumber} з балансом {savings.Balance} $");

            var deposit = new FixedDepositAccount("ACC-DEP-002", 1000.00m, 12);
            Console.WriteLine($"Створено депозитний рахунок {deposit.AccountNumber} з балансом {deposit.Balance} $ на {deposit.TermMonths} місяців");

            Console.WriteLine("\nОбробка щомісячної комісії");

            bankService.ProcessMonthlyFee(savings, 15.00m);

            deposit.Deposit(200.00m);
            Console.WriteLine($"[Deposit] Депозит {deposit.AccountNumber} поповнено. Новий баланс: {deposit.Balance} $");
        }
    }
}