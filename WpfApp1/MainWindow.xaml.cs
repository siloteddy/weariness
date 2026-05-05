using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Data;
using WpfApp1.Logic;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private DataRepository repo;
        private BudgetLogic logic;
        private Transaction selectedTransaction;

        public MainWindow()
        {
            InitializeComponent();
            repo = new DataRepository();
            logic = new BudgetLogic(repo);
            LoadData();
        }

        private void LoadData()
        {
            var transactions = repo.GetAllTransactions();
            DgTransactions.ItemsSource = transactions;
            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            TxtTotalIncome.Text = $"{logic.GetTotalIncome():F2} руб";
            TxtTotalExpense.Text = $"{logic.GetTotalExpense():F2} руб";

            decimal balance = logic.CalculateBalance();
            TxtBalance.Text = $"{balance:F2} руб";

            if (balance >= 0)
                TxtBalance.Foreground = System.Windows.Media.Brushes.Green;
            else
                TxtBalance.Foreground = System.Windows.Media.Brushes.Red;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Проверка суммы
            if (!decimal.TryParse(TxtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму (положительное число)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка категории
            if (string.IsNullOrWhiteSpace(CmbCategory.Text))
            {
                MessageBox.Show("Выберите или введите категорию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем выбранный тип (Доход/Расход)
            string selectedType = (CmbType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "💰 Доход";
            bool isIncome = selectedType.Contains("Доход");

            var transaction = new Transaction
            {
                Amount = amount,
                Category = CmbCategory.Text,
                Date = DpDate.SelectedDate ?? DateTime.Now,
                IsIncome = isIncome,
                Description = TxtDescription.Text ?? ""
            };

            repo.AddTransaction(transaction);

            // Очистка полей (описание не очищаем, может пригодиться повторно)
            TxtAmount.Text = "";
            CmbCategory.SelectedIndex = 0;
            DpDate.SelectedDate = DateTime.Now;

            LoadData();

            string typeText = isIncome ? "доход" : "расход";
            MessageBox.Show($"Транзакция ({typeText}) добавлена!", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DgTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedTransaction = DgTransactions.SelectedItem as Transaction;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTransaction == null)
            {
                MessageBox.Show("Выберите транзакцию для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить транзакцию?\n\n" +
                $"Тип: {(selectedTransaction.IsIncome ? "Доход" : "Расход")}\n" +
                $"Сумма: {selectedTransaction.Amount:F2} руб\n" +
                $"Категория: {selectedTransaction.Category}",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                repo.DeleteTransaction(selectedTransaction.Id);
                LoadData();
                selectedTransaction = null;
            }
        }

        private void BtnForecast_Click(object sender, RoutedEventArgs e)
        {
            var forecast = logic.PredictNextMonthExpenses();
            TxtForecast.Text = $"📊 Прогноз на след. месяц: {forecast:F2} руб";

            MessageBox.Show($"📈 Прогноз расходов на следующий месяц:\n\n" +
                $"{forecast:F2} руб\n\n" +
                "Расчёт основан на среднем арифметическом\n" +
                "расходов за последние 3 месяца.",
                "Математическое моделирование", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}