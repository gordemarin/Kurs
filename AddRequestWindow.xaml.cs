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
            if (EquipmentComboBox is null || string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                // Получаем номер телефона из поля ввода
                string Phone = NumberPhoneTextBox.Text;
                
                // Ищем клиента с указанным номером телефона
                var client = context.Clients.FirstOrDefault(c => c.phone == Phone);
                
                int clientId;
                
                if (client != null)
                {
                    // Если клиент найден, используем его ID
                    clientId = client.ClientID;
                }
                else
                {
                    // Если клиент не найден, создаем нового клиента
                    var newClient = new Clients
                    {
                        ClientName = EquipmentComboBox.Text, // Можно добавить поле для ввода имени клиента
                        phone = Phone
                    };
                    
                    context.Clients.Add(newClient);
                    context.SaveChanges();
                    
                    clientId = newClient.ClientID;
                }
                
                var newRequest = new Request
                {
                    
                    DeviceName = DeviceComboBox.Text,
                    Description = DescriptionTextBox.Text,
                    Status = "Новая",
                    RequestDate = DateTime.Now,
                    Phone = Phone,
                    ClientID = clientId // Используем ID найденного или созданного клиента
                };

                context.Requests.Add(newRequest);
                context.SaveChanges();
            }

            DialogResult = true; // Закрыть окно и вернуть управление
        }
    }
}
