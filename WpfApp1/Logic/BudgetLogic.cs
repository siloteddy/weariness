using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Logic
{
    /// <summary>
    /// Класс бизнес-логики для расчётов бюджета
    /// </summary>
    public class BudgetLogic
    {
        private DataRepository repo;

        public BudgetLogic(DataRepository repository)
        {
            repo = repository;
        }

        public decimal CalculateBalance()
        {
            var transactions = repo.GetAllTransactions();
            decimal balance = 0;
            foreach (var t in transactions)
            {
                if (t.IsIncome)
                    balance += t.Amount;
                else
                    balance -= t.Amount;
            }
            return balance;
        }

        public decimal GetTotalIncome()
        {
            return repo.GetAllTransactions()
                .Where(t => t.IsIncome)
                .Sum(t => t.Amount);
        }

        public decimal GetTotalExpense()
        {
            return repo.GetAllTransactions()
                .Where(t => !t.IsIncome)
                .Sum(t => t.Amount);
        }

        public decimal PredictNextMonthExpenses()
        {
            var transactions = repo.GetAllTransactions();
            var expensesByMonth = transactions
                .Where(t => !t.IsIncome)
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month)
                .Take(3)
                .Select(g => g.Sum(t => t.Amount))
                .ToList();

            if (expensesByMonth.Count == 0) return 0;
            return Math.Round(expensesByMonth.Average(), 2);
        }
    }
}