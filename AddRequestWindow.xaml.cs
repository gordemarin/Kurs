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
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DeviceNameTextBox.Text) || 
                string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                string phone = PhoneTextBox.Text;
                string firstName = FirstNameTextBox.Text;
                string lastName = LastNameTextBox.Text;
                
                // Найти или создать клиента
                var client = context.FindOrCreateClient(phone, firstName, lastName);
                
                var newRequest = new Request
                {
                    ClientID = client.ClientID,
                    DeviceName = DeviceNameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    StatusID = RequestStatusConstants.New,
                    Priority = (int)RequestPriority.Normal
                };

                context.Requests.Add(newRequest);
                context.SaveChanges();
            }

            DialogResult = true; // Закрыть окно и вернуть управление
        }
    }
}
