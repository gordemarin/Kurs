using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ServiceCenter
{
    public partial class AddRequestWindow : Window
    {
        public AddRequestWindow()
        {
            InitializeComponent();
            LoadEquipment();
        }

        private void LoadEquipment()
        {
            using (var context = new ApplicationDbContext())
            {
                var equipmentList = context.Equipment.Select(e => new { e.EquipmentID, e.Name }).ToList();
                EquipmentComboBox.ItemsSource = equipmentList;
                EquipmentComboBox.DisplayMemberPath = "Name";
                EquipmentComboBox.SelectedValuePath = "EquipmentID";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentComboBox.SelectedValue is null || string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                var newRequest = new Request
                {
                    EquipmentID = (int)EquipmentComboBox.SelectedValue,
                    Description = DescriptionTextBox.Text,
                    Status = "Новая",
                    RequestDate = DateTime.Now
                };

                context.Requests.Add(newRequest);
                context.SaveChanges();
            }

            DialogResult = true; // Закрыть окно и вернуть управление
        }
    }
}
