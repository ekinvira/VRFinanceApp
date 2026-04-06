using DanceStudioFinance.Data;
using DanceStudioFinance.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DanceStudioFinance.Views
{
    public partial class InventoryWindow : Window
    {
        public InventoryWindow()
        {
            InitializeComponent();
            dpDate.SelectedDate = DateTime.Today;
        }

        private void OnlyNumber(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtItemName.Text) ||
                    cmbCategory.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(txtQuantity.Text) ||
                    string.IsNullOrWhiteSpace(txtPrice.Text))
                {
                    MessageBox.Show("Lütfen tüm alanları doldurun.");
                    return;
                }

                int quantity = int.Parse(txtQuantity.Text);
                decimal price = decimal.Parse(txtPrice.Text);

                string category = ((ComboBoxItem)cmbCategory.SelectedItem).Content.ToString();

                InventoryItem item = new InventoryItem
                {
                    ItemName = txtItemName.Text,
                    Category = category,
                    Quantity = quantity,
                    UnitPrice = price,
                    PurchaseDate = dpDate.SelectedDate ?? DateTime.Today,
                    Note = txtNote.Text
                };

                using (AppDbContext db = new AppDbContext())
                {
                    db.InventoryItems.Add(item);
                    db.SaveChanges();
                }

                MessageBox.Show("Envanter kaydedildi.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
    }
}