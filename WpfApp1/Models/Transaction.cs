using System;

namespace WpfApp1.Models
{
    /// <summary>
    /// Класс модели транзакции (доход/расход)
    /// </summary>
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public bool IsIncome { get; set; }
        public string Description { get; set; }
    }
}