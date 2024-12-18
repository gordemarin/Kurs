using Microsoft.EntityFrameworkCore;
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
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class UpdateRequestWindow : Window
    {
        private int RequestID;

        public UpdateRequestWindow(int requestId)
        {
            InitializeComponent();
            RequestID = requestId;

            LoadRequestData();
        }

        private void LoadRequestData()
        {
            using (var context = new ApplicationDbContext())
            {
                var request = context.Requests.Include(r => r.Equipment).FirstOrDefault(r => r.RequestID == RequestID);

                if (request != null)
                {
                    EquipmentTextBox.Text = request.Equipment.Name; // Отображаем название оборудования
                    DescriptionTextBox.Text = request.Description;  // Загружаем текущее описание
                    StatusComboBox.SelectedItem = StatusComboBox.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(item => (string)item.Content == request.Status); // Устанавливаем текущий статус
                }
                else
                {
                    MessageBox.Show("Заявка не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text) || StatusComboBox.SelectedItem is null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                var request = context.Requests.FirstOrDefault(r => r.RequestID == RequestID);

                if (request != null)
                {
                    request.Description = DescriptionTextBox.Text;
                    request.Status = (string)((ComboBoxItem)StatusComboBox.SelectedItem).Content;                   

                    context.SaveChanges();
                    DialogResult = true; // Закрыть окно и вернуть управление
                }
                else
                {
                    MessageBox.Show("Заявка не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
