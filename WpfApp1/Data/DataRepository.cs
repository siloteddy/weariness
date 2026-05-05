using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using WpfApp1.Models;

namespace WpfApp1.Data
{
    /// <summary>
    /// Класс для работы с базой данных SQLite
    /// </summary>
    public class DataRepository
    {
        private string connectionString;

        public DataRepository()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "budget.db");
            connectionString = $"Data Source={dbPath};Version=3;";
            CreateDatabase();
        }

        private void CreateDatabase()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Transactions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Amount REAL NOT NULL,
                        Category TEXT NOT NULL,
                        Date TEXT NOT NULL,
                        IsIncome INTEGER NOT NULL,
                        Description TEXT
                    )";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddTransaction(Transaction transaction)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO Transactions (Amount, Category, Date, IsIncome, Description)
                    VALUES (@Amount, @Category, @Date, @IsIncome, @Description)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Amount", transaction.Amount);
                    cmd.Parameters.AddWithValue("@Category", transaction.Category);
                    cmd.Parameters.AddWithValue("@Date", transaction.Date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@IsIncome", transaction.IsIncome ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Description", transaction.Description ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Transaction> GetAllTransactions()
        {
            var transactions = new List<Transaction>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT Id, Amount, Category, Date, IsIncome, Description FROM Transactions ORDER BY Date DESC";
                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = reader.GetInt32(0),
                            Amount = reader.GetDecimal(1),
                            Category = reader.GetString(2),
                            Date = DateTime.Parse(reader.GetString(3)),
                            IsIncome = reader.GetInt32(4) == 1,
                            Description = reader.GetString(5)
                        });
                    }
                }
            }
            return transactions;
        }

        public bool UpdateTransaction(Transaction transaction)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    UPDATE Transactions 
                    SET Amount = @Amount, Category = @Category, Date = @Date, 
                        IsIncome = @IsIncome, Description = @Description
                    WHERE Id = @Id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", transaction.Id);
                    cmd.Parameters.AddWithValue("@Amount", transaction.Amount);
                    cmd.Parameters.AddWithValue("@Category", transaction.Category);
                    cmd.Parameters.AddWithValue("@Date", transaction.Date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@IsIncome", transaction.IsIncome ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Description", transaction.Description ?? "");
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteTransaction(int id)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "DELETE FROM Transactions WHERE Id = @Id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}