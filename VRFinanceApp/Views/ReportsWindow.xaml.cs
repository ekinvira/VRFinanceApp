using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VRFinanceApp.Data;

namespace VRFinanceApp.Views
{
    public partial class ReportsWindow : Window
    {
        public ReportsWindow()
        {
            InitializeComponent();
            LoadMonths();
            LoadYears();
            LoadInitialReport();
        }

        private void LoadMonths()
        {
            cmbMonth.Items.Clear();

            for (int i = 1; i <= 12; i++)
            {
                cmbMonth.Items.Add(new MonthItem
                {
                    Value = i,
                    Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i)
                });
            }

            cmbMonth.DisplayMemberPath = "Name";
            cmbMonth.SelectedValuePath = "Value";
            cmbMonth.SelectedValue = DateTime.Now.Month;
        }

        private void LoadYears()
        {
            cmbYear.Items.Clear();

            for (int i = 2020; i <= DateTime.Now.Year + 2; i++)
            {
                cmbYear.Items.Add(i);
            }

            cmbYear.SelectedItem = DateTime.Now.Year;
        }

        private void LoadInitialReport()
        {
            if (cmbMonth.SelectedValue != null && cmbYear.SelectedItem != null)
            {
                LoadReport((int)cmbMonth.SelectedValue, (int)cmbYear.SelectedItem);
            }
        }

        private void btnLoadReport_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMonth.SelectedValue == null || cmbYear.SelectedItem == null)
            {
                MessageBox.Show("Lütfen ay ve yıl seçiniz.");
                return;
            }

            LoadReport((int)cmbMonth.SelectedValue, (int)cmbYear.SelectedItem);
        }

        private void LoadReport(int month, int year)
        {
            using (AppDbContext db = new AppDbContext())
            {
                var incomeSource = db.Incomes
                    .AsEnumerable()
                    .Where(x => x.Date.Month == month && x.Date.Year == year)
                    .ToList();

                var expenseSource = db.Expenses
                    .AsEnumerable()
                    .Where(x => x.Date.Month == month && x.Date.Year == year)
                    .ToList();

                var incomeData = incomeSource
                    .GroupBy(x => x.Branch)
                    .Select(g => new BranchIncomeDto
                    {
                        Branch = g.Key,
                        TotalAmountValue = g.Sum(x => x.Amount),
                        TotalAmount = g.Sum(x => x.Amount).ToString("N0") + " ₺"
                    })
                    .OrderByDescending(x => x.TotalAmountValue)
                    .ToList();

                var expenseData = expenseSource
                    .GroupBy(x => x.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        TotalAmount = g.Sum(x => x.Amount).ToString("N0") + " ₺"
                    })
                    .OrderByDescending(x => ParseCurrency(x.TotalAmount))
                    .ToList();

                dgIncomeReport.ItemsSource = incomeData.Select(x => new
                {
                    x.Branch,
                    x.TotalAmount
                }).ToList();

                dgExpenseReport.ItemsSource = expenseData;

                decimal totalIncome = incomeSource.Sum(x => x.Amount);
                decimal totalExpense = expenseSource.Sum(x => x.Amount);
                decimal net = totalIncome - totalExpense;

                txtMonthlyIncome.Text = totalIncome.ToString("N0") + " ₺";
                txtMonthlyExpense.Text = totalExpense.ToString("N0") + " ₺";
                txtMonthlyNet.Text = net.ToString("N0") + " ₺";

                borderMonthlyNet.Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(net < 0 ? "#EF4444" : "#3B82F6"));

                LoadIncomePieChart(incomeData);
            }
        }

        private void LoadIncomePieChart(List<BranchIncomeDto> incomeData)
        {
            var series = new List<ISeries>();

            foreach (var item in incomeData)
            {
                series.Add(new PieSeries<decimal>
                {
                    Values = new[] { item.TotalAmountValue },
                    Name = item.Branch,
                    InnerRadius = 70,
                    DataLabelsSize = 14,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N0") + " ₺"
                });
            }

            incomePieChart.Series = series;
        }

        private decimal ParseCurrency(string value)
        {
            string cleaned = value.Replace("₺", "").Trim().Replace(".", "");
            decimal.TryParse(cleaned, out decimal result);
            return result;
        }

        private class MonthItem
        {
            public int Value { get; set; }
            public string Name { get; set; } = "";
        }

        private class BranchIncomeDto
        {
            public string Branch { get; set; } = "";
            public decimal TotalAmountValue { get; set; }
            public string TotalAmount { get; set; } = "";
        }
    }
}